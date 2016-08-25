using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源加载管理器
/// </summary>
public class ResLoaderManager
{
    public static readonly ResLoaderManager instance = new ResLoaderManager();

 	// 加载进度
	public delegate void DelegateLoaderProgress(string path, float currentValue);
    // 总队列进度
    public delegate void DelageteLoaderListProgress(string listName,float currentValue,float totalValue);
    // 加载完成. --- 如果是队列加载listName表示orderName，如果是单个加载表示路径
    public delegate void DelegateListLoaded(string listName,object param = null);
    // 单个文件加载完成
    public delegate void DelegateFileLoaded(string filePath,object param = null);
	// 加载失败
    public delegate void DelegateLoaderError(string filePath , string errorInfo);

	/// <summary>
	/// 加载订阅列表
	/// </summary>
	private IList<ResLoaderOrder> _orderList;

    /// <summary>
    /// 单个资源加载
    /// </summary>
    /// <param name="resPath"></param>
    /// <param name="loaderComplete"></param>
    /// <param name="loaderProgress"></param>
    /// <param name="loaderError"></param>
    /// <param name="param"></param>
    public void Loader(
        StResPath resPath,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete, 
        ResLoaderManager.DelegateLoaderProgress loaderProgress = null,
        ResLoaderManager.DelegateLoaderError loaderError = null,
        object param = null)
    
    {
        List<StResPath> pathList = new List<StResPath>();
        pathList.Add(resPath);
        Loader(pathList, resPath.path, null, loaderProgress, loaderError, null, loaderFileComplete, param);
    }

	/// <summary>
    /// 队列加载
	/// </summary>
	/// <param name="pathList"></param>
    /// <param name="id"></param>
    /// <param name="loaderListComplete">队列加载完成</param>
	/// <param name="loaderProgress">单个文件进度</param>
	/// <param name="loaderError">单个文件错误</param>
	/// <param name="loaderListProgress">队列进度</param>
    /// <param name="loaderFileComplete">单个加载完成</param>
    /// <param name="param"></param>
    public void Loader(
        List<StResPath> pathList, 
        string id, 
        ResLoaderManager.DelegateListLoaded loaderListComplete, 
        ResLoaderManager.DelegateLoaderProgress loaderProgress = null, 
        ResLoaderManager.DelegateLoaderError loaderError = null ,
        ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete = null,
        object param = null)

	{
		if(this._orderList == null) this._orderList = new List<ResLoaderOrder>();
        //去重处理
		ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName (id);
        if (resLoaderOrder!=null)
        {
            if (LoadThreadManager.showDebug) Log.info("[Warn! 与当前加载中的list名相同，请确保唯一性！] id: " + id);
            //如果已有相同的资源在加载，添加新回调
            resLoaderOrder.AttachEvent(loaderListComplete, loaderProgress, loaderError, loaderListProgress, loaderFileComplete);
            return;
        }


        //添加到订阅队列中
        IList<StResPath> pList = new List<StResPath>(pathList.ToArray());
        resLoaderOrder = new ResLoaderOrder(id, pList, loaderProgress, loaderListComplete, loaderError, loaderListProgress, loaderFileComplete,param);
	    this._orderList.Add (resLoaderOrder);
            
        //遍历订阅文件，开始推入线程加载
        int len = resLoaderOrder.pathList.Count;
        if (len == 0)
        {
            this.LoaderOperater(id);
            return;
        }

        for (int i = 0; i < len; i++)
        {
            StResPath stPath = resLoaderOrder.pathList[i];
            //检查是否在缓存对象中，是否已经加载过
            bool existsStatus = ResDataManager.instance.ExistWWWData(stPath.path);
            if (!existsStatus)
            {//不存在，加载并缓存
                ResLoader resLoader = new ResLoader(resLoaderOrder.Id);
                resLoader.Loader(stPath, OnProgressHandler, OnCompleteHandler, OnErrorHandler);
            }
            else
            {//资源已存在，无需加载
                if (LoadThreadManager.showDebug) Log.info("加载 " + stPath.path + " 资源已存在 !");
                this.LoaderOperater(id, stPath);
            }
        }
	}

    /// <summary>
    /// 根据序列名称检索加载序列
    /// </summary>
    /// <returns>The www loader order by id.</returns>
    /// <param name="id">id.</param>
    private ResLoaderOrder GetWwwLoaderOrderByOrderName(string id)
    {
        if (this._orderList == null || this._orderList.Count == 0) return null;
        int len = this._orderList.Count;
        for (int i = 0; i < len; i++)
        {
            ResLoaderOrder resLoaderOrder = this._orderList[i];
            if (resLoaderOrder.Id == id) return resLoaderOrder;
        }
        return null;
    }

