using UnityEngine;
using System.Collections;

public class MainThread : IMotivated {

	private static MainThread g_mainThread = null;
	private RunLoop _runLoop = null;
	
	public static MainThread Instance
	{
		get 
		{
			if (null == g_mainThread)
			{
				g_mainThread = new MainThread();
			}
			
			return g_mainThread;
		}
	}

	private MainThread()
	{
		_runLoop = new RunLoop();
		Motor.DefaultMotor.attachMachine(this);
	}

	public void addExcutable(IExcutable excutable)
	{
		_runLoop.addExcutable (excutable);
	}

	public void removeExcutable(IExcutable excutable)
	{
		_runLoop.removeExcutable (excutable);
	}

	bool IMotivated.isActive()
	{
		return !_runLoop.isEmpty();
	}
	
	void IMotivated.onDrived()
	{
		_runLoop.oneLoop ();
	}
	
	void IMotivated.onShutdown()
	{

	}
}
