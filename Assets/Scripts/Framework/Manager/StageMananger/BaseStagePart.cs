using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// Stage子对象基类
/// 注意，执行顺序：Awake > AddEventListeners > Init > Start
/// </summary>
public class BaseStagePart : AbstractView
{
    protected float effectTime_Enter = 0.2f;
    protected float effectTime_Leave = 0.2f;

    private DelegateEnums.NoneParam fn;
    private DelegateEnums.DataParam fnData;
	private DelegateEnums.NoneParam levelFn;



    override public void Dispose()
    {
        fn = null;
        fnData = null;
        levelFn = null;
        base.Dispose();
    }

    public override void Init() { }

    /// <summary>
    /// 延迟初始化(入场效果结束后初始化)
    /// </summary>
    public virtual void LaterInit() { }

    /// <summary>
    /// Resource资源，AB资源手动卸载
    /// </summary>
    public override void Unload() { }
    /// <summary>
    /// 根据实际需要，清除缓动 
    /// </summary>
    public virtual void TweenClear() { }
    /// <summary>
    /// 入场效果，如果重写，需要先设定特效时间，否则立刻触发效果结束
    /// </summary>
    protected virtual void EnterEffect() { }

    /// <summary>
    /// 离场效果，如果重写，需要先设定特效时间，否则立刻触发效果结束
    /// </summary>
    protected virtual void LeaveEffect() { }

    /// <summary>
    /// 设置缓动效果所用时间
    /// </summary>
    /// <param name="value"></param>
    public void SetEffectTime(float value)
    {
        effectTime_Enter = effectTime_Leave = value;
    }
    /// <summary>
    /// 执行入场效果
    /// </summary>
    /// <param name="fn"></param>
    public void ExecEnterEffect(DelegateEnums.NoneParam fn)
    {
        this.fn = fn;
        if (effectTime_Enter > 0)
        {
            EnterEffect();
            ToolKit.coroutineDelay(effectTime_Enter, OnEnterEffectOver);
        }
        else {
            fn();
        }
    }
    /// <summary>
    /// 执行入场效果
    /// </summary>
    /// <param name="fn"></param>
    public void ExecEnterEffect(DelegateEnums.DataParam fn)
    {
        this.fnData = fn;
        if (effectTime_Enter > 0)
        {
            EnterEffect();
            ToolKit.coroutineDelay(effectTime_Enter, OnEnterEffectOver);
        }
        else
        {
            this.fnData(this);
        }
    }
    /// <summary>
    /// 执行出场效果
    /// </summary>
    /// <param name="fn"></param>
    public void ExecLeaveEffect(DelegateEnums.NoneParam fn)
    {
		this.levelFn = fn;
        if (effectTime_Leave > 0)
        {
            LeaveEffect();
            ToolKit.coroutineDelay(effectTime_Leave, OnLeaveEffectOver);
        }
        else
        {
			levelFn();
		}
    }

    private void OnLeaveEffectOver()
    {
        this.TweenClear();
		if(levelFn!=null)
			this.levelFn();
	}

    internal void OnEnterEffectOver()
    {
        if (fn != null)
        {
            fn();
            fn = null;
        }
        if (fnData != null)
        {
            fnData(this);
            fnData = null;
        }
        LaterInit();
    }
}