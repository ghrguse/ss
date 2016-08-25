using UnityEngine;
using System.Collections;

public class SceneEvents {


    // 场景加载进度
    public static string EVENT_SCENE_PROGRESS = "EVENT_SCENE_PROGRESS";
    /// 场景加载界面图片加载成功
    public static string EVENT_SCENE_PIC_LOADED = "EVENT_SCENE_PIC_LOADED";
    /// 加载界面信息变化
    public static string EVENT_SCENE_INFO_CHANGED = "EVENT_SCENE_INFO_CHANGED";
    /// 显示初始化转圈小动画
    public static string EVENT_SCENE_SHOW_INIT_ANIMATION = "EVENT_SCENE_SHOW_INIT_ANIMATION";



    //初始化准备
    public const string EVENT_SCENE_INIT_READY = "EVENT_SCENE_INIT_READY";
    //Out切换开始
    public const string EVENT_SCENE_TRANS_OUT_START = "EVENT_SCENE_TRANS_OUT_START";
    //切换中
    public const string EVENT_SCENE_TRANS_LOADING = "EVENT_SCENE_TRANS_LOADING";
    //In切换开始
    public const string EVENT_SCENE_TRANS_IN_START = "EVENT_SCENE_TRANS_IN_START";
    //切换结束
    public const string EVENT_SCENE_TRANS_END = "EVENT_SCENE_TRANS_END";

}
