using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utils.Event;
using DG.Tweening;


public struct StStageData
{
    /// <summary>
    /// 是否是主界面
    /// </summary>
    public bool isMainStage;
    /// <summary>
    /// 被记录下来的Stage
    /// </summary>
    public Type stage;
    /// <summary>
    /// 简单的存储数据。比如：切到当前Stage子页面，可以通过此数据，可以返回时候跳到子页面
    /// </summary>
    public int data;

    public StStageData(Type stage, int data = 0, bool isMainStage = false)
    {
        this.isMainStage = false;
        this.stage = stage;
        this.data = 0;
    }
}



public class StageManager
{
    public static DelegateEnums.NoneParam OnRunToDefaultStageCallBack = null;
    /// <summary>
    /// 场景摄像头
    /// </summary>
    public static GameObject mainCamera = null;
    /// <summary>
    /// Stage集合
    /// </summary>
    public static Dictionary<string, AbstractStage> StageDict = new Dictionary<string, AbstractStage>();

    //舞台NGUI根节点
    public static Canvas root;

    //当前场景
    public static AbstractStage nowStage = null;
    //下一个场景
    public static AbstractStage nextStage = null;
    /// <summary>
    /// 当前是否是主场景
    /// </summary>
    public static bool nowIsMainStage = false;

    //打开的窗体历史记录
    private static Stack<StStageData> _stageHistory = new Stack<StStageData>();

    //是前进还是后退，调用goToPreviousStage都是true
    private static bool backFlag = false;
    
    private static bool _inited = false;
    private static bool _isNeedAni = true;


    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (!_inited)
        {
            _inited = true;
            Log.info("UIStageManager 场景管理器初始化");
            ReBindRoot();
            EventManager.AddEventListener(SceneEvents.EVENT_SCENE_TRANS_LOADING, OnSceneChange);
        }
    }

    /// <summary>
    /// 清除所有场景
    /// </summary>
    public static void ClearAllStage()
    {
        if (nowStage != null)
        {
            nowStage.Dispose();
            nowStage = null;
        }

        if (nextStage != null)
        {
            nextStage = null;
        }

        //DOTween.KillAll();
        mainCamera = null;
        _stageHistory.Clear();
        root = null;
        //previousStage = null;

        Log.info("[StageManager] clearAllStage!");
        //TODO 清理缓存
    }

    public static bool CheckIsStage<T>() where T : AbstractStage
    {
        if (StageManager.nowStage == null) return false;
        Type type = StageManager.nowStage.GetType();
        return type.Name == typeof(T).ToString();
    }

    /// <summary>
    /// 场景切换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isNeedAni"></param>
    public static void GotoUIStage<T>(bool isNeedAni = true) where T : AbstractStage
    {
        if (root == null) ReBindRoot();

        //检测是否在切换中
        if (nextStage != null)
        {
            Log.info("场景切换中，禁止重复操作");
            return;
        }
        //检测是否是开启的场景
        string stageName = typeof(T).ToString();
        if (ModuleManager.IsOpen(stageName) == false)
        {
            Log.info("Stage:" + stageName + " 未开启！");
            return;
        }

        AbstractStage stg = null;
        //获取Stage
        CreateStage(typeof(T), ref stg);
        if (stg == null) return;
        //进入
        ExecToStage(stg, true); 
    }

    /// <summary>
    /// 返回上一个场景
    /// </summary>
    public static void GotoPrvUIStage()
    {
        backFlag = true;
        if (_stageHistory.Count == 0)
        {
            if (OnRunToDefaultStageCallBack != null) OnRunToDefaultStageCallBack();
        }
        else
        {
            StStageData stageData = _stageHistory.Pop(); 
            AbstractStage stg = null;
            CreateStage(stageData.stage, ref stg);
            if (stg == null) return;
            ExecToStage(stg, true);  
        }
    }

    /// <summary>
    /// 离开当前场景时，push场景记录
    /// </summary>
    /// <param name="stg"></param>
    /// <param name="data"></param>
    public static void LeaveCurrStageOver(AbstractStage stg, StStageData stageData)
    {
        if (stg == nowStage)
        {
            if (!backFlag)
            {
                stageData.stage = nowStage.GetType();
                _stageHistory.Push(stageData);
                //Debug.Log("push :"+stageData.stage.ToString());
            }
            //Log.info("[StageManager] 离开场景:" + stg.getViewName());
            nextStage.RecoverStagePart(nowStage);

            //下个场景进场
            nowStage = nextStage;
            nextStage.Show();
            backFlag = false;
        }
    }

    /// <summary>
    /// 场景切换结束
    /// </summary>
    public static void StageChangeOver()
    {
        nextStage = null;
    }


    /// <summary>
    /// 创建Stage
    /// </summary>
    /// <param name="stageType"></param>
    /// <param name="stg"></param>
    private static void CreateStage(Type stageType, ref AbstractStage stg)
    {

        string stageName = stageType.ToString();
        if (!StageDict.ContainsKey(stageName))
        {           
            stg = System.Activator.CreateInstance(stageType) as AbstractStage;
            if (stg == null)
            {
                Log.info("STG_ is Null!");
                return;
            }
            GameObject go = NGUITools.AddChild(root.gameObject, true);
            //GameObject go = new GameObject();
            //go.transform.parent = root.gameObject.transform;
            //go.transform.localPosition = Vector3.zero;
            //go.transform.localRotation = Quaternion.identity;
            //go.transform.localScale = Vector3.one;
            //go.layer = root.gameObject.layer;

            stg.gameObj = go;
            StageDict[stageName] = stg;
        }
        else {
            stg = StageDict[stageName];
        }
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="stg"></param>
    /// <param name="isNeedAni"></param>
    private static void ExecToStage(AbstractStage stg, bool isNeedAni = true)
    {
        _isNeedAni = isNeedAni;
        nextStage = stg;
        if (nowStage != null)
        {//当前场景退出
            nowStage.Exit();
        }
        else
        {//执行新场景
            nowStage = nextStage;
            nextStage.Show();
            backFlag = false;
        }
    }

    private static void OnSceneChange(GEvent e)
    {
        //Log.info("StageManager onSceneChange To clearAllStage.");
        ClearAllStage();
    }

    /// <summary>
    /// 设置根节点并创建遮罩层
    /// </summary>
    private static void ReBindRoot()
    {
        root = GameObject.FindObjectOfType<Canvas>();
        
    }

}
