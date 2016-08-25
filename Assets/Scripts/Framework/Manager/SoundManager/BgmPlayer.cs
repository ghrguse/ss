using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 背景音乐播放器
/// 默认循环播放，同时只播放一首
/// </summary>
public class BgmPlayer 
{
    private bool _isMute = false;
    private float _volume = 0.7f;
    private bool _isLoop = true;

    private AudioSource _audioSource;

    public void Dispose()
    {
        this.Stop();

        if (_audioSource != null)
        {
            GameObject.Destroy(_audioSource);
            _audioSource = null;
        }
    }
    
    /// <summary>
    /// Init
    /// </summary>
    /// <param name="parent"></param>
    public void Init(Transform parent)
    {
        GameObject go = new GameObject("BgmAudioSource");
        this._audioSource = go.AddComponent<AudioSource>();
        go.transform.parent = parent;
    }

    //背景音乐静音
    public bool IsMute
    {
        get { return _isMute; }
        set { 
            _isMute = value;
            if (_audioSource != null) _audioSource.mute = _isMute;
        }
    }
    
    //音乐音量
    public float Volume
    {
        get { return _volume; }
        set { 
            _volume = value;
            if (_audioSource != null) _audioSource.volume = _volume;
        }
    }
    
    public void Play(string bgmName)
    {
        this.Stop();
        
        //静音不做处理
        if (_isMute) return;

        AudioClip clip =  SoundResLib.GetAudioClip(bgmName);
        if (clip == null)
        {
            string localPath = "Sounds/bgm/" + bgmName;
            this.LoadSoundAssetBundle(localPath,ResTag.BGM);
        }
        else {
            this.playBGM(clip);
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void Stop()
    {
        if (_audioSource == null)
            return;

        _audioSource.Stop();
        if (_audioSource.clip != null)
        {
            GameObject.DestroyImmediate(_audioSource.clip,true);
            _audioSource.clip = null;
        }
    }

    private void playBGM(AudioClip clip)
    {
        if (_audioSource == null) return;
        _audioSource.clip = clip;
        _audioSource.loop = _isLoop;
        _audioSource.mute = _isMute;
        _audioSource.volume = _volume;
        _audioSource.Play();
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
            this.playBGM(clip);
        } 
    }

    private void OnSoundLoadError(string filePath, string errorInfo)
    {
        Log.infoError("音频文件加载失败!" + filePath);
    }
}
