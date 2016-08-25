using UnityEngine;
using System.Collections;
using System.Threading;

public class LoopThread : BasicThread{
	private OntimeRunLoop _Loop;

	public LoopThread(OntimeRunLoop loop) : base()
	{
		_Loop = loop;
	}

	public LoopThread() : base()
	{
		_Loop = new OntimeRunLoop();
	}

	protected override void Main()
	{
		while (!IsTerminateFlagSet()){
			_Loop.oneLoop();
		}
	}

	public OntimeRunLoop mainLoop()
	{
		return _Loop;
	}

	public void addExecution(OnTimeExecution execution)
	{
		_Loop.addExcutable (execution);
	}

	public void removeExecution(OnTimeExecution execution)
	{
		_Loop.removeExcutable (execution);
	}
}
