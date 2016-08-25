using UnityEngine;
using System.Collections;

public interface IMotivated {
	
	bool isActive();
	
	void onDrived();
	
	void onShutdown();
}


public class Motor : MonoBehaviour {

	static private Motor g_motor;
	private ArrayList _machines;
	private ArrayList _addMachines;
	private ArrayList _removeMachines;

	static uint UPDATE_INTERVAL = 1;
	private uint _frameIndex = 0;
	// Use this for initialization
	void Awake()
	{
		//程序周期内不销毁//
		DontDestroyOnLoad(gameObject);

		if (g_motor == null) {
			g_motor = this;	
		}

		_machines = new ArrayList ();
		_addMachines = new ArrayList ();
		_removeMachines = new ArrayList ();
	}

	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (++_frameIndex % UPDATE_INTERVAL != 0) {
			return;	
		}

		updateMachines ();
		if(_machines==null){
			return;
		}
		int count = _machines.Count;
		IMotivated machine;
		for (int i=0; i < count; i++)// (IMotivated machine in _machines)
		{
			machine = (IMotivated)_machines[i];
			if (machine.isActive())
			{
				machine.onDrived();
			}
		}
	}

	void OnDestroy()
	{
        if (null == _machines) return;
		int count = _machines.Count;
		IMotivated machine;
		for (int i=0; i < count; i++)
		{
			machine = (IMotivated)_machines[i];
			machine.onShutdown();
		}
	}
	
	static public Motor DefaultMotor
	{
		get{
			if (g_motor == null){
				GameObject go = new GameObject("Motor");
				g_motor = go.AddComponent<Motor>();
			}

			return g_motor;
		}
	}

    public void Init(Transform applicationPluginsContainer)
    {
        g_motor.transform.parent = applicationPluginsContainer;
    }

	public void attachMachine(IMotivated motivatedMachine)
	{
		if (_machines.Contains (motivatedMachine) == false) {
			_addMachines.Add(motivatedMachine);	
		}
	}

	public void detachMachine(IMotivated motivatedMachine)
	{
		_removeMachines.Add (motivatedMachine);
	}

	private void updateMachines()
	{
        if (null == _machines) return;
		IMotivated machine;
		int count = _addMachines.Count;

		if (count > 0) {
			for (int i=0; i < count; i++)
			{
				machine = (IMotivated)_addMachines[i];
				_machines.Add(machine);
			}

			_addMachines.Clear();
		}
		
		count = _removeMachines.Count;

		if (count > 0) {
			for (int i=0; i < count; i++)
			{
				machine = (IMotivated)_removeMachines[i];
				_machines.Remove(machine);
			}

			_removeMachines.Clear();
		}
	}
}
