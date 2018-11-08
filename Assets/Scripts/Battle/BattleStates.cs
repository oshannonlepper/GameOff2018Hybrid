using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleState : State
{

	public BattleContext Context { get; set; }

	public override void OnRegistered()
	{
		base.OnRegistered();
		(Owner as BattleStateMachine).OnSetContext += BattleState_OnContextUpdated;
	}

	private void BattleState_OnContextUpdated(BattleContext context)
	{
		Context = context;
	}

	public override void OnStateEnter()
	{
	}

	public override void OnStateExit()
	{
	}

	public override void UpdateState(float deltaTime)
	{
	}

}

public class BattleStateStart : BattleState
{

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		Debug.Log("Battle Start.");

		Owner.RequestState("DetermineNextTurn");
	}

}

public class BattleStateSelectAttack : BattleState
{

	private BattleCharacter _currentCharacter;
	private int _currentSelection = 0;
	private bool _finishedChoosing = false;

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());
		_finishedChoosing = false;
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_currentCharacter = null;
		_currentSelection = 0;
		_finishedChoosing = false;
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentCharacter.IsAIControlled)
		{
			// do something crazy to pick a good result
			_finishedChoosing = true;
		}
		else
		{
			// change attack selection inputs

			bool refresh = false;

			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (_currentSelection > 0)
				{
					--_currentSelection;
				}

				refresh = true;
			}

			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (_currentSelection < _currentCharacter.GetNumActions() - 1)
				{
					++_currentSelection;
				}

				refresh = true;
			}

			if (refresh)
			{
				string printString = "";
				int numActions = _currentCharacter.GetNumActions();
				for (int index = 0; index < numActions; ++index)
				{
					if (index == _currentSelection)
					{
						printString += "> ";
					}
					printString += _currentCharacter.GetActionLabel(index) + '\n';
				}
				printString += "\nChoose an action (up and down arrow keys).";
				Debug.Log(printString);
			}

			// confirm selection(s)

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_finishedChoosing = true;
			}
		}

		if (_finishedChoosing)
		{
			Debug.Log("Selecting action = " + _currentCharacter.GetActionLabel(_currentSelection));
			Context.SetCurrentAction(_currentSelection);
			Owner.RequestState("SelectTarget");
		}
	}

}

public class BattleStateSelectTarget : BattleState
{
	private List<int> _characterIDPool;
	private BattleCharacter _currentCharacter;
	private int _currentSelection = 0;
	private bool _finishedChoosing = false;
	private int _numTargets = 0;

	public BattleStateSelectTarget()
	{
		_characterIDPool = new List<int>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());

		_numTargets = Context.GetNumCharacters();
		for (int index = 0; index < _numTargets; ++index)
		{
			_characterIDPool.Add(Context.GetCharacterByIndex(index).ID);
		}

		_finishedChoosing = false;
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_characterIDPool.Clear();
		_currentCharacter = null;
		_currentSelection = 0;
		_finishedChoosing = false;
		_numTargets = 0;
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentCharacter.IsAIControlled)
		{
			// do something crazy to pick a good result
			_finishedChoosing = true;
		}
		else
		{
			// change attack selection inputs

			bool refresh = false;

			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (_currentSelection > 0)
				{
					--_currentSelection;
				}
				refresh = true;
			}

			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (_currentSelection < _numTargets - 1)
				{
					++_currentSelection;
				}
				refresh = true;
			}

			if (refresh)
			{
				string printString = "";
				foreach (int ID in _characterIDPool)
				{
					BattleCharacter character = Context.GetCharacterByID(ID);

					if (ID == _characterIDPool[_currentSelection])
					{
						printString += "> ";
					}
					printString += character.ToString() + '\n';
				}
				printString += "\nChoose a target (up and down arrow keys).";
				Debug.Log(printString);
			}

			// confirm selection(s)

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_finishedChoosing = true;
			}
		}

		if (_finishedChoosing)
		{
			Debug.Log("Selecting target = " + Context.GetCharacterByID(_characterIDPool[_currentSelection]));

			// validate target ??

			// if ok, confirm and move on to attacks

			Context.SetCurrentTarget(_characterIDPool[_currentSelection]);
			Owner.RequestState("AttackAnimation");
		}
	}

}

public class BattleStateAttackAnimation : BattleState
{

	private string _attacker = "";
	private string _target = "";
	private string _attackLabel = "";

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_attacker = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID()).Name;
		_target = Context.GetCharacterByID(Context.GetCurrentTarget()).Name;
		_attackLabel = Context.GetCurrentAction().ToString();
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_attacker = "";
		_target = "";
		_attackLabel = "";
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		Debug.Log(_attacker + " used " + _attackLabel + " on " + _target + ".");

		Owner.RequestState("DetermineNextTurn");
	}

}

public class BattleStateDetermineNextTurn : BattleState
{
	private BattleTurnCalculator _battleTurnCalculator;
	private int _nextTurn = -1;

	public BattleStateDetermineNextTurn()
	{
		_battleTurnCalculator = new BattleTurnCalculator();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_nextTurn = _battleTurnCalculator.GetNextTurn();
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		Context.SetCurrentCharacter(_nextTurn);
		Owner.RequestState("SelectAttack");
	}

}

public class BattleStateEnd : BattleState
{

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		Debug.Log("Battle ended.");
	}

}
