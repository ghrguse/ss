using UnityEngine;
using System.Collections;
using System;
using GSProtocal;
using System.Diagnostics;

/// <summary>
/// 服务器时间同步Server
/// </summary>
public class GameTimeServer : Server 
{
    //ServerFactory.Init 
    override public void RegProtocols()
    {
        m_serverCeneter.Regist( CS_CMD_ID.CS_SYN_TIME, this.RServerTime);   
    }

    override public void UnRegProtocols()
    {
		m_serverCeneter.UnRegist(CS_CMD_ID.CS_SYN_TIME, this.RServerTime);  
    }

    //----------------------------------------------------------------------------------------------------------
    //  发包：m_serverCeneter.SendAndBack(GSPkg, tips, relatedCMD, isKeyProtocal, timeoutInterval , showTimeOut)
    //----------------------------------------------------------------------------------------------------------
    
    private DateTime time1970;

    protected override void Init()
    {
        base.Init();
        int offset = 8;//东八区
        time1970 = new DateTime(1970, 1, 1, offset, 0, 0);
    }

    /// <summary>
    /// 返回服务器时间
    /// </summary>
    /// <param name="tData"></param>
    private void RServerTime(MsgData tData)
    {
        SCSynTime iData = tData.Body.stScSynTime;

        long openServerTime = 10000000L * iData.iInitTime + time1970.Ticks;
        DateTime openServerDateTime = new DateTime(openServerTime);

        long serverTime = 10000000L * iData.iTime + time1970.Ticks;
        DateTime serverDateTime = new DateTime (serverTime);

        m_models.GameTimer.SetOpenServerTime(openServerDateTime);
        m_models.GameTimer.SetServerTime(serverDateTime);
    }
}
