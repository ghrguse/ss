using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Utils.Event;


/// <summary>
/// 检测网络。wifi / 移动网络
/// </summary>
public class NetworkMonitor : MonoBehaviour
{
    //网络是否连接上
    public static bool Network_Connected = false;

    private static NetworkMonitor g_monitor;

    private DateTime pauseTime;
    private Ping netPing = null;
    private bool getPing = false;
    private int pingTime = 0;
    private string autoPingHandle = "AutoGetPing";
    private string serverIP = "";
    private List<NetworkConnect> _svrList;

#if UNITY_ANDROID
    private static AndroidJavaObject _androidProxy;
    private static AndroidJavaClass _networkAndroidProxy;
    private static AndroidJavaClass networkAndroidProxy
    {
        get
        {
            if (_networkAndroidProxy == null)
            {//TODO:::所有unity项目都会有各自的入口activity.申请一个改下。
                _networkAndroidProxy = new AndroidJavaClass("com.tencent.tmgp.lucifer.UnityPlayerNativeActivity");
            }

            return _networkAndroidProxy;
        }
    }
#endif

    public static NetworkMonitor Create(Transform root)
    {
        if (g_monitor == null)
        {
            GameObject go = new GameObject("NetworkMonitor");
            go.transform.parent = root;
            g_monitor = go.AddComponent<NetworkMonitor>();
            g_monitor.Init();
        }
        return g_monitor;
    }
    
    void Awake()
    {
        Log.info("Monitor start");
        if (g_monitor == null)
        {
            g_monitor = this;
        }

        DontDestroyOnLoad(gameObject);

        NetworkInfo info = activeNetworkInfo;
        onConnectivityChange(info);
        if (info != null)
        {
            Log.info(info.ToString());
        }
        else
        {
            Log.info("No net!");
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            Log.info("手机唤醒，尝试获得网络状态", "NetworkMonitor");
#if UNITY_ANDROID
            NetworkInfo info = activeNetworkInfo;
            onConnectivityChange(info);
#endif
            TimeSpan span = DateTime.Now - pauseTime;
            bool checkHeartbeat = 60 <= span.TotalSeconds;

            for (int i = 0; i < _svrList.Count; i++)
            {
                if (_svrList[i] == null) continue;
                _svrList[i].CheckNetworkState(true, checkHeartbeat);
            }
        }
        else
        {
            pauseTime = DateTime.Now;
        }
    }

    void Update()
    {
        if (getPing)
        {
            if (null != netPing && netPing.isDone)
            {
                getPing = false;
                //Log.info("--------------------------------------netPing.time: " + netPing.time);
                pingTime = netPing.time >= 0 ? netPing.time : int.MaxValue;
                netPing.DestroyPing();
                netPing = null;               
                //Log.info("--------------------------------------ping: " + pingTime);
                EventManager.Send(EventType_Net.UPDATE_GAME_PING, pingTime);
            }
        }
    }

    public void Init()
    {
        _svrList = new List<NetworkConnect>();
    }

    public void UnLoad()
    {
        _svrList.Clear();
    }

	public int getPingTime ()
	{
		return pingTime;
	}

    public static NetworkInfo activeNetworkInfo
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
				string jsonStr = Util.AStringToCString(networkAndroidProxy.CallStatic<AndroidJavaObject>("GetActiveNetworkInfo"));
			return NetworkInfo.createFromJson(jsonStr);
#else
            return null;
#endif
        }
    }

    public static long getAvaliableMemSize()
    {
#if UNITY_ANDROID
        if (_androidProxy == null)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _androidProxy = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }

        return _androidProxy.Call<long>("GetAvailableMemSize");
#else
			return 0;
#endif
    }

    public void AddServerNet(NetworkConnect svr)
    {
        _svrList.Add(svr);
    }
    
    void onConnectivityChange(NetworkInfo info)
    {
        if (info == null)
        {
            Log.info("网络切换->当前无网络！");
            EventManager.Send(EventType_Net.NET_STATE_TIP_NO_NETWORK);
        }
        else
        {
            //ConsoleLabel.PrintLine("NetInfo: " + info.ToString());
            Log.info("网络切换->" + info.ToString());
            if (NetworkState.CONNECTED == info.State)
            {
                Network_Connected = true;
                EventManager.Send(EventType_Net.NETWORK_STATE_CONNECTED);
            }
            else
            {
                Network_Connected = false;
                EventManager.Send(EventType_Net.NETWORK_STATE_DISCONNECTED);
                Log.info("网络连接断开，验证tgcp连接状态", "NetworkMonitor");

                for (int i = 0; i < _svrList.Count; i++)
                {
                    if (_svrList[i] == null) continue;
                    _svrList[i].CheckNetworkState(false);
                }
            }
        }
    }


    public void StartPing()
    {
        Log.info("StartPing");
        serverIP = GlobalData.system.ipAddress;
        if (IsInvoking())
        {
            CancelInvoke(autoPingHandle);
        }
        InvokeRepeating(autoPingHandle, 0f, 5f);
    }

    public void StopPing()
    {
        Log.info("StopPing");
        if (IsInvoking())
        {
            CancelInvoke(autoPingHandle);
        }
        if (null != netPing)
        {
            netPing.DestroyPing();
            netPing = null;
        }
        pingTime = 0;
        getPing = false;
    }

    void AutoGetPing()
    {        
        if (null != netPing)
        {
            //pingTime = int.MaxValue;
            getPing = false;
            netPing.DestroyPing();
            netPing = null;
        }
        if (string.IsNullOrEmpty(serverIP))
        {
            pingTime = int.MaxValue;
            getPing = false;
            Log.info("serverIP is null");
            EventManager.Send(EventType_Net.UPDATE_GAME_PING, pingTime);
            return;
        }
        Log.info("create ping object to " + serverIP);
        netPing = new Ping(serverIP);
        getPing = true;        
    }    
}


