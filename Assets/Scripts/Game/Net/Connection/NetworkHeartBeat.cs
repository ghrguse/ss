using UnityEngine;
using System.Collections;
using Utils.Event;
using System;


/// <summary>
/// 网络心跳计时器
/// </summary>
public class NetworkHeartBeat
{


    private static float ForceHeartbeatTimeInterval = 120f;//2分钟强制同步一次
    public static float HeartbeatTimeInterval = 5;//10秒钟执行一次

    private static float _lastTime = 0f;
    private static float _internal = 0f;

    private NetworkConnect _netConnector;
    private ProtoUtil _protoUtil;

    private DelegateEnums.NoneParam _heartBeatHandler = null;
    /// <summary>
    /// 是否启动
    /// </summary>
    private bool isStart = false;


    private void Reset()
    {
        _lastTime = 0;
        _internal = 0;
    }

    /// <summary>
    /// 注册引用关系
    /// </summary>
    /// <param name="m_netConnector"></param>
    /// <param name="m_protoUtil"></param>
    /// <param name="heartBeatHandler"></param>
    public void RegisterNetConnector(NetworkConnect m_netConnector, ProtoUtil m_protoUtil, DelegateEnums.NoneParam heartBeatHandler)
    {
        _netConnector = m_netConnector;
        _protoUtil = m_protoUtil;
        _heartBeatHandler += heartBeatHandler;

        Reset();
    }

    /// <summary>
    /// 反注册
    /// </summary>
    /// <param name="heartBeatHandler"></param>
    public void UnRegisterNetConnect(DelegateEnums.NoneParam heartBeatHandler)
    {
        _netConnector = null;
        _protoUtil = null;
        _heartBeatHandler -= heartBeatHandler;
    }


    //连接后启动心跳,NetConnector连接上后调用
    public void OnNetConnectedToStartHeartBeat()
    {
        Reset();
        isStart = true;
    }

    //网络断开重置，心跳，避免干扰错误提示
    public void OnDisConnected()
    {
        isStart = false;
    }

    public void Render()
    {
        if (!isStart) return;

        _lastTime += Time.deltaTime;
        _internal += Time.deltaTime;

        //每秒钟渲染一次
        if (_internal >= HeartbeatTimeInterval)
        {
            _internal = 0;
            SendHeartbeat();
        }

        //2分钟强制执行一次
        if (_lastTime >= ForceHeartbeatTimeInterval)
        {
            _lastTime = 0;
            SendHeartbeat();
        }
    }

    private void SendHeartbeat()
    {
        // 发送同步心跳包
        if (null != _heartBeatHandler)
            _heartBeatHandler();
    }
}
