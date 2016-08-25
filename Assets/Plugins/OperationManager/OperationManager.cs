using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//开头的V表示Void，即没有返回值，后面的数字表示参数数量//
public delegate void VFunc();
public delegate void VFunc1<T>(T arg1);
public delegate void VFunc2<T,K>(T arg1, K arg2);

//带返回值函数模型//
public delegate T Func<T>();
public delegate T Func1<T, K>(K arg);
public delegate T Func2<T, K, L>(K arg1, L arg2);

//不推荐使用反射来实现，效率不高，大量使用可能会有性能问题
public class OperationManager  {

	static public void DoOperation(Operation operation, bool onMainThread = true)
	{
		if (onMainThread) {
			MainThread.Instance.addExcutable(operation);	
		}
		else
		{
			ApplicationEX.BackgroundThread.addExecution(operation);
		}
	}
	
	static public void DoOperationInBackground(Operation operation)
	{
		ApplicationEX.BackgroundThread.addExecution (operation);
	}
	
	static public Operation DoVoidFuncOnMainThread(VFunc func, long delay = 0, VFunc cb = null)
	{ 
		Invocation<Null,Null,Null> invocation = new Invocation<Null,Null,Null> (func, 0, null);
		Invocation<Null,Null,Null> cbInvo = cb == null ? null : new Invocation<Null,Null,Null> (cb, 0, null);
		Operation operation = new Operation (invocation, ApplicationEX.GetCurrnSystemMillisecond() + delay, cbInvo);
		//MainThread.Instance.addExcutable (operation);
		DoOperation (operation);
		return operation;
	}

	static public Operation DoVoidFuncOnMainThread<Arg1>(VFunc1<Arg1> func, Arg1 arg1, long delay = 0, VFunc cb = null)
	{
		object[] argList = {arg1};
		Invocation<Null,Arg1,Null> invocation = new Invocation<Null,Arg1,Null> (func, 1, argList);
		Invocation<Null,Null,Null> cbInvo = cb == null ? null : new Invocation<Null,Null,Null> (cb, 0, null);
		Operation operation = new Operation (invocation, ApplicationEX.GetCurrnSystemMillisecond() + delay, cbInvo);
		MainThread.Instance.addExcutable (operation);
		return operation;
	}

	static public Operation DoVoidFuncOnMainThread<Arg1,Arg2>(VFunc2<Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, long delay = 0, VFunc cb = null)
	{
		object[] argList = {arg1, arg2};
		Invocation<Null,Arg1,Arg2> invocation = new Invocation<Null,Arg1,Arg2> (func, 2, argList);
		Invocation<Null,Null,Null> cbInvo = cb == null ? null : new Invocation<Null,Null,Null> (cb, 0, null);
		Operation operation = new Operation (invocation, ApplicationEX.GetCurrnSystemMillisecond() + delay, cbInvo);
		MainThread.Instance.addExcutable (operation);
		return operation;
	}

