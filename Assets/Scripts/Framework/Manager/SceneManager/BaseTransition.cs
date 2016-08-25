using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Utils.Event;

public enum SMTransitionState
{
    In,
    Out,
    Hold,
}

public class BaseTransition : MonoBehaviour
{

    public delegate void DataParam(int value);
    //进度
    public BaseTransition.DataParam onLoadProcess = null;

    //准备要加载的场景ID
    [HideInInspector]
    public string willLoadSceneID = "";
    //持续时间
    public float duration = 0.1f;
    /// <summary>
    /// 等待进出效果
    /// </summary>
    public static bool isWaitInOut = false;
    //进度
    protected float m_loadProgress = 0;
    //切换状态
    protected SMTransitionState m_state = SMTransitionState.Out;



    void Start()
    {
        Log.info("BaseTransition Start " + willLoadSceneID);

        if (willLoadSceneID == "")
        {
            Log.infoError("[BaseTransition] willLoadSceneID is empty！");
            return;
        }

        //开启携程
        SceneManager.instance.isRunning = true;
        StartCoroutine(doTransition());
    }

    protected virtual IEnumerator doTransition()
    {
        //通知开始加载
        EventManager.Send(SceneEvents.EVENT_SCENE_TRANS_OUT_START, willLoadSceneID);
        m_state = SMTransitionState.Out;

        //------------ state Out ------------
        //设定一个当前经过的时间，该时间会在进度中，不断累加一个帧的时间
        float time = 0f;
        //帧进度
        while (Process(time))
        {
            time += Time.deltaTime;
            //如果time没有到特效的总时间（duration）则返回false
            yield return 0;
        }
        //------------ state Hold -------------
        //用于手动销毁旧场景对象
        EventManager.Send(SceneEvents.EVENT_SCENE_TRANS_LOADING, willLoadSceneID);

        m_loadProgress = 0f;
        m_state = SMTransitionState.Hold;

        //防止销毁自己
        DontDestroyOnLoad(gameObject);

        yield return 0;

        //异步加载关卡场景
        AsyncOperation ao = Application.LoadLevelAsync(willLoadSceneID);
        ao.allowSceneActivation = false;//设置为false 后，进度只会到0.9，ao.isDone一直为false
        while (ao.progress < 0.9f)
        {
            if (onLoadProcess != null) onLoadProcess((int)ao.progress * 100);
            yield return new WaitForEndOfFrame();
        }

        if (onLoadProcess != null) onLoadProcess(100);
        yield return new WaitForEndOfFrame();
        //激活场景
        ao.allowSceneActivation = true;

        //GC
        Log.info("SceneManager 清理闲置资源");
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return 0;

        while (SceneManager.waitForInit)
        {
            yield return 0;
        }

        //------------ state In ------------
        EventManager.Send(SceneEvents.EVENT_SCENE_TRANS_IN_START, willLoadSceneID);

        m_state = SMTransitionState.In;

        time = 0;
        while (Process(time))
        {
            time += Time.deltaTime;
            yield return 0;
        }

        SceneManager.instance.isRunning = false;
        DOTween.Clear(true);
        yield return 0;
        //结束
        SceneManager.instance.currentScene = willLoadSceneID;
        EventManager.Send(SceneEvents.EVENT_SCENE_TRANS_END, willLoadSceneID);
        onLoadProcess = null;
        Destroy(gameObject);
    }

    private int alpha = 0;
    protected bool ProcessIn()
    {
        if (alpha < 20 && isWaitInOut)
        {
            alpha++;
            return true;
        }
        return false;
    }

    protected bool ProcessOut()
    {
        if (alpha > 0 && isWaitInOut)
        {
            alpha--;
            return true;
        }
        isWaitInOut = false;
        return false;
    }

    //更新 ,需重写
    protected virtual bool Process(float elapsedTime) { return false; }
}
