using UnityEngine;
using System.Collections;

/// <summary>
/// 加载资源类型枚举
/// </summary>
public enum ResTypeEnum
{
	TEXT = 1, // 文本
	TEXTURE = 2, //贴图
	AUDIO = 3, // 声音
	ASSET_BUNDLE = 4, // AB资源
	UNITY_SCENE = 5, // 场景
    BTYES = 6 //字节数据
}


[System.Flags]
public enum ResTag
{
    None = 0x00,
    Battle = 0x01, //战斗资源
	BattleRole = 0x02,//战斗人物资源
	BattleEffect = 0x04,//战斗特效
	BattleProjectile = 0x08,//飞行道具
    BattleSceneSound = 0x09,//战斗场景音效
    BattleSceneObj = 0x10,//场景摆件-场景特效资源


    UISound = 0x50,//ui音效
    BGM = 0x51//bgm
}
