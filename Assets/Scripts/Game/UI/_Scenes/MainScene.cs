using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MainScene : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        init();
        StageManager.GotoUIStage<STG_Main>();

    }

    private void init()
    {
        Core.Setup();
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
    }
}
