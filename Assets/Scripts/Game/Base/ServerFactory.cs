using UnityEngine;
using System.Collections;

public class ServerFactory : ServerCenter{

    public GameTimeServer GameTime{ get { return Creator<GameTimeServer>();}}
    public AccountServer Account { get { return Creator<AccountServer>(); } }
    /// <summary>
    /// 初始化，协议注册
    /// </summary>
    /// <param name="model"></param>
    override public void Init(BaseModelFactory model)
    {
        Log.info(">> 初始化 Servers 模块!");
        base.Init(model);

        this.GameTime.RegProtocols();
        this.Account.RegProtocols();
    }

    public void Dispose()
    {
        this.GameTime.Dispose();
        this.Account.Dispose();
        base.Dispose();
    }
}
