using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class NetWorkThread : BasicThread
{
    
    static System.DateTime updateTime = DateTime.Parse("1970-1-1");
    public NetWorkThread() : base()
    {
        interval = 0;
    }
    protected override void Main() 
    {
        //注释原因，将其移到主线程执行
        //while (!IsTerminateFlagSet())
        //{
        //    TimeSpan ts = DateTime.Now - updateTime;
        //    int Milliseconds = ts.Milliseconds;
        //    //0.1秒读取一次，减少CUP消耗
        //    if (Milliseconds < 20)
        //    {
        //        SleepThread(1);
        //        continue;
        //    }
        //    updateTime = DateTime.Now;
        //    Net.Instance.NetMainLoop();
            
        //    //Net.Instance.SessionPVPRecv();
          
        //}

        //if (IsTerminateFlagSet())
        //{
        //    Net.Instance.ClearPackage();
        //}
    }

    //更新间隔
    private float interval;
}

