using UnityEngine;
using System.Collections;

public class SMFadeTransiton : BaseTransition
{
    public Texture overlayTexture;
    private float m_alpha = 0f;

    void Awake()
    {
        if (overlayTexture == null)
        {
            Debug.LogError("[SMFadeTransiton] 背景资源不存在！！");
        }
    }

    protected override bool Process(float elapsedTime)
    {
       float effectTime = elapsedTime;
        // invert direction if necessary
        if (m_state == SMTransitionState.In)
        {
            effectTime = duration - effectTime;
        }

        m_alpha = SmoothProgress(0, duration, effectTime);

        return elapsedTime < duration;
    }

    public void OnGUI()
    {
        GUI.depth = 0;
        Color c = GUI.color;
        GUI.color = new Color(1, 1, 1, m_alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);
        GUI.color = c;
    }	

    public static float SmoothProgress(float startOffset, float duration, float time)
    {
        return Mathf.SmoothStep(0, 1, Progress(startOffset, duration, time));
    }

    public static float Progress(float startOffset, float duration, float time)
    {
        return Mathf.Clamp(time - startOffset, 0, duration) / duration;
    }
}
