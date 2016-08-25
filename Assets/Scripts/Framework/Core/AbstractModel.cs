using UnityEngine;
using System.Collections;
using Utils.Event;
public class AbstractModel : IModel
{
    protected bool m_isDispose = false;

    private BaseDataFactory m_datas = null;
    private BaseServerFactory m_servers = null;
    private BaseModelFactory m_models = null;

    public virtual void Dispose()
    {
        if (m_isDispose) return;
        m_isDispose = true;

        this.RemoveEventListeners();

        m_datas = null;
        m_servers = null;
        m_models = null;
    }

    public void Setup(BaseModelFactory model, BaseDataFactory data, BaseServerFactory server)
    {
        m_datas = data as BaseDataFactory;
        m_servers = server as BaseServerFactory;
        m_models = model as BaseModelFactory;
        Init();
    }

    virtual protected void Init()
    {
 
    }

    virtual public void AddEventListeners()
    {
 
    }

    virtual public void RemoveEventListeners()
    { 

    }

    public T GetModels<T>() where T : BaseModelFactory
    {
        return m_models as T;
    }

    public T GetDatas<T>() where T : BaseDataFactory
    {
        return m_datas as T;
    }

    public T GetServers<T>() where T : BaseServerFactory
    {
        return m_servers as T;
    }
}
