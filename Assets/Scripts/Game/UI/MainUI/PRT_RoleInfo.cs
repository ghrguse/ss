using UnityEngine;
using System.Collections;

public class PRT_RoleInfo : BaseStagePart
{

    public override void AddEventListeners()
    {
    //    this.AddViewEventListener(EventType_Global.SERVERlIST_LOAD_SUCCESS, loadServerListComplete);
    }
    public override void RemoveEventListeners()
    {
    //    this.RemoveViewEventListener(EventType_Global.SERVERlIST_LOAD_SUCCESS, loadServerListComplete);
    }

    public override void Dispose()
    {
        base.Dispose();
        //serverSelectCtrl = null;

    }

    public override void Init()
    {
        if (m_isInited)
            return;
        m_isInited = true;

        //serverSelectCtrl = this.GetController<ServerSelectCtrl>();

    }

}
