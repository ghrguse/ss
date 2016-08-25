using System;
using UnityEngine;

[Serializable]
public class SystemSet
{
    private static bool isSound = true;
    private static bool isMusic = true;
    public static bool isSendHeart = true;
    public static uint displayType = 1;
    public static bool isOpenWinBlur = true;

    //渲染品质 级别 2， 1， 0

    //shaderLOD
    private static uint _textureType;
    public static uint textureType
    {
        get
        {
            return _textureType;
        }

        set
        {
            _textureType = value;
            switch (value)
            {
                case 0:
                    Shader.globalMaximumLOD = 1000;
                    break;
                case 1:
                    //if (Application.platform == RuntimePlatform.Android)
                    //{
                    Shader.globalMaximumLOD = 1100;
                    //}
                    //else
                    //{
                    //    Shader.globalMaximumLOD = 1200;
                    //}
                    break;
                case 2:
                    Shader.globalMaximumLOD = 1100;
                    break;
            }

            Log.info("当前ShaderLOD: " + Shader.globalMaximumLOD);
        }
    }

    //模型品质 级别 1， 0
    private static uint _modleType;
    public static uint modleType
    {
        get
        {
            return _modleType;
        }

        set
        {
            _modleType = value;
            //Log.infoError(value);
            //LODSet.TypeLevel.Clear();
            //LODSet.TypeLevel.Add(LODSetType.CharacterEffect, (int)value + 1);
            //LODSet.TypeLevel.Add(LODSetType.CharacterModel, (int)value + 1);
            //LODSet.TypeLevel.Add(LODSetType.SceneEffect, (int)value + 1);
            //LODSet.TypeLevel.Add(LODSetType.SceneModel, (int)value + 1);
        }
    }

    //特效品质 级别 1， 0
    private static uint _effectType;
    public static uint effectType
    {
        get
        {
            return _effectType;
        }

        set
        {
            _effectType = value;
        }
    }

    //动画融合 级别 2， 1， 0
    private static uint _animationType;
    public static uint animationType
    {
        get
        {
            return _animationType;
        }

        set
        {
            _animationType = value;
            switch (value)
            {
                case 0:
                    QualitySettings.blendWeights = BlendWeights.OneBone;
                    break;
                case 1:
                    QualitySettings.blendWeights = BlendWeights.TwoBones;
                    break;
                case 2:
                    QualitySettings.blendWeights = BlendWeights.FourBones;
                    break;
            }

        }
    }

    //抗锯齿 级别 2， 1， 0
    private static uint _antialiasedType;
    public static uint antialiasedType
    {
        get
        {
            return _antialiasedType;
        }

        set
        {
            _antialiasedType = value;

            //switch (value)
            //{
            //    case 0:
            //        QualitySettings.antiAliasing = 0;
            //        break;
            //    case 1:
            //        QualitySettings.antiAliasing = 2;
            //        break;
            //    case 2:
            //        QualitySettings.antiAliasing = 4;
            //        break;
            //}
        }

    }


