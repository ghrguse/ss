using UnityEngine;
using System.Collections;

public class Null {
	//
}

public interface IInvocation {
	void invoke ();
	
	object getReturnValue();
	
	int getArgNum ();
	
	void setArgs (object[] argList);
}

//R:返回值类型
//Arg1:第一个参数类型
//Arg2：第二个参数类型.
public class Invocation<R,Arg1,Arg2> : IInvocation
{
	private bool _hasReturnValue = false;
	private int _argCount;
	private object[] _argList;
	private object _returnValue = null;
	
	private object _func;
	
	public Invocation(object f, int argCount = 0, object[] argList = null, bool hasRet = false)
	{
		_func = f;
		_argCount = argCount;
		_argList = argList;
		_hasReturnValue = hasRet;
	}
	
	void IInvocation.invoke()
	{
		if (avalabilityCheck() == false) {
			return;	
		}

		if (_hasReturnValue) {
			R obj = default(R);
			switch (_argCount){
			case 0:
				Func<R> f = (Func<R>)_func;
				obj = f();
				break;
				
			case 1:
				Func1<R,Arg1> f1 = (Func1<R,Arg1>)_func;
				obj = f1((Arg1)_argList[0]);
				break;
				
			case 2:
				Func2<R,Arg1,Arg2> f2 = (Func2<R,Arg1,Arg2>)_func;
				obj = f2((Arg1)_argList[0], (Arg2)_argList[1]);
				break;
				
			default:
                Log.infoError("Function with " + _argCount + " arguments is not supported!");
				break;
			}	
			
			_returnValue = (object)obj;
		}
		else
		{
			switch (_argCount){
			case 0:
				VFunc vf = (VFunc)_func;
				vf();
				break;

			case 1:
				VFunc1<Arg1> vf1 = (VFunc1<Arg1>)_func;
				vf1((Arg1)_argList[0]);
				break;

			case 2:
				VFunc2<Arg1, Arg2> vf2  = (VFunc2<Arg1, Arg2>)_func;
				vf2((Arg1)_argList[0], (Arg2)_argList[1]);
				break;

			default:
                Log.infoError("Function with " + _argCount + " arguments is not supported!");
				break;
			}
		}
	}
	
	object IInvocation.getReturnValue()
	{
		return _returnValue;
	}
	
	int IInvocation.getArgNum()
	{
		return _argCount;
	}

	void IInvocation.setArgs (object[] argList)
	{
		_argList = argList;
	}

	private bool avalabilityCheck()
	{
		if (_func == null) {
			Log.info("Warning: func is null!");
			return false;	
		}

		int actualArgCount = _argList == null ? 0 : _argList.Length;
		if (_argCount > actualArgCount)
		{
            Log.infoError("Fatal error: argument count is not match method!");
			return false;
		}

		return true;
	}
}