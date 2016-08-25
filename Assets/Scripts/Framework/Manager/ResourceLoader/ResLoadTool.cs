using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResLoadTool {

    /// <summary>
    /// 是否走www加载
    /// </summary>
    public static bool isByWWWLoad = true;

    private static Dictionary<string, int> _listStateDict = new Dictionary<string, int>();

    private static Dictionary<string, int> _listProgressDict = new Dictionary<string, int>();

    private static List<VOLocalLoadNode> _listNode = new List<VOLocalLoadNode>();

    /// <summary>
    /// 队列加载
    /// </summary>
    /// <param name="pathList"></param>
    /// <param name="listId"></param>
    /// <param name="loaderListComplete"></param>
    /// <param name="loaderProgress"></param>
    /// <param name="loaderError"></param>
    /// <param name="loaderListProgress"></param>
    /// <param name="loaderFileComplete"></param>
    /// <param name="param"></param>
    public static void LoadList<T>(List<StResPath> pathList,
        string listId,
        ResLoaderManager.DelegateListLoaded loaderListComplete,
        ResLoaderManager.DelegateLoaderProgress loaderProgress = null,
        ResLoaderManager.DelegateLoaderError loaderError = null,
        ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete = null,
        object param = null) where T:UnityEngine.Object
    {
        if (!isByWWWLoad)
        {
            if (!_listStateDict.ContainsKey(listId))
            {
                _listStateDict[listId] = pathList.Count;
                int len = pathList.Count;
                for (int i = 0; i < len; i++)
                {
                    LoadFromLocalRes<T>(pathList[i], listId, loaderListComplete, loaderFileComplete, loaderError, loaderListProgress, param);
                }
            }
            else
            {
                Log.info("队列正在加载中,请检查 listID: " + listId);
                return;
            }
        }
        else
        {
            ResLoaderManager.instance.Loader(pathList, listId, loaderListComplete, loaderProgress, loaderError, loaderListProgress, loaderFileComplete, param);
        }
    }


    /// <summary>
    /// 单个文件加载
    /// </summary>
    /// <param name="resPath"></param>
    /// <param name="loaderFileComplete"></param>
    /// <param name="loaderProgress"></param>
    /// <param name="loaderError"></param>
    /// <param name="param"></param>
    public static void Load<T>(
        StResPath resPath,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete,
        ResLoaderManager.DelegateLoaderProgress loaderProgress = null,
        ResLoaderManager.DelegateLoaderError loaderError = null,
        object param = null) where T:UnityEngine.Object
    {
        resPath.path = FormatLocalPath(resPath.path);
        if (!isByWWWLoad)
        {
            LoadFromLocalRes<T>(resPath, resPath.path, null, loaderFileComplete, loaderError, null, param);
        }
        else
        {
            resPath.path = SystemConfig.localPlatformResPath + resPath.path + ".unity3d";
            ResLoaderManager.instance.Loader(resPath, loaderFileComplete, loaderProgress, loaderError, param);
        }
    }

    /// <summary>
    /// 单个文件加载 - 图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="loaderFileComplete"></param>
    /// <param name="loaderProgress"></param>
    /// <param name="loaderError"></param>
    /// <param name="param"></param>
    public static void LoadTexture(
        string path, 
        ResLoaderManager.DelegateFileLoaded loaderFileComplete,
        ResLoaderManager.DelegateLoaderProgress loaderProgress = null,
        ResLoaderManager.DelegateLoaderError loaderError = null,
        object param = null)
    {
        path = FormatLocalPath(path);
        if (!isByWWWLoad)
        {
            LoadFromLocalRes<Texture>(new StResPath(path), path, null, loaderFileComplete, loaderError, null, param);
        }
        else
        {
            path = SystemConfig.localPlatformResPath + path + ".unity3d";
            ResLoaderManager.instance.Loader(new StResPath(path), loaderFileComplete, loaderProgress, loaderError, param);
        }
    }


    #region private
    /// <summary>
    /// 格式化为本地 Resources.Load使用的路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string FormatLocalPath(string path)
    {
        //如果包含后缀
        int index = path.IndexOf(".");
        if (index != -1)
        {
            return path.Substring(0, index);
        }
        return path;
    }

    /// <summary>
    /// 本地加载,支持  texture，audio，txt,types，prefab
    /// </summary>
    /// <param name="resPath">路径需包含后缀名，本地载入会进行截取</param>
    /// <param name="loaderFileComplete"></param>
    /// <param name="loaderError"></param>
    /// <param name="param"></param>
    private static void LoadFromLocalRes<T>(
        StResPath resPath,
        string id,
        ResLoaderManager.DelegateListLoaded loaderListComplete,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete,
        ResLoaderManager.DelegateLoaderError loaderError,
        ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null,
        object param = null) where T:UnityEngine.Object
    {
        //完整路径
        string fullPath = SystemConfig.localPlatformResPath + resPath.path + ".unity3d";
        if (ResDataManager.instance.HasEditorRes(fullPath))
        {
            if (loaderFileComplete != null) loaderFileComplete(fullPath, param);
        }
        else {

            VOLocalLoadNode node = new VOLocalLoadNode();
            node.resPath = resPath;
            node.id = id;
            node.loaderListComplete = loaderListComplete;
            node.loaderFileComplete = loaderFileComplete;
            node.loaderError = loaderError;
            node.loaderListProgress = loaderListProgress;
            node.param = param;
            node.type = typeof(T).ToString();
            StartLoad(node);
        }
    }

    private static bool UpdateProgress(string id)
    {
        if (!_listStateDict.ContainsKey(id)) return false;
        if (!_listProgressDict.ContainsKey(id)) _listProgressDict[id] = 0;
        _listProgressDict[id]++;
        return true;
    }

    private static bool CheckLoaded(string id)
    {
        if (!_listStateDict.ContainsKey(id)) return false;
        if (!_listProgressDict.ContainsKey(id)) return false;
        return (_listStateDict[id] == _listProgressDict[id]);
    }

    private static void RemoveList(string id)
    {
        if (_listStateDict.ContainsKey(id)) _listStateDict.Remove(id);
        if (_listProgressDict.ContainsKey(id)) _listStateDict.Remove(id);
    }
    #endregion


    #region 本地加载协程分帧载入
    private static void StartLoad(VOLocalLoadNode node)
    {
        _listNode.Add(node);
        if (_listNode.Count == 1)
        {
            CoroutineDelegate.StartCoroutines(OnLocalLoadThread(node));
        }
    }

    private static IEnumerator OnLocalLoadThread(VOLocalLoadNode node)
    {
        while(_listNode.Count>0)
        {
            UnityEngine.Object obj = Resources.Load(node.resPath.path);
            _listNode.Remove(node);

            //完整路径
            if (node.resPath.path.IndexOf(".unity3d")==-1)
            {
                node.resPath.path = SystemConfig.localPlatformResPath + node.resPath.path + ".unity3d";
            }

            if (obj != null)
            {
                ResDataManager.instance.AddEditorRes(node.resPath.path, obj);
                if (node.loaderFileComplete != null) node.loaderFileComplete(node.resPath.path, node.param);
            }
            else
            {
                Log.info("[ResLoadTool LoadFromLocalRes]加载失败！" + node.resPath.path);
                if (node.loaderError != null)
                {
                    node.loaderError(node.resPath.path, "[ResLoadTool LoadFromLocalRes]加载失败！" + node.resPath.path);
                    yield return 2;
                } 
            }

            bool b = UpdateProgress(node.id);
            if (b)
            {
                if (node.loaderListProgress != null) node.loaderListProgress(node.id, _listProgressDict[node.id], _listStateDict[node.id]);
                if (CheckLoaded(node.id))
                {//队列完成
                    if (node.loaderListComplete != null) node.loaderListComplete(node.id, node.param);
                    RemoveList(node.id);
                }
            }
            yield return 2;
        }
        CoroutineDelegate.StopCoroutines(OnLocalLoadThread(node));
    }
    #endregion
}

class VOLocalLoadNode
{
    public StResPath resPath;
    public string id;
    public ResLoaderManager.DelegateListLoaded loaderListComplete = null;
    public ResLoaderManager.DelegateFileLoaded loaderFileComplete = null;
    public ResLoaderManager.DelegateLoaderError loaderError = null;
    public ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null;
    public object param = null;
    public string type;
}
