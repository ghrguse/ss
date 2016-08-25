using UnityEngine;
using System.Collections;

public interface IExcutable {
	bool shouldCancel();
	bool canExcute();
	void excute();
}

public abstract class OnTimeExecution : IExcutable{
	public abstract long when (); //ms

	bool IExcutable.shouldCancel()
	{
		return false;
	}

	bool IExcutable.canExcute()
	{
		return when() - ApplicationEX.GetCurrnSystemMillisecond () <= 0;
	}

	void IExcutable.excute()
	{

	}
}

public class Execution : IExcutable{
	public delegate bool CancelJudgement();
	public delegate bool ExcuteJudgement();

	public CancelJudgement cancelJudgement;
	public ExcuteJudgement excuteJudgement;
	protected IInvocation _inv;
	protected bool _isCanceled = false;

	public Execution()
	{

	}

	public Execution(IInvocation inv)
	{
		_inv = inv;
	}

	public void cancel(){
		_isCanceled = true;
	}

	public void start()
	{
		MainThread.Instance.addExcutable (this);
	}

	bool IExcutable.shouldCancel()
	{
		if (cancelJudgement != null) {
			return cancelJudgement();	
		}

		return _isCanceled;
	}

	bool IExcutable.canExcute()
	{
		if (excuteJudgement != null) {
			return excuteJudgement();	
		}

		return true;
	}

	void IExcutable.excute()
	{
		if (_inv != null) {
			_inv.invoke ();	
		}
	}
}
