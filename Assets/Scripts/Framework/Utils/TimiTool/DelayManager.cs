using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayManager : MonoBehaviour
{
    private static int autoID = 10000;
    //当前延迟字典
    private Dictionary<string, DelayCaller> dict = new Dictionary<string, DelayCaller>();
    //对象池
    private List<DelayCaller> pool = new List<DelayCaller>();
    //回调委托
    public delegate void DelayCallBack(DelayCaller caller);

    //从对象池中获取
    public DelayCaller getCaller()
    {
        DelayCaller caller;
        if (pool.Count == 0)
        {
            caller = gameObject.AddComponent<DelayCaller>();
            caller.onFinishCallBack = onFinish;
        }
        else
        {
            caller = pool[0];
            caller.enabled = true;
            pool.RemoveAt(0);
        }
        return caller;
    }
    //放回池中
    public void addToPool(DelayCaller caller)
    {
        //使用这个停止延迟效率会高吗？
        caller.StopAllCoroutines();
        caller.enabled = false;
        caller.ID = "0";
        caller.clearTmp();
        pool.Add(caller);
    }

    public string addDelay(float t, DelegateEnums.DataParam fn_Data, object data, string ID = "")
    {
        if (ID == "")
        {
            autoID++;
            ID = autoID.ToString();
        }
        else
        {
            if (dict.ContainsKey(ID))
            {
                stopDelay(ID);
            }
        }
        //
        DelayCaller caller = getCaller();
        caller.ID = ID;
        caller.addDelay(t, fn_Data, data);

        dict.Add(ID, caller);

        updateTitle();
        return ID;
    }

    public string addDelay(float t, DelegateEnums.NoneParam fn_None, string ID = "")
    {
        if (ID == "")
        {
            autoID++;
            ID = autoID.ToString();
        }
        else
        {
            if (dict.ContainsKey(ID))
            {
                stopDelay(ID);
            }
        }

        DelayCaller caller = getCaller();
        caller.ID = ID;
        caller.addDelay(t, fn_None);

        dict.Add(ID, caller);

        updateTitle();
        return ID;
    }

    //停止一个延迟
    public void stopDelay(string ID)
    {
        if (dict.ContainsKey(ID))
        {
            addToPool(dict[ID]);
            dict.Remove(ID);
        }

        updateTitle();
    }
    //暂停一个延迟
    public void pauseDelay(string ID)
    {
        if (dict.ContainsKey(ID))
        {
            dict[ID].pause();
        }
        updateTitle();
    }
    //暂停一个延迟
    public void continuePlay(string ID)
    {
        if (dict.ContainsKey(ID))
        {
            dict[ID].continuePlay();
        }
        updateTitle();
    }

    //当一个延迟自然结束
    public void onFinish(DelayCaller caller)
    {
        if (dict.ContainsKey(caller.ID))
        {
            dict.Remove(caller.ID);
        }
        addToPool(caller);

        updateTitle();
    }
    private void updateTitle()
    {
#if UNITY_EDITOR
        //仅为了调试
        gameObject.name = "DelayManager " + dict.Count + " / " + pool.Count;
#else
#endif
    }



    void OnDestroy()
    {
        dict.Clear();
        pool.Clear();
    }


}
