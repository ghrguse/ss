using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using Utils.Event;

public class ServerSelectModel: Model
{
    private WWW www = null;
    private bool isDownloading = false;
    private bool isLoadingServerData = false;
    public string dirServerURL
    {
        get{return m_datas.serverList.dirServerURL;}
        set{m_datas.serverList.dirServerURL = value;}
    }

    public override void AddEventListeners()
    {
    }

    public override void RemoveEventListeners()
    {
    }

    protected override void Init()
    {
        base.Init();
        initServerURL();
    }

    public void LoadServerList()
    {
        if (isLoadingServerData == false)
        {
            isLoadingServerData = true;
            ServerXMLloader.startLoad(this);
        }
    }

    private string initServerURL()
    {
        string gameServerurl = "http://dir.ml.qq.com:2000/assets/svr_leo.xml";
        m_datas.serverList.dirServerURL = gameServerurl;
        return gameServerurl;
    }

    public void parseSvrListXML(string xmlText)
    {
        Dictionary<int, VOServerData> serverDict = m_datas.serverList.serverDict;
        serverDict.Clear();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);
        XmlElement xmlRoot = doc.DocumentElement;
        XmlNodeList xmlAreaList = (xmlRoot.GetElementsByTagName("SvrList")[0] as XmlElement).GetElementsByTagName("Area");

        VOWorldServerData worldServerData = new VOWorldServerData();
        foreach (XmlNode node in xmlAreaList)
        {
            VOServerAreaData areaData = new VOServerAreaData();
            worldServerData.areaList.Add(areaData);

            XmlElement areaElement = (XmlElement)node;
            areaData.name = areaElement.GetAttribute("name");
            XmlNodeList serverNodeList = areaElement.GetElementsByTagName("Server");
            foreach (XmlNode server in serverNodeList)
            {
                VOServerData serverData = new VOServerData();
                areaData.serverList.Add(serverData);

                XmlElement serverElement = (XmlElement)server;
                serverData.tag = serverElement.GetElementsByTagName("Tag")[0].InnerText;
                serverData.port = (ushort)(System.Int32.Parse(serverElement.GetElementsByTagName("Port")[0].InnerText));
                serverData.zone = System.Int32.Parse(serverElement.GetElementsByTagName("Zone")[0].InnerText);
                serverData.url = serverElement.GetElementsByTagName("Url")[0].InnerText;
                
                serverData.status = System.Int32.Parse(serverElement.GetElementsByTagName("Status")[0].InnerText);
                //serverData.r = System.Int32.Parse(serverElement.GetElementsByTagName("R")[0].InnerText);
                //serverData.o = System.Int32.Parse(serverElement.GetElementsByTagName("O")[0].InnerText);
                XmlNodeList rList = serverElement.GetElementsByTagName("R");
                if (rList != null && rList.Count > 0)
                    serverData.r = System.Int32.Parse(rList[0].InnerText);
                
                if (!serverDict.ContainsKey(serverData.zone))
                {
                    serverDict.Add(serverData.zone, serverData);
                }
            }
            areaData.serverList.Sort(new ServerComparer());
        }
        worldServerData.areaList.Sort(new AreaComparer());
        m_datas.serverList.serverListData = worldServerData;

        EventManager.Send(EventType_Global.SERVERlIST_LOAD_SUCCESS);
    }

    public VOServerData getDefaultServer()
    {
        VOWorldServerData worldServerData = m_datas.serverList.serverListData;
        if (worldServerData.areaList == null || worldServerData.areaList.Count <= 0)
            return null;

        VOServerAreaData areaData = worldServerData.areaList[0];
        if (areaData == null || areaData.serverList == null || areaData.serverList.Count == 0)
            return null;

        VOServerData serverData = areaData.serverList[0];
        if (serverData == null)
            return null;

        if (string.IsNullOrEmpty(serverData.url))
        {
            areaData.serverList.RemoveAt(0);
            return null;
        }

        return serverData;
    }
}

class ServerComparer : IComparer<VOServerData>
{
    public int Compare(VOServerData left, VOServerData right)
    {
        if (left.o > right.o)
            return 1;
        else if (left.o == right.o)
            return 0;
        else
            return -1;
    }
}

class AreaComparer : IComparer<VOServerAreaData>
{
    public int Compare(VOServerAreaData left, VOServerAreaData right)
    {
        if (left.o > right.o)
            return 1;
        else if (left.o == right.o)
            return 0;
        else
            return -1;
    }
}
