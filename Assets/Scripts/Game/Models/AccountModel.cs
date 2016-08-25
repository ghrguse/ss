using UnityEngine;
using System.Collections;
using Utils.Event;
using GSProtocal;

public class AccountModel : Model
{
    public override void AddEventListeners()
    {
        EventManager.AddEventListener(EventType_Net.NET_CONNECTED, OnGamesvrConnected);
    }

    public override void RemoveEventListeners()
    {
        EventManager.RemoveEventListener(EventType_Net.NET_CONNECTED, OnGamesvrConnected);
    }

    public void EnterGame(VOServerData server)
    {
        GlobalData.system.serverData = server;
        m_servers.ConnectToGameSvr();
    }

    private void OnGamesvrConnected(GEvent evt)
    {
        m_servers.Account.SGetRole(GlobalData.system.serverData.zone);
    }

    public void UpdateRoleInfo(SCRoleInfo roleInfo)
    {
        m_datas.Account._roleInfo = roleInfo;

        if (roleInfo.ullRoleId <= 0)
        {
            m_servers.Account.SCreateRole("Leo", 1);
        }
        else
        {
            SceneManager.instance.goToScene(ModuleName.SCENE_MAIN, SceneTransEffect.Fade);
        }
    }

    public void CreateRoleRet(int ret, ulong ullRoleId)
    {
        switch (ret)
        {
            case 0:
                Log.show("创建角色成功，iD为 " + ullRoleId);
                //EventManager.Send(EventType_Global.CREATE_ROLE_SUCCESS, ullRoleId);
                return;
            default:
                break;
        }
    }
}
