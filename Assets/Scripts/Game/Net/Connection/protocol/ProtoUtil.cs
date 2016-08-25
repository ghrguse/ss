using UnityEngine;
using System.Collections;
using GSProtocal;
using System.Collections.Generic;
using System;
using tsf4g_tdr_csharp;
using Utils.Event;

public delegate void EventHandleMsg(MsgData data);
public delegate void EventHandleTimeOut();

public delegate void PkgSendDelegate(UInt16 cmd);
public delegate void PkgTimeoutDelegate(UInt16 cmd);
public delegate void PkgRecvDelegate(UInt16 cmd);
public delegate int SvrSendDelegate(ref byte[] buf, int len, int timeout, UInt16 cmd, UInt16 relatedCMD);

delegate void PackageTimeoutHandle(UInt16 cmd);
//负责解包，数据发送代理，数据timeout ,协议回调分发
public class ProtoUtil
{
    //private Dictionary<UInt16, PackageInfo> DicPackageInfo;
    private Dictionary<UInt16, UInt16> CmdConver;
    //回调列表
    private Dictionary<UInt16, List<EventHandleMsg>> eventTable;
    //超时回调列表
    private Dictionary<UInt16, List<EventHandleTimeOut>> handleTimeOut;
    //事件映射
    //private List<UInt16> mPackageInfoKeys = new List<UInt16>();
    private byte[] buffer = new byte[1024 * 1024];
    //发包后回调，重置心跳包计时器
    public PkgSendDelegate pkgSendCallback;
    //net发包
    public SvrSendDelegate svrSendCallback;

    System.DateTime startTime = DateTime.Parse("1970-1-1");
    //超时要删除的协议
    private List<UInt16> delelist = new List<ushort>();
    ////外部逻辑主动取消的协议
    //private List<UInt16> receiveCMDList = new List<UInt16>();

    //存储所有关键包和对应协议信息
    private KeyPackageManager key2PackageRegistry = null;
    //超时锁屏标识管理器
    private PackageLockMananger _lockManager = null;

    #region 内部类，记录每个发送成功包信息
    //定义一个内部类，记录每个发送成功包信息
    class PackageInfo
    {
        //协议cmd
        UInt16 m_Cmd;
        //超时时间
        UInt64 m_TimeOut;
        //流水号,服务器透传
        //uint m_Seq;
        //超时回调函数
        //EventHandleTimeOut m_Handle;
        public UInt16 Cmd
        {
            get{return m_Cmd;}
            set{m_Cmd = value;}
        }

        public UInt64 TimeOut
        {
            get{return m_TimeOut;}
            set{m_TimeOut = value;}
        }

        //public EventHandleTimeOut Handle
        // {
        //     get{return m_Handle;}
        //     set{m_Handle = value;}
        // }
    }
    #endregion

    #region 内部类，记录超时锁屏标识
    //定义一个内部类，记录每个发送成功包信息
    class PackageLockMananger
    {
        private Dictionary<UInt16, bool> _lockCmdDict = new Dictionary<UInt16, bool>();

        //移出cmd
        public bool RemovePackageCmd(UInt16 cmd)
        {
            if (_lockCmdDict.ContainsKey(cmd))
            {
                _lockCmdDict.Remove(cmd);
            }
            if (_lockCmdDict.Count <= 0) return true;
            return false;
        }

        //添加标识
        public void AddPackageCmd(UInt16 cmd)
        {
            if (!_lockCmdDict.ContainsKey(cmd))
            {
                _lockCmdDict[cmd]= true;
            }
        }
    }
    #endregion

    #region 内部类，存储所有关键包和对应协议信息
    class KeyPackageManager
    {
        private Dictionary<UInt16, PackageInfo> packageInfoDic;
        private List<UInt16> packageInfoKeys;


        public KeyPackageManager()
        {
            packageInfoDic = new Dictionary<UInt16, PackageInfo>();
            packageInfoKeys = new List<UInt16>();
        }
        //记录关键包
        public void RecordKeyPackage(UInt16 cmd, PackageInfo packageInfo, bool isCMDInDeleteList)
        {
            if (!packageInfoDic.ContainsKey(cmd))
            {
                packageInfoDic.Add(cmd, packageInfo);
                packageInfoKeys.Add(cmd);
            }
            else
            {
                if (isCMDInDeleteList)
                {
                    Log.info("将协议从删除列表中还原");
                    packageInfoDic[cmd] = packageInfo;
                }
            }
        }

        public void PurgeKeyPackage(UInt16 cmd)
        {
            if (packageInfoDic.ContainsKey(cmd))
            {
                packageInfoDic.Remove(cmd);
                packageInfoKeys.Remove(cmd);
            }
        }

