using UnityEngine;
using System.Collections;
using System;


public enum NetworkState
{
    CONNECTED,
    CONNECTING,
    DISCONNECTED,
    DISCONNECTING,
    SUSPENDED,
}

public class NetworkInfo
{

    private bool _isAvailable;
    private bool _isConnected;
    private int _subType;
    private int _type;
    private bool _isFailover;
    private bool _isRoaming;
    private string _extraInfo;
    private string _reason;
    private NetworkState _state;
    private string _subTypeName;
    private string _typeName;

    public NetworkInfo(bool isavailabe, bool isconnected, int subtype, int type_, bool isfailover, bool isroaming,
                        string extrainfo, string reason, int netstate, string subtypename, string typename)
    {
        _isAvailable = isavailabe;
        _isConnected = isconnected;
        _subType = subtype;
        _type = type_;
        _isFailover = isfailover;
        _isRoaming = isroaming;
        _extraInfo = extrainfo;
        _reason = reason;
        _state = (NetworkState)netstate;
        _subTypeName = subtypename;
        _typeName = typename;
    }

    public override string ToString()
    {
        return "Type" + _typeName + ",Avaliable:" + _isAvailable + ",\nConnect:" + _isConnected + " State:" + _state;
    }

    public NetworkState State
    {
        get
        {
            return _state;
        }
    }

}




