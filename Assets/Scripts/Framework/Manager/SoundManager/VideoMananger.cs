using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public class VideoSet
{
    //是否已播放
    public bool isPlayed = false;
    //视频名称
    public string vedioName = "";
}

public class VideoMananger : MonoBehaviour
{
    public static VideoMananger Instance = null;

    private bool _isPlaying = false;
    private bool _isCloseBackGroundMusic = false;
    private string _currFileName = "";
    private string _currFilePath = "";
    private VideoSet _currVedioSet = null;

    public static void Init(Transform root)
    {
        GameObject obj = new GameObject();
        obj.name = "VedioManager";
        obj.transform.parent = root;
        Instance = obj.AddComponent<VideoMananger>();
    }


    /// <summary>
    /// 播放视频
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="isNeedGuideSet">是否需要读取新手指引配置，false 必播放。true 会根据是否已播放过，播放过不再播放</param>
    /// <param name="isCloseBackGroundMusic">是否关闭背景音乐</param>
    public void Play(string fileName,bool isNeedGuideSet = false,bool isCloseBackGroundMusic = true)
    {
        if (Instance == null)
        {
            Log.infoError("请初始化 VedioMananger::Init");
            return;
        }

        if (_isPlaying)
        {
            Log.infoError("VedioMananger::Play严谨同时播放多个视频文件！！ veido: "+fileName);
            return;
        }

        _isCloseBackGroundMusic = isCloseBackGroundMusic;
        _currFileName = fileName;
        _currFilePath = "Video/" + fileName + ".mp4";

        Log.info("==========================" + _currFilePath + " exist = " + File.Exists(_currFilePath));

        if (isNeedGuideSet)
        {
            //读取本地配置
            ResDataManager.instance.ReadLocalCache<VideoSet>(fileName, OnLoadedSet);
        }
        else {      
            //开始播放

            //背景音乐是否静音    
            if (_isCloseBackGroundMusic)
                SoundManager.BgmMute = true;

            StartCoroutine(PlayMovie());
        }
    }

    private void OnLoadedSet(object data)
    {
        _currVedioSet = (VideoSet)data;
        if (_currVedioSet == null)
        {
            _currVedioSet = new VideoSet();
            _currVedioSet.vedioName = _currFileName;
            _currVedioSet.isPlayed = false;
        }

        if (!_currVedioSet.isPlayed)
        {//开始播放
            _isPlaying = true;
            StartCoroutine(PlayMovie());
        }
    }

    private IEnumerator PlayMovie()
    {
        _isPlaying = true;
        //直到播完消失
        //Handheld.PlayFullScreenMovie(_currFilePath, Color.black, FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit);
        
        //点击消失
        Handheld.PlayFullScreenMovie(_currFilePath, Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
        yield return new WaitForEndOfFrame();

        //恢复背景音乐
        if (_isCloseBackGroundMusic) SoundManager.BgmMute= false;    

        _isPlaying = false;
        if (_currVedioSet != null)
        {
            _currVedioSet.isPlayed = true;
            ResDataManager.instance.SaveToLocalCache<VideoSet>(_currVedioSet, _currFileName);
            _currVedioSet = null;
        }  
    }
}
