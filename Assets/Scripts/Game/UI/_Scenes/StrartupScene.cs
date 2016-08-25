using UnityEngine;
using System.Collections;
using DG.Tweening;

public class StrartupScene : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        init();
        StageManager.GotoUIStage<STG_Startup>();
	
	}
	
    private void init()
    {
        Core.Setup();
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
    }
}
