/*
 * 数据 接口
 * piaochen
 * 2014-11-16
 */
using UnityEngine;
using System.Collections;

public interface IData : IModuleCenter
{
	/// <summary>
	/// Dispose this instance.
	/// </summary>
	void Dispose();
    /// <summary>
    /// 清理数据
    /// </summary>
    void Clear();
}