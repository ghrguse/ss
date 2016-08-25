using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AbstractStage : ILifeCycle
{
    public DelegateEnums.NoneParam OnPlayBGMCallBack = null;

    //遮罩特效所需时间
    public float effectTime_FadeIn = 0.1f;
    public float effectTime_FadeIn_Delay = 0f;
    public float effectTime_FadeOut = 0.1f;
    public float effectTime_FadeOut_Delay = 0f;
    public Dictionary<string, BaseStagePart> uiStageParts = new Dictionary<string, BaseStagePart>();
    //导航返回使用数据
    public StStageData stStageData;
    public GameObject gameObj;

    /// <summary>
    /// 生命周期类型。 默认不缓存，一关闭就卸载。
    /// </summary>
    protected ModuleLifeType m_lifeCycleType = ModuleLifeType.Cache_None;
    /// <summary>
    /// 是否需要Destory类、卸载资源（Resource，AssetBuddle）
    /// </summary>
    protected bool m_isNeedDestory = true;
    /// <summary>
    /// 是否初始化
    /// </summary>
    protected bool m_isInited = false;
    /// <summary>
    /// 是否需要隐藏场景摄像头
    /// </summary>
    protected bool m_isNeedShowMainCamera = false;

    //背景音乐路径
    protected string BGMPath = "";
    protected int m_effectCount = 0;
    protected ModuleResLoader m_loader;
    /// <summary>
    /// 舞台组件路径列表
    /// </summary>
    protected List<string> m_pathList = new List<string>();
    /// <summary>
    /// 舞台标题
    /// </summary>
    public string titleName = "";


    /// <summary>
    /// 离开
    /// </summary>
    public virtual void Exit()
    {
        Log.info(this.GetName() + " Exit.");

        RemoveEventListeners();
        if (this.LifeCycleType == ModuleLifeType.Cache_60s || this.LifeCycleType == ModuleLifeType.Cache_120s || this.LifeCycleType == ModuleLifeType.Cache_Always)
        {//不需要销毁
            m_isNeedDestory = false;
        }
        else
        {
            m_isNeedDestory = true;
        }
        if (this == null) return;
        this.LeaveScene();
    }

    /// <summary>
    /// Destory类、卸载资源（Resource，AssetBuddle）
    /// 建议使用 this.Exit();内置缓存逻辑判断与管理
    /// </summary>
    public virtual void Dispose()
    {
        this.RemoveEventListeners();
        ModuleManager.UnRegLifeCycle(this);

        foreach (BaseStagePart part in uiStageParts.Values)
        {
            if (part != null)
            {
                part.Dispose();
            }
        }
        
        uiStageParts.Clear();
        m_pathList.Clear();
        m_loader.UnLoad(true);
        StageManager.StageDict.Remove(GetName());
        
        uiStageParts = null;
        m_pathList = null;
        m_loader = null;

        if (this.gameObj != null) GameObject.DestroyImmediate(this.gameObj);
    }

    public virtual void Clear() { }

    /// <summary>
    /// 根据实际需要，清除缓动 
    /// </summary>
    public virtual void TweenClear(){}


    /// <summary>
    /// 添加监听事件,已在Show中调用,请勿重复调用
    /// </summary>
    public virtual void AddEventListeners() { }

    /// <summary>
    /// 移除事件，已在Dispose，Hide中调用，请勿重复调用
    /// </summary>
    public virtual void RemoveEventListeners() { }

    public virtual void Init()
    {
        if (m_isInited) return;
        m_isInited = true;

        this.Build();

        this.gameObj.name = this.GetName();
        this.stStageData.stage = this.GetType();
        StageManager.nowIsMainStage = false;   
    }

    /// <summary>
    /// 是否显示场景摄像机
    /// </summary>
    protected virtual void SetSceneMainCamaraShowOrHide()
    {
        if (StageManager.mainCamera != null)
        {
            StageManager.mainCamera.SetActive(m_isNeedShowMainCamera);
            return;
        }

        StageManager.mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (StageManager.mainCamera != null)
        {
            StageManager.mainCamera.SetActive(m_isNeedShowMainCamera);
        }     
    }

    public virtual void Show()
    {
        Log.info(this.GetName()+" Show.");

        if (m_loader == null) m_loader = new ModuleResLoader(GetName());

        if (m_loader.IsLoaded)
        {
            this.AddEventListeners();
            this.Init();
            this.LoadStagePart();
            if (this.OnPlayBGMCallBack != null) this.OnPlayBGMCallBack();
            this.EnterScene();

            StageManager.nowIsMainStage = this.stStageData.isMainStage;
            ModuleManager.UnRegLifeCycle(this);
            //性能优化默认隐藏主场景摄像机
            this.SetSceneMainCamaraShowOrHide();
            return;
        }

        if (m_loader.IsLoading) return;
        StartLoad();
    }

    /// <summary>
    /// 隐藏 SetActive = false
    /// 建议使用 this.Exit();内置缓存逻辑判断与管理
    /// </summary>
    public virtual void Hide()
    {
        ModuleManager.RegLifeCycle(this, this.LifeCycleType);
        this.RemoveEventListeners();
        this.gameObj.SetActive(false);
    }

    protected void StartLoad()
    { 
        //TODO show LoadingMask

        this.gameObj.SetActive(false);
        m_loader.StartLoad(this.OnAssetBuddleLoaded);
    }

    protected void OnAssetBuddleLoaded(object moduleName)
    {
        //TODO close LoadingMask
        Show();
    }

    #region 入场效果
    protected void EnterScene()
    {
        m_effectCount = 0;
        ToolKit.coroutineDelay(effectTime_FadeIn_Delay, ExecStagePartsEffect);
    }

    /// <summary>
    /// 执行StagePart入场效果
    /// </summary>
    protected void ExecStagePartsEffect()
    {
        m_effectCount = 0;
        foreach (BaseStagePart part in uiStageParts.Values)
        {
            part.ExecEnterEffect(EnterSceneOver);
        }
        EnterSceneOver();
    }

    /// <summary>
    /// 入场结束
    /// </summary>
    protected void EnterSceneOver()
    {
        m_effectCount++;
        if (m_effectCount >= (uiStageParts.Count + 1))
        {
            ToolKit.delayCall(0.1f, WaitAnchor);
            StageManager.StageChangeOver();
        }
    }

    private static void WaitAnchor()
    {
        //TODO 隐藏_uiMask
    }
    #endregion

    #region 离场效果
    /// <summary>
    /// 离开
    /// </summary>
    public virtual void LeaveScene()
    {
        m_effectCount = 0;
        foreach (BaseStagePart part in uiStageParts.Values)
        {
            if (part == null) continue;
            part.ExecLeaveEffect(LeaveSceneOver);
        }
        LeaveSceneOver();
    }

    protected void LeaveSceneOver()
    {
        m_effectCount++;
        //如果part离场结束，正式切换Stage。
        if (m_effectCount >= (uiStageParts.Count + 1))
        {
            this.TweenClear();
            StageManager.LeaveCurrStageOver(this, this.stStageData);
            if (m_isNeedDestory)
            {
                Log.info(this.GetName() + " Dispose.");
                this.Dispose();
            }
            else
            {
                Log.info(this.GetName() + " Hide.");
                this.Hide();
            }
        }
    }
    #endregion


    #region 载入StagePart
    protected virtual void Build()
    {
        throw new Exception("未设定m_pathList的组件定义");
        //需要加一个层级,例如：
        //m_pathList.Add("role/role");
        //m_pathList.Add("role/skill");
    }

    /// <summary>
    /// 载入各个部分view
    /// </summary>
    protected virtual void LoadStagePart()
    {
        int depth = 1;
        foreach (string path in m_pathList)
        {
            AddUiPart(path, depth);
            depth += 10;
        }
    }

    protected BaseStagePart AddUiPart(string path, int depth)
    {
        if (!uiStageParts.ContainsKey(path))
        {
            GameObject prefab = ResDataManager.instance.GetObjectFromLocalPrefab(path,true);
            //TODO 缓存 prefab
            if (prefab == null || StageManager.root == null)
            {
				Log.infoError("BaseStage AddUiPart error:" + (prefab == null ? " 场景不存在 " : "") + (StageManager.root == null ? " StageManager.root不存在 " : "") + " path:" + path);
                return null;
            }

            GameObject go = NGUITools.AddChild(this.gameObj, prefab);
            BaseStagePart part = go.GetComponent<BaseStagePart>();
            if (part)
            {
                //Log.info(Application.loadedLevelName+" addUIPart : " + path);
                //part.GetComponent<UIPanel>().depth = depth;
                uiStageParts.Add(path, part);
                part.Show();
                return part;
            }
            else
            {
                throw new Exception("组件没有挂载UIStagePart脚本" + path);
            }
        }
        else
        {
            BaseStagePart part = this.uiStageParts[path].gameObject.GetComponent<BaseStagePart>();
            //part.GetComponent<UIPanel>().depth = depth;
            //执行初始化
            uiStageParts[path].Init();
        }
        return uiStageParts[path];
    }

    /// <summary>
    /// 回收组件，取出两个场景同时存在的舞台组件，转移到新的场景，老的清除
    /// </summary>
    /// <param name="stage"></param>
    public virtual void RecoverStagePart(AbstractStage stage)
    {
        //遍历自身元件列表，找对方也有的
        foreach (string path in m_pathList)
        {
            if (stage.uiStageParts.ContainsKey(path))
            {
                //取出对方的组件，删除
                BaseStagePart part = stage.uiStageParts[path];
                stage.uiStageParts.Remove(path);
                //放入自身的组件列表中
                if (this.uiStageParts.ContainsKey(path)) 
                    continue;
                this.uiStageParts.Add(path, part);
                //Log.info("回收组件" + part.getViewName());
            }
        }
    }
    #endregion


    /// <summary>
    /// 是否进入生命周期管理
    /// </summary>
    /// <returns></returns>
    public virtual bool IsCheckLife()
    {
        if (m_lifeCycleType == ModuleLifeType.Cache_60s || m_lifeCycleType == ModuleLifeType.Cache_120s)
            return true;
        return false;
    }
    /// <summary>
    /// 生命周期
    /// </summary>
    public ModuleLifeType LifeCycleType
    {
        get
        {
            return m_lifeCycleType;
        }
        set
        {
            m_lifeCycleType = value;
        }
    }

    /// <summary>
    /// StageName
    /// </summary>
    /// <returns></returns>
    public virtual string GetName() { return GetType().ToString(); }
}
