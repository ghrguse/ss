using UnityEngine;
using System.Collections;
using System.Timers;
using Utils.Event;


/// <summary>
/// 网络超时检测器
/// </summary>
public class NetworkTimeoutMonitor
{
    private int Timeout = 7000;
    private int MaxWaitPkg = 5;
    private int sendPkgCount = 0;
    private Timer timer = null;
    private NetworkConnect _netConnect;
    private DelegateEnums.NoneParam _onHeartBeatHandler = null;

    public void RegisterNetConnector(NetworkConnect netConnector, DelegateEnums.NoneParam heartBeatHandler)
    {
        this._onHeartBeatHandler += heartBeatHandler;
        this._netConnect = netConnector;
    }

    public void UnRegisterNetConnector(DelegateEnums.NoneParam heartBeatHandler)
    {
        this._onHeartBeatHandler -= heartBeatHandler;
        this._netConnect = null;
    }

    public void SendPkg(bool isHeartbeat = false)
    {
        sendPkgCount++;
        if (isHeartbeat || MaxWaitPkg <= sendPkgCount)
        {
            StartCount(isHeartbeat);
        }
    }

    public void ReceivePkg()
    {
        if (0 != sendPkgCount)
        {
            Reset();
        }
    }


    private void Reset()
    {
        sendPkgCount = 0;
        StopCount();
    }

    private void StartCount(bool isHeartbeat = false)
    {
        //editor模式下注释超时重连
#if UNITY_EDITOR
        return;
#endif
        if (null == timer)
        {
            timer = new Timer(Timeout);
            timer.Elapsed += OnTimerEclapsed;
            timer.Enabled = false;
        }
        if (!timer.Enabled)
        {
            timer.Enabled = true;
            if (!isHeartbeat)
            {
                if (_onHeartBeatHandler != null)
                {
                    _onHeartBeatHandler();
                }
            }
        }
    }

    private void StopCount()
    {
        if (null == timer)
        {
            return;
        }
        timer.Enabled = false;
    }

    private void OnTimerEclapsed(object sender, ElapsedEventArgs e)
    {
        StopCount();
        if (0 < sendPkgCount)
        {
            sendPkgCount = 0;
            //Log.info(Timeout + "毫秒内未收到任何包");
            this._netConnect.OnReceivePkgFailed();
        }
    }
}
