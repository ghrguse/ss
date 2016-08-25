using UnityEngine;
using System.Collections;
using GSProtocal;
using System;

/// <summary>
/// 用于判断游戏处在哪个状态
/// </summary>
public enum GameState
{
    DisLogin,
    EnterGame,
}


/// <summary>
/// Server模块。
/// 连接服务器；发包、收包；断线重连等
/// </summary>
public class ServerCenter : BaseServerFactory
{
    public NetworkConnect.OnPkgError OnPkgError;

    private bool _netComponentInitted = false;
    //网络状态监测
    private NetworkMonitor _networkMonitor = null;
    private GameState _gameState = GameState.DisLogin;

    //GameSvr
    private ProtoUtil _gameSvrProtoUtil = null;
    private NetworkConnect _gameSvrConnect = null;


    public override void Init(BaseModelFactory model)
    {
        base.Init(model);
        this.InitNetComponent();
    }

    public void Dispose()
    {
        if (_gameSvrProtoUtil != null) _gameSvrProtoUtil.Clear();

        if (_gameSvrConnect != null)
        {
            _gameSvrConnect.UnRegisterHandle(this.SendHeartBeadPkg, this.ChkSendPkg, this.ChkRecvPkg, this.PkgError);
        }

        _gameSvrConnect = null;
        _gameSvrProtoUtil = null;
        _networkMonitor = null;
        base.Dispose();
    }

    //初始化网络组件
    public void InitNetComponent()
    {
        if (_netComponentInitted)
        {
            return;
        }

        Log.info(">> 初始化网络组件!");
        _netComponentInitted = true;


        //------------- 定义GameSvr连接 -----------
        //协议
        _gameSvrProtoUtil = new ProtoUtil();
        //连接
        _gameSvrConnect = NetworkConnect.Create("GameSvrNet", Core.SystemPluginRoot, _gameSvrProtoUtil);
        _gameSvrConnect.RegisterHandle(this.SendHeartBeadPkg, this.ChkSendPkg, this.ChkRecvPkg, this.PkgError);

        //TODO 后续可以定义 BattleSvr 连接,并添加到管理列表

        //网络状态监控
        _networkMonitor = NetworkMonitor.Create(Core.SystemPluginRoot);
        //降定义的svr连接加入到网络状态监控器中
        _networkMonitor.AddServerNet(_gameSvrConnect);
    }

    //连接服务器
    public void ConnectToGameSvr()
    {
        _gameSvrConnect.Connect(GlobalData.system.serverData.url, GlobalData.system.serverData.port);
    }

    //重连
    public void ReConnectToGameSvr()
    {
        _gameSvrConnect.ReConnect(15000, true);
    }

    //断开连接
    public void DisConnectToGameSvr()
    {
        _gameSvrConnect.DisConnect();
    }

    //设置游戏状态
    //注意，角色登陆成功后需要把这个状态设置成  GameState.HeroEnter,否则后续包无法发送出去
    public GameState gameState
    {
        get{return _gameState;}
        set { _gameState = value; }
    }

    //注册,模块可以注册自己关心的协议id,设置回调函数
    public void Regist(CS_CMD_ID CmdType, EventHandleMsg CallBack, EventHandleTimeOut CallBackTimeout = null)
    {
        _gameSvrProtoUtil.Regist(CmdType, CallBack, CallBackTimeout);
    }

    public void UnRegist(CS_CMD_ID CmdType, EventHandleMsg Handle, EventHandleTimeOut CallBackTimeout = null)
    {
        _gameSvrProtoUtil.UnRegist(CmdType, Handle, CallBackTimeout);
    }

    /// <summary>
    /// 发送数据包到服务器
    /// </summary>
    /// <returns>The and back.</returns>
    /// <param name="stGSPkg"> 发送包 </param>
    /// <param name="tips"> 提示信息 </param>
    /// <param name="relatedCMD">对应回包id</param>
    /// <param name="isKeyProtocal"> 表示关键包会锁屏 </param>
    /// <param name="timeoutInterval"> 超时时间 </param>
    /// <param name="showTimeOut"> 不锁屏但是也要监听回包超时 </param>
    public int SendAndBack(CMSG stGSPkg, string tips = null, ushort relatedCMD = 0, bool isKeyProtocal = false, ushort timeoutInterval = 5, bool showTimeOut = false)
    {
        return _gameSvrProtoUtil.SendAndBack(stGSPkg, tips, relatedCMD, isKeyProtocal, timeoutInterval, showTimeOut);
    }

