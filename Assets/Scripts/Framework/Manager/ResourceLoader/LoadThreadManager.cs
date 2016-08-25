using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class LoadThreadManager:MonoBehaviour
{
    // 载入最大并行链接
    public static int maxThread = 5;
    //是否开启日志
    public static bool showDebug = false;
    /// <summary>
    /// 是否初始化
    /// </summary>
    public static bool isInited = false;

    public static LoadThreadManager instance = null;
    //go
    private static GameObject _threadGO = null;

    // 当前队列
    private List<ResLoader> _loadingItemList = new List<ResLoader>();
    // 全部队列
    private List<ResLoader> _waitItemList = new List<ResLoader>();


    //计算下载速度
    private float _lastTime = 0f;
    private float OneSecond = 1f;//1秒钟执行一次
    //下载速度，单位为KB
    private float _netSpeed = 1124;
    //上次最新下载的字节数
    private int _lastLoadedBytes = 0;
    private int _currLoadedBytes = 0;
    private int _currFileSize = 0;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init(Transform root)
    {
        if (isInited) return;
        isInited = true;

        _threadGO = new GameObject();
        _threadGO.name = "LoadThreadManager(0 / " + maxThread + ")";
        _threadGO.transform.parent = root;
        instance = _threadGO.AddComponent<LoadThreadManager>();
    }

    /// <summary>
    /// 返回速度  k/s
    /// </summary>
    public float NetSpeed
    {
        get { return _netSpeed; }
    }

    void Update()
    {
        _lastTime += Time.deltaTime;
        //每秒钟渲染一次
        if (_lastTime >= OneSecond)
        {
            _lastTime = 0;
            _netSpeed = ((float)_currLoadedBytes - (float)_lastLoadedBytes)/1024;//换算成K
            _lastLoadedBytes = _currFileSize;
        }

        int len = _loadingItemList.Count;
        for(int i=0;i<len;i++)
        {
            _loadingItemList[i].OnUpdate();
        }
    }

    //添加一个下载节点
    public void AddNode(ResLoader item)
    {
        if (_loadingItemList.Count < maxThread)
        {//如果队列有空闲加入
            _loadingItemList.Add(item);
            UpdateGameObjectName(_loadingItemList.Count);

            ExecOne(item);
        }
        else
        {// 添加到等待队列
            _waitItemList.Add(item);
        }
    }

    //检查是否可以继续下载
    public void CheckQueue()
    {
        if (showDebug) Debug.Log(ToString());

        int len = (_waitItemList.Count - maxThread >= 0) ? maxThread : _waitItemList.Count;

        for (int i = 0; i < len; i++)
        {
            ResLoader item = _waitItemList[0];
            _waitItemList.RemoveAt(0);
            _loadingItemList.Add(item);
            ExecOne(item);
        }

        UpdateGameObjectName(len);

        if (showDebug && _waitItemList.Count == 0 && _loadingItemList.Count == 0)
        {
            Debug.Log("[LoadThreadManager] *** All File Loaded ***");
        }
    }

    //开始下载一个
    private void ExecOne(ResLoader item)
    {
        if (showDebug)
        {
            Debug.Log("[LoadThreadManager] start load : " + item.ToString());
        }
        StartCoroutine(DownLoadEnumrator(item));
    }

    private IEnumerator DownLoadEnumrator(ResLoader item)
    {
        while (!Caching.ready)
        {
            yield return null;
        }

        item.StartLoad();

        yield return item.www;

        item.UpdateWWWState();

        StopCoroutine(DownLoadEnumrator(item));
    }

    private void UpdateGameObjectName(int loadNum)
    {
        _threadGO.name = "LoadThreadManager(" + loadNum + " / " + maxThread + ")";
    }

    //下载完成
    public void OnLoadItemFinish(ResLoader item)
    {
        _loadingItemList.Remove(item);
        CheckQueue();
    }

    public void UpadteSpeed(int size, int loaded)
    {
        _currLoadedBytes = loaded;
        _currFileSize = size;
    }

    public override string ToString()
    {
        return "[LoadTheradManager] maxThread:" + maxThread + " , wait:" + _waitItemList.Count;
    }
}
