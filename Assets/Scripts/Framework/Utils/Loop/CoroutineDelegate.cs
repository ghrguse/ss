using UnityEngine;
using System.Collections;

/// <summary>
/// 携程代理工具
/// </summary>
public class CoroutineDelegate : MonoBehaviour
{
	// 单例
	private static CoroutineDelegate instance;

	void Awake()
	{
		instance = this;
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public static void Init(Transform parent)
	{
		GameObject go = new GameObject();
		go.transform.parent = parent;
		go.name = "CoroutineDelegate";
		go.AddComponent<CoroutineDelegate>();
	}

	/// <summary>
	/// 启动协程
	/// </summary>
	public static void StartCoroutines(IEnumerator fun)
	{
		if (instance == null)
			return;

		instance.StartCoroutine(fun);
	}


	/// <summary>
	/// 关闭协程
	/// </summary>
	public static void StopCoroutines(IEnumerator fun)
	{
		if (instance == null)
			return;

		instance.StopCoroutine(fun);
	}
}
