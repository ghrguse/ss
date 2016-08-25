using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerList : Data {

    public string dirServerURL;

    public Dictionary<int, VOServerData> serverDict = new Dictionary<int, VOServerData>();
    public VOWorldServerData serverListData;

    public override void Clear()
    {
    }
}
