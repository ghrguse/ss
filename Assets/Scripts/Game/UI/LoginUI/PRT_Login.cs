using UnityEngine;
using System.Collections;

public class PRT_Login : BaseStagePart
{
    private ServerSelectCtrl serverSelectCtrl;
    private LoginCtrl loginCtrl;
    public LoginUI loginUI;

    public override void AddEventListeners()
    {
        this.AddViewEventListener(EventType_Global.SERVERlIST_LOAD_SUCCESS, loadServerListComplete);
    }
    public override void RemoveEventListeners()
    {
        this.RemoveViewEventListener(EventType_Global.SERVERlIST_LOAD_SUCCESS, loadServerListComplete);
    }

    public override void Dispose()
    {
        base.Dispose();
        serverSelectCtrl = null;

    }

    public override void Init()
    {
        if (m_isInited)
            return;
        m_isInited = true;

        loginCtrl = this.GetController<LoginCtrl>();
        serverSelectCtrl = this.GetController<ServerSelectCtrl>();

        loginUI.OnLoginDelegate = EnterGame;

        serverSelectCtrl.LoadServerList();
    }

    private void loadServerListComplete(GEvent e = null)
    {
        VOServerData serverData = serverSelectCtrl.getDefaultServer();
        updateServer(serverData);
    }
    
    private void updateServer(GEvent e)
    {
        VOServerData serverData = e.data as VOServerData;
        updateServer(serverData);
    }

    private void updateServer(VOServerData serverData)
    {
        GlobalData.system.serverData = serverData;
        loginUI.SetServerInfo(serverData);
    }

    private void EnterGame(object data)
    {
        VOServerData server = (VOServerData)data;
        loginCtrl.EnterGame(server);
    }
}
