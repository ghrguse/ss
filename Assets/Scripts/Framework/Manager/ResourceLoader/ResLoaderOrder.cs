using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 加载订阅管理
/// 主要负责事件的管理
/// </summary>
public class ResLoaderOrder
{
    // 加载文件进度委托
    public ResLoaderManager.DelegateLoaderProgress OnLoaderFileProgress;
    // 加载队列进度委托
    public ResLoaderManager.DelageteLoaderListProgress OnLoaderListProgress;
    // 加载完成委托
    public ResLoaderManager.DelegateListLoaded OnLoaderListComplete;
    //加载失败委托
    public ResLoaderManager.DelegateLoaderError OnLoaderFileError;
    // 单个加载完毕
    public ResLoaderManager.DelegateFileLoaded OnLoaderFileComplete;

	/// <summary>
	/// id
	/// </summary>
	public string Id;
	/// <summary>
	/// 路径列表
	/// </summary>
	public IList<StResPath> pathList;
    /// <summary>
    /// 初始加载总数
    /// </summary>
    public int total = 0;
    /// <summary>
    /// 当前加载完成的个数
    /// </summary>
    public int currLoadedNum = 0;
    /// <summary>
    /// 附带参数
    /// </summary>
    private object param = null;


	/// <summary>
    /// 构造函数
	/// </summary>
	/// <param name="orderName"></param>
	/// <param name="pathList"></param>
	/// <param name="loaderProgress"></param>
	/// <param name="loaderComplete"></param>
	/// <param name="loaderError"></param>
	/// <param name="loaderListProgress"></param>
    /// <param name="loaderFileComplete"></param>
	/// <param name="param"></param>
    public ResLoaderOrder(
        string orderName, 
        IList<StResPath> pathList, 
        ResLoaderManager.DelegateLoaderProgress loaderProgress, 
        ResLoaderManager.DelegateListLoaded loaderListComplete, 
        ResLoaderManager.DelegateLoaderError loaderError, 
        ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null,
        ResLoaderManager.DelegateFileLoaded loaderFileComplete = null,
        object param = null)
	{
		this.Id = orderName;
        this.pathList = new List<StResPath>();
        this.ClearRepeatItem(pathList);
        this.total = this.pathList.Count;
        this.param = param;
        this.AttachEvent(loaderListComplete,loaderProgress, loaderError,loaderListProgress,loaderFileComplete);
	}

    /// <summary>
    /// 销毁
    /// </summary>
    public void Dispose()
    {
        this.pathList.Clear();
        this.param = null;
        this.OnLoaderListComplete = null;
        this.OnLoaderFileError = null;
        this.OnLoaderListProgress = null;
        this.OnLoaderFileProgress = null;
        this.OnLoaderFileComplete = null;
    }

    /// <summary>
    /// 更新队列的加载情况
    /// </summary>
    /// <param name="stPath"></param>
    /// <returns></returns>
    public bool UpdateLoaderOrderState(StResPath stPath)
    {
        if (this.pathList.Contains(stPath))
        {
            this.currLoadedNum++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 队列是否完成加载
    /// </summary>
    /// <returns></returns>
    public bool IsFinished()
    {
        return (this.currLoadedNum == this.total);
    }

    /// <summary>
    /// 触发进度委托函数
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="fileProgressValue">file progress value.</param>
    public void InvokeLoaderProgress(string path, float fileProgressValue)
    {
        if (this.OnLoaderFileProgress != null) this.OnLoaderFileProgress(path, fileProgressValue);
    }

    /// <summary>
    /// 触发队列进度委托函数
    /// </summary>
    public void InvokeLoaderListProgress()
    {
        if (this.OnLoaderListProgress != null) this.OnLoaderListProgress(this.Id, this.currLoadedNum, this.total);
    }

    /// <summary>
    /// 触发单个文件加载完成委托函数
    /// </summary>
    public void InvokeLoaderFileComplete(string filePath)
    {
        if (this.OnLoaderFileComplete != null) this.OnLoaderFileComplete(filePath, this.param);
    }
    
    /// <summary>
    /// 触发队列完成委托函数
    /// </summary>
    public void InvokeLoaderListComplete()
    {
        if (this.OnLoaderListComplete != null) this.OnLoaderListComplete(this.Id,this.param);
    }

    /// <summary>
    /// 出发失败委托函数
    /// </summary>
    /// <param name="path"></param>
    public void InvokeLoaderError(string path, string errorText)
    {
        if (this.OnLoaderFileError != null) this.OnLoaderFileError(path, errorText);
    }

	/// <summary>
    /// 添加事件
	/// </summary>
	/// <param name="loaderComplete"></param>
	/// <param name="loaderProgress"></param>
	/// <param name="loaderError"></param>
	/// <param name="loaderListProgress"></param>
	/// <param name="loaderFileComplete"></param>
    public void AttachEvent(ResLoaderManager.DelegateListLoaded loaderComplete, ResLoaderManager.DelegateLoaderProgress loaderProgress, ResLoaderManager.DelegateLoaderError loaderError, ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null, ResLoaderManager.DelegateFileLoaded loaderFileComplete=null)
	{
		if (loaderProgress != null) this.OnLoaderFileProgress += loaderProgress;
		if (loaderComplete != null) this.OnLoaderListComplete += loaderComplete;
        if (loaderError != null) this.OnLoaderFileError += loaderError;
        if (loaderListProgress != null) this.OnLoaderListProgress += loaderListProgress;
        if (loaderFileComplete != null) this.OnLoaderFileComplete += loaderFileComplete;
	}
	
	/// <summary>
    /// 移除事件
	/// </summary>
	/// <param name="loaderComplete"></param>
	/// <param name="loaderProgress"></param>
	/// <param name="loaderError"></param>
	/// <param name="loaderListProgress"></param>
	/// <param name="loaderFileComplete"></param>
    public void RemoveEvent(ResLoaderManager.DelegateListLoaded loaderComplete, ResLoaderManager.DelegateLoaderProgress loaderProgress, ResLoaderManager.DelegateLoaderError loaderError, ResLoaderManager.DelageteLoaderListProgress loaderListProgress = null, ResLoaderManager.DelegateFileLoaded loaderFileComplete=null)
	{
		if (loaderProgress != null) this.OnLoaderFileProgress -= loaderProgress;
		if (loaderComplete != null) this.OnLoaderListComplete -= loaderComplete;
        if (loaderError != null) this.OnLoaderFileError -= loaderError;
        if (loaderListProgress != null) this.OnLoaderListProgress -= loaderListProgress;
        if (loaderFileComplete != null) this.OnLoaderFileComplete -= loaderFileComplete;
	}

    /// <summary>
    /// 清理相同资源
    /// </summary>
    /// <param name="pathList"></param>
    private void ClearRepeatItem(IList<StResPath> pathList)
    {
        Dictionary<StResPath,bool> tmp = new Dictionary<StResPath,bool>();

        int len = pathList.Count;
        for (int i = 0; i < len; i++)
        {
            if (tmp.ContainsKey(pathList[i]))
            {
                continue;
            }
            else {
                tmp[pathList[i]] = true;
                this.pathList.Add(pathList[i]);
            }
        }
        tmp.Clear();
        tmp = null;
    }

    public override string ToString()
    {
        return "List:" + Id + " total:" + this.total + " , loaded:" + this.currLoadedNum;
    }
}
