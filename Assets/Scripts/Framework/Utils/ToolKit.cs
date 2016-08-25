using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ToolKit : MonoBehaviour
{

    private static bool inited = false;
    private static DelayManager dm;

    public static void init(Transform root)
    {
        if (!inited)
        {
            GameObject dmObj = new GameObject();
            dmObj.name = "DelayManager";
            dmObj.transform.parent = root;
            GameObject.DontDestroyOnLoad(dmObj);
            dm = dmObj.AddComponent<DelayManager>();
            inited = true;
        }

    }
    //public static ToolKit getInstence()
    //{
    //    if (instence == null)
    //    {
    //        GameObject go = new GameObject();
    //        go.name = "ToolKit";
    //        instence = go.AddComponent<ToolKit>();

    //        DontDestroyOnLoad(go);

    //        //GameObject tmObj = new GameObject();
    //        //tmObj.name = "TimeMachine";
    //        //tmObj.transform.parent = go.transform;
    //        //instence.tm = tmObj.AddComponent<TimeMachine>();

    //    }
    //    return instence;

    //}
    public static GameObject getKid(string name, GameObject parent)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    //移出所有子对象
    public static void RemoveAllChild(Transform transform)
    {
        while (0 < transform.childCount)
        {
            GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    //获得一个组件，包括deactive的
    public static T GetComponentInChildren<T>(Transform parent) where T : Component
    {
        T component = GetComponent<T>(parent);
        return component;
    }
    //遍历所有子对象
    public static T GetComponent<T>(Transform transfrom) where T : Component
    {
        T component = transfrom.GetComponent<T>();
        if (null != component)
        {
            return component;
        }
        foreach (Transform child in transfrom)
        {
            return GetComponent<T>(child);
        }
        return null;
    }

    //获得所有组件，包括deactive的
    public static List<T> GetComponentsInChildren<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        GetComponent<T>(parent, ref components);
        return components;
    }

    //遍历所有子对象
    private static void GetComponent<T>(Transform transfrom, ref List<T> list) where T : Component
    {
        T component = transfrom.GetComponent<T>();
        if (null != component)
        {
            list.Add(component);
        }
        foreach (Transform child in transfrom)
        {
            GetComponent<T>(child, ref list);
        }
    }

    public static string delayCall(float t, DelegateEnums.NoneParam fn, string ID = "")
    {
        // return getInstence().tm.addDelay(t, fn, ID);
        return dm.addDelay(t, fn, ID);
    }
    public static string delayCall(float t, DelegateEnums.DataParam fn, object data, string ID = "")
    {
        // return getInstence().tm.addDelay(t, fn, data, ID);
        return dm.addDelay(t, fn, data, ID);
    }

    //暂停一个延迟
    public static void pauseDelay(string ID)
    {
        dm.pauseDelay(ID);
    }
    //继续一个延迟
    public static void continuePlay(string ID)
    {
        dm.continuePlay(ID);
    }
    public static void stopDelayCall(string id)
    {
        //if (instence != null)
        //{
        //    getInstence().tm.stopDelay(id);
        //}
        if (dm != null)
        {
            dm.stopDelay(id);
        }
    }

    //设置基本属性
    public static void initUIAttr(GameObject ui, GameObject parent = null)
    {
        if (parent)
        {
            ui.transform.parent = parent.transform;
        }
        //初始化属性
        ui.gameObject.transform.localPosition = new Vector3(0, 0, 0);
        ui.gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    //协程做延迟
    public static void coroutineDelay(float t, DelegateEnums.NoneParam fn)
    {
        dm.addDelay(t, fn);
        //getInstence().StartCoroutine(instence.delayExec(t, fn));
    }
    //执行
    //private IEnumerator delayExec(float t, DelegateEnums.NoneParam fn)
    //{
    //    yield return new WaitForSeconds(t);
    //    fn();
    //}

    //设置label 的文本内容
    //public static void SetLabel(GameObject go, string content)
    //{
    //    UILabel tmpLabel = go.GetComponent<UILabel>();
    //    if (tmpLabel)
    //    {
    //        tmpLabel.text = content;
    //    }
    //}

    public static string GetColorStringByColor(Color tmpColor)
    {
        int rInt = (int)(tmpColor.r * 255.0f);
        int gInt = (int)(tmpColor.g * 255.0f);
        int bInt = (int)(tmpColor.b * 255.0f);

        string red = Convert.ToString(rInt, 16);
        string green = Convert.ToString(gInt, 16);
        string blue = Convert.ToString(bInt, 16);
        if (red.Length == 1)
        {
            red = "0" + red;
        }
        if (green.Length == 1)
        {
            green = "0" + green;
        }
        if (blue.Length == 1)
        {
            blue = "0" + blue;
        }

        string colorStr = "[" + red + green + blue + "ff]";

        return colorStr;
    }

    public static int toInt(string str)
    {
        return Convert.ToInt32(str);
    }
    public static ushort toShort(string str)
    {
        return Convert.ToUInt16(str);
    }

    public static void modifyRenderQ(GameObject go, int queue)
    {
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            // 默认UI为3000，略高于UI
            renderer.material.renderQueue = queue;
        }
    }

    public static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }

    public static void setSkinMeshRenderLayer(GameObject go, int layer)
    {
        SkinnedMeshRenderer[] skinnes = go.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0, len = skinnes.Length; i < len; i++)
        {
            SkinnedMeshRenderer t = skinnes[i];
            t.gameObject.layer = layer;
        }

        MeshRenderer[] skinnesMesh = go.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0, len = skinnesMesh.Length; i < len; i++)
        {
            MeshRenderer t = skinnesMesh[i];
            t.gameObject.layer = layer;
        }
    }

    public static GameObject GetChildByName(string name, GameObject _parent)
    {
        MeshRenderer[] list = _parent.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer t in list)
        {
            if (t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    public static void DestroyComponent<T>(GameObject go, bool needDestroyGo = false) where T : Component
    {
        T t = go.GetComponent<T>();
        if (t != null)
        {
            if (true == needDestroyGo)
            {
                Destroy(t.gameObject);
            }
            else
            {
                DestroyImmediate(t);
            }

        }
    }

    public static void DestroyChildrenComponent<T>(GameObject go, bool needDestroyGo = false) where T : Component
    {
        T t = go.GetComponentInChildren<T>();
        if (t != null)
        {
            if (true == needDestroyGo)
            {
                Destroy(t.gameObject);
            }
            else
            {
                Destroy(t);
            }

        }
    }

    //移除 内存里的资源
    public static void UnloadAssetObj(GameObject clearObj)
    {
        //foreach(Transform child in targetObj.transform)
        if (null == clearObj)
        {
            return;
        }
        UnloadAsset(clearObj);
        for (int i = 0, len = clearObj.transform.childCount; i < len; i++)
        {
            Transform child = clearObj.transform.GetChild(i);
            //UnloadAsset(child.gameObject);
            UnloadAssetObj(child.gameObject);
        }
    }
    public static void UnloadAsset(GameObject obj)
    {
        SkinnedMeshRenderer[] list = obj.GetComponents<SkinnedMeshRenderer>();
        for (int i = 0, len1 = list.Length; i < len1; i++)
        {
            Resources.UnloadAsset(list[i].sharedMesh);

            Material[] mList = list[i].materials;
            for (int j = 0, len = mList.Length; j < len; ++j)
            {
                //Resources.UnloadAsset(mList[j]);
                //Log.info(mList[j].mainTexture.name);
                Resources.UnloadAsset(mList[j].mainTexture);

                Texture tmp2D = null;
#if UNITY_EDITOR

                tmp2D = mList[j].GetTexture("_RampTex");
                if (null != tmp2D)
                {
                    Resources.UnloadAsset(tmp2D);
                }


                tmp2D = mList[j].GetTexture("SpecularTexture");
                if (null != tmp2D)
                {
                    Resources.UnloadAsset(tmp2D);
                }

#else 
				if(mList[j].HasProperty("_RampTex"))
				{
					tmp2D = mList[j].GetTexture("_RampTex");
					if (null != tmp2D)
					{
						Resources.UnloadAsset(tmp2D);
	                }
                }
                if(mList[j].HasProperty("SpecularTexture"))
                {
	                tmp2D = mList[j].GetTexture("SpecularTexture");
	                if (null != tmp2D)
	                {
	                    Resources.UnloadAsset(tmp2D);
	                }
                }
#endif
            }
        }
        MeshRenderer[] list2 = obj.GetComponents<MeshRenderer>();
        for (int i = 0; i < list2.Length; i++)
        {
            Material[] mList = list2[i].materials;
            for (int j = 0, len = mList.Length; j < len; ++j)
            {
                //Resources.UnloadAsset(mList[j]);
                //Log.info(mList[j].mainTexture.name);
                Resources.UnloadAsset(mList[j].mainTexture);
            }
        }
        MeshFilter[] meshFilterList = obj.GetComponents<MeshFilter>();

        for (int i = 0, len1 = meshFilterList.Length; i < len1; i++)
        {
            Resources.UnloadAsset(meshFilterList[i].sharedMesh);
        }
        //特效
        ParticleSystem[] particles = obj.GetComponents<ParticleSystem>();

        for (int i = 0, len1 = particles.Length; i < len1; i++)
        {
            Material[] mList = particles[i].GetComponent<Renderer>().materials;
            for (int j = 0, len = mList.Length; j < len; ++j)
            {
                //Resources.UnloadAsset(mList[j]);
                //Log.info(mList[j].mainTexture.name);
                Resources.UnloadAsset(mList[j].mainTexture);
            }
        }

        Animation[] animations = obj.GetComponents<Animation>();

        for (int i = 0, len1 = animations.Length; i < len1; i++)
        {
            foreach (AnimationState state in animations[i])
            {
                Resources.UnloadAsset(state.clip);
            }

        }
    }



    public static void ResetTransformValues(Transform to)
    {

        to.localScale = new Vector3(1, 1, 1);
        to.localRotation = Quaternion.identity;
        to.localPosition = new Vector3(0, 0, 0);

    }


    public static void CopyTransformValues(Transform from, Transform to)
    {
        //to.parent = from.parent;
        to.localScale = new Vector3(from.localScale.x, from.localScale.y, from.localScale.z);
        to.localRotation = Quaternion.Euler(from.localRotation.eulerAngles.x, from.localRotation.eulerAngles.y, from.localRotation.eulerAngles.z);
        to.localPosition = new Vector3(from.localPosition.x, from.localPosition.y, from.localPosition.z);

    }
    /// <summary>
    /// 遍历查找制定名称的孩子 只找一层
    /// </summary>
    public static GameObject SimpleFindChild(GameObject parent, string name)
    {
        Transform trans = parent.transform;
        GameObject child = null;

        for (int i = 0; i < trans.childCount; i++)
        {
            child = trans.GetChild(i).gameObject;

            if (child.name == name)
                return child;
        }
        return null;

    }

    /// <summary>
    /// 遍历查找制定名称的孩子
    /// </summary>
    public static GameObject FindChildDeep(GameObject parent, string name)
    {
        Transform trans = parent.transform;
        GameObject result = null;

        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject child = trans.GetChild(i).gameObject;

            if (child.name == name)
                return child;
        }

        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject child = trans.GetChild(i).gameObject;
            result = FindChildDeep(child, name);
            if (result != null)
                return result;
        }

        return null;
    }

    public static long GetRandomValueBySeed(long seed)
    {
        return (seed * 1103515245L + 12345L) & 0x7fffffff;
    }
    /// <summary>
    /// 获取用于TDR字符串长度，utf-8下，一个中文占3个长度
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int GetStringTdrLength(string str)
    {
        string temp = str;
        int j = 0;
        for (int i = 0; i < temp.Length; i++)
        {
            if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
            {
                j += 3;
            }
            else
            {
                j += 1;
            }
        }
        return j;
    }
    public static int GetStringLength(string str)
    {
        string temp = str;
        int j = 0;
        for (int i = 0; i < temp.Length; i++)
        {
            if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
            {
                j += 2;
            }
            else
            {
                j += 1;
            }
        }
        return j;
    }

    public static void SetButtonState(GameObject button, bool enable)
    {
        if (null == button)
        {
            return;
        }
        BoxCollider box = button.GetComponent<BoxCollider>();
        if (null == box)
        {
            return;
        }
        box.enabled = enable;
    }

    /// <summary>
    /// 获取粒子播放的时长
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static float ParticleSystemLength(Transform transform)
    {
        ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
        float maxDuration = 0;
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.enableEmission)
            {
                if (ps.loop)
                {
                    return -1f;
                }
                float dunration = 0f;
                if (ps.emissionRate <= 0)
                {
                    dunration = ps.startDelay + ps.startLifetime;
                }
                else
                {
                    dunration = ps.startDelay + Mathf.Max(ps.duration, ps.startLifetime);
                }
                if (dunration > maxDuration)
                {
                    maxDuration = dunration;
                }
            }
        }
        return maxDuration;
    }

    /// <summary>
    /// 屏蔽，开启 NGUI的输入接口
    /// </summary>
    /// <param name="value"></param>
    //public static void InputEnabel(bool value)
    //{
    //    //启动界面屏蔽鼠标，手势触摸
    //    UICamera uiCamera = UIRoot.FindObjectOfType<UICamera>();
    //    if (uiCamera != null)
    //    {
    //        uiCamera.useMouse = value;
    //        uiCamera.useTouch = value;
    //        uiCamera.useKeyboard = value;
    //    }
    //}

    /// <summary>
    /// 图片设置正常颜色or灰度
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="enable"></param>
    //public static void SetUITextureColorEnable(UITexture tex, bool enable)
    //{
    //    if (tex == null) return;

    //    Shader shader = null;
    //    if (enable == true)
    //    {
    //        tex.color = Color.white;
    //        shader = Shader.Find("Sprites/TextureCompress_fore");
    //    }
    //    else
    //    {
    //        tex.color = Color.black;
    //        shader = Shader.Find("Unlit/Transparent Colored Gray");
    //    }
        
    //    if (tex.shader != shader)
    //    {
    //        tex.shader = shader;
    //    }
    //}

	/// <summary>
	/// 挂在光效
	/// </summary>
	public static void AddEffect(string effectName, Transform parent)
	{
		string path = "Effects/UIEffects/" + effectName;
//#if UNITY_EDITOR	
		GameObject go = ResDataManager.instance.GetObjectFromLocalPrefab(path);
		if (go == null)
			return;
		
		GameObject effect = GameObject.Instantiate(go) as GameObject;
		effect.transform.parent = parent;
		effect.transform.localPosition = Vector3.zero;

//#else
//		ResLoadTool.Load<GameObject>(new StResPath(path), (listName, param) => {
//			if (parent == null)
//				return;
//			
//			GameObject go = ResDataManager.instance.CreateObjectFromCache<GameObject>(listName);
//			if (go == null)
//				return;
//			
//			GameObject effect = GameObject.Instantiate(go) as GameObject;
//			go.transform.parent = parent;
//		});
//#endif

	}
}