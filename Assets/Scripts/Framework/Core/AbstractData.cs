using UnityEngine;
using System.Collections;

public class AbstractData : IData
{
    protected bool m_isDispose = false;

    private BaseDataFactory m_datas = null;

    public virtual void Dispose()
    {
        if (m_isDispose) return;
        m_isDispose = true;

        m_datas = null;
    }

    public void Setup(BaseModelFactory model, BaseDataFactory data, BaseServerFactory server)
    {
        m_datas = data as BaseDataFactory;
        Init();
    }

    public T GetDatas<T>() where T : BaseDataFactory
    {
        return m_datas as T;
    }

    virtual protected void Init()
    { 
    }

    virtual public void Clear()
    {
 
    }
}