	//异步执行func，执行结束把他的返回值作为callback的参数回调回来//
	static public Operation DoFuncOnMainThread<R>(Func<R> func, long delay = 0, VFunc1<R> callback = null)
	{
		Invocation<R,Null, Null> inv = new Invocation<R,Null, Null> (func, 0, null, true);
		Invocation<Null, R, Null> cbInv = callback == null ? null : new Invocation<Null,R, Null> (callback, 1, null);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv);
		MainThread.Instance.addExcutable (operation);
		return operation;
	}

	static public Operation DoFuncOnMainThread<R,Arg1>(Func1<R,Arg1> func, Arg1 arg, long delay = 0, VFunc1<R> callback = null)
	{
		object[] args = {arg};
		Invocation<R, Arg1, Null> inv = new Invocation<R, Arg1, Null> (func, 1, args, true);
		Invocation<Null,R, Null> cbInv = callback == null ? null : new Invocation<Null,R, Null> (callback, 1, null);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv);
		MainThread.Instance.addExcutable (operation);
		return operation;
	}

	static public Operation DoFuncOnMainThread<R,Arg1,Arg2>(Func2<R,Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, long delay = 0, VFunc1<R> callback = null)
	{
		object[] args = {arg1, arg2};
		Invocation<R, Arg1, Arg2> inv = new Invocation<R, Arg1, Arg2> (func, 2, args, true);
		Invocation<Null,R, Null> cbInv = callback == null ? null : new Invocation<Null,R, Null> (callback, 1, null);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv);
		MainThread.Instance.addExcutable (operation);
		return operation;
	}

	//*************************************************后台操作分割线********************************************************//


	static public Operation DoVoidFuncInBackground(VFunc func, VFunc cb = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoVoidFuncOnThread (ApplicationEX.BackgroundThread, func, cb, callbackOnMainThread, delay);
	}

	static public Operation DoVoidFuncInBackground<Arg>(VFunc1<Arg> func, Arg arg, VFunc callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoVoidFuncOnThread<Arg> (ApplicationEX.BackgroundThread, func, arg, callback, callbackOnMainThread, delay);
	}

	static public Operation DoVoidFuncInBackground<Arg1, Arg2>(VFunc2<Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, VFunc callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoVoidFuncOnThread<Arg1, Arg2> (ApplicationEX.BackgroundThread, func, arg1, arg2, callback, callbackOnMainThread, delay);
	}

	static public Operation DoFuncInBackground<R>(Func<R> func, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoFuncOnThread<R> (ApplicationEX.BackgroundThread, func, callback, callbackOnMainThread, delay);
	}

	static public Operation DoFuncInBackground<R, Arg>(Func1<R,Arg> func, Arg arg, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoFuncOnThread<R, Arg> (ApplicationEX.BackgroundThread, func, arg, callback, callbackOnMainThread, delay);
	}

	static public Operation DoFuncInBackground<R, Arg1, Arg2>(Func2<R,Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		return DoFuncOnThread<R, Arg1, Arg2>(ApplicationEX.BackgroundThread, func, arg1, arg2, callback, callbackOnMainThread, delay);
	}

	
	static public Operation DoVoidFuncOnThread(LoopThread loopThrad, VFunc func, VFunc callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		Invocation<Null,Null,Null> inv = new Invocation<Null, Null, Null> (func);
		Invocation<Null,Null,Null> cbInv = callback == null ? null : new Invocation<Null, Null, Null> (callback);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}

	static public Operation DoVoidFuncOnThread<Arg>(LoopThread loopThrad, VFunc1<Arg> func, Arg arg, VFunc callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		object[] args = {arg};
		Invocation<Null,Arg,Null> inv = new Invocation<Null, Arg, Null> (func, 1, args);
		Invocation<Null,Null,Null> cbInv = callback == null ? null : new Invocation<Null, Null, Null> (callback);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}

	static public Operation DoVoidFuncOnThread<Arg1, Arg2>(LoopThread loopThrad, VFunc2<Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, VFunc callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		object[] args = {arg1, arg2};
		Invocation<Null,Arg1,Arg2> inv = new Invocation<Null, Arg1, Arg2> (func, 2, args);
		Invocation<Null,Null,Null> cbInv = callback == null ? null : new Invocation<Null, Null, Null> (callback);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}


	static public Operation DoFuncOnThread<R>(LoopThread loopThrad, Func<R> func, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		Invocation<R,Null,Null> inv = new Invocation<R, Null, Null> (func, 0, null, true);
		Invocation<Null,R,Null> cbInv = callback == null ? null : new Invocation<Null, R, Null> (callback, 1);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}

	static public Operation DoFuncOnThread<R, Arg>(LoopThread loopThrad, Func1<R,Arg> func, Arg arg, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		object[] args = {arg};
		Invocation<R,Arg,Null> inv = new Invocation<R, Arg, Null> (func, 1, args, true);
		Invocation<Null,R,Null> cbInv = callback == null ? null : new Invocation<Null, R, Null> (callback, 1);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}

	static public Operation DoFuncOnThread<R, Arg1, Arg2>(LoopThread loopThrad, Func2<R,Arg1,Arg2> func, Arg1 arg1, Arg2 arg2, VFunc1<R> callback = null, bool callbackOnMainThread = true, long delay = 0)
	{
		object[] args = {arg1, arg2};
		Invocation<R,Arg1,Arg2> inv = new Invocation<R, Arg1, Arg2> (func, 2, args, true);
		Invocation<Null,R,Null> cbInv = callback == null ? null : new Invocation<Null, R, Null> (callback, 1);
		Operation operation = new Operation (inv, ApplicationEX.GetCurrnSystemMillisecond () + delay, cbInv, callbackOnMainThread);
		loopThrad.addExecution(operation);
		return operation;
	}
}
