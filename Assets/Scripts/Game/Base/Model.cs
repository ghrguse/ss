using UnityEngine;
using System.Collections;

public class Model : AbstractModel {

    protected ModelFactory m_models;
    protected DataFactory m_datas;
    protected ServerFactory m_servers;

    override protected void Init()
    {
        m_models = this.GetModels<ModelFactory>();
        m_datas = this.GetDatas<DataFactory>();
        m_servers = this.GetServers<ServerFactory>();
    }

    override public void Dispose()
    {
        m_models = null;
        m_datas = null;
        m_servers = null;
        base.Dispose();
    }
}
