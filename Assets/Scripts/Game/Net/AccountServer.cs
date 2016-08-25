using UnityEngine;
using System.Collections;
using System;
using GSProtocal;

public class AccountServer : Server
{
    public override void RegProtocols()
    {
        m_serverCeneter.Regist(CS_CMD_ID.CS_GET_ROLE, RGetRole);
        m_serverCeneter.Regist(CS_CMD_ID.CS_CREATE_ROLE, RCreateRole);
    }

    public override void UnRegProtocols()
    {
        m_serverCeneter.UnRegist(CS_CMD_ID.CS_GET_ROLE, RGetRole);
        m_serverCeneter.UnRegist(CS_CMD_ID.CS_CREATE_ROLE, RCreateRole);
    }

    public void SGetRole(int iZoneId)
    {
        CMSG cMsg = new CMSG();
        cMsg.wCmdId = (UInt16)CS_CMD_ID.CS_GET_ROLE;
        cMsg.stBody.stCsGetRole = new CSGetRole();
        cMsg.stBody.stCsGetRole.wZoneId = (ushort)iZoneId;
        //TODO

        m_serverCeneter.SendAndBack(cMsg);
    }

    public void SCreateRole(string name, int imageId)
    {
        CMSG cMsg = new CMSG();
        cMsg.wCmdId = (UInt16)CS_CMD_ID.CS_CREATE_ROLE;
        cMsg.stBody.stCsCreateRole = new CSCreateRole();
        cMsg.stBody.stCsCreateRole.szRoleName = TDRTools.strToByte(name);
        cMsg.stBody.stCsCreateRole.wImageID = (ushort)imageId;
        m_serverCeneter.SendAndBack(cMsg);
    }

    private void RGetRole(MsgData data)
    {
        if (data.Eno == 0)
        {
            m_models.Account.UpdateRoleInfo(data.Body.stScRoleInfo);
        }
        else
        {
            Log.infoError("RGetRole：" + data.Eno);
        }
    }

    private void RCreateRole(MsgData data)
    {
        if (data.Eno == 0)
        {
            m_models.Account.CreateRoleRet(data.Body.stScCreateRole.iRet, data.Body.stScCreateRole.ullPara1);
        }
        else
        {
            Log.infoError("RCreateRole：" + data.Eno);
        }
    }
}
