using UnityEngine;
using System.Collections;

/// <summary>
/// 加载路径结构
/// </summary>
public struct StResPath
{
	/// <summary>
	/// 加载路径
	/// </summary>
	public string path;

	/// <summary>
	/// 资源版本号
	/// </summary>
	public int version;
    
    /// <summary>
    /// 标记 
    /// </summary>
    public ResTag tag;

    /// <summary>
    /// 资源路径结构
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resTag"></param>
    /// <param name="version"></param>
    public StResPath(string path, ResTag resTag = ResTag.None, int version = 1)
    {
        this.path = path;
        this.version = version;
        this.tag = resTag;
    }

    public string GetFileName()
    {
        string fileName = "";
        if (path != "")
        {
            fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        return fileName;
    }
}
