using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音效管理器
/// </summary>
public class SoundManager : MonoBehaviour {

    private static SoundManager _instance = null;
    private static BgmPlayer _bgmPlayer = null;
    private static SoundPlayer _soundPlayer = null;


    public static void Dispose()
    {
        if (_bgmPlayer != null)
        {
            _bgmPlayer.Dispose();
            _bgmPlayer = null;
        }

        if (_soundPlayer != null)
        {
            _soundPlayer.Dispose();
            _soundPlayer = null;
        }
    }

    /// <summary>
    /// 初始化，挂载舞台上
    /// </summary>
    /// <param name="root"></param>
    public static void InitRoot(Transform root)
    {
        if (_instance != null) return;

        GameObject go = new GameObject();
        go.name = "SoundManager";
        go.transform.parent = root;
        _instance = go.AddComponent<SoundManager>();

        _bgmPlayer = new BgmPlayer();
        _bgmPlayer.Init(go.transform);

        _soundPlayer = new SoundPlayer();
        _soundPlayer.Init(go.transform);
    }

    //音效静音
    public static bool SoundMute
    {
        get { return _soundPlayer.IsMute; }
        set { _soundPlayer.IsMute = value; }
    }
    //背景音乐静音
    public static bool BgmMute
    {
        get { return _bgmPlayer.IsMute; }
        set { _bgmPlayer.IsMute = value; }
    }
    //音效音量
    public static float SoundVolume
    {
        get { return _soundPlayer.Volume; }
        set { _soundPlayer.Volume = value; }
    }
    //音乐音量
    public static float BgmVolume
    {
        get { return _bgmPlayer.Volume; }
        set { _bgmPlayer.Volume = value; }
    }

    /// <summary>
    /// 播放BGM,默认循环播放
    /// </summary>
    /// <param name="bgmName">文件名</param>
    public static void PlayBGM(string bgmName)
    {
        _bgmPlayer.Play(bgmName);
    }

    /// <summary>
    /// 停止BGM
    /// </summary>
    public static void StopBGM()
    {
        _bgmPlayer.Stop();
    }

    /// <summary>
    /// 播放UI音效
    /// </summary>
    /// <param name="soundName">文件名</param>
    public static void PlayUISound(string soundName)
    {
        _soundPlayer.PlayUI(soundName);
    }

    /// <summary>
    /// 播放技能音效
    /// </summary>
    /// <param name="soundName"></param>
    public static void PlaySkillSound(string soundName)
    {
        _soundPlayer.PlaySkill(soundName);
    }

    /// <summary>
    /// 停止UI音效
    /// </summary>
    /// <param name="soundName"></param>
    public static void StopSound(string soundName)
    {
        _soundPlayer.Stop(soundName);
    }
}
