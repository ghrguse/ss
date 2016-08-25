using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LoginScene : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        init();
        StageManager.GotoUIStage<STG_Login>();

    }

    private void init()
    {
        Core.Setup();
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
    }
}
