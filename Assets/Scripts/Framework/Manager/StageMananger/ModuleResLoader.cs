using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils.Event;

/// <summary>
/// 模块资源加载器
/// </summary>
public class ModuleResLoader {

    /// <summary>
    /// 场景加载进度
    /// </summary>
    public static string MODULE_LOADING_RES_PROGRESS = "MODULE_LOADING_RES_PROGRESS";
    /// <summary>
    /// 模块名
    /// </summary>
    public string ModuleName = "";
    /// <summary>
    /// 是否加载成功
    /// </summary>
    public bool IsLoaded = true;
    /// <summary>
    /// 是否在加载中
    /// </summary>
    public bool IsLoading = false;
    /// <summary>
    /// 资源列表
    /// </summary>
    private List<StResPath> _resList;
    /// <summary>
    /// 成功回调
    /// </summary>
    private DelegateEnums.DataParam _callback;


    public ModuleResLoader(string moduleName)
    {
        ModuleName = moduleName;
        _resList = new List<StResPath>();        
    }

    public void UnLoad(bool unLoadUseObj=false)
    {
        for (int i = 0; i < _resList.Count; i++)
        {
            StResPath resPath = _resList[i];
            ResDataManager.instance.RemoveAssetBundle(resPath.path, true, unLoadUseObj);
        }
        _resList.Clear();
        _resList = null;
        _callback = null;
    }

    public void AddList(StResPath path)
    {
        _resList.Add(path);
        IsLoaded = false;
    }

    public void StartLoad(DelegateEnums.DataParam callback)
    {
        _callback = callback;
        if (IsLoaded && _callback!=null)
        {
            _callback(ModuleName);
        }
        IsLoading = true;
        ResLoaderManager.instance.Loader(_resList, ModuleName + "_list", OnResLoaded, null, OnResLoadError, OnResLoadListProgress);
    }

    private void OnResLoaded(string listName,object param = null)
    {
        IsLoaded = true;
        IsLoading = false;
        if (_callback != null)
        {
            _callback(ModuleName);
        }
    }

    private void OnResLoadError(string path, string errorInfo)
    {
        Log.infoError("模块资源加载失败: "+path+"  ... "+errorInfo);
    }

    private void OnResLoadListProgress(string listName,float currentValue,float totalValue)
    {
        string data = "模块: " + ModuleName + "  loading " + currentValue + " / " + totalValue;
        Log.info(data);
        EventManager.Send(ModuleResLoader.MODULE_LOADING_RES_PROGRESS, data);
    }
}