    /// <summary>
    /// 发心跳包
    /// </summary>
    private void SendHeartBeadPkg()
    {
        CMSG stGSPkg = new CMSG();
        stGSPkg.wCmdId = (UInt16)CS_CMD_ID.CS_SYN_TIME;
        stGSPkg.stBody.stCsSynTime = new CSSynTime();
        stGSPkg.stBody.stCsSynTime.iReq = 0;
        this.SendAndBack(stGSPkg);
    }

    /// <summary>
    /// 判断是否可接收的包
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private bool ChkRecvPkg(ushort cmd)
    {
        return true;
        //         bool enable = false;
        //         //true 如果在主城 or 副本场景中
        //         enable = _gameState >= GameState.STATE_SCENE;
        //         if (enable) return true;
        // 
        //         //true 如果协议号大于 CS_CMD_TYPE.CS_CMD_GET_GID
        //         CS_CMD_TYPE cmdType = (CS_CMD_TYPE)cmd;
        //         if ((int)CS_CMD_TYPE.CS_CMD_GET_GID >= (int)cmdType) return true;
        // 
        //         //如果在角色选择界面
        //         if (_gameState == GameState.STATE_CHARACTER)
        //         {
        //             switch (cmdType)
        //             {
        //                 case CS_CMD_TYPE.CS_CMD_GET_GID:
        //                 case CS_CMD_TYPE.CS_CMD_GET_ROLE:
        //                 case CS_CMD_TYPE.CS_CMD_REQSELFPROFILE:
        //                 case CS_CMD_TYPE.CS_CMD_GET_HERO_LIST:
        //                 case CS_CMD_TYPE.CS_CMD_CHANGE_HERO:
        //                     enable = true;
        //                     break;
        //                 default:
        //                     enable = false;
        //                     break;
        //             }
        //         }
        // 
        //         return enable;
    }

    /// <summary>
    /// 检测是否可发包
    /// 注意:需要区分角色登陆成功。未登陆成功前，部分包是不能发送!!
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private bool ChkSendPkg(ushort cmd)
    {
        return true;

//         bool enable = false;
//         enable = GameState.HeroEnter <= _gameState;
//         if (enable)
//         {
//             return true;
//         }
// 
//         PkgType cmdType = (PkgType)cmd;
//         if (16 > (int)cmdType)
//         {
//             return true;
//         }
//         switch (cmdType)
//         {//以下协议包可直接发送
//             case PkgType.CS_SYN_TIME://时间同步
//             case PkgType.CS_QUERY_RELOAD_TIME:
//             case PkgType.CS_ENTER_GAME://进入游戏      
//                 enable = true;
//                 break;
//             default:
//                 break;
//         }
//         if (!enable)
//         {
//            Log.info("主角未登录前协议包不可发送" + cmd.ToString());
//         }
//         return enable;
    }

    private void PkgError(UInt16 cmd, int errorCode)
    {
        if (this.OnPkgError != null)
            this.OnPkgError(cmd, errorCode);
    }

    //     //每次连接都使用新ip，保证成功率    
    //     private string GetParseIP()
    //     {
    //         string domain = GlobalData.system.serverData.url;
    //         string ipAddress = "";
    //         if (null == ipQueue)
    //         {
    //             //首次连接
    //             ipQueue = new HttpDNSIPAddressQueue();
    //             ipQueue.ParseIPByDNS(domain);
    //         }
    //         else if (!ipQueue.domainAddress.Equals(domain))
    //         {
    //             //更换服务器
    //             ipQueue.ParseIPByDNS(domain);
    //         }
    //         //更换ip
    //         ipAddress = ipQueue.GetCurrentAddress();
    //         if (string.IsNullOrEmpty(ipAddress)) return "";
    // 
    //         //保存当前ip
    //         Log.info("domain: " + domain + "解析成功: " + ipAddress, "HttpDNS");
    //         return ipAddress;
    //     }
}