    //分辨率 级别 2， 1， 0
    private static uint _ratioType;
    public static uint ratioType
    {
        get
        {
            return _ratioType;
        }

        set
        {
            _ratioType = value;

            Log.show(" _ratioType : " + _ratioType);
            //             // 不能做，所有的设置都是在一进游戏就生效，如果加这个操作，除非玩家手动去设置里面调整，否则分辨率设置永远不生效
            //             if (!Application.loadedLevelName.Equals(ModuleName.SCENE_GAME_MAIN) && !Application.loadedLevelName.Equals(ModuleName.SCENE_CHARACTER_SELECT))
            //                 // GameMain场景才会主动调整分辨率，其他场景自动调整，具体场景根据时机处理
            //                 // 解决切换分辨率时闪烁问题
            //                 return;

            //安卓进行详细调整，对大部分中高配置机型不做任何调整
            if (Application.platform == RuntimePlatform.Android)
            {
                switch (_ratioType)
                {
                    case 0:
                        setResolution(480);
                        break;
                    case 1:
                        setResolution(720);
                        break;
                    case 2:
                        setResolution(1334);
                        break;
                    default:
                        setResolution(1440);
                        break;
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //ios下基本只需要调整ipad的分辨率
                switch (_ratioType)
                {
                    //iphone4
                    case 0:
                        setResolution(480);
                        break;
                    //iphone4s
                    case 1:
                        setResolution(720);
                        break;
                    case 2:
                        setResolution(1334);
                        break;
                    default:
                        setResolution(1440);
                        break;
                }
            }
            else
            {
                //其他平台下最多限制1440P
                setResolution(1440);
            }

        }
    }
    //设置分辨率
    public static void setResolution(int h)
    {
        Log.info("原始分辨率: " + SystemInfoCheck.defaultScreenWidth + "×" + SystemInfoCheck.defaultScreenHeight);
        Log.info("当前分辨率: " + Screen.width + "×" + Screen.height);
        //与原始分辨率对比
        if (SystemInfoCheck.defaultScreenHeight > h)
        {
            int width = Mathf.RoundToInt(System.Convert.ToSingle(SystemInfoCheck.defaultScreenWidth) / System.Convert.ToSingle(SystemInfoCheck.defaultScreenHeight) * h);
            Screen.SetResolution(width, h, true);
            Log.info("分辨率调整为: " + width + "×" + h);
        }
        else
        {
            if (Screen.width != SystemInfoCheck.defaultScreenWidth || Screen.height != SystemInfoCheck.defaultScreenHeight)
            {
                Screen.SetResolution(SystemInfoCheck.defaultScreenWidth, SystemInfoCheck.defaultScreenHeight, true);
            }
            Log.info("因分辨率小于限制值 " + h + "p ，不做调整。");
        }
    }

    //游戏帧率 级别 1， 0
    private static uint _frameType;
    public static uint frameType
    {
        get
        {
            return _frameType;
        }

        set
        {
            _frameType = value;

            switch (value)
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    Application.targetFrameRate = 60;
                    break;
            }
        }
    }
    /// <summary>
    /// 是否有地图特效 true=有地图特效 false=没有
    /// </summary>
    private static bool _mapEffect;
    public static bool mapEffect
    {
        get
        {
            return _mapEffect;
        }

        set
        {
            _mapEffect = value;
        }
    }

    //帧率开关
    private static bool _fpsSwitch;
    public static bool fpsSwitch
    {
        get
        {
            return _fpsSwitch;
        }

        set
        {
            _fpsSwitch = value;
        }

    }


    public static bool isVerticalSync = false;
    public static bool isDebug = false;


    //信鸽开关
    private static bool _isPowerInfo = false;

    public static bool isPowerInfo
    {
        get
        {
            return _isPowerInfo;
        }

        set
        {
            _isPowerInfo = value;
            if (_isPowerInfo == false)
            {
                //GD_ZoneStateData.clearXGmessage();
            }        
        }
    }

    //内存清理
    private static bool _isClearMemory;
    public static bool isClearMemory
    {
        get
        {
            return _isClearMemory;
        }

        set
        {
            _isClearMemory = value;
        }
    }
    //资源缓存
    private static bool _isCacheResource;
    public static bool isCacheResource
    {
        get
        {
            return _isCacheResource;
        }

        set
        {
            _isCacheResource = value;
            //            if (value == false)
            //            {
            //                Caching.CleanCache();
            //            }
        }
    }

    private static string savePath = SystemConfig.localUserCacheSavePath + "LocalCache/systemSetFile.xml";
    /// <summary>
    /// 0-开启，1-只有局部开启，2-关闭
    /// </summary>
    public static int isGuide = 0;

    public static bool isUIGuide
    {
        get
        {
            return isGuide != 2;
        }

        set
        {
            isGuide = value ? 0 : 2;
        }
    }

    public static bool IsSound
    {
        get
        {
            return isSound;
        }
        set
        {
            isSound = value;
            //SoundMgr.Instance.SoundMute = !isSound;
        }
    }

    public static bool IsMusic
    {
        get
        {
            return isMusic;
        }
        set
        {
            isMusic = value;
            //SoundMgr.Instance.MusicMute = !isMusic;
        }
    }

    //物理特性
    private static bool _isPhysical;
    public bool isPhysical
    {
        get
        {
            return _isPhysical;
        }

        set
        {
            _isPhysical = value;
        }
    }

    //重看剧情
    private static bool _isStory;
    public static bool isStory
    {
        get
        {
            return _isStory;
        }

        set
        {
            _isStory = value;
        }
    }


    //好友申请
    private static bool _isFriendApply = true;
    public static bool isFriendApply
    {
        get
        {
            return _isFriendApply;
        }

        set
        {
            _isFriendApply = value;
        }
    }

    public static void saveData()
    {
        //XmlUtil.SaveXml("systemSetFile", this);
    }
}

