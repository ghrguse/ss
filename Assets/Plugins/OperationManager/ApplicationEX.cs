using UnityEngine;
using System.Collections;
using System;

public class ApplicationEX {

	static LoopThread g_backgroundThread = null;

	static public long GetCurrnSystemMillisecond()
	{
		return DateTime.Now.Ticks / 10000;
	}

	static public LoopThread BackgroundThread
	{
		get {
			if (g_backgroundThread == null) {
				g_backgroundThread = new LoopThread();
				g_backgroundThread.Run();
			}
			
			return g_backgroundThread;
		}
	}
}
