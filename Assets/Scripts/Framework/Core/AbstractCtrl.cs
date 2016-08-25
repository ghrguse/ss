using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class AbstractCtrl : ICtrl
{
    protected bool m_isDispose = false;

    private BaseModelFactory m_models = null;

    public virtual void Dispose()
    {
        if (m_isDispose) return;
        m_isDispose = true;

        m_models = null;
    }

    public void Setup(BaseModelFactory model, BaseDataFactory data, BaseServerFactory server)
    {
        m_models = model;
        Init();
    }

    public T GetModels<T>() where T : BaseModelFactory
    {
        return m_models as T;
    }

    protected virtual void Init()
    {
 
    }
}

