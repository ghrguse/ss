using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Utils.Event;

public enum SceneTransEffect
{
    None,
    Fade,//淡入淡出
    //    Common,//通用全屏加载界面，非战斗
}

public enum SceneResut
{
    Error,//报错
    Close,//该模块未开启
    Open//该模块开启
}


public class SceneManager
{

    //等待初始化
    public static bool waitForInit = false;
    //将要下载的场景
    public static string willLoadScene = "";
    //Loading场景背景图id
    public static int LoadingBgID = 1;

    //单例
    private static SceneManager _instance = null;
    public bool isRunning = false;
    private string _current = "";
    private bool _isBattleScene = false;

    //获取单例
    public static SceneManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SceneManager();
                _instance.init();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 当前场景
    /// </summary>
    public string currentScene
    {
        get { return _current; }
        set { _current = value; }
    }

    /// <summary>
    /// 用户判断当前场景
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public bool isInScene(string scene)
    {
        return scene == _current;
    }

    //切换场景
    public SceneResut goToScene(string sceneID, SceneTransEffect type, float duration = 0)
    {
        Log.info("当前scene"+currentScene+" goTo :"+sceneID);
        //CLogSys.Log(ELogLevel.Verbose, ELogTag.Map, ("goToScene " + sceneID));

        if (ModuleManager.IsOpen(sceneID) == false)
        {
            return SceneResut.Close;
        }

        if (currentScene == sceneID)
        {
            //CLogSys.Log(ELogLevel.Verbose, ELogTag.Map, "相同场景，不做切换 .." + sceneID);
            return SceneResut.Error;
        }

        if (isRunning == true)
        {
            //CLogSys.Log(ELogLevel.Verbose, ELogTag.Map, "GoToScene isRunning...");
            return SceneResut.Error;
        }

        DOTween.PauseAll();
        //如果是需要过渡场景，此字段用于存储过渡场景切换用的目标值
        willLoadScene = sceneID;

        string path = "";
        switch (type)
        {
            //执行场景切换
            case SceneTransEffect.None:
                path = "SceneTrans_None";
                break;
            //             case SceneTransEffect.Common:
            //                 sceneID = "CommonLoad";
            //                 path = "SceneTrans_None";
            //                 break;
            case SceneTransEffect.Fade:
                path = "SceneTrans_Fade";
                break;
        }

        GameObject go = ResDataManager.instance.GetObjectFromLocalPrefab("GUI/_Common/" + path);
        if (go == null)
        {
            Log.infoError("[SceneManager]预置文件无效:" + path);
            return SceneResut.Error;
        }
        GameObject prefab = GameObject.Instantiate(go) as GameObject;

        BaseTransition effect = prefab.GetComponent<BaseTransition>();
        effect.willLoadSceneID = sceneID;
        effect.duration = duration;
        effect.enabled = true;
        effect.onLoadProcess = OnLoadProcess;
        return SceneResut.Open;
    }

    //初始化
    private void init()
    {
        EventManager.AddEventListener(SceneEvents.EVENT_SCENE_INIT_READY, onSceneInitReady);
    }

    //载入结束
    private void onSceneInitReady(GEvent evt)
    {
        waitForInit = false;
    }

    private void OnLoadProcess(int value)
    {
        EventManager.Send(SceneEvents.EVENT_SCENE_PROGRESS, value);
    }
}
