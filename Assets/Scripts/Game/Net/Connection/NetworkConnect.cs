using UnityEngine;
using System;
using System.Net;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using tsf4g_tdr_csharp;
using Apollo;

using Utils.Event;
using GSProtocal;

#region  网络模块相关数据
/// <summary>
/// 网络状态
/// </summary>
public enum NetState
{
    Disconnected,
    Connecting,
    Connected,
}

public class PkgInfo
{
    public byte[] buffer;
    public int bufferLen;
    public int timeoutInterval;
    public UInt16 cmd;

    public PkgInfo(byte[] buf, int len, int timeout, UInt16 cmd)
    {
        buffer = buf;
        bufferLen = len;
        timeoutInterval = timeout;
        this.cmd = cmd;
    }
}
#endregion


public class NetworkConnect : MonoBehaviour {

    public delegate void DisconnectedNotify(TGCP_ERROR error);
    public delegate void PkgSendNotify(bool isHeartbeat = false);
    public delegate bool ChkEnablePkg(UInt16 cmd);
    public delegate void OnPkgError(UInt16 cmd, int errorCode);
    

    //-------------------------------------------------------------------------
    //                         Create - NetworkConnect
    //-------------------------------------------------------------------------

    private static Transform _root = null;
    /// <summary>
    /// 创建一个客户端网络连接
    /// </summary>
    /// <returns></returns>
    public static NetworkConnect Create(string svrName, Transform pluginRoot, ProtoUtil protoUtil)
    {
        if (_root == null) _root = pluginRoot;
        GameObject go = new GameObject();
        go.name = svrName;
        go.transform.parent = _root;
        NetworkConnect server = go.AddComponent<NetworkConnect>();
        server.Init(protoUtil);
        return server;
    }

    //-------------------------------------------------------------------------
    //                              NetworkConnect
    //-------------------------------------------------------------------------
    private ChkEnablePkg onChkSendPkg;//检测是否可发送的包
    private ChkEnablePkg onChkRecvPkg;//检测是否是可接收的包
    private DelegateEnums.NoneParam onApplicationWakeupSendHeartbeat;
    private OnPkgError onPkgError;

    private DateTime updateTime = DateTime.Parse("1970-1-1");

    private bool m_inited = false;
    private string m_ipAddress = "1.0.0.0";
    private ushort m_port = 8080;
    //重连次数上限
    private int m_reConnectLimit = 3;
    //网络状态
    private NetState m_netState = NetState.Disconnected;
    private bool m_isConnect = false;


    private TGCPAPI m_TgcpHandle;
    private TGCPEvent m_events;
    private Queue<MsgData> m_RecvQueue;
    private Queue<MsgData> m_pkgBuff;
    private byte[] m_recv;
    //要发送的协议包
    private List<PkgInfo> m_pkgList;


    //private LoginRetInfo m_loginInfo = null;
    private string m_openid = UnityEngine.SystemInfo.deviceUniqueIdentifier;


    //负责解包，数据发送代理，数据timeout ,协议回调分发
    private ProtoUtil m_protoUtil = null;
    //超时检测
    private NetworkTimeoutMonitor m_networkTimeoutMonitor = null;
    private NetworkHeartBeat m_networkHeartBeat;//心跳发送器


    //开始连接时间
    private long m_StartConnectTime;
    private long connTimeoutInterval = 10000;
    //超时时间
    private long m_connTimeoutInterval = 15000;
    private int connectRetryCount=0;
    private int currentRetryMaxTimes = 4;
    private Operation m_timeoutOperation;
    //重连
    private Operation _timerForReconnect;


