using UnityEngine;
using System.Collections;

public class Data :AbstractData {

    protected DataFactory m_datas = null;
    override protected void Init()
    {
        m_datas = GetDatas<DataFactory>();
        base.Init();
    }

    override public void Dispose()
    {
        m_datas = null;
        base.Dispose();
    }
}
