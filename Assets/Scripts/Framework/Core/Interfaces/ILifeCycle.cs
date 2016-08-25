using UnityEngine;
using System.Collections;

public interface ILifeCycle
{
    //是否进入生命周期管理
    bool IsCheckLife();
    //名称
    string GetName();
    //销毁
    void Dispose();
}
