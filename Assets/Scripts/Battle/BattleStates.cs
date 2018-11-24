using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleState : State
{

	public BattleContext Context { get; set; }

	public override void OnRegistered()
	{
		base.OnRegistered();
		BattleStateMachine.OnSetContext += BattleState_OnContextUpdated;
	}

	private void BattleState_OnContextUpdated(BattleStateMachine machine, BattleContext context)
	{
		if (machine == Owner)
		{
			Context = context;
		}
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

	private BattleCharacterInstance _currentCharacter;
	private int _currentSelection = 0;
	private bool _finishedChoosing = false;

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());
		_finishedChoosing = false;

		if (!_currentCharacter.IsAIControlled)
		{
			PrintAttacks();
		}
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
			int numActions = _currentCharacter.GetNumActions();
			_currentSelection = Random.Range(0, numActions);
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
				PrintAttacks();
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

	private void PrintAttacks()
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
		Debug.Log(printString);
		Debug.Log("Choose an action (up and down arrow keys).");
	}

}

public class BattleStateSelectTarget : BattleState
{
	private List<int> _characterIDPool;
	private BattleCharacterInstance _currentCharacter;
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

		if (!_currentCharacter.IsAIControlled)
		{
			PrintTargets();
		}
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
			List<int> targets = _characterIDPool.FindAll(x => x != _currentCharacter.ID);
			// do something crazy to pick a good result
			_currentSelection = targets[Random.Range(0, targets.Count)]; // please don't hit yourself please
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
				PrintTargets();
			}

			// confirm selection(s)

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_finishedChoosing = true;
			}
		}

		if (_finishedChoosing)
		{
			Debug.Log("Selecting target = " + Context.GetCharacterByID(_characterIDPool[_currentSelection]).ToString());

			// validate target ??

			// if ok, confirm and move on to attacks

			Context.SetCurrentTarget(_characterIDPool[_currentSelection]);
			Owner.RequestState("ResolveAttack");
		}
	}
	
	void PrintTargets()
	{
		string printString = "";
		foreach (int ID in _characterIDPool)
		{
			BattleCharacterInstance character = Context.GetCharacterByID(ID);

			if (ID == _characterIDPool[_currentSelection])
			{
				printString += "> ";
			}
			printString += character.ToString();

			if (ID == _currentCharacter.ID)
			{
				printString += " (Yourself)";
			}

			printString += '\n';
		}
		Debug.Log(printString);
		Debug.Log("Choose a target (up and down arrow keys).");
	}

}

public class BattleStateResolveAttack : BattleState
{

	private bool _finishedResolving = false;

	private BattleCharacterInstance _currentCharacter;
	private BattleCharacterInstance _targetCharacter;
	private BattleActionInstance _currentAction;

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_finishedResolving = false;
		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());
		_targetCharacter = Context.GetCharacterByID(Context.GetCurrentTarget());
		_currentAction = Context.GetCurrentAction();
		_currentAction.OnBeginAction(_currentCharacter, _targetCharacter);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_currentAction.OnEndAction(_currentCharacter, _targetCharacter);
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentAction.ResolveAction())
		{
			Context.ActionPerformed(_currentCharacter, _targetCharacter, _currentAction);
			_finishedResolving = true;
		}

		if (_finishedResolving)
		{
			Owner.RequestState("AttackAnimation");
		}
	}
}

public class BattleStateAttackAnimation : BattleState
{

	private string _attacker = "";
	private string _target = "";
	private string _attackLabel = "";
	private float _waitTime = 2.0f;
	private float _startTime = 0.0f;

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_attacker = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID()).Name;
		_target = Context.GetCharacterByID(Context.GetCurrentTarget()).Name;
		_attackLabel = Context.GetCurrentAction().ToString();

		UIBattleText.OnTextQueueExhausted += UIBattleText_OnTextQueueExhausted;
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_attacker = "";
		_target = "";
		_attackLabel = "";

		UIBattleText.OnTextQueueExhausted -= UIBattleText_OnTextQueueExhausted;
	}

	private void UIBattleText_OnTextQueueExhausted()
	{
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

		_battleTurnCalculator.TryInit(Context);

		// mark any fallen characters as out

		for (int characterID = 0; characterID < Context.GetNumCharacters(); ++characterID)
		{
			if (Context.GetCharacterByID(characterID).HP <= 0)
			{
				Context.MarkCharacterOut(characterID);
			}
		}

		_nextTurn = _battleTurnCalculator.GetNextTurn(Context);
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		Context.SetCurrentCharacter(_nextTurn);
		
		if (Context.GetActiveCharacterCount() <= 1)
		{
			// if we are down to one (or 0) character, end the battle
			Owner.RequestState("End");
		}
		else
		{
			// if not, go to select attack phase
			Owner.RequestState("SelectAttack");
		}
	}

}

public class BattleStateEnd : BattleState
{

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		Debug.Log("Battle ended, " + Context.GetCharacterByID(Context.GetCurrentTurnCharacterID()).Name + " was victorious.");
	}

}