    #region 对外接口
    /// <summary>
    /// 网络状态
    /// </summary>
    public NetState NetWorkState { get { return m_netState; } set { m_netState = value; } }
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ProtoUtil protoUtil)
    {
        m_inited = true;

        m_RecvQueue = new Queue<MsgData>();
        m_pkgBuff = new Queue<MsgData>();
        m_recv = new byte[1024 * 1024];
        m_pkgList = new List<PkgInfo>();
        m_networkHeartBeat = new NetworkHeartBeat();
        m_networkTimeoutMonitor = new NetworkTimeoutMonitor();
        m_protoUtil = protoUtil;
        m_protoUtil.svrSendCallback += this.Send;
        
        //string openid = "testMage111";
        //string openid = "EBF56FF9F559CED6A367D2302786774F";
        //string openid = "B130A6EDF67F92AD93075669C6850FE8";
		string openid = "leolin01";
        //string openid = "openid" + UnityEngine.SystemInfo.deviceUniqueIdentifier;

        //pc默认值
        //m_loginInfo = new LoginRetInfo(
        //    0,
        //    (int)PlatformType.None,
        //    openid,
        //    "pf",
        //    "pf123456",
        //    "userId123456",//userID
        //    "pcToken",
        //    "refreshToken",
        //    "payToken"); 

    }
    /// <summary>
    /// 注册心跳发包句柄
    /// </summary>
    /// <param name="onHeartBeatHandler"></param>
    public void RegisterHandle(DelegateEnums.NoneParam onHeartBeatHandler, ChkEnablePkg chkSendPkg, ChkEnablePkg chkRecvPkg = null,OnPkgError pkhError = null)
    {
        m_networkHeartBeat.RegisterNetConnector(this, m_protoUtil, onHeartBeatHandler);
        m_networkTimeoutMonitor.RegisterNetConnector(this, onHeartBeatHandler);
        onApplicationWakeupSendHeartbeat += onHeartBeatHandler;
        onChkSendPkg += chkSendPkg;
        onChkRecvPkg += chkRecvPkg;
        onPkgError += pkhError;
    }

    public void UnRegisterHandle(DelegateEnums.NoneParam onHeartBeatHandler, ChkEnablePkg chkSendPkg, ChkEnablePkg chkRecvPkg, OnPkgError pkhError)
    {
        m_networkHeartBeat.UnRegisterNetConnect(onHeartBeatHandler);
        m_networkTimeoutMonitor.UnRegisterNetConnector(onHeartBeatHandler);
        onApplicationWakeupSendHeartbeat -= onHeartBeatHandler;
        onChkSendPkg -= chkSendPkg;
        onChkRecvPkg -= chkRecvPkg;
        onPkgError -= pkhError;
    }

    /// <summary>
    /// 重新连接
    /// </summary>
    public void ReConnect(long timeout = 15000, bool isManual = false)
    {
        Log.info("Net reconnect, maxRetryCount:" + connectRetryCount);

        if (isManual)
        {
            EventManager.Send(EventType_Net.NET_STATE_CHANGE_TIP, 0);
        }

        m_connTimeoutInterval = timeout;
        //通知重新连接
        EventManager.Send(EventType_Net.NET_RECONNECT_START);

#if UNITY_EDITOR || UNITY_IPHONE
        this.Connect(this.m_ipAddress, this.m_port);
        return;
#else
        if (NetworkMonitor.Network_Connected)
        {
            Log.info("Net_ReConned");
            this.Connect(this.m_ipAddress, this.m_port);
        }
        else
        {
            Log.info("wait network monitor connected");
            EventManager.AddEventListener(EventType_Net.NETWORK_STATE_CONNECTED, OnNetworkReConnected);
        }
#endif
    }
    /// <summary>
    /// 连接网络
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    public void Connect(string ipAddress, ushort port)
    {
        this.m_ipAddress = ipAddress;
        this.m_port = port;
        m_timeoutOperation = OperationManager.DoVoidFuncOnMainThread(OnConnectTimeout, m_connTimeoutInterval);
        this.ConnectInternal();
    }
    /// <summary>
    /// 主动关闭服务器连接
    /// </summary>
    /// <param name="clearPkgList"></param>
    public void DisConnect(bool clearPkgList = true)
    {
        Log.info("DisConnect");
        m_netState = NetState.Disconnected;

        if (null != m_TgcpHandle)
        {
            m_TgcpHandle.CloseConnection();
            m_TgcpHandle.Fini();
            m_TgcpHandle.Destroy();
            m_TgcpHandle = null;
        }

        if (null != m_timeoutOperation)
        {
            m_timeoutOperation.cancel();
            m_timeoutOperation = null;
        }

        if (clearPkgList)
        {
            Log.info("客户端主动断开,清除m_RecvQueue", "NetworkConnect");
            if (null != m_RecvQueue)
            {
                m_RecvQueue.Clear();
            }
            Log.info("客户端主动断开,清除未发送完的回包", "NetworkConnect");
            if (null != m_pkgList)
            {
                m_pkgList.Clear();
            }
        }
        //         else {
        //             Log.info("后台主动断开", "NetworkConnect");
        //         }
        //TODO 通知隐藏tip
        //EventManager.Send(EventType_Net.TGCP_DisconnectedByManual_HideTip);
    }

    /// <summary>
    /// 发送协议
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="len"></param>
    /// <param name="timeout"></param>
    /// <param name="cmd"></param>
    /// <param name="relatedCMD"></param>
    /// <returns></returns>
    public int Send(ref byte[] buf, int len, int timeout, UInt16 cmd, UInt16 relatedCMD)
    {
        if (m_netState != NetState.Connected)
        {
            Log.info("网络末连接上");
            return -1;
        }

        int iRet = 0;
        iRet = m_TgcpHandle.Update(ref m_events);
        if (0 != iRet)
        {
            Log.info("after update,happend error: " + iRet);
            return iRet;
        }

        if (relatedCMD != 0)
        {//如果需要检测配对协议，记录之前发的包
            m_pkgList.Add(new PkgInfo(buf, len, timeout, cmd));
        }

        if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_DATA_OUT) > 0)
        {
#if UNITY_EDITOR
            if (cmd != 12)
            {//过滤心跳包打印
                Log.info("Send Pkg: " + cmd);
            }
#else
            Log.info("Send Pkg: " + cmd);
#endif
            iRet = m_TgcpHandle.Send(buf, len, timeout);
            if (0 != iRet)
            {
                Log.info("after send,happend error: " + iRet);
                return iRet;
            }
            SendInternal();
        }
        else
        {
            //TODO error
            Log.info("tgcp send pkg failed,data out is not TGCP_EVENT_DATA_OUT ");
            iRet = (int)TGCP_ERROR.TGCP_ERR_EVENT_DATA_OUT;
        }
        return iRet;
    }
    
    //长时间末收到的服务端的包
    public void OnReceivePkgFailed()
    {
        Log.info("长时间末从服务收到包");
        NotifyDisconnected(TGCP_ERROR.TGCP_EVENT_TGCP_LONG_TIME_NOT_RECEIVE_PKG);
    }

    //网络切换
    public void CheckNetworkState(bool applicationWakeup = true, bool checkHeartbeat = false)
    {
        if (null == m_TgcpHandle)
        {
            if (applicationWakeup)
            {
                Log.info("m_TgcpHandle is null on application wake up");
            }
            else
            {
                Log.info("m_TgcpHandle is null on network disconnect");
            }
            return;
        }
        int iRet = 0;
        iRet = m_TgcpHandle.Update(ref m_events);
        if (iRet < 0)
        {
            NotifyDisconnected((TGCP_ERROR)iRet);
            return;
        }
        if (checkHeartbeat)
        {
            if (null != onApplicationWakeupSendHeartbeat)
            {
                onApplicationWakeupSendHeartbeat();
            }
        }
    }
    #endregion

    #region FixedUpdate
    void FixedUpdate()
    {
        if (!m_inited) return;

        if (m_netState == NetState.Disconnected) return;

        this.NetMainLoop();

        //处理超时协议
        m_protoUtil.Tick();

        //处理收到的回包
        m_pkgBuff.Clear();

        int count = m_RecvQueue.Count;
        for (int i = 0; i < count; ++i)
        {
            m_pkgBuff.Enqueue(m_RecvQueue.Dequeue());
        }
        m_RecvQueue.Clear();

        int countOfMSG = m_pkgBuff.Count;
        for (int i = 0; i < countOfMSG; i++)
        {
            MsgData msg = m_pkgBuff.Dequeue();
            if (this.onChkRecvPkg(msg.Cmd))
            {
                m_protoUtil.DispatchEvent(msg);
            }

        }

        //统一清理超时和已经收到的包
        m_protoUtil.Clear();

        //          //收包、网络状态变更检测
        //          TimeSpan ts = DateTime.Now - updateTime;
        //          int Milliseconds = ts.Milliseconds;
        //          Debug.Log("Update tike " + Milliseconds);
        //          updateTime = DateTime.Now;
        m_networkHeartBeat.Render();
    }

    private int NetMainLoop()
    {
        int iRet = 0;
        if (null == m_TgcpHandle)
        {
            return (int)TGCP_ERROR.TGCP_EVENT_TGCP_IS_NULL;
        }
        iRet = m_TgcpHandle.Update(ref m_events);
        if (iRet < 0)
        {
            //连接失败处理
            NotifyDisconnected((TGCP_ERROR)iRet);
            return iRet;
        }
        for (uint i = 0; i < m_events.event_num; ++i)
        {
            if (m_events.event_num > 0)
            {
                //判断是否有数据可读
                if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_DATA_IN) > 0)
                {
                    //读取网络数据
                    int len = m_recv.Length;
                    iRet = m_TgcpHandle.Recv(m_recv, ref len, 0);
                    if (0 != iRet) return iRet;
                    //数据解包
                    MsgData msgdata = new MsgData();
                    msgdata.Set(m_recv, len);
                    msgdata.UnPack();
                    if (msgdata.Eno != 0 && this.onPkgError != null)
                    {
                        this.onPkgError(msgdata.Cmd, msgdata.Eno);
                    }
                    else {
                        Log.info("收到协议:" + msgdata.Cmd);
                        m_RecvQueue.Enqueue(msgdata);
                        m_networkTimeoutMonitor.ReceivePkg();
                    }
                }
            }
        }

        //连接成功
        if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_DATA_OUT) > 0)
        {
            //表示网络层异步连接成功
            if (false == m_isConnect)
            {
                Log.info("---------NetMainLoop() m_isConnect suc---------------");
                NotifyConnected();
                m_isConnect = true;

                EventManager.Send(EventType_Net.NET_CONNECTED);
                Log.info("Net_Conned");
            }
            if (m_netState == NetState.Connected)
            {
                SendInternal();
            }
        }

        //服务器主动关闭会话
        if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_SSTOPED) > 0)
        {
            m_isConnect = true;
            Log.info("server stop connection");
            if (m_netState == NetState.Connected)
            {
                NotifyDisconnected(TGCP_ERROR.TGCP_EVENT_SOCKET_DISCONNECT);
            }
        }

        //正在排队
        if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_WAITING) > 0)
        {
            Log.info("server TGCP_EVENT_WAITING", "Net.Mainloop");
            NotifyDisconnected(TGCP_ERROR.TGCP_EVENT_WAITING);
        }

        //服务器爆满通知
        if ((m_events.events & (uint)eTGCPEvent.TGCP_EVENT_SVR_IS_FULL) > 0)
        {
            Log.info("server TGCP_EVENT_SVR_IS_FULL", "Net.Mainloop");
            NotifyDisconnected(TGCP_ERROR.TGCP_EVENT_SVR_IS_FULL);
        }
        return iRet;
    }
    #endregion

    //超时的处理
    private void OnConnectTimeout()
    {
        if (m_netState != NetState.Connected)
        {
            Log.info("Net_onConnectTimeout");
            DisConnect(false);
            this.OnDisConnectError(TGCP_ERROR.TGCP_ERR_TIMEOUT);
        }
    }

    //连接
    private int ConnectInternal()
    {
        int ret = 0;
        int iSerivceID = 123;
        int iBuffLen = 1024 * 1024;
        string appid = "appId123456";

        eAccountType AccountType = eAccountType.TGCP_ACCOUNT_TYPE_PC_OPENID;
        eEncryptMethod iEncMethod = eEncryptMethod.TGCP_ENCRYPT_METHOD_AES;
        eKeyMaking iKeyMode = eKeyMaking.TGCP_KEY_MAKING_INSVR;
        
//        if (this.m_loginInfo == null)
//        {
//#if !UNITY_EDITOR
//            m_loginInfo = MSDK.Instance.getLoginRecord();
//            GlobalData.system.SaveLoginRetInfo(m_loginInfo);

//            appid = (PlatformType.Weixin == m_loginInfo.platform) ? NetConfig.wx_appid : NetConfig.qq_appid;
//#endif
//        }

        if (null != m_TgcpHandle) 
            Log.info("有旧的tcp handler", "NetConnector.connectInternal");
        m_TgcpHandle = new TGCPAPI();

#if UNITY_STANDALONE_WIN
        m_TgcpHandle.WSAStartUp(2, 0);
#endif

        //switch (this.m_loginInfo.platform)
        //{
        //    case PlatformType.Weixin:
        //        AccountType = eAccountType.TGCP_ACCOUNT_TYPE_WX_OPENID;
        //        break;
        //    case PlatformType.QQ:
        //        AccountType = eAccountType.TGCP_ACCOUNT_TYPE_QQ_OPENID;
        //        break;
        //    case PlatformType.Guest:
        //        AccountType = eAccountType.TGCP_ACCOUNT_TYPE_IOS_GUEST;
        //        break;
        //    default:
        //        Log.info("AccountType 末赋值");  
        //        break;
        //}
  
        Log.info("appid: " + appid);
        Log.info("openid: " + m_openid);
        Log.info("accesstoken: " + "");
        Log.info("AccountType: " + AccountType);
        Log.info("iEncMethod: " + iEncMethod);
        Log.info("iKeyMode: " + iKeyMode);
        Log.info("iSerivceID: " + iSerivceID);

        //if (string.IsNullOrEmpty(m_loginInfo.openid))
        //{
        //    Log.info("openid为空，须重新登录");
        //    //openid为空操作待补
        //    OnOpenIdIsEmptyHandler();
        //    return ret;
        //}
        // 发送更新鉴权信息
        //EventManager.Send(EventType_DataReport.QQ_REPORT_UPDATE_AUTH_INFO, new object[] { accesstoken, openid });

        byte[] byteid = TDRTools.strToByte(appid);
        byte[] bytetoken = TDRTools.strToByte("access tokey");

        ret = m_TgcpHandle.create_and_init_string(iSerivceID, byteid, byteid.Length, iBuffLen, AccountType, m_openid, bytetoken, bytetoken.Length);
        if (0 != ret)
        {
            Log.info("m_TgcpHandle.create_and_init_string failed: " + ret);
            return ret;
        }

        ret = m_TgcpHandle.set_security_info(iEncMethod, iKeyMode, "");
        if (0 != ret)
        {
            Log.info("m_TgcpHandle.set_security_info failed: " + ret);
            return ret;
        }

        ret = m_TgcpHandle.set_refresh_token_expire(0);
        if (0 != ret)
        {
            Log.info("m_TgcpHandle.set_refresh_token_expire failed: " + ret);
            return ret;
        }

        // #if UNITY_EDITOR
        //         ret = m_TgcpHandle.set_authtype(eAuthType.TGCP_AUTH_NONE);
        // #endif
        //暂时都关闭鉴权
        ret = m_TgcpHandle.set_authtype(eAuthType.TGCP_AUTH_NONE);

        Log.info("m_TgcpHandle init success");

        try
        {
            Log.info("ip: " + m_ipAddress);
            Log.info("port: " + GlobalData.system.serverData.port);
            ret = AsynConnectServer(m_ipAddress, m_port);
            if (0 != ret)
            {
                Log.infoWarning("Could not connect!");
                NotifyDisconnected((TGCP_ERROR)ret);
            }
            Log.info("Net_initConned");
        }
        catch (Exception ex)
        {
            Log.info(ex.ToString());
        }

        return ret;
    }

    //异步连接,超时时间单位为毫秒
    private int AsynConnectServer(string pszIP, ushort nPort)
    {
        int iRet = 0;
        m_isConnect = false;
        string url = "tcp://" + pszIP + ":" + nPort.ToString();
        Log.info("conntect to " + url);
        iRet = m_TgcpHandle.Start(url);
        Log.info("AsynConnectServer:" + iRet.ToString());
        m_netState = NetState.Connecting;

        if (0 != iRet)
        {
            return iRet;
        }
        m_StartConnectTime = System.Environment.TickCount;
        return iRet;
    }

    //发送协议逻辑
    private int SendInternal()
    {
        int len = m_pkgList.Count;
        if (len > 0)
        {
            PkgInfo info = null;
            int index = 0;
            PkgInfo tempInfo;
            while (index < len)
            {
                tempInfo = m_pkgList[index];
                if (onChkSendPkg(tempInfo.cmd))
                {//包发送条件
                    info = tempInfo;
                    //Log.info("前面的协议包要等角色登录后才能发送，调整发送索引到" + index);
                    break;
                }
                index++;
            }
            if (null == info)
            {
                //Log.info("所有的协议包要等角色登录后才能发送");
                return 0;
            }
            int iRet = 0;
            Log.info("SendInternal Send pkg: " + info.cmd);
            //Log.info("pkg len: " + info.bufferLen);
            //Log.info("pkg buffer len: " + info.buffer.Length);
            iRet = m_TgcpHandle.Send(info.buffer, info.bufferLen, info.timeoutInterval);

            if (0 != iRet)
            {
                Log.info("after send,happend error: " + iRet);
                return iRet;
            }
            m_pkgList.RemoveAt(index);
            m_networkTimeoutMonitor.SendPkg((UInt16)CS_CMD_ID.CS_SYN_TIME == info.cmd);
            
        }
        return 0;
    }

    private void OnOpenIdIsEmptyHandler()
    {
        //openid为空不操作，直接登出
        //        EventManager.Send(EventType_Net.HIDE_MW_CONN_TIP);
        //        EventManager.Send(EventType_Net.TGCP_DisconnectedByManual_HideTip);
        if (m_netState == NetState.Disconnected)
        {
            if (null != m_timeoutOperation)
            {//取消超时操作
                m_timeoutOperation.cancel();
                m_timeoutOperation = null;
            }
        }
        else
        {
            NotifyDisconnected(TGCP_ERROR.TGCP_EVENT_SOCKET_ON_OPENID_IS_NULL_OR_EMPTY);
        }
    }

    //连接成功处理
    protected void NotifyConnected()
    {
        if (m_netState == NetState.Connected) return;

        m_netState = NetState.Connected;
        if (m_timeoutOperation != null)
        {
            m_timeoutOperation.cancel();
            m_timeoutOperation = null;
        }

        //重置重连信息
        connectRetryCount = 0;
        if (_timerForReconnect != null)
        {
            _timerForReconnect.cancel();
        }

        //启动心跳器
        if (m_networkHeartBeat != null)
        {
            m_networkHeartBeat.OnNetConnectedToStartHeartBeat();
        }

        EventManager.Send(EventType_Net.MSDK_AUTH_SUCCESS_TSS);
        Log.info("send success info to tss");
        Log.info("Connect Success!");
    }
    //连接失败处理
    private void NotifyDisconnected(TGCP_ERROR retError)
    {
        if (m_netState == NetState.Disconnected)
        {
            return;
        }
        Log.info("notifyDisconnected", "Net");
        Log.info("net will set net state: " + NetState.Disconnected);

        if (m_timeoutOperation != null)
        {
            m_timeoutOperation.cancel();
            m_timeoutOperation = null;
        }
        //后台主动断开
        DisConnect(false);
        this.OnDisConnectError(retError);

        EventManager.Send(EventType_Net.MSDK_AUTH_FAILED_TSS);
        //Log.info("send failed info to tss");
        //Log.info("Connect Failed! Error:" + retError);
    }
    //确认网络状态转换为重新连接上后连接
    private void OnNetworkReConnected(GEvent e)
    {
        Log.info("Net_ReConned after network monitor reconnect");
        EventManager.RemoveEventListener(EventType_Net.NETWORK_STATE_CONNECTED, OnNetworkReConnected);
        Connect(this.m_ipAddress,this.m_port);
    }


    #region 连接错误处理
    //断开连接
    private void OnDisConnectError(TGCP_ERROR error)
    {
        Log.infoError("**NetConnect::OnDisConnectError::Code: " + error.ToString());
        //网络断开重置，心跳，避免干扰错误提示
        m_networkHeartBeat.OnDisConnected();

        if (_timerForReconnect != null) _timerForReconnect.cancel();

        switch (error)
        {
            case TGCP_ERROR.TGCP_ERR_AUTH_FAIL:
            case TGCP_ERROR.TGCP_ERR_NO_OPENID:
            case TGCP_ERROR.TGCP_ERR_NO_TOKEN:
            case TGCP_ERROR.TGCP_ERR_AUTH_REFRESH_FAIL:
            case TGCP_ERROR.TGCP_ERR_TOKEN_INVALID:
                if (connectRetryCount > currentRetryMaxTimes)
                {
                    EventManager.Send(EventType_Net.NET_AUTH_FAILED, (int)error);
                }
                else
                {
                    Log.info("重连前验证token有效性");
                    //AuthenticationManager.IsPayTokenExpired(this.OnReconnect);
                    EventManager.Send(EventType_Net.EVENT_CHECK_PAYTOKEN);
                }
                break;
            case TGCP_ERROR.TGCP_ERR_CONNECT_FAILED:
                //可能是网络闪断引起（无网络覆盖），可以适当增加重试频率同时放宽最大重试次数
                if (connectRetryCount > currentRetryMaxTimes * 2)
                {
                    EventManager.Send(EventType_Net.NETWORK_STATE_DISCONNECTED);
                    return;
                }
                Log.info("网络闪断，快速重连...count：" + connectRetryCount);
                connectRetryCount++;
                this.ReConnect();
                break;
#if UNITY_EDITOR
            case TGCP_ERROR.TGCP_ERR_PEER_CLOSED_CONNECTION:
            case TGCP_ERROR.TGCP_ERR_PEER_STOPPED_SESSION:
                EventManager.Send(EventType_Net.NET_SERVER_DISCONNECTED, error);
                break;
#else
            case TGCP_ERROR.TGCP_ERR_PEER_CLOSED_CONNECTION:
            case TGCP_ERROR.TGCP_ERR_PEER_STOPPED_SESSION:
#endif
            case TGCP_ERROR.TGCP_ERR_TIMEOUT:
            case TGCP_ERROR.TGCP_ERR_NETWORK_EXCEPTION:
            case TGCP_ERROR.TGCP_EVENT_TGCP_LONG_TIME_NOT_RECEIVE_PKG:
            case TGCP_ERROR.TGCP_EVENT_SOCKET_DISCONNECT:
            case TGCP_ERROR.TGCP_ERR_SERVER_IS_FULL:
                if (connectRetryCount > currentRetryMaxTimes)
                {//弹窗Alert确认框，手动尝试重连。
                    EventManager.Send(EventType_Net.NETWORK_STATE_DISCONNECTED);
                }
                else
                {
                    EventManager.Send(EventType_Net.NET_DISCONNECTED);
                    if (TGCP_ERROR.TGCP_ERR_TIMEOUT == error)
                    {
                        EventManager.Send(EventType_Net.TGCP_ConnectTimeout);
                    }
                    _timerForReconnect = OperationManager.DoVoidFuncOnMainThread(this.OnReconnect, connTimeoutInterval / 2);
                }
                break;
            case TGCP_ERROR.TGCP_EVENT_SOCKET_ON_OPENID_IS_NULL_OR_EMPTY:
                //openid为空错误不处理
                break;
            default:
                //延迟一会再连
                _timerForReconnect = OperationManager.DoVoidFuncOnMainThread(OnReconnect, connTimeoutInterval);
                break;
        }//end switch
    }//end OnDisConnectError

    private void OnReconnect()
    {
        Log.info("[NetController] Auto reconnect...");
        connectRetryCount++;
        EventManager.Send(EventType_Net.NET_STATE_CHANGE_TIP, 0);
        this.ReConnect((connectRetryCount + 1) * connTimeoutInterval);
    }
    #endregion
}
