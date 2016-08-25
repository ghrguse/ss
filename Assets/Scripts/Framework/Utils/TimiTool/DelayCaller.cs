using UnityEngine;
using System.Collections;

public class DelayCaller : MonoBehaviour
{
    //唯一ID
    public string ID;
    public float time;
    public DelayManager.DelayCallBack onFinishCallBack;

    //
    private object tmp_data;
    //
    private DelegateEnums.DataParam tmp_fn_Data;
    private DelegateEnums.NoneParam tmp_fn_None;



#if UNITY_EDITOR
    //仅为了开启Inspector的勾选框，方便调试
    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0)
        {
            time = 0;
        }
    }
#else
#endif


    //带参数的
    public void addDelay(float t, DelegateEnums.DataParam fn_Data, object data)
    {
        this.time = t;
        this.tmp_fn_Data = fn_Data;
        this.tmp_data = data;
        StartCoroutine(execWithParam(t, fn_Data, data));
    }
    private IEnumerator execWithParam(float t, DelegateEnums.DataParam fn_Data, object data)
    {
        yield return new WaitForSeconds(t);
        onFinish();
        fn_Data(data);

    }

    //不带参数的延迟
    public void addDelay(float t, DelegateEnums.NoneParam fn_None)
    {
        this.time = t;
        this.tmp_fn_None = fn_None;
        StartCoroutine(execNoParam(t, fn_None));
    }
    private IEnumerator execNoParam(float t, DelegateEnums.NoneParam fn_None)
    {
        yield return new WaitForSeconds(t);
        onFinish();
        fn_None();
    }
    //暂停
    public void pause()
    {
        this.StopAllCoroutines();
        this.enabled = false;
    }
    //继续
    public void continuePlay()
    {
        if (tmp_fn_Data != null)
        {
            StartCoroutine(execWithParam(time, tmp_fn_Data, tmp_data));
        }
        else
        {
            StartCoroutine(execNoParam(time, tmp_fn_None));
        }
        this.enabled = true;
    }

    //完成后返回manager
    public void onFinish()
    {
        onFinishCallBack(this);
    }
    public void clearTmp()
    {
        this.tmp_data = null;
        this.tmp_fn_None = null;
        this.tmp_fn_Data = null;
    }
    void OnDestroy()
    {
        this.clearTmp();
        this.StopAllCoroutines();
    }
}
