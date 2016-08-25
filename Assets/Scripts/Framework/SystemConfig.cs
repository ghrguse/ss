using UnityEngine;
using System.Collections;


public class SystemConfig : MonoBehaviour
{
    //远程版本配置名
    //public static readonly string remoteVerFileName = "version.txt";
    //远程文件列表配置名
    //public static readonly string localMD5FileName = "md5Lite.txt";
    //本地文件列表配置名
    //public static readonly string remoteMD5FileName = "md5Full.txt";

    //当前程序版本号
    public static string logicVer = "1.0.0";
    //远程文件服务器目录
    // public static readonly string remoteFileRoot = "file:///D:/lucifer_output/";
    public static string remoteFileRoot = "http://172.25.40.227/lucifer_remote/";
#if UNITY_ANDROID
    public static string platformFolder = "Android";
#elif UNITY_IPHONE
    public static string platformFolder = "iOS"; 
#else
    public static string platformFolder = "Android";
#endif
    //是否使用IFS的目录，不要在这里设置，去Set（个人设置）里设置！
    public static bool useIFSData = false;

    public static bool useTrackTools = false; 
    //是否使用网络通信（测试用）
    public static bool useNetForDebug = false;
    //是否在使用DebugTools进行单元测试
    public static bool useDebugTools = false;


    // 获取资源文件夹
    public static string getResDataByLang()
    {
        string path = "ResData/";
        switch (LangLocalizations.Language)
        {
            case LanguageTypeEnum.TW:
                path = "ResData_TW/";
                break;
            case LanguageTypeEnum.EN:
                path = "ResData_EN/";
                break;
            case LanguageTypeEnum.KOR:
                path = "ResData_KOR/";
                break;
            case LanguageTypeEnum.CN:
            default:
                path = "ResData/";
                break;
        }
        return path;
    }
    
    //本地静态存储的目录(可以在一开始进行一次缓存！）
    public static string localResPath
    {
        get
        {
            string dataRoot = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    if (SystemConfig.useIFSData)
                    {
                        dataRoot = "file://" + Application.persistentDataPath + "/";
                    }
                    else
                    {
                        dataRoot = "jar:file://" + Application.dataPath + "!/assets/";
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    if (SystemConfig.useIFSData)
                    {
                        dataRoot = "file://" + Application.persistentDataPath + "//assets//";
                    }
                    else
                    {
                        dataRoot = "file://" + Application.streamingAssetsPath + "/";
                    }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
#if UNITY_EDITOR
                    if (useIFSData)
                    {
                        dataRoot = "file://" + Application.dataPath + "/IFS_Export/";
                        break;
                    }
#endif
                    dataRoot = "file://" + Application.dataPath + "/_Project/ResRoot/Data/";
                    //dataRoot = "file://" + Application.dataPath.Replace("/Assets", "") + "/" + getResDataByLang();
                    break;
            }
            return dataRoot;
        }
    }

    /// <summary>
    /// 本地平台路径
    /// </summary>
    public static string localPlatformResPath
    {
        get { return SystemConfig.localResPath + "PlatformAssets/" + SystemConfig.platformFolder + "/";}
    }

    //本地动态写入文件的目录
    public static string localUserCacheSavePath
    {
        get
        {
            string dataRoot = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    dataRoot = Application.persistentDataPath + "//";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    dataRoot = Application.persistentDataPath + "/assets/";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    dataRoot = "file://" + Application.dataPath + "/_Project/ResRoot/Data/";
                    //dataRoot = Application.dataPath.Replace("/Assets", "") + "/" + getResDataByLang();
                    break;
            }
            return dataRoot;
        }
    }
    //本地动态读取文件的目录
    public static string localUserCacheReadPath
    {
        get
        {
            string dataRoot = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    dataRoot = "file://" + Application.persistentDataPath + "//";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    dataRoot = "file://" + Application.persistentDataPath + "//assets//";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    dataRoot = "file://" + Application.dataPath + "/_Project/ResRoot/Data/";
                    //dataRoot = "file://" + Application.dataPath.Replace("/Assets", "") + "/" + getResDataByLang();
                    break;
            }
            return dataRoot;
        }
    }

    //IFS动态写入文件的目录
    public static string IFS_DataPath
    {
        get
        {
            string dataRoot = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    dataRoot = Application.persistentDataPath + "//";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    dataRoot = Application.persistentDataPath + "//assets//";
                    break;
                default:
                    dataRoot = Application.dataPath + "/IFS_Export/";
                    break;
            }
            return dataRoot;
        }
    }
}
