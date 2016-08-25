using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 资源管理器
/// </summary>
public class ResDataManager
{
    public static readonly ResDataManager instance = new ResDataManager();
    /// <summary>
    /// www(webstream) 缓存数据
    /// </summary>
    private Dictionary<string, VOAssetInfo> _dataList = new Dictionary<string, VOAssetInfo>();


    #region 编辑器模式下存储数据
    /// <summary>
    /// 编辑器模式下，本地数据缓存
    /// </summary>
    private Dictionary<string, UnityEngine.Object> _editorDataDict = new Dictionary<string, UnityEngine.Object>();

    public void AddEditorRes(string path, UnityEngine.Object data)
    {
        if (_editorDataDict.ContainsKey(path)) return;
        _editorDataDict[path] = data;
    }

    public bool HasEditorRes(string path)
    {
        bool b = _editorDataDict.ContainsKey(path);
        if (b == false) return false;

        if (_editorDataDict[path] != null) return true;

        _editorDataDict.Remove(path);
        return false;
    }
    #endregion


    #region 缓存webstream(www)管理
    /// <summary>
    /// 数据总数
    /// </summary>
    /// <returns>The count.</returns>
    public int DataCount()
    {
        if (this._dataList == null) return 0;
        return this._dataList.Keys.Count;
    }
    /// <summary>
    /// 移除数据AB对象,释放WWW
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isForceUnload">是否强制Unload。强制释放，不计数引用计数</param>
    /// <param name="unloadUsedObjects">是否Unload 所有资源 包括正在使用的</param>
    public void RemoveAssetBundle(string path, bool isForceUnload = false, bool unloadUsedObjects = false)
    {
        if (this._dataList == null || !this._dataList.ContainsKey(path)) return;

        VOAssetInfo resInfo = this._dataList[path];
        if (resInfo != null)
        {
            if (LoadThreadManager.showDebug) Log.info("ResDataManager::RemoveAssetBundle 清除资源！path " + path);
            // 释放引用的资源
            if (resInfo.www != null && resInfo.www.assetBundle != null)
            {          
                resInfo.www.assetBundle.Unload(unloadUsedObjects);
            }
            resInfo.Dispose();
            resInfo = null;
        }
        this._dataList.Remove(path);
    }