    /// <summary>
    /// 加载进度
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="path"></param>
    /// <param name="currentValue"></param>
    /// <param name="totalValue"></param>
    private void OnProgressHandler(string orderId,string path, float currentValue)
    {
        ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName(orderId);
        if (resLoaderOrder == null) return;

        resLoaderOrder.InvokeLoaderProgress(path, currentValue);
    }

    /// <summary>
    /// 加载完成
    /// </summary>
    /// <param name="resLoader"></param>
    /// <param name="data"></param>
    private void OnCompleteHandler(ResLoader resLoader, WWW data)
    {
        this.LoaderOperater(resLoader, data);
    }

    /// <summary>
    /// 加载错误
    /// </summary>
    /// <param name="resLoader"></param>
    /// <param name="errorText"></param>
    private void OnErrorHandler(ResLoader resLoader, string errorText)
    {
        ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName(resLoader.ownerId);
        if (resLoaderOrder == null) return;

        resLoaderOrder.InvokeLoaderError(resLoader.stResPath.path, errorText);
        this.LoaderOperater(resLoader.ownerId, resLoader.stResPath);
    }

    /// <summary>
    /// 加载队列总进度
    /// </summary>
    private void UpdateListProgressHanler(ResLoaderOrder loaderOrder)
    {
        if (loaderOrder != null)
        {
            loaderOrder.InvokeLoaderListProgress();
        }
    }

    /// <summary>
    /// 空队列
    /// </summary>
    /// <param name="id"></param>
    private void LoaderOperater(string id)
    {
        ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName(id);
        if (resLoaderOrder == null) return;

        this._orderList.Remove(resLoaderOrder);
        resLoaderOrder.InvokeLoaderListComplete();
        resLoaderOrder.Dispose();
        resLoaderOrder = null;
        //next
        LoadThreadManager.instance.CheckQueue();
    }

    /// <summary>
    /// 已经存在资源，或者加载失败后操作
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stPath"></param>
    private void LoaderOperater(string id, StResPath stPath)
	{
        ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName(id);
        if (resLoaderOrder == null) return;
        //刷新队列加载情况
        resLoaderOrder.UpdateLoaderOrderState(stPath);
		//更新当前队列进度
        UpdateListProgressHanler(resLoaderOrder);
        //触发单个文件完成
        resLoaderOrder.InvokeLoaderFileComplete(stPath.path);

        //如果队列完成通知结束
        if (resLoaderOrder.IsFinished())
        {
            this._orderList.Remove(resLoaderOrder);
            resLoaderOrder.InvokeLoaderListComplete();
            resLoaderOrder.Dispose();
            resLoaderOrder = null;
        } 
        //next
        LoadThreadManager.instance.CheckQueue();
	}

    /// <summary>
    /// 加载完成后操作
    /// </summary>
    /// <param name="resLoader"></param>
    /// <param name="www"></param>
    private void LoaderOperater(ResLoader resLoader,WWW www)
    {
        if (resLoader == null) return;

        ResLoaderOrder resLoaderOrder = this.GetWwwLoaderOrderByOrderName(resLoader.ownerId);
        if (resLoaderOrder == null) return;
        bool existsStatus = ResDataManager.instance.ExistWWWData(resLoader.stResPath.path);
        if (!existsStatus)
        {
            VOAssetInfo resInfo = new VOAssetInfo(www, resLoader.stResPath.path,resLoader.stResPath.tag);
            if (www != null)
            {
                ResDataManager.instance.InsertWWWData(resInfo);
            }
            else {
                if (LoadThreadManager.showDebug) Log.info("ResLoaderManager::InsertWWWData www is Null!!");
            }
        }
        //刷新队列加载情况
        resLoaderOrder.UpdateLoaderOrderState(resLoader.stResPath);
        //加载完成，析构resLoader，解除loaderItem监听
        resLoader.UnLoader(this.OnProgressHandler, this.OnCompleteHandler, this.OnErrorHandler, false);
        //更新当前队列进度
        UpdateListProgressHanler(resLoaderOrder);
        //触发单个文件完成
        resLoaderOrder.InvokeLoaderFileComplete(resLoader.stResPath.path);
        //如果队列完成通知结束
        if (resLoaderOrder.IsFinished())
        {
            this._orderList.Remove(resLoaderOrder);
            resLoaderOrder.InvokeLoaderListComplete();
            resLoaderOrder.Dispose();
            resLoaderOrder = null;
        }
        //next
        LoadThreadManager.instance.CheckQueue();
    }
}
