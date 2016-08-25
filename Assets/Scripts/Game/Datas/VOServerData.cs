using System;
using System.Collections.Generic;

[Serializable]
public class VOServerData
{
	public string tag;
	public string url;
	public ushort port;
	public int zone;
	//0就是关，1就是开
	public int status = 1;
	//停服公告
	public string notice;
	//1-推荐服务器，其余不是
	public int r;
	//排序字段，越小越靠前
	public int o;

	public VOServerData ()
	{
	}
}

[Serializable]
public class VOServerAreaData
{
    public string name = "";
    public List<VOServerData> serverList = new List<VOServerData>();
    public int o;//排序字段，越小越靠前

    public VOServerAreaData()
    {
    }

    public VOServerAreaData(string areaName)
    {
        name = areaName;
    }
}


[Serializable]
public class VOWorldServerData
{
    public List<VOServerAreaData> areaList = new List<VOServerAreaData>();
    
    public int worldId;

    public string url;

    public int state;

    public VOWorldServerData()
    {
    }
}
