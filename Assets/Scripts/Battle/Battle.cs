using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{

	private BattleStateMachine _battleStateMachine;

	public Battle()
	{
		_battleStateMachine = new BattleStateMachine();
		_battleStateMachine.RegisterState("Start", new BattleStateStart());
		_battleStateMachine.RegisterState("SelectAttack", new BattleStateSelectAttack());
		_battleStateMachine.RegisterState("SelectTarget", new BattleStateSelectTarget());
		_battleStateMachine.RegisterState("AttackAnimation", new BattleStateAttackAnimation());
		_battleStateMachine.RegisterState("DetermineNextTurn", new BattleStateDetermineNextTurn());
		_battleStateMachine.RegisterState("End", new BattleStateEnd());
	}

	public void BeginEncounter(/* TODO - params, probably want participants */)
	{
		_battleStateMachine.OnStateChanged += _battleStateMachine_OnStateChanged;

		BattleContext context = new BattleContext();

		BattleCharacter charA = new BattleCharacter();
		charA.Name = "Snake";

		BattleAction actionWriggle = new BattleAction();
		actionWriggle.Label = "Wriggle";

		BattleAction actionBite = new BattleAction();
		actionBite.Label = "Bite";

		BattleAction actionHiss = new BattleAction();
		actionHiss.Label = "Hiss";

		charA.AddAction(actionWriggle);
		charA.AddAction(actionBite);
		charA.AddAction(actionHiss);

		BattleCharacter charB = new BattleCharacter();
		charB.Name = "Bear";

		BattleAction actionRoar = new BattleAction();
		actionRoar.Label = "Roar";

		BattleAction actionSlash = new BattleAction();
		actionSlash.Label = "Slash";

		BattleAction actionStomp = new BattleAction();
		actionStomp.Label = "Stomp";

		BattleAction actionSleep = new BattleAction();
		actionSleep.Label = "Sleep";

		charB.AddAction(actionRoar);
		charB.AddAction(actionSlash);
		charB.AddAction(actionStomp);
		charB.AddAction(actionSleep);

		context.AddCharacter(charA);
		context.AddCharacter(charB);

		_battleStateMachine.SetContext(context);
		_battleStateMachine.RequestState("Start");
	}

	private void _battleStateMachine_OnStateChanged(string oldState, string newState)
	{
		if (newState.Equals("End"))
		{
			EndEncounter();
		}
	}

	public void EndEncounter(/* TODO - params, encounter outcome (victory/defeat) ? */)
	{

	}

	public void UpdateBattle(float deltaTime)
	{
		_battleStateMachine.UpdateStateMachine(deltaTime);
	}

}

public class BattleStateMachine : StateMachine
{

	public delegate void BattleStateMachineEvent(BattleContext context);
	public event BattleStateMachineEvent OnSetContext;

	private BattleContext _context;

	public void SetContext(BattleContext inContext)
	{
		_context = inContext;
		if (OnSetContext != null)
		{
			OnSetContext(_context);
		}
	}

}

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
				if (_currentSelection < _currentCharacter.GetNumActions()-1)
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
				if (_currentSelection < _numTargets-1)
				{
					++_currentSelection;
				}
				refresh = true;
			}

			if (refresh)
			{
				string printString = "";
				int numCharacters = Context.GetNumCharacters();
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

		Debug.Log(_attacker + " used " + _attackLabel + " on " + _target+".");

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

public class BattleTurnCalculator
{
	public int GetNextTurn()
	{
		return 0;
	}
}

public class BattleContext
{
	private List<BattleCharacter> _characters;
	private int _currentTurn = 0;
	private int _currentAction = 0;
	private int _currentTarget = 0;

	public BattleContext()
	{
		_characters = new List<BattleCharacter>();
	}

	public int AddCharacter(BattleCharacter character)
	{
		int id = _characters.Count;
		character.ID = id;
		_characters.Add(character);
		return id;
	}

	public bool RemoveCharacter(int characterID)
	{
		return _characters.Remove(GetCharacterByID(characterID));
	}

	public int GetNumCharacters()
	{
		return _characters.Count;
	}

	public BattleCharacter GetCharacterByID(int id)
	{
		foreach (BattleCharacter character in _characters)
		{
			if (character.ID == id)
			{
				return character;
			}
		}

		Debug.LogError("Unable to find character with id = " + id);
		return null;
	}

	public BattleCharacter GetCharacterByIndex(int index)
	{
		if (index >= 0 && index < GetNumCharacters())
		{
			return _characters[index];
		}

		Debug.LogError("Invalid index specified: " + index + ", (max = " + GetNumCharacters() + ")");
		return null;
	}

	public BattleAction GetCurrentAction()
	{
		return GetCharacterByID(_currentTurn).GetAction(_currentAction);
	}

	public int GetCurrentTarget()
	{
		return _currentTarget;
	}

	public int GetCurrentTurnCharacterID()
	{
		return _currentTurn;
	}

	public void SetCurrentCharacter(int currentCharacter)
	{
		_currentTurn = currentCharacter;
	}

	public void SetCurrentAction(int actionIndex)
	{
		_currentAction = actionIndex;
	}

	public void SetCurrentTarget(int currentTarget)
	{
		_currentTarget = currentTarget;
	}

}

public class BattleCharacter
{
	public string Name { get; set; }
	public int ID { get; set; }
	public bool IsAIControlled = false;

	private List<BattleAction> _actions;
	private int _numActions = 0;

	public BattleCharacter()
	{
		_actions = new List<BattleAction>();
	}

	public void AddAction(BattleAction action)
	{
		_actions.Add(action);
		++_numActions;
	}

	public int GetNumActions()
	{
		return _numActions;
	}

	public BattleAction GetAction(int index)
	{
		if (index < 0 || index >= GetNumActions())
		{
			Debug.LogError("Invalid action index (id = " + index + ", max = " + GetNumActions() + ")");
			return null;
		}
		else
		{
			return _actions[index];
		}
	}

	public string GetActionLabel(int index)
	{
		BattleAction action = GetAction(index);
		return action == null ? "[ Null Action ]" : action.ToString();
	}
	
	public string ToString()
	{
		return Name;
	}

}

public class BattleAction
{
	public string Label { get; set; }

	public override string ToString()
	{
		return Label;
	}
}
