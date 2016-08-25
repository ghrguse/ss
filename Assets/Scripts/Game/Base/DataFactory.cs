using UnityEngine;
using System.Collections;

public class DataFactory : BaseDataFactory {

    //public BattleData Battle { get { return Creator<BattleData>(); } }
    public ServerList serverList { get { return Creator<ServerList>(); } }
    public AccountData Account { get { return Creator<AccountData>(); } }
    
}
