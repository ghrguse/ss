using UnityEngine;
using ObjectPoolTool;
using System.Collections;

public class GEvent:IPoolItem
{
    public string type = "";
    public object data = null;

    //释放
    public void Release()
    {
        Reset();
    }
    //有效性
    public void Reset()
    {
        type = "";
        data = null;
    }
}
