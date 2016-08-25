using UnityEngine;
using System.Collections;


/// <summary>
/// 继承此接口的对象才可以派发事件
/// </summary>
public interface IDispatcher  {

    //派发事件
    void DispatchEvent(string eventType, object data);
    //添加监听事件
    void AddEventListeners();
    //移除监听事件
    void RemoveEventListeners();
}
