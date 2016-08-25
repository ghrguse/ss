using UnityEngine;
using System.Collections;

public class Controller : AbstractCtrl {

    protected ModelFactory m_models;

    override public void Dispose()
    {
        m_models = null;
        base.Dispose();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    override protected void Init()
    {
        m_models = this.GetModels<ModelFactory>();
    }
}