        public bool ContainsKey(UInt16 cmd)
        {
            return packageInfoDic.ContainsKey(cmd);
        }
        //查询超时包，如果超时回调
        public void SeekTimeoutPackage(ulong ticks, PackageTimeoutHandle timeoutHandle, ref List<UInt16> delelist)
        {
            int countOfPKInfo = packageInfoKeys.Count;
            for (int i = 0; i < countOfPKInfo; i++)
            {
                if (ticks >= packageInfoDic[packageInfoKeys[i]].TimeOut)
                {
                    timeoutHandle(packageInfoKeys[i]);
                    if (packageInfoDic.ContainsKey(packageInfoKeys[i]))
                    {
                        delelist.Add(packageInfoKeys[i]);
                    }
                }//end ticks
            }//end i
        }
    }
    #endregion

    public ProtoUtil()
    {
        eventTable = new Dictionary<UInt16, List<EventHandleMsg>>();
        handleTimeOut = new Dictionary<UInt16, List<EventHandleTimeOut>>();
        key2PackageRegistry = new KeyPackageManager();
        _lockManager = new PackageLockMananger();
    }
    #region 对外接口
    //注册,模块可以注册自己关心的协议id,设置回调函数
    public void Regist(CS_CMD_ID CmdType, EventHandleMsg CallBack, EventHandleTimeOut CallBackTimeout = null)
    {
        UInt16 cmd = (UInt16)CmdType;
        if (cmd > 0 && CallBack != null)
        {
            EventHandleMsg handle = CallBack;
            AddFun(cmd, handle);
        }


        if (cmd > 0 && CallBackTimeout != null)
        {
            EventHandleTimeOut handle = CallBackTimeout;
            AddTimeoutFun(cmd, handle);
        }
    }

    public void UnRegist(CS_CMD_ID CmdType, EventHandleMsg Handle, EventHandleTimeOut CallBackTimeout = null)
    {
        UInt16 cmd = (UInt16)CmdType;
        if (eventTable.ContainsKey(cmd))
        {
            List<EventHandleMsg> evtList = eventTable[cmd];
            evtList.Remove(Handle);
        }

        if (handleTimeOut.ContainsKey(cmd))
        {
            List<EventHandleTimeOut> timeOutList = handleTimeOut[cmd];
            if (null != CallBackTimeout)
            {
                if (timeOutList.Contains(CallBackTimeout))
                {
                    timeOutList.Remove(CallBackTimeout);
                }
            }
        }
    }

    public int DispatchEvent(MsgData data)
    {
        int iRet = 0;
        if (eventTable.ContainsKey(data.Cmd))
        {
            List<EventHandleMsg> evtList = eventTable[data.Cmd];
            List<EventHandleMsg> clone = new List<EventHandleMsg>(evtList);
            for (int i = 0; i < clone.Count; i++)
            {
                EventHandleMsg hd = clone[i];
                if (null != hd)
                    hd(data);
            }
        }

        if (key2PackageRegistry.ContainsKey(data.Cmd))
        {
            //如果是关键包，记录后待删除
            RecordReceiveCMD(data.Cmd);
            Pop(data.Cmd);
        }

        //通知停止锁屏
        bool canUnlock = _lockManager.RemovePackageCmd(data.Cmd);
        if (canUnlock)
        {
            EventManager.Send(EventType_Net.EVENT_UNLOCK_SCREEN);
        }
        return iRet;
    }

