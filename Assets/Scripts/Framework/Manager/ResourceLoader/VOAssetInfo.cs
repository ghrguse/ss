using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 加载完成资源存储结构
/// 用于存储 www 资源
/// </summary>
public class VOAssetInfo
{
	/// <summary>
	/// 加载路径
	/// </summary>
	public string path;
	/// <summary>
	/// 加载类型枚举
	/// </summary>
	//public ResTypeEnum loaderTypeEnum;
    /// <summary>
    /// 场景类型：比如战斗中等
    /// </summary>
    public ResTag tag;
	/// <summary>
	/// 加载完成之后的数据
	/// </summary>
    public WWW www;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="www"></param>
    /// <param name="path"></param>
    /// <param name="resTag"></param>
    public VOAssetInfo(WWW www, string path, ResTag resTag)
    {
        this.path = path;
        this.tag = resTag;
        this.www = www;
    }

    public void Dispose()
    {
        if (LoadThreadManager.showDebug)  Log.info("VOAssetInfo :: Dispose! " + path);
        path = "";
        tag = 0;

        if (this.www != null)
        {
            
            if (this.www.assetBundle != null)
            {
                this.www.assetBundle.Unload(false);
            }
            this.www.Dispose();
            this.www = null;
            if (LoadThreadManager.showDebug)  Log.info("VOAssetInfo :: www Dispose! ");
        }
    }
}
