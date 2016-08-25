using UnityEngine;
using System.Collections;

public class LoginCtrl : Controller {

	public void EnterGame(VOServerData server)
    {
        m_models.Account.EnterGame(server);
    }
}
