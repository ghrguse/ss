using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 主要负责音频资源加载与管理
/// </summary>
public class SoundResLib{

    public static DelegateEnums.NoneParam OnPreLoadSoundsEnd = null;
    /// <summary>
    /// 音效库
    /// </summary>
    private static Dictionary<string, AudioClip> _audioDic = new Dictionary<string, AudioClip>();
    /// <summary>
    /// 资源tag对应的列表，tag记录为了后续更方便的资源管理
    /// </summary>
    private static Dictionary<string, List<string>> _tag2SoundNameList = new Dictionary<string, List<string>>();

    /// <summary>
    /// 获取音频文件
    /// </summary>
    /// <param name="soundName"></param>
    /// <returns></returns>
    public static AudioClip GetAudioClip(string soundName)
    {
        if (!_audioDic.ContainsKey(soundName)) 
            return null;

        return _audioDic[soundName];
    }

    /// <summary>
    /// 移出音频缓存文件
    /// </summary>
    /// <param name="tag"></param>
    public static void RemoveSounds(ResTag tag)
    {
        string key = tag.ToString();
        if (!_tag2SoundNameList.ContainsKey(key)) 
            return;

        List<string> list = _tag2SoundNameList[key];
        for (var i = 0; i < list.Count; i++)
        {
            if (_audioDic.ContainsKey(list[i]))
                _audioDic.Remove(list[i]); 
        }
        list.Clear();
        _tag2SoundNameList.Remove(key);
    }

    /// <summary>
    /// 缓存AudioClip
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="clip"></param>
    /// <param name="tag"></param>
    public static void AddAudioClip(string fileName, AudioClip clip, ResTag tag)
    {
        if (!_audioDic.ContainsKey(fileName))
        {
            _audioDic[fileName] = clip;
            string key = tag.ToString();
            if (!_tag2SoundNameList.ContainsKey(key))
            {
                _tag2SoundNameList[key] = new List<string>();
            }
            _tag2SoundNameList[key].Add(fileName);
        }
    }

    /// <summary>
    /// 预加载音频资源
    /// </summary>
    /// <param name="soundsList"></param>
    /// <param name="preLoadSoundsEnd"></param>
    public static void PreLoadSounds(
        List<StResPath> soundsList, 
        DelegateEnums.NoneParam preLoadSoundsEnd, 
        ResLoaderManager.DelegateLoaderProgress preLoadSoundsProgress = null)
    {
        OnPreLoadSoundsEnd = preLoadSoundsEnd;
        ResLoadTool.LoadList<AudioClip>(soundsList, "preLoadSoundList", OnPreLoadSoundComplete, preLoadSoundsProgress, null, null, null, soundsList);
    }

    /// <summary>
    /// 预加载完成
    /// </summary>
    /// <param name="listName"></param>
    /// <param name="param"></param>
    private static void OnPreLoadSoundComplete(string listName, object param)
    {
        List<StResPath> soundsList = param as List<StResPath>;
        if (soundsList != null)
        {
            for (int i = 0; i < soundsList.Count; i++)
            {
                //获取audio，并释放ab文件
                AudioClip clip = ResDataManager.instance.CreateObjectFromCache<AudioClip>(soundsList[i].path, true);
                if (clip != null)
                {//缓存
                    SoundResLib.AddAudioClip(soundsList[i].GetFileName(), clip, soundsList[i].tag);
                }
            }
        }//end if

        if (OnPreLoadSoundsEnd != null) OnPreLoadSoundsEnd();
    }
}
