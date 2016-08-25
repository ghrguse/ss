/*
 * Server 接口
 * piaochen
 * 2014-11-16
 */
using UnityEngine;
using System.Collections;

public interface IServer : IModuleCenter
{
	/// <summary>
	/// Dispose this instance.
	/// </summary>
	void Dispose();
}