    /// <summary>
    /// 发送数据包到服务器
    /// </summary>
    /// <param name="stGSPkg"></param>
    /// <param name="tips"></param>
    /// <param name="relatedCMD">对应回包id</param>
    /// <param name="isKeyProtocal"> 表示关键包会锁屏 </param>
    /// <param name="timeoutInterval"> 超时时间 </param>
    /// <param name="showTimeOut"> 不锁屏但是也要监听回包超时 </param>
    /// <returns></returns>
    public int SendAndBack(CMSG stGSPkg, string tips = null, UInt16 relatedCMD = 0, bool isKeyProtocal = false, ushort timeoutInterval = 5, bool showTimeOut = false)
    {
        int iRet = 0;
        int usedPackBufSize = 0;
        //Log.info("wanna send pkg: " + stGSPkg.wCmdId);
        //获取最新的Seq
        stGSPkg.iSeq = MsgData.ISeq;
        //打包Pack
        TdrError.ErrorType ret = stGSPkg.pack(ref buffer, buffer.Length, ref usedPackBufSize, 0);
        if (ret != TdrError.ErrorType.TDR_NO_ERROR)
        {
            Log.info("TDR_ERR:" + ret);
            return (int)ret;
        }

        byte[] tempBuffer = new byte[usedPackBufSize];
        Array.Copy(buffer, 0, tempBuffer, 0, usedPackBufSize);
        //底层tgcpapi发包是异步，防止内存冲突，分配临时空间
        iRet = svrSendCallback(ref tempBuffer, usedPackBufSize, 0, stGSPkg.wCmdId, relatedCMD);
        if (0 == iRet)
        {
            if (pkgSendCallback != null)
            {//发包后回调，重置心跳包计时器
                pkgSendCallback(stGSPkg.wCmdId);
            }

            PackageInfo packinfo = new PackageInfo();
            packinfo.Cmd = stGSPkg.wCmdId;

            TimeSpan ts = DateTime.Now - startTime;
            //计算该配对协议返回的超时时间
            packinfo.TimeOut = (ulong)ts.Ticks + (ulong)timeoutInterval * TimeSpan.TicksPerSecond;
            bool isCMDInDeleteList = (-1 != delelist.IndexOf(stGSPkg.wCmdId));
            if (isCMDInDeleteList && key2PackageRegistry.ContainsKey(stGSPkg.wCmdId))
            {//从超时待删除的协议中移除
                delelist.Remove(stGSPkg.wCmdId);
            }
            //记录关键包
            key2PackageRegistry.RecordKeyPackage(stGSPkg.wCmdId, packinfo, isCMDInDeleteList);
        }
        else
        {
            Log.info("Net.Instance.Send return error: " + iRet);
        }

        //通知发送了关键包，并锁屏
        if (0 == iRet && ((0 != relatedCMD) || isKeyProtocal))
        {
            //标记锁屏cmd
            _lockManager.AddPackageCmd(stGSPkg.wCmdId);

            if (string.IsNullOrEmpty(tips))
            {
                tips = "正在发送请求";
            }
            Log.info("发送关键包,锁屏");
            EventManager.Send(EventType_Net.EVENT_LOCK_SCREEN, tips);
        }
        Log.info("Send Pkg " + stGSPkg.wCmdId + " ret + " + iRet);
        return iRet;
    }

    //每个周期调用
    public void Tick()
    {
        delelist.Clear();

        TimeSpan ts = DateTime.Now - startTime;
        //查询超时包
        //1.如果超时回调
        //2.加入超时待删除delelist
        key2PackageRegistry.SeekTimeoutPackage((ulong)ts.Ticks, CallHandleTimeout,ref delelist);

        if (delelist.Count <= 0) return;
        
        int _countOfDelete = delelist.Count;
        for (int i = 0; i < _countOfDelete; i++)
        {
            //移出关键包对应的协议信息
            Pop(delelist[i]);
            //解除锁屏
            bool canUnlock = _lockManager.RemovePackageCmd(delelist[i]);
            if (canUnlock)
            {
                EventManager.Send(EventType_Net.EVENT_UNLOCK_SCREEN);
            }
        } 
    }


    //清除要标记为要清理的协议
    public void Clear()
    {
        if (0 < delelist.Count)
        {
            int countOfDelete = delelist.Count;
            for (int i = 0; i < countOfDelete; i++)
            {
                Pop(delelist[i]);
            }
        }
    }
    #endregion


    #region 内部逻辑
    private void AddFun(UInt16 cmd, EventHandleMsg delgate)
    {
        EventHandleMsg Handle = delgate;
        List<EventHandleMsg> evtList = null;
        if (eventTable.ContainsKey(cmd))
        {
            evtList = eventTable[cmd];
            if (!evtList.Contains(Handle))
                evtList.Add(Handle);
        }
        else
        {
            evtList = new List<EventHandleMsg>();
            evtList.Add(Handle);
            eventTable[cmd] = evtList;
        }
    }

    private void AddTimeoutFun(UInt16 cmd, EventHandleTimeOut delgate)
    {
        EventHandleTimeOut Handle = delgate;
        List<EventHandleTimeOut> timeOutList = null;
        if (handleTimeOut.ContainsKey(cmd))
        {
            timeOutList = handleTimeOut[cmd];

            if (!timeOutList.Contains(Handle))
            {
                timeOutList.Add(Handle);
            }
        }
        else
        {
            timeOutList = new List<EventHandleTimeOut>();
            timeOutList.Add(Handle);
            handleTimeOut.Add(cmd, timeOutList);
        }
    }

    //移出关键包对应的协议信息
    private void Pop(UInt16 cmd)
    {
        key2PackageRegistry.PurgeKeyPackage(cmd);
    }

    //超时处理
    private void CallHandleTimeout(UInt16 cmd)
    {
        delelist.Add(cmd);
        if (handleTimeOut.ContainsKey(cmd))
        {
            List<EventHandleTimeOut> evtList = handleTimeOut[cmd];
            if (null != evtList && 0 != evtList.Count)
            {
                for (int i = evtList.Count - 1; i >= 0; i--)
                {
                    EventHandleTimeOut handle = evtList[i];
                    if (null != handle)
                    {
                        handle();
                    }
                }
            }
        }
    }

    //记录已经收到的包
    private void RecordReceiveCMD(UInt16 cmd)
    {
        delelist.Add(cmd);
    }

    #endregion


}
