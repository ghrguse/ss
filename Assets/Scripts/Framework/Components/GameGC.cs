using UnityEngine;
using System.Collections;

public class GameGC  {

    /// <summary>
    /// 每隔 X 时间GC一次
    /// </summary>
    public static float ClearTime = 120f;//2分钟 
    public static float OneSecond = 1f;//1秒钟执行一次

    private static float _lastTime = 0f;
    private static float _internal = 0f;
 

    public static void OnUpdate()
    {
        //_lastTime += Time.deltaTime;
        _internal += Time.deltaTime;

        //每秒钟渲染一次
        if (_internal >= OneSecond)
        {
            _internal = 0;
            ModuleManager.UpdateLifeCycle();
        }

        //会导致部分预加载机制无效。暂时先关闭 by piaochen

        //         //大于ClearTime执行一次GC
        //         if (_lastTime >= ClearTime)
        //         {
        //             _lastTime = 0;
        //             Log.info(">>>>>>>> GameGC 自动清理多余资源! <<<<<<");
        //             ResDataManager.instance.DestroyAll();
        //         }
    }


}
