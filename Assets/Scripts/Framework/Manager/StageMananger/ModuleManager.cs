using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 模块生命周期类型
/// </summary>
public enum ModuleLifeType
{
   Cache_Always = -1, //加载后，永不卸载
   Cache_None = 0,//常规状态，关闭就卸载
   Cache_60s = 60,//关闭后缓存x时间
   Cache_120s = 120//关闭后缓存x时间
}



/// <summary>
/// 模块管理器
/// 
/// 1. 是否开启
/// 2. 生命周期管理
/// 3. 显示状态
/// </summary>
public class ModuleManager{

    public static void Clear()
    {
        ModuleStatueDict.Clear();
    }


    #region 获取模块开启状态 IsOpen(ModuleName)
    /// <summary>
    /// 模块开启状态
    /// </summary>
    public static Dictionary<string, bool> ModuleStatueDict = new Dictionary<string, bool>();
    /// <summary>
    /// 判断模块是否开启,默认为开启状态
    /// </summary>
    /// <param name="ModuleName"></param>
    /// <returns></returns>
    public static bool IsOpen(string ModuleName)
    {
        if (ModuleStatueDict.ContainsKey(ModuleName))
        {
            return ModuleStatueDict[ModuleName];
        }
        return true;
    }
    #endregion


    #region 注册、更新模块生命周期

    /// <summary>
    /// 视图资源生命周期
    /// </summary>
    private static Dictionary<ILifeCycle, int> _moduleLifeCycle = new Dictionary<ILifeCycle, int>();
    private static List<ILifeCycle> _moduleList = new List<ILifeCycle>();
    private static List<ILifeCycle> _willRemoveList = new List<ILifeCycle>();
    private static List<ILifeCycle> _cacheList = new List<ILifeCycle>();
    private static int _cacheNumMax = 5;

    /// <summary>
    /// 全部清零
    /// </summary>
    public static void ClearLifeCycle()
    {
        _moduleLifeCycle.Clear();
        _moduleList.Clear();
    }

    /// <summary>
    /// 解除注册
    /// </summary>
    /// <param name="viewName"></param>
    public static void UnRegLifeCycle(ILifeCycle view)
    {
        if (_moduleLifeCycle.ContainsKey(view))
        {
            _moduleLifeCycle.Remove(view);
            _moduleList.Remove(view);
        }
    }

    /// <summary>
    /// 注册生命周期管理
    /// </summary>
    /// <param name="view"></param>
    /// <param name="type"></param>
    public static void RegLifeCycle(ILifeCycle view, ModuleLifeType type)
    {
        if (type == ModuleLifeType.Cache_None || type == ModuleLifeType.Cache_Always) return;

        if (!_moduleLifeCycle.ContainsKey(view))
        {
            _moduleList.Add(view);
        }
        AddCacheList(view);
        _moduleLifeCycle[view] = (int)type;
    }

    
    /// <summary>
    /// 更新模块生命周期
    /// </summary>
    public static void UpdateLifeCycle()
    {
        int count = _moduleList.Count;
        for (int i = 0; i < count; i++)
        {
            ILifeCycle view = _moduleList[i];
            if (!_moduleLifeCycle.ContainsKey(view)) continue;
            //时间减1秒
            _moduleLifeCycle[view]--;
            if (_moduleLifeCycle[view] <= 0 && !_willRemoveList.Contains(view))
            {//如果=0，Dispose
                _willRemoveList.Add(view);
            }
        }

        //卸载后消耗
        count = _willRemoveList.Count;
        for (int i = 0; i < count; i++)
        {
            Log.info("生命周期结束，自动销毁：：viewName:" + _willRemoveList[i].GetName());
            _willRemoveList[i].Dispose();
            
            _moduleLifeCycle.Remove(_willRemoveList[i]);
            _moduleList.Remove(_willRemoveList[i]);
            RemoveFromCacheList(_willRemoveList[i]);//从缓存队列里移除
        }
        _willRemoveList.Clear();
    }

    private static void AddCacheList(ILifeCycle view)
    {
        if (!_cacheList.Contains(view))
        {//如果不存在
            if (_cacheList.Count < _cacheNumMax)
            {
                _cacheList.Add(view);
            }
            else
            {//超出缓存个数，添加到待销毁列表
                _willRemoveList.Add(_cacheList[0]);
                _cacheList.RemoveAt(0);
                _cacheList.Add(view);
            }
        }
        else {//如果存在
            _cacheList.Remove(view);
            _cacheList.Add(view);//放到最后面
        }
    }

    private static void RemoveFromCacheList(ILifeCycle view)
    {
        if (_cacheList.Contains(view))
        {
            _cacheList.Remove(view);
        }
    }
    #endregion
}
