//------------------------------------------------------------------------------
// piaochen
// 2014.12.16
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections;

public class BaseServerFactory
{
	
	//是否初始化
	protected bool m_isInited = false;



    //模型引用
    private BaseModelFactory m_model;
	//模块字典
	private Dictionary<Type,AbstractServer> m_modelList;
	
	
	
	
	public BaseServerFactory ()
	{
        m_modelList = new Dictionary<Type, AbstractServer>();
	}

    virtual public void Dispose()
    {
        m_model = null;
        DestoryAllObj();
    }

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="model">Model.</param>
    public virtual void Init(BaseModelFactory model)
	{
		m_model = model;
		m_isInited = true;
	}
	
	/// <summary>
	/// 销毁对象
	/// </summary>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public void DestoryObj<T>()
	{
		m_modelList [typeof(T)].Dispose ();
        m_modelList[typeof(T)] = null;
		m_modelList.Remove (typeof(T));
	}
	
	/// <summary>
	/// 销毁所有对象
	/// </summary>
	public void DestoryAllObj()
	{
		foreach (Type t in m_modelList.Keys) {
			m_modelList [t].Dispose ();
            m_modelList[t] = null;
		}
        m_modelList.Clear();
	}
	
	/// <summary>
	/// 创建模块
	/// </summary>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
    protected T Creator<T>() where T : AbstractServer
	{
		if (!m_isInited) {
			throw new Exception("ServerFactory 还未初始化！");		
		}

        T obj = null;
        if (!m_modelList.ContainsKey(typeof(T)))
        {
            obj = System.Activator.CreateInstance(typeof(T)) as T;
            m_modelList[typeof(T)] = obj;
            (obj as IModuleCenter).Setup(m_model, null, this);
        }
        else
        {
            obj = m_modelList[typeof(T)] as T;
        }
        return obj;
	}
}