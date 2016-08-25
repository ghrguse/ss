using UnityEngine;
using System.Collections;

public class ComponentsInit : MonoBehaviour {

	public static bool inited = false;
	public static GameObject root;

	void Awake () {
		if(inited == false)
		{
			init();
			inited= true;
		}
	}
	
	private static void init()
	{
		root = new GameObject ();
		root.name = "_Components";
		GameObject.DontDestroyOnLoad (root);

        ToolKit.init(ComponentsInit.root.transform);
	}
}
