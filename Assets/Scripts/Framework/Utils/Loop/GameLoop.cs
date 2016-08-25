using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLoop : MonoBehaviour {

	// Use this for initialization
    private static List<DelegateEnums.NoneParam> _frameProcessList;
    private static List<DelegateEnums.NoneParam> _timerProcessList;
    private static List<DelegateEnums.NoneParam> _waitDelFrameList;
    private static List<DelegateEnums.NoneParam> _waitDelTimerList;
    private static bool _isUpdate = false;
	private static float lastTime = 0f;

    public static void Init(Transform root)
    {
        GameObject obj = new GameObject();
		obj.AddComponent<GameLoop>();
        obj.name = "GameLoop";
        obj.transform.parent = root;


        _frameProcessList = new List<DelegateEnums.NoneParam>();
        _timerProcessList = new List<DelegateEnums.NoneParam>();
        _waitDelFrameList = new List<DelegateEnums.NoneParam>();
        _waitDelTimerList = new List<DelegateEnums.NoneParam>();
    }

    public static void EnableFrameLoop(bool enable)
    {
        _isUpdate = enable;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isUpdate) return;

        int count = 0;
        int l = 0;
        //Timer
        if (Time.time - lastTime > 1f)
        {
            lastTime = Time.time;

            //clear
            l = _waitDelTimerList.Count;
            for (int j = 0; j < l; j++)
            {
                DelegateEnums.NoneParam fun = _waitDelTimerList[j];
                if (fun != null && _timerProcessList.Contains(fun)) _timerProcessList.Remove(fun);
            }
            _waitDelTimerList.Clear();

            //update
            count  = _timerProcessList.Count;
            for (int i = 0; i < count; i++)
            {
                DelegateEnums.NoneParam fun = _timerProcessList[i];
                if (fun != null)
                {
                    fun();
                }
            }
        }//end-Timer



        //clear
        l = _waitDelFrameList.Count;
        for (int j = 0; j < l; j++)
        {
            DelegateEnums.NoneParam fun = _waitDelFrameList[j];
            if (fun != null && _frameProcessList.Contains(fun)) _frameProcessList.Remove(fun);
        }
        _waitDelFrameList.Clear();

        //update
        count = _frameProcessList.Count;
        for (int i = 0; i < count; i++)
        {
            DelegateEnums.NoneParam fun = _frameProcessList[i];
            if (fun != null)
            {
                fun();
            }
        }
	}
	
	void OnDestroy ()
	{

	}

    public static void AddFrameProcessList(DelegateEnums.NoneParam fun)
    {
        if (fun != null && !_frameProcessList.Contains(fun)) _frameProcessList.Add(fun);
    }

    public static void RemoveFrameProcess(DelegateEnums.NoneParam fun)
    {
        if (fun != null && !_waitDelFrameList.Contains(fun)) _waitDelFrameList.Add(fun);
    }

    public static void AddTimerProcessList(DelegateEnums.NoneParam fun)
    {
        if (fun != null && !_timerProcessList.Contains(fun)) _timerProcessList.Add(fun);
    }

    public static void RemoveTimerProcessList(DelegateEnums.NoneParam fun)
    {
        if (fun != null && !_waitDelTimerList.Contains(fun)) _waitDelTimerList.Add(fun);
    }
}
