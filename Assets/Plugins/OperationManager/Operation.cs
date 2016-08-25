using UnityEngine;
using System.Collections;

public class Operation : OnTimeExecution , IExcutable{
	
	private long _when = 0;
	private IInvocation _inv;
	private IInvocation _cb;
	private bool _bCallbackOnMainThread; 
	protected bool _isCanceled = false;
	
	public Operation(IInvocation invocation, long when = 0, IInvocation callback = null, bool callbackOnMainThread = false)
	{
		_inv = invocation;
		_when = when;
		_cb = callback;
		_bCallbackOnMainThread = callbackOnMainThread;
	}
	
	bool IExcutable.shouldCancel()
	{
		return _isCanceled;
	}
	
	bool IExcutable.canExcute()
	{
		return _when - ApplicationEX.GetCurrnSystemMillisecond () <= 0;
	}
	
	void IExcutable.excute()
	{
		if (_inv == null) {
            Log.infoError("Fatal error, invacation is null!");
			return;	
		}

		_inv.invoke ();
		object result = _inv.getReturnValue();

		if (_cb == null) {
			return;	
		}
		
		if (_bCallbackOnMainThread) {
			OperationManager.DoVoidFuncOnMainThread<object>(callbackWithResult, result);	
		}
		else
		{
			callbackWithResult(result);
		}
	}
	
	override public long when()
	{
		return _when;
	}

	public void cancel()
	{
		_isCanceled = true;
	}
	
	private void callbackWithResult(object result)
	{
		if (_cb != null) {
			int argNum = _cb.getArgNum();

			if (argNum == 1)
			{
				object[] args = {result};
				_cb.setArgs(args);
			}
			else if (argNum != 0)
			{
                Log.infoError("Unsupported callback arg count!");
			}
			
			_cb.invoke();
		}
	}
	
}
