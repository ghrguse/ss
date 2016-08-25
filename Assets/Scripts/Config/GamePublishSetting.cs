using UnityEngine;
using System.Collections;

public class GamePublishSetting 
{
    /// <summary>
    /// 显示日志 true不显示 false显示
    /// </summary>
    public static bool release = false;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        //确保release版是没有日志
        Log.IsShowScreen = !release;
        Log.ShowLogWarning = !release;
        Log.ShowLogError = !release;
        Log.IsShow = !release;
        Log.IsShowScreen = !release;
    }
}
