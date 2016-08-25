using UnityEngine;
using System.Collections;

public class ServerSelectCtrl : Controller {

    public void LoadServerList()
    {
        m_models.ServerSelect.LoadServerList();
    }

    public VOServerData getDefaultServer()
    {
        return m_models.ServerSelect.getDefaultServer();
    }
}
