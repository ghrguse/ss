using UnityEngine;
using System.Collections;

public class ModelFactory : BaseModelFactory
{
    public FactionModel Factions { get { return Creator<FactionModel>(); } }
    //游戏时间
    public GameTimeModel GameTimer { get { return Creator<GameTimeModel>(); } }

    public ServerSelectModel ServerSelect { get{return Creator<ServerSelectModel>();}}
    public AccountModel Account{get{return Creator<AccountModel>();}}

    //初始化models
    override public void Init(BaseDataFactory data, BaseServerFactory server)
    {
        base.Init(data, server);
        m_isInited = true;

        this.Factions.AddEventListeners();
        this.GameTimer.AddEventListeners();
        this.ServerSelect.AddEventListeners();
        this.Account.AddEventListeners();
        
    }
}
