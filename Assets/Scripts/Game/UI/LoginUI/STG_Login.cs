﻿using UnityEngine;
using System.Collections;

public class STG_Login : AbstractStage
{
    public override void Init()
    {
        base.Init();
    }

    protected override void Build()
    {
        m_pathList.Add("GUI/Login/PRT_Login");
    }
}
