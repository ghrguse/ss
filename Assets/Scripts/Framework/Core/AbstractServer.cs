using UnityEngine;
using System.Collections;

public class AbstractServer : IServer
{
    protected bool m_isDispose = false;

    private BaseModelFactory m_models = null;
    private BaseServerFactory m_servers = null;

    public virtual void Dispose()
    {
        if (m_isDispose) return;
        m_isDispose = true;

        this.UnRegProtocols();
        m_models = null;
        m_servers = null;
    }

    public void Setup(BaseModelFactory model, BaseDataFactory data, BaseServerFactory server)
    {
        m_models = model as BaseModelFactory;
        m_servers = server;
        Init();
    }

    public T GetModels<T>() where T : BaseModelFactory
    {
        return m_models as T;
    }

    public T GetServers<T>() where T : BaseServerFactory
    { 
        return m_servers as T;
    }

    /// <summary>
    /// 注册协议
    /// </summary>
    public virtual void RegProtocols()
    { 
    }

    /// <summary>
    /// 反注册
    /// </summary>
    public virtual void UnRegProtocols()
    {
 
    }

    protected virtual void Init()
    {

    }
}