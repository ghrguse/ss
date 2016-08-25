using System;
using System.Collections.Generic;
using System.Collections;

public class BaseModelFactory
{
	
	//是否初始化
	protected bool m_isInited = false;


    //数据集合引用
    private BaseDataFactory m_data;
    //Server集合引用
    private BaseServerFactory m_server;
	//模块字典
	private Dictionary<Type,AbstractModel> m_modelList;
	
	
	public BaseModelFactory ()
	{
        m_modelList = new Dictionary<Type, AbstractModel>();
	}

    public void Dispose()
    {
        m_data = null;
        m_server = null;
        DestoryAllObj();
    }
	
	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="data">Data.</param>
	/// <param name="server">Server.</param>
	virtual public void Init(BaseDataFactory data,BaseServerFactory server)
	{
		m_data = data;
		m_server = server;
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
    protected T Creator<T>() where T : AbstractModel
	{
		if (!m_isInited) {
			throw new Exception("ModelFactory 还未初始化！");	
		}

        T obj = null;
        if (!m_modelList.ContainsKey(typeof(T)))
        {
            obj = System.Activator.CreateInstance(typeof(T)) as T;
            m_modelList[typeof(T)] = obj;
            (obj as IModuleCenter).Setup(this, m_data, m_server);
        }
        else {
            obj = m_modelList[typeof(T)] as T;
        }
        return obj;
	}
}



