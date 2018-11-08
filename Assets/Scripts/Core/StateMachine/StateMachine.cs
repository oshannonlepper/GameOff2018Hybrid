using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
	public State() { }

	protected StateMachine Owner;

	public void SetOwner(StateMachine InOwner) { Owner = InOwner; }
	public virtual void OnRegistered() { }
	public abstract void OnStateEnter();
	public abstract void OnStateExit();
	public abstract void UpdateState(float deltaTime);
}

public class StateMachine
{

	public delegate void StateEvent(string oldState, string newState);
	public event StateEvent OnStateChanged;

	private List<string> _pendingStateRequests;
	private Dictionary<string, State> _stateMap;
	private string _currentState = "";
	private int _numStates = 0;

	public StateMachine()
	{
		_pendingStateRequests = new List<string>();
		_stateMap = new Dictionary<string, State>();
	}

	public virtual void RegisterState(string name, State inState)
	{
		_stateMap.Add(name, inState);
		inState.SetOwner(this);
		inState.OnRegistered();
		++_numStates;
	}

	public void UpdateStateMachine(float deltaTime)
	{
		if (IsValidState(_currentState))
		{
			_stateMap[_currentState].UpdateState(deltaTime);
		}

		string previousState = _currentState;
		while (_pendingStateRequests.Count > 0)
		{
			string incomingStateName = _pendingStateRequests[0];

			if (incomingStateName != _currentState && IsValidState(incomingStateName))
			{
				if (IsValidState(_currentState))
				{
					State currentState = _stateMap[_currentState];
					currentState.OnStateExit();
				}

				_currentState = incomingStateName;

				State newState = _stateMap[_currentState];
				newState.OnStateEnter();
			}

			_pendingStateRequests.RemoveAt(0);
		}

		if (!previousState.Equals(_currentState) && OnStateChanged != null)
		{
			OnStateChanged(previousState, _currentState);
		}
	}

	public void RequestState(string stateName)
	{
		if (!IsValidState(stateName))
		{
			Debug.LogError("Attempting to request an invalid state (" + stateName + ")");
			return;
		}

		_pendingStateRequests.Add(stateName);
	}

	public bool IsValidState(string stateName)
	{
		return _stateMap.ContainsKey(stateName);
	}

}
