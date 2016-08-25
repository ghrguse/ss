using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundPlayer 
{
    private List<AudioSource> _sourcePool;
    private bool _isMute = false;
    private float _volume = 0.7f;
    private Transform _soundManagerGO;
    private int index = 0;

    public void Dispose()
    {
        AudioSource source = null;
        for (int i = 0; i < _sourcePool.Count; i++)
        {
            source = _sourcePool[i];
            source.Stop();
            source.clip = null;
            GameObject.DestroyImmediate(source);
        }
        _sourcePool.Clear();
    }

    /// <summary>
    /// Init
    /// </summary>
    /// <param name="parent"></param>
    public void Init(Transform parent)
    {
        _soundManagerGO = parent;
        _sourcePool = new List<AudioSource>();
        //默认创建一个
        this.createOneAudioSource();
    }

    //音效静音
    public bool IsMute
    {
        get { return _isMute; }
        set { 
            _isMute = value;
            AudioSource source = null;
            for (int i = 0; i < _sourcePool.Count; i++)
            {
                source = _sourcePool[i];
                if (!source.isPlaying) continue;
                source.mute = _isMute;
            }
        }
    }

    //音效音量
    public float Volume
    {
        get { return _volume; }
        set { 
            _volume = value;
            AudioSource source = null;
            for (int i = 0; i < _sourcePool.Count; i++)
            {
                source = _sourcePool[i];
                if (!source.isPlaying) continue;
                source.volume = _volume;
            }
        }
    }

    public void PlayUI(string soundName)
    {
        //静音不做处理
        if (_isMute) return;

        AudioClip clip = SoundResLib.GetAudioClip(soundName);
        if (clip == null)
        {
            string localPath = "Sounds/ui/" + soundName;
            this.LoadSoundAssetBundle(localPath, ResTag.UISound);
        }
        else {
            this.play(clip,this.getFreeAudioSource());
        }
    }

    public void PlaySkill(string soundName)
    {
        //静音不做处理
        if (_isMute) return;

        AudioClip clip = SoundResLib.GetAudioClip(soundName);
        if (clip == null)
        {
            string localPath = "Sounds/skill/" + soundName;
            this.LoadSoundAssetBundle(localPath, ResTag.BattleSceneSound);
        }
        else
        {
            this.play(clip, this.getFreeAudioSource());
        }
    }


    public void Stop(string soundName)
    {
        if (soundName == "") return;

        AudioSource source = null;
        for (int i = 0; i < _sourcePool.Count; i++)
        {
            source = _sourcePool[i];
            if (!source.isPlaying) continue;
            if (source.clip.name.ToLower() != soundName.ToLower()) continue;

            source.Stop();
            source.clip = null;
        }
    }


    /// <summary>
    /// 创建一个新的AudioSource
    /// </summary>
    /// <returns></returns>
    private AudioSource createOneAudioSource()
    {
        GameObject go = new GameObject("AudioSource_"+index);
        AudioSource source = go.AddComponent<AudioSource>();
        go.transform.parent = _soundManagerGO;
        _sourcePool.Add(source);
        index++;
        return source;
    }

    /// <summary>
    /// 获取空闲的 AudioSource
    /// </summary>
    /// <returns></returns>
    private AudioSource getFreeAudioSource()
    {
        AudioSource source = null;
        for (int i = 0; i < _sourcePool.Count; i++)
        {
            source = _sourcePool[i];
            if (!source.isPlaying)
            {//获取空闲的source
                return source;
            }
        }

        if (source == null) 
            source = this.createOneAudioSource();
        return source;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="source"></param>
    private void play(AudioClip clip,AudioSource source)
    {
        source.clip = clip;
        source.loop = false;
        source.mute = _isMute;
        source.volume = _volume;
        source.Play();
    }

    private void LoadSoundAssetBundle(string path, ResTag tag)
    {
        StResPath stPath = new StResPath(path, tag);
        ResLoadTool.Load<AudioClip>(stPath, OnSoundLoaded, null, OnSoundLoadError, stPath);
    }

    private void OnSoundLoaded(string filePath, object param = null)
    {
        StResPath stPath = (StResPath)param;
        string fileName = stPath.GetFileName();

        //获取audio，并释放ab文件
        AudioClip clip = ResDataManager.instance.CreateObjectFromCache<AudioClip>(filePath, true);
        if (clip != null)
        {
            SoundResLib.AddAudioClip(fileName, clip, stPath.tag);
            this.play(clip,this.getFreeAudioSource());
        }
    }

    private void OnSoundLoadError(string filePath, string errorInfo)
    {
        Log.infoError("音频文件加载失败!" + filePath);
    }
}
