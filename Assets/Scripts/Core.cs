//////////////////////////////////////////////////////////////////////////
//  游戏初始化
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Core : MonoBehaviour {
    //是否是地图编辑器模式
    public static bool isInEditorMode = false;
    
    public static bool inited = false;

    private static Transform _core;
    private static Transform _sysPluginsRoot;
    private static Canvas _uiRoot;
    private static DataFactory _dataFactory = null;
    private static ServerFactory _serverFactory = null;
    private static ModelFactory _modelFactory = null;

	void Start()
    {
        Init();

//         string info = LangLocalizations.GetWord("提示");
//         string value = LangLocalizations.GetWord("获取{0}：{1}个","金币",100);
	}

    void Update()
    {
        if (!inited)
            return;
        GameGC.OnUpdate();
    }

    public void Init()
    {
        //Log.info(">> 1.GameInit");
        if (!inited)
        {
#if UNITY_EDITOR
			GamePublishSetting.release = false;
			ResLoadTool.isByWWWLoad = false;
#else
			GamePublishSetting.release = false;
			ResLoadTool.isByWWWLoad = true;
#endif
            GamePublishSetting.Init(); 
            GameObject.DontDestroyOnLoad(this.gameObject);

            SystemInfoCheck.cacheDefaultResolution();
            SystemSet.ratioType = 2;//设置默认分辨率级别

            createMonoRoot();
            //初始化管理器
            managersInit();
            //初始化 ctrl data server
            initModules();
            inited = true;
        }
        else
        {
            Destroy(this);
        }
    }

    //核心插件根节点
    public static Transform CorePluginRoot
    {
        get {
            if (_core == null) createMonoRoot();
            return _core; 
        }
    }

    //系统插件根节点
    public static Transform SystemPluginRoot
    {
        get {
            if (_core == null) createMonoRoot();
            return _sysPluginsRoot;
        }
    }

    //游戏UI root
    public static Canvas uiRoot
    {
        get {
            if (_uiRoot == null) createMonoRoot();
            return _uiRoot;
        }
    }

    public static void Setup()
    {
        if (!Core.inited)
        {
            GameObject go = new GameObject();
            go.name = "Core";
            go.AddComponent<Core>().Init();
        }
        Debug.Log("Application.persistentDataPath:" + Application.persistentDataPath);
        Debug.Log("Application.streamingAssetsPath:" + Application.streamingAssetsPath);
    }

    public static void AddChild(GameObject ob)
    {
        ob.transform.parent = _core;
    }

    public void UnloadThis()
    {
        _core = null;

        if (_dataFactory != null)   _dataFactory.Dispose();
        if (_serverFactory != null) _serverFactory.Dispose();
        if (_modelFactory != null)  _modelFactory.Dispose();

        _dataFactory = null;
        _serverFactory = null;
        _modelFactory = null;

        AbstractView.DestoryAllObj();

        Destroy(this);
    }
	
	private static void createMonoRoot()
	{
        _uiRoot = GameObject.FindObjectOfType<Canvas>();
		if (_uiRoot == null)
		{
			GameObject uiRootGO = Resources.Load("GUI/UI Root") as GameObject;
			GameObject uirootA = GameObject.Instantiate(uiRootGO) as GameObject;
            _uiRoot = uirootA.GetComponent<Canvas>();
		}
        GameObject.DontDestroyOnLoad(_uiRoot.gameObject);

		GameObject rootGO = new GameObject ();
		rootGO.name = "_CorePlugins";
		GameObject.DontDestroyOnLoad (rootGO);
		_core = rootGO.transform;

        GameObject sysPluginsGO = new GameObject();
        sysPluginsGO.name = "SystemPlugins";
        sysPluginsGO.transform.parent = _core.transform;
        _sysPluginsRoot = sysPluginsGO.transform;

        
	}

    #region 初始化组件
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void managersInit()
    {
		//Log.info(">> 2.初始化各种组件类");
		//TODO init
        //初始化组件
        this.gameObject.AddComponent<ComponentsInit>();

        //SystemPlugins.Init();
        StageManager.Init();
        VideoMananger.Init(Core.CorePluginRoot);
        SoundManager.InitRoot(Core.CorePluginRoot);
        LoadThreadManager.Init(Core.CorePluginRoot);

        //GameLoop
        GameLoop.Init(Core.CorePluginRoot);
        GameLoop.EnableFrameLoop(true);

        // 协程代理工具
        CoroutineDelegate.Init(Core.CorePluginRoot);
	}

    /// <summary>
    /// 初始化模块工厂类
    /// </summary>
    private void initModules()
    {
        //Log.info(">> 3.初始化模块工厂类");

        //建立之间引用关系
        //data   无权访问任何模块，只被调用，做读取、写入
        //ctrl   只被view调用，不实现任何业务逻辑，而是选择想要的model，处理数据。
        //server 只能调用model，将数据交给model处理
        //model  操作data，操作server发送数据给服务器；处理后的消息，通过信号or事件通知视图View
        _dataFactory = new DataFactory();
        _serverFactory = new ServerFactory();
        _modelFactory = new ModelFactory();

        _dataFactory.Init();
        _serverFactory.Init(_modelFactory);
        _modelFactory.Init(_dataFactory, _serverFactory);

        AbstractView.Setup(_modelFactory);
    }
    #endregion

    #region 网络组件初始化、连接服务器、重连、断开连接、自动登录
    //网络组件初始化
    public static void InitNetComponent()
    {
        _serverFactory.InitNetComponent();
    }

    //设置游戏状态
    public static void SetGameState(GameState state)
    {
        _serverFactory.gameState = state;
    }
    #endregion

    #region 仅供Editor编辑器中使用
#if UNITY_EDITOR
    public static ServerFactory Server
    {
        get { return _serverFactory; }
    }
#endif
    #endregion
}
