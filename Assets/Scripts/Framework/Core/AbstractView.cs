using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Utils.Event;

public class AbstractView : MonoBehaviour
{
    //----------------------------------------------------------------------
    //                    Controller
    //----------------------------------------------------------------------

    #region 获取Controller

    //控制器字典
    private static Dictionary<Type, AbstractCtrl> _ctrlList = new Dictionary<Type, AbstractCtrl>();
    //模型集合引用
    private static BaseModelFactory _models;

    /// <summary>
    /// Setup the specified model.
	/// </summary>
	/// <param name="models">Models.</param>
	public static void Setup(BaseModelFactory models)
	{
        _models = models;
	}

    /// <summary>
    /// 销毁所有对象
    /// </summary>
    public static void DestoryAllObj()
    {
        foreach (Type t in _ctrlList.Keys)
        {
            _ctrlList[t].Dispose();
            _ctrlList[t] = null;
        }
        _ctrlList.Clear();
        _models = null;
    }

    /// <summary>
    /// 获取控制器,创建完后会调用Init方法
    /// </summary>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    protected T GetController<T>() where T : AbstractCtrl
    {
        T obj = null;
        if (!_ctrlList.ContainsKey(typeof(T)))
        {
            obj = System.Activator.CreateInstance(typeof(T)) as T;
            _ctrlList[typeof(T)] = obj;
            (obj as IModuleCenter).Setup(_models, null, null);
        }
        else
        {
            obj = _ctrlList[typeof(T)] as T;
        }
        return obj;
    }

	/// <summary>
	/// Removes the controller.
	/// </summary>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	protected void RemoveController<T>() where T : AbstractCtrl
	{
		if (_ctrlList.ContainsKey(typeof(T)))
		{
			_ctrlList[typeof(T)].Dispose();
			_ctrlList.Remove(typeof(T));
		}
	}
    #endregion

    //----------------------------------------------------------------------
    //                    View
    //----------------------------------------------------------------------

    
    /// <summary>
    /// 是否初始化
    /// </summary>
    protected bool m_isInited = false;
    /// <summary>
    /// 是否被销毁
    /// </summary>
    protected bool m_isDispose = false;

    /// <summary>
    /// 离开
    /// 预留接口，为之后关闭是否缓存做准备
    /// </summary>
    public virtual void Exit()
    {
        if (this == null || this.gameObject == null) return;
        
        this.Dispose();
    }
    /// <summary>
    /// Dispose类、卸载资源（Resource，AssetBuddle）
    /// 建议使用 this.Exit();内置缓存逻辑判断与管理
    /// </summary>
    public virtual void Dispose()
    {
        this.m_isDispose = true;
        this.RemoveEventListeners();
        this.Unload();
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// 自动销毁
    /// </summary>
    void OnDestroy()
    {
        if (this.m_isDispose) return;
        this.Dispose();
    }

    /// <summary>
    /// 显示
    /// </summary>
    public virtual void Show() {
        this.AddEventListeners();
        this.Init();
        this.gameObject.SetActive(true); 
        this.OnShow();
    }

    //显示后调用
    protected virtual void OnShow() { }

    /// <summary>
    /// 隐藏 SetActive = false
    /// 建议使用 this.Exit();内置缓存逻辑判断与管理
    /// </summary>
    public virtual void Hide() {
        this.RemoveEventListeners();
        this.gameObject.SetActive(false);
        this.OnHide();
    }

    //隐藏后调用
    protected virtual void OnHide() { }

    /// <summary>
    /// Resource资源，AB资源手动卸载
    /// </summary>
    public virtual void Unload() { }
    /// <summary>
    /// 重置一些数据 or 显示信息
    /// </summary>
    public virtual void Clear() { }
    /// <summary>
    /// Show的时候被调用。建议重写Init，并做判断是否初始化过
    /// </summary>
    public virtual void Init() 
    {
        throw new Exception("需重写此类！判断是否已经初始化过！"+this.GetName());
        //         
        //         if (this.m_isInited) return;
        //         this.m_isInited = true;   
    }
    /// <summary>
    /// 视图名
    /// </summary>
    /// <returns></returns>
    public virtual string GetName() { return GetType().ToString(); }


    #region Events
    /// <summary>
    /// 添加监听事件,已在Show中调用,请勿重复调用
    /// </summary>
    public virtual void AddEventListeners() { }
    
    /// <summary>
    /// 移除事件，已在Dispose，Hide中调用，请勿重复调用
    /// </summary>
    public virtual void RemoveEventListeners() { }

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fn"></param>
    public void AddViewEventListener(string type, EventManager.EventCallBack fn)
    {
        EventManager.AddEventListener(type,fn);
    }

    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fn"></param>
    public void RemoveViewEventListener(string type, EventManager.EventCallBack fn)
    {
        EventManager.RemoveEventListener(type, fn);
    }

    /// <summary>
    /// 派发事件
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    public void DispatchViewEvent(string eventType, object data)
    {
        EventManager.Send(eventType, data);
    }
    #endregion
}
