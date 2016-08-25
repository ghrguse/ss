using UnityEngine;
using System;
using System.Collections.Generic;
using GSProtocal;


public class GlobalData
{
    //集合所有用户的基本系统信息（服务器域名、商号、帐号、大区、角色数量等）
    public static GameSysInfo system = new GameSysInfo();
    
    //系统设置
    public static SystemSet systemSet = new SystemSet();
    
    private static bool inited = false;
        
    public static void init()
    {
        if (inited)
        {
            return;
        }
        inited = true;

        //初始化各大数据模块数据中心
     
    }

}
