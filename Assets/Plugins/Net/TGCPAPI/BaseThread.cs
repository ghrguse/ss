using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public abstract class BasicThread
{
    private Thread m_thread;
    private bool m_terminateFlag;
    private System.Object m_terminateFlagMutex;

    public BasicThread()
    {
        m_thread = new Thread(ThreadProc);
        m_terminateFlag = false;
        m_terminateFlagMutex = new System.Object();
    }

    public void Run()
    {
        m_terminateFlag = false;
        m_thread.Start(this);
      
    }

    public void Abort()
    {
        m_terminateFlag = true;
        m_thread.Abort();
    }

    protected void SleepThread(int ms)
    {
        Thread.Sleep(ms);
    }

    protected static void ThreadProc(object obj)
    {
        BasicThread me = (BasicThread)obj;
        me.Main();
    }

    protected abstract void Main();

    public void WaitTermination()
    {
        m_thread.Join();
    }

    public void SetTerminateFlag()
    {
        lock (m_terminateFlagMutex)
        {
            m_terminateFlag = true;
        }
    }

    protected bool IsTerminateFlagSet()
    {
        lock (m_terminateFlagMutex)
        {
            return m_terminateFlag;
        }
    }
}
