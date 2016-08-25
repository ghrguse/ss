using UnityEngine;
using System.Collections;

/// <summary>
/// 单个对象加载器
/// </summary>
public class ResLoader
{
    // 加载进度
    public delegate void DelegateResLoaderProgress(string orderId, string path, float currentValue);
    // 加载完成
    public delegate void DelegateResLoaderComplete(ResLoader resLoader, WWW data);
    // 加载出错
    public delegate void DelegateResLoaderError(ResLoader resLoader, string errorText);


    /// <summary>
    /// 所属的订阅列表id
    /// </summary>
    public string ownerId = "";
    /// <summary>
    /// 资源信息
    /// </summary>
    public StResPath stResPath;
    /// <summary>
    /// 加载对象
    /// </summary>
    public WWW www;



    /// <summary>
    /// 加载状态
    /// </summary>
    private bool _progressStatus = false;
    /// <summary>
    /// 加载进度
    /// </summary>
    private event DelegateResLoaderProgress _loaderProgress;
    /// <summary>
    /// 加载完成
    /// </summary>
    private event DelegateResLoaderComplete _loaderComplete;
    /// <summary>
    /// 加载错误
    /// </summary>
    private event DelegateResLoaderError _loaderError;
	


    /// <summary>
    /// 
    /// </summary>
    /// <param name="ownerId"></param>
    public ResLoader(string ownerId)
    {
        this.ownerId = ownerId;
    }

    /// <summary>
    /// 加载
    /// </summary>
    /// <param name="stPath"></param>
    /// <param name="loaderProgress"></param>
    /// <param name="loaderComplete"></param>
    /// <param name="loaderError"></param>
    public void Loader(StResPath stPath, ResLoader.DelegateResLoaderProgress loaderProgress, ResLoader.DelegateResLoaderComplete loaderComplete, ResLoader.DelegateResLoaderError loaderError)
	{
		this._loaderProgress += loaderProgress;
		this._loaderComplete += loaderComplete;
		this._loaderError += loaderError;

        this.stResPath = stPath;

        //添加到加载队列
        LoadThreadManager.instance.AddNode(this);
    }

    /// <summary>
    /// 触发加载进度
    /// </summary>
    public void OnUpdate()
    {
        if (this.www != null && !this._progressStatus)
        {
            //Debug.Log("size:"+www.size+", loaded:"+www.bytesDownloaded);
            if (this.www.progress >= 1) this._progressStatus = true;
            this.InvokeProgress(this.www.url, this.www.progress);
        }
    }

    /// <summary>
    /// 开启加载
    /// </summary>
    public void StartLoad()
    {
        this._progressStatus = false;
// 一般Unity Web中使用 by piao
//         if (this.stResPath.loaderTypeEnum == ResTypeEnum.UNITY_SCENE)
//         {
//             this.www = WWW.LoadFromCacheOrDownload(this.stResPath.path, this.stResPath.version);
//         }
//         else
//         {
            this.www = new WWW(this.stResPath.path);
//        }
    }

    /// <summary>
    /// 刷新www状态
    /// </summary>
    public void UpdateWWWState()
    {
        if (!string.IsNullOrEmpty(this.www.error))
        {
            LoadThreadManager.instance.OnLoadItemFinish(this);
            if (LoadThreadManager.showDebug) Log.info("加载失败：" + this.www.error);
            this.InvokeError(this.stResPath.path, this.www.error);
        }
        else
        {
            LoadThreadManager.instance.UpadteSpeed(www.size, www.bytesDownloaded);

            if (this.www.isDone)
            {
                LoadThreadManager.instance.OnLoadItemFinish(this);
                this.InvokeComplete(this.www);
            }
        }
    }

	/// <summary>
	/// 销毁对象
	/// </summary>
    public void UnLoader(ResLoader.DelegateResLoaderProgress loaderProgress, ResLoader.DelegateResLoaderComplete loaderComplete, ResLoader.DelegateResLoaderError loaderError, bool destory = true)
	{
        this._loaderProgress -= loaderProgress;
        this._loaderComplete -= loaderComplete;
        this._loaderError -= loaderError;

        // 销毁自己
        if (destory) www.Dispose();
        www = null;
	}

    /// <summary>
    /// 加载进度
    /// 传出 orderId ,path ,currentValue，totalValue
    /// </summary>
    /// <param name="path"></param>
    /// <param name="currentValue"></param>
    /// <param name="totalValue"></param>
    private void InvokeProgress(string path, float currentValue)
    {
        if (this._loaderProgress != null) this._loaderProgress(this.ownerId,path,currentValue);
    }

    /// <summary>
    /// 加载错误
    /// 传出 orderId ,stPath ,errorText
    /// </summary>
    /// <param name="path"></param>
    /// <param name="errorText"></param>
    private void InvokeError(string path, string errorText)
    {
        if (this._loaderError != null) this._loaderError(this, errorText);
    }

    /// <summary>
    /// 加载完成
    /// 传出 orderId , stPath , www
    /// </summary>
    /// <param name="data"></param>
    private void InvokeComplete(WWW data)
    {
        if (this._loaderComplete != null) this._loaderComplete(this, data);
    }

    public override string ToString()
    {
        return "ResLoader："+this.stResPath.path;
    }
}