    /// <summary>
    /// 释放webstrem(ab包)，通过ResTag。
    /// 比如统一清除战斗中的资源
    /// </summary>
    /// <param name="tag"></param>
    public void RemoveAssetBundleByTag(ResTag tag, bool unloadUsedObjects = false)
    {
        if (LoadThreadManager.showDebug) Log.info("ResDataManager::RemoveAssetBundleByTag 清除资源！ResTag " + tag.ToString());
        List<string> list = new List<string>(_dataList.Count);
        foreach (VOAssetInfo info in _dataList.Values)
        {
            if ((info.tag & tag) != ResTag.None)
            {
                list.Add(info.path);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            string path = list[i];
            if (path == null || path == "") continue;
            this.RemoveAssetBundle(path, true, unloadUsedObjects);
        }
        list.Clear();
        list = null;
    }
    /// <summary>
    /// 添加数据到缓存
    /// </summary>
    /// <param name="wwwData">Www data.</param>
    public void InsertWWWData(VOAssetInfo resInfo)
    {
        if (resInfo == null)
        {
            if (LoadThreadManager.showDebug) Log.info("ResDataManager::InsertWWWData  resInfo is Null!");
            return;
        } 
        if (LoadThreadManager.showDebug) Log.info("ResDataManager::InsertWWWData " + resInfo.path);
        this._dataList.Add(resInfo.path, resInfo);
    }

    /// <summary>
    /// 卸载释放AssetBundle里实例化的资源
    /// <param name="assetToUnload"></param>
    public void UnloadAsset(UnityEngine.Object assetToUnload)
    {
        if (LoadThreadManager.showDebug) Log.info("ResDataManager::UnloadAsset 清除资源！");
        Resources.UnloadAsset(assetToUnload);
    }
    #endregion

    /// <summary>
    /// 清楚所有资源
    /// </summary>
    public void DestroyAll()
    {
        if (LoadThreadManager.showDebug) Log.info("ResDataManager::DestroyAll 清除所有资源！");
        foreach (VOAssetInfo info in _dataList.Values)
        {
            if (info != null)
            {
                info.Dispose();
            }
        }
        _dataList.Clear();
        _dataList = null;
    }

    /// <summary>
    /// 仅AB打包多个资源，获取方式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="prefabName">如果是ab包里只有单一文件，此处可以传空字符串</param>
    /// <param name="unload">是否删除缓存，默认删除</param>
    /// <returns></returns>
    public T CreateObjectByPrefabName<T>(string filePath, string prefabName, bool unload = true) where T : UnityEngine.Object
    {
        #region 走本地读取
        if (!ResLoadTool.isByWWWLoad && this._editorDataDict.ContainsKey(filePath))
        {
            switch (typeof(T).ToString())
            {
                case "UnityEngine.GameObject":
                case "UnityEngine.Texture":
                case "UnityEngine.AudioClip":
                    T img = this._editorDataDict[filePath] as T;
                    if (unload) this._editorDataDict.Remove(filePath);
                    if (img != null) return img;
                    break;
                default:
                    Log.info("Type:" + typeof(T).ToString() + " 非AB打包的资源，走其他的获取资源方式!");
                    break;
            }
        }
        #endregion

        T prefabAsset;
        //从webstrem中获取
        AssetBundle assetBundle = this.GetDataAssetBundle(filePath);
        if (assetBundle == null)
        {
            if (LoadThreadManager.showDebug) Log.info("ResDataManager::AssetBundle不存在！" + filePath);
            return null;
        }

        if (prefabName == "")
        {
            prefabAsset = assetBundle.mainAsset as T;
        }
        else
        {
            prefabAsset = assetBundle.LoadAsset(prefabName, typeof(T)) as T;
        }

        if (unload)
        {
            RemoveAssetBundle(filePath);
        }
        return prefabAsset;
    }

    /// <summary>
    /// 仅AB打包单个资源，获取方式
    /// 注意：此接口缓存对象在_dataList中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="unload"></param>
    /// <returns></returns>
    public T CreateObjectFromCache<T>(string filePath, bool unload = true) where T : UnityEngine.Object
    {
        return CreateObjectByPrefabName<T>(filePath, "", unload);
    }

    /// <summary>
    /// 直接加载txt 获取字符串
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isClear"></param>
    /// <returns></returns>
    public string GetDataText(string path, bool isClear = true)
    {
        #region 走本地读取
        if (!ResLoadTool.isByWWWLoad && this._editorDataDict.ContainsKey(path))
        {//如果是文本
            if (ResTypeEnum.TEXT == GetResType(path))
            {
                TextAsset txtAsset = this._editorDataDict[path] as TextAsset;
                if (txtAsset != null)
                {
                    this._editorDataDict.Remove(path);
                    return txtAsset.text;
                }
            }
        }
        #endregion
        if (this._dataList == null || !this._dataList.ContainsKey(path)) return "";

        string data = "";

        VOAssetInfo resInfo = this._dataList[path];
        if (resInfo != null)
        {
            data = resInfo.www.text;
            if (isClear) this.RemoveAssetBundle(path, true);
        }
        return data;
    }

    /// <summary>
    /// 直接加载bytes 获取 Bytes 对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isClear"></param>
    /// <returns></returns>
    public byte[] GetDataBytes(string path, bool isClear = true)
    {
        #region 走本地读取
        if (!ResLoadTool.isByWWWLoad && this._editorDataDict.ContainsKey(path))
        {//如果是Bytes
            if (ResTypeEnum.BTYES == GetResType(path))
            {
                TextAsset txtAsset = this._editorDataDict[path] as TextAsset;
                if (txtAsset != null)
                {
                    byte[] bytesTmp = new byte[txtAsset.bytes.Length];
                    Array.Copy(txtAsset.bytes, 0, bytesTmp, 0, txtAsset.bytes.Length);
                    this._editorDataDict.Remove(path);
                    return bytesTmp;
                }
            }
        }
        #endregion
        if (this._dataList == null || !this._dataList.ContainsKey(path)) return null;

        byte[] bytesNew = null;
        VOAssetInfo resInfo = this._dataList[path];
        if (resInfo != null)
        {
            bytesNew = new byte[resInfo.www.bytes.Length];
            Array.Copy(resInfo.www.bytes, 0, bytesNew, 0, resInfo.www.bytes.Length);
            if (isClear) this.RemoveAssetBundle(path, true);
        }
        return bytesNew;
    }

    /// <summary>
    /// 直接加载Audio的获取方式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isClear"></param>
    /// <returns></returns>
    public AudioClip GetAudioClip(string path, bool isClear = true)
    {
        #region 走本地读取
        if (!ResLoadTool.isByWWWLoad && this._editorDataDict.ContainsKey(path))
        {//如果是Bytes
            if (ResTypeEnum.AUDIO == GetResType(path))
            {
                AudioClip audio = this._editorDataDict[path] as AudioClip;
                return audio;
            }
        }
        #endregion
        if (this._dataList == null || !this._dataList.ContainsKey(path)) return null;

        VOAssetInfo resInfo = this._dataList[path];
        AudioClip clip = null;
        if (resInfo != null)
        {
            clip = resInfo.www.audioClip;
            if (isClear) this.RemoveAssetBundle(path, true);
        }
        return clip;
    }

    /// <summary>
    /// 从预制文件中实例GameObject对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isAsyncLoad">是否异步获取</param>
    /// <returns></returns>
    public GameObject GetObjectFromLocalPrefab(string path, bool isAsyncLoad = false)
    {
        GameObject sp = null;
        if (isAsyncLoad)
        {
            ResourceRequest req = Resources.LoadAsync(path);
            if (req == null)
            {
                if (LoadThreadManager.showDebug) Log.infoError("ResDataManager::GetObjectFromPrefab..Error " + path + " 无效文件!");
                return null;
            }
            sp = req.asset as GameObject;
        }
        else
        {
            sp = Resources.Load(path) as GameObject;
            if (sp == null)
            {
                if (LoadThreadManager.showDebug) Log.infoError("ResDataManager::GetObjectFromPrefab..Error" + path + " 无效文件!");
                return null;
            }
        }
        return sp;
    }


    /// <summary>
    /// 获取 AssetBundle 对象 (不对外公开此方法)
    /// </summary>
    /// <returns>The data asset bundle.</returns>
    /// <param name="path">Path.</param>
    /// <param name="loaderTypeEnum">Loader type enum.</param>
    private AssetBundle GetDataAssetBundle(string path)
    {
        if (this._dataList == null || !this._dataList.ContainsKey(path))
        {
            //if (LoadThreadManager.showDebug) Log.info("ResDataManager:: dataList containsKey is False, key:" + path);
            return null;
        }
            
        VOAssetInfo resInfo = this._dataList[path];
        if (resInfo != null)
        {
            if (resInfo.www == null)
            {
                //if (LoadThreadManager.showDebug) Log.info("ResDataManager:: resInfo.www is Null!");
                return null;
            } 
            return resInfo.www.assetBundle;
        }
        //if (LoadThreadManager.showDebug) Log.info("ResDataManager:: resInfo is Null!");
        return null;
    }

    /// <summary>
    /// 获取资源tag
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private ResTag GetResTagByPath(string path)
    {
        if (this._dataList == null || !this._dataList.ContainsKey(path)) return ResTag.None;

        VOAssetInfo resInfo = this._dataList[path];
        if (resInfo != null)
        {
            return resInfo.tag;
        }
        return ResTag.None;
    }

    /// <summary>
    /// 判断是否存在数据
    /// </summary>
    /// <returns><c>true</c>, if data was existsed, <c>false</c> otherwise.</returns>
    /// <param name="path">Path.</param>
    public bool ExistWWWData(string path)
    {
        if (this._dataList == null) return false;

        return this._dataList.ContainsKey(path);
    }


    //写入本地文件
    public void SaveToLocalCache<T>(T data, string key)
    {
        FileTools.SaveDataToBin(data, SystemConfig.localUserCacheSavePath + "LocalCache/" + key + ".data");
    }

    //读取本地文件
    public void ReadLocalCache<T>(string key, DelegateEnums.DataParam fn)
    {
        FileTools.GetDataFromPath<T>(SystemConfig.localUserCacheReadPath + "LocalCache/" + key + ".data", fn);
    }


    //判断文件类型
    public static ResTypeEnum GetResType(string filePath)
    {
        int index = filePath.IndexOf(".");
        string extension = filePath.Substring(index + 1, filePath.Length - index - 1);
        switch (extension)
        {
            case "png":
            case "PNG":
            case "jpg":
            case "JPG":
                return ResTypeEnum.TEXTURE;
            case "MP3":
            case "mp3":
            case "wav":
            case "WAV":
            case "ogg":
            case "OGG":
                return ResTypeEnum.AUDIO;
            case "prefab":
            case "PREFAB":
            case "unity3d":
            case "UNITY3D":
                return ResTypeEnum.ASSET_BUNDLE;
            case "txt":
            case "xml":
                return ResTypeEnum.TEXT;
            default:
                return ResTypeEnum.BTYES;
        }
    }
}

/***
///-------------------------------------------------------------------
/// AB包里资源引用计数器
///-------------------------------------------------------------------

public struct StReferenceCounter
{
    /// <summary>
    /// AB名
    /// </summary>
    public string AssetBunldeName;
    /// <summary>
    /// AB中资源的引用计数器
    /// </summary>
    private int _resRefCount;

    public StReferenceCounter(string path)
    {
        AssetBunldeName = path;
        _resRefCount = 0;
    }

    /// <summary>
    /// 获取当前引用计数
    /// </summary>
    /// <returns></returns>
    public int ResRefCount
    {
        get { return _resRefCount; }
    }

    /// <summary>
    /// resName 资源引用计数+1
    /// </summary>
    public void AddReference()
    {
        _resRefCount++;
    }

    /// <summary>
    /// resName 资源引用计数-1
    /// </summary>
    /// <returns></returns>
    public int DelReference()
    {
        if (_resRefCount > 0) return _resRefCount--;
        return 0;
    }

    public override string ToString()
    {
        return AssetBunldeName + " RefCount :" + _resRefCount.ToString()+"\n";
    }
}
**/