using UnityEngine;
using System.Collections;

public class GameSysInfo
{
    //ip地址
    public string ipAddress = "0.0.0.0";
	//连接的服务器数据
	public VOServerData serverData;
    
    //用户OpenID
    private string selfOpenID = "";
    private bool _isGuestPlatform = true;
    
    public string SelfOpenID
    {
        get
        {
#if UNITY_EDITOR
            return UnityEngine.SystemInfo.deviceUniqueIdentifier;
#else
            return selfOpenID;
#endif
        }
    }

    public bool isGuestPlatfrom()
    {
        return _isGuestPlatform;
    }

}
