using UnityEngine;
using System.Collections;

public class Server : AbstractServer {

    protected ModelFactory m_models = null;
    protected ServerCenter m_serverCeneter= null;

    protected override void Init()
    {
        m_models = this.GetModels<ModelFactory>();
        m_serverCeneter = this.GetServers<ServerCenter>();
        base.Init();
    }

    override public void Dispose()
    {
        //TODO
        m_models = null;
        m_serverCeneter = null;
        base.Dispose();
    }


}
