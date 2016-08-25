using System;
using UnityEngine;

namespace ObjectPoolTool
{
    public interface IPoolItem
    {
        //释放
        void Release();
        //有效性
        void Reset();
    }
}


