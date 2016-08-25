using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ObjectPoolTool;

/// <summary>
/// 
/// 对象池（仅针对非显示对象） 
/// 
/// by piaochen
/// 
/// _pool = new ObjectPool(typeof(TestData), null, 100, 150);
/// _pool.GetObject();//取出
/// _pool.FreeObject(((IPoolItem)_list[0]).GetHashCode());//扔回对象池
/// _pool.Clear();//清理
/// _pool.Release();//释放
/// 
/// </summary>

public sealed class ObjectPool
{
    // 容量
    public int _nCapacity;
    // 当前大小
    public int _nCurrentSize;
    //对象列表
    private Hashtable _listObjects;
    //使用状态
    private Hashtable _objUseState;
    //空闲中的对象列表
    private ArrayList _listFreeIndex;
    //使用中的对象列表
    private ArrayList _listUsingIndex;
    //对象类型
    private Type _objType;
    //对象构造参数
    private object[] _cArhs;
    //是否支持重置数据
    private bool _supportReset = false;

    /// <summary>
    /// 对象池
    /// </summary>
    /// <param name="objType">对象类型</param>
    /// <param name="cArgs">缓存对象构造参数</param>
    /// <param name="initSize">初始尺寸，默认不小于1个</param>
    /// <param name="capacity">容量大小,默认不小于5个.注意：如果太小，频繁变化list大小会触发GCloc，建议尽可能大些。</param>
    public ObjectPool(Type objType, object[] cArgs,int initSize, int capacity)
    {
        if (initSize < 1) initSize = 1;
        if (capacity < 5) capacity = 5;

        _objType = objType;
        _cArhs = cArgs;
        _nCapacity = capacity;

        _listObjects = new Hashtable();
        _objUseState = new Hashtable();
        _listFreeIndex = new ArrayList();
        _listUsingIndex = new ArrayList();

        //缓存的类型是否支持IPoolItem接口
        Type supType = typeof(IPoolItem);
        if (supType.IsAssignableFrom(supType))
        {
            _supportReset = true;
        }

        //默认创建对象
        for (int i = 0; i < initSize; i++)
        {
            CreateOneObject();
        }
        _nCurrentSize = _listObjects.Count;
    }


    private int CreateOneObject()
    {
        object obj = null;
        try
        {
            obj = Activator.CreateInstance(_objType, _cArhs);
        }
        catch (System.Exception ex)
        {
            Debug.Log("ObjectPool 初始化对象失败！" + ex.ToString());
            return -1;
        }

        int key = obj.GetHashCode();
        _listObjects.Add(key, obj);
        _listFreeIndex.Add(key);
        _objUseState.Add(key, false);
        _nCurrentSize++;
        return key;
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        lock (this)
        {
            Type supType = typeof(IPoolItem);
            if (supType.IsAssignableFrom(supType))
            {
                foreach (DictionaryEntry de in _listObjects)
                {
                    ((IPoolItem)de.Value).Release();
                }
            }    
            _listObjects.Clear();
            _objUseState.Clear();
            _listUsingIndex.Clear();
            _listFreeIndex.Clear();
            _nCurrentSize = 0;
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Release()
    {
        Clear();
        _listObjects = null;
        _objUseState = null;
        _listUsingIndex = null;
        _listFreeIndex = null;
        _cArhs = null;
    }

    /// <summary>
    /// 当前个数
    /// </summary>
    public int CurrentSize
    {
        get { return _nCurrentSize; }
    }

    /// <summary>
    /// 使用中的个数
    /// </summary>
    public int UsingCount
    {
        get { return _listUsingIndex.Count; }
    }

    /// <summary>
    /// 从池中获取一个对象
    /// </summary>
    /// <returns></returns>
    public object GetObject()
    {
        lock (this)
        {
            object target = null;
            if (_listFreeIndex.Count > 0)
            {//有闲置资源
                int key = (int)_listFreeIndex[0];
                target = _listObjects[key];

                _listFreeIndex.RemoveAt(0);
                _listUsingIndex.Add(key);
                _objUseState[key]=true;
                
            }

            if (target == null)
            {//无闲置资源，直接创建个新的
                if (_nCurrentSize < _nCapacity)
                {
                    int key = CreateOneObject();
                    if (key != -1)
                    {
                        target = _listObjects[key];

                        _listFreeIndex.RemoveAt(0);
                        _listUsingIndex.Add(key);
                        _objUseState[key] = true;
                    } 
                }  
            }
            return target;
        }
    }

    /// <summary>
    /// 释放一个对象
    /// </summary>
    /// <param name="objHashCode"></param>
    public void FreeObject(int objHashCode)
    {
        if ((bool)_objUseState[objHashCode] == false)
        {
            return;
        }

        lock (this)
        {
            if (_supportReset)
            {
                IPoolItem item = (IPoolItem)_listObjects[objHashCode];
                item.Reset();
            }
            _listUsingIndex.Remove(objHashCode);
            _listFreeIndex.Add(objHashCode);
            _objUseState[objHashCode] = false;
        }
    }

    /// <summary>
    /// 是否正在被使用
    /// </summary>
    /// <param name="objHashCode"></param>
    /// <returns></returns>
    public bool IsUse(int objHashCode)
    {
        if (_objUseState.ContainsKey(objHashCode))
        {
            return (bool)_objUseState[objHashCode];
        }
        return false;
    }

    /// <summary>
    /// 减少个数
    /// </summary>
    /// <param name="size"></param>
    public int DecreaseSize(int size)
    {
        int nDecrease = size;
        lock (this)
        {
            if (nDecrease <= 0)
            {
                return 0;
            }

            if (nDecrease > _listFreeIndex.Count)
            {
                nDecrease = _listFreeIndex.Count;
            }

            //清理掉nDecrease长度的对象
            for (int i = 0; i < nDecrease; i++)
            {
                _listObjects.Remove(_listFreeIndex[i]);
            }

            //清理
            _listFreeIndex.Clear();
            _listUsingIndex.Clear();

            //重新建立使用中与空闲的索引队列
            foreach (DictionaryEntry de in _listObjects)
            {
                if ((bool)_objUseState[de.Key])
                {
                    _listUsingIndex.Add(de.Key);
                }
                else
                {
                    _listFreeIndex.Add(de.Key);
                }
            }
            _nCurrentSize -= nDecrease;
            return nDecrease;
        }
    }

    public override string ToString()
    {
        return "对象池>>  空闲个数：" + _listFreeIndex.Count.ToString() + "  ,使用中个数： " + _listUsingIndex.Count.ToString() + ", 当前个数：" + _nCurrentSize.ToString() + " , 容量：" + _nCapacity.ToString();
    }

}