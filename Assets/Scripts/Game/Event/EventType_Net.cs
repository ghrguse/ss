using UnityEngine;
using System.Collections;

public class EventType_Net
{
    //网络通用报错消息(带一个NetCommonError的数据)
    public static string NET_GLOBAL_ERROR = "NET_GLOBAL_ERROR";

    //发送协议  
    public static string Send_Proto_Suc = "Send_Proto_Suc";


    //网络连接失败  
    public static string Net_Conned_Error = "Net_Conned_Error";
    //网络掉线  
    public static string Net_DisConned = "Net_DisConned";


    //更新ping值
    public static string UPDATE_GAME_PING = "UPDATE_GAME_PING";
    //关键包发出
    public static string ON_NET_KEY_PKG_SEND = "ON_NET_KEY_PKG_SEND";
    //通知锁屏
    public static string EVENT_LOCK_SCREEN = "EVENT_LOCK_SCREEN";
    //通知取消锁屏
    public static string EVENT_UNLOCK_SCREEN = "EVENT_UNLOCK_SCREEN";
    //关键包收到反馈
    public static string ON_NET_KEY_PKG_FEEDBACK = "ON_NET_KEY_PKG_FEEDBACK";
    //验证token有效性
    public static string EVENT_CHECK_PAYTOKEN = "EVENT_CHECK_PAYTOKEN";



    //连接服务超时，修改服务器选择界面显示
    public static string TGCP_ConnectTimeout = "TGCP_ConnectTimeout";

    //-------------------------以下socket连接相关--------------------------------
    //socket连接断开
    public static string NET_DISCONNECTED = "NET_DISCONNECTED";
    //socket连接上
    public static string NET_CONNECTED = "NET_CONNECTED";
    //服务器返回鉴权失效
    public static string NET_AUTH_FAILED = "NET_AUTH_FAILED";
    //因用户不被允许登录此服务器(错误的服务器)或服务器日注册名额已用完，要主动断开重连机制并返回登录界面或在服务器选择界面不停止连接
    public static string NET_NOT_ALLOW_TO_CONNECT_SERVER = "NET_NOT_ALLOW_TO_CONNECT_SERVER";
    //服务器主动断开连接（TODO 服务器选择界面要有相应的处理）
    public static string NET_SERVER_DISCONNECTED = "NET_SERVER_DISCONNECTED";

    //网络开始重新连接
    public static string NET_RECONNECT_START = "NET_RECONNECT_START";
    //网络重新连接成功通知UI
    public static string RECONNECT_SUCCESS_NOTICE_UI = "RECONNECT_SUCCESS_NOTICE_UI";
    //战斗场景中要在战斗结果界面时发出的事件之后手动连接
    public static string NET_NEED_RECONNECT_AFTER_BATTLE_END = "NET_NEED_RECONNECT_AFTER_BATTLE_END";


    //-------------------------以下网络连接状态相关------------------------------
    //网络连接上
    public static string NETWORK_STATE_CONNECTED = "NETWORK_STATE_CONNECTED";
    //网络连接失败
    public static string NETWORK_STATE_DISCONNECTED = "NETWORK_STATE_DISCONNECTED";


    //-------------------------以下tss组件相关--------------------------------
    //为tss返回连接成功并返回msdk鉴权信息
    public static string MSDK_AUTH_SUCCESS_TSS = "MSDK_AUTH_SUCCESS_TSS";
    //通知连接失败通知tss关闭
    public static string MSDK_AUTH_FAILED_TSS = "MSDK_AUTH_FAILED_TSS";
    //通知tss组件初始化
    public static string CHARACTER_ENTER_NOTICE_TSS = "CHARACTER_ENTER_NOTICE_TSS";


    //-------------------------以下Net相关UI操作--------------------------------
    //网络状态变更、关键包收发超时tip变更
    public static string NET_STATE_CHANGE_TIP = "NET_STATE_CHANGE_TIP";
    //当前无网络
    public static string NET_STATE_TIP_NO_NETWORK = "NET_STATE_TIP_NO_NETWORK";
}
