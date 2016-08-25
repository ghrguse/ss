using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System;

public abstract class BasicLoop  {
	protected List<IExcutable> _excutableList = new List<IExcutable>();
	public abstract void oneLoop ();

	public bool isEmpty()
	{
		return _excutableList.Count == 0;
	}
	
	protected void traversalAndExcute(bool breakAtFirstUnableToExcute)
	{
		List<IExcutable> removeList = null;
		
		for (int i=0; i < _excutableList.Count; i++){
			IExcutable excutable = (IExcutable)_excutableList[i];
			
			if (!excutable.shouldCancel())
			{
				if (excutable.canExcute())
				{
					excutable.excute();

					if (removeList == null){
						removeList = new List<IExcutable>();
					}
					removeList.Add(excutable);
				}
				else
				{
					if (breakAtFirstUnableToExcute)
					{
						break;
					}
				}
			}
			else
			{
				if (removeList == null){
					removeList = new List<IExcutable>();
				}

				removeList.Add(excutable);
			}
		}

		//remove all canceled or excuted excutables
		if (removeList != null) {
			for (int i=0; i < removeList.Count; i++){
				IExcutable discardedExcutable = removeList[i];
				_excutableList.Remove(discardedExcutable);
			}	
		}
	}
}

//
public class RunLoop : BasicLoop {
	
	public void addExcutable(IExcutable excutable)
	{
		lock (_excutableList) {
			_excutableList.Add(excutable);
		}
	}

	public void removeExcutable(IExcutable excutable)
	{
		lock (_excutableList) {
			_excutableList.Remove(excutable);
		}
	}
	
	override public void oneLoop()
	{
		lock (_excutableList) {
			if (_excutableList.Count != 0)
			{
				traversalAndExcute(false);
			}
		}
	}
}

//基于执行时间来驱动的runLoop，时间未到时会将线程挂起.
public class OntimeRunLoop : BasicLoop{

	private ManualResetEvent maualResetEvent = new ManualResetEvent(true);
	
    public void addExcutable(OnTimeExecution excutable)
	{
		lock (_excutableList) {
			
			bool inserted = false;
			
			//sort by time asc.
			for (int i=0; i < _excutableList.Count; i++)
			{
				if (excutable.when() < ((OnTimeExecution)_excutableList[i]).when())
				{
					_excutableList.Insert(i, excutable);
					inserted = true;
					break;
				}
			}
			
			if (inserted == false)
			{
				_excutableList.Add(excutable);
			}
			
			maualResetEvent.Set ();
		}
	}

	public void removeExcutable(OnTimeExecution excutable)
	{
		lock (_excutableList) {
			_excutableList.Remove(excutable);
		}
	}
	
	override public void oneLoop()
	{
		int timeIntervalToNextLoop = 0;

		lock (_excutableList) {
			if (_excutableList.Count != 0)
			{
				traversalAndExcute(true);
			}

			if (_excutableList.Count > 0){
				OnTimeExecution execution = (OnTimeExecution)_excutableList[0];
				if (execution != null)
				{
					timeIntervalToNextLoop = (int)(execution.when() - ApplicationEX.GetCurrnSystemMillisecond());
				}
			}

			maualResetEvent.Reset();	
		}

		if (timeIntervalToNextLoop > 0)
		{
			maualResetEvent.WaitOne(timeIntervalToNextLoop);
		}
		else
		{
			maualResetEvent.WaitOne();
		}
	}
}
