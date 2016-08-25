using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using Utils.Event;

public class GameTimeModel : Model {

    //开服时间
    private DateTime _openServerTime;
    //最新同步的服务器时间
    private DateTime _lastServerTime;

    private DateTime _lastTime;

    //当前实例测量
    private Stopwatch _stopwatch = new Stopwatch();


    override public void Dispose()
    {
        base.Dispose();
    }

    protected override void Init()
    {
        base.Init();

        _lastServerTime = DateTime.Now;
        _lastTime = this.LocalToServerTime;
    }

    //标准服务器时间 = 收包后流逝的时间 + 最近一次的服务器时间
    public DateTime LocalToServerTime
    {
        get
        {
            if (_lastServerTime == null || _stopwatch == null)
                return DateTime.Now;

            return new DateTime(_stopwatch.ElapsedTicks + _lastServerTime.Ticks);
        }
    }

    public DateTime OpenServerTime
    {
        get{ return _openServerTime; }
    }

    /// <summary>
    /// 获取开服那天日期
    /// </summary>
    /// <returns></returns>
    public int getOpenServerDays()
    {
        TimeSpan span = this.LocalToServerTime - this.OpenServerTime;
        return span.Days;
    }

    /// <summary>
    /// 设置开服时间
    /// </summary>
    /// <param name="time"></param>
    public void SetOpenServerTime(DateTime time)
    {
        _openServerTime = time;
    }

    /// <summary>
    /// 设置最新的服务器时间
    /// </summary>
    /// <param name="lastTime"></param>
    public void SetServerTime(DateTime lastTime)
    {
        _stopwatch.Reset();
        _stopwatch.Start();

        _lastServerTime = lastTime;
        //GameSysTime.UpdateLocalToServerTime(this.LocalToServerTime);

        //Log.info("receive Heartbeat 服务器时间:" + serverDateTime.ToLongTimeString() + "  本地换算服务器时间:" + this.LocalToServerTime);

        DateTime nowTime = this.LocalToServerTime;
        if (nowTime.Minute <= 1)
        {
            if (_lastTime.Hour != nowTime.Hour)
            {
                if (_lastTime.Hour < 6 && nowTime.Hour >= 6)
                {//6点刷新报时
                    EventManager.Send(EventType_GameTime.GAME_DAY_CLOCK_TIME);
                }
                _lastTime = nowTime;
                //整点报时
                EventManager.Send(EventType_GameTime.GAME_OCLOCK_TELL_TIME, nowTime);
            }
        }
        //时间同步
        EventManager.Send(EventType_GameTime.UPDATE_SERVER_TIME);
    }
}



