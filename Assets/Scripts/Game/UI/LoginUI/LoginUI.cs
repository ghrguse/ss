using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour {

    public DelegateEnums.DataParam OnLoginDelegate = null;
    public Text txServerName;
    private VOServerData _serverInfo;

    public void OnBtnLogin()
    {
        if (OnLoginDelegate != null)
            OnLoginDelegate(_serverInfo);
    }

    public void SetServerInfo(VOServerData serverInfo)
    {
        _serverInfo = serverInfo;
        txServerName.text = _serverInfo.tag;
    }
}
