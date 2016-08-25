/*
 * 视图借口
 * piaochen
 * 2014-11-16
 */
using UnityEngine;
using System.Collections;

public interface IView
{
	/// <summary>
	/// Dispose this instance.
	/// </summary>
	void Dispose();
    /// <summary>
    /// 清理
    /// </summary>
    void Clear();
    /// <summary>
    /// 视图名称
    /// </summary>
    string GetName();
    /// <summary>
    /// 是否开启检测生命周期
    /// </summary>
    /// <returns></returns>
    bool IsCheckLife();
    /// <summary>
    /// 初始化
    /// </summary>
    void Init();
}
