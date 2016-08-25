using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ModuleName
{
    #region 定义 SceneName / ModuleName / WinName .  需跟类名一样，不容易出错。

    //Game  Scene -------------------
    public const string SCENE_STARTUP = "Startup";
    public const string SCENE_PLATFORM_LOGIN = "Login";
    public const string SCENE_SERVER_SELECT = "ServerSelect"; 
    public const string SCENE_MAIN = "Main";
    public const string SCENE_GAME_BATTLE = "GameBattle";

    //Stage -------------------------

    public const string STG_Login = "STG_Login";        //平台登录界面
    public const string STG_ServerSelect = "STG_ServerSelect";//服务器选择
    public const string STG_Main = "STG_Main";          //主场景
    public const string STG_GameBattle = "STG_GameBattle";//战斗场景
    public const string STG_Mail = "STG_Mail";          //邮件
    //Window -------------------------



    #endregion
}