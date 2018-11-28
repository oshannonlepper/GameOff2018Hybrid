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

public class MenuItemAction : IMenuItemSelectorItem
{

	private BattleActionInstance _action;

	public BattleActionInstance Action
	{
		get
		{
			return _action;
		}
	}

	public MenuItemAction(BattleActionInstance inAction)
	{
		_action = inAction;
	}

	public string GetItemLabel()
	{
		return _action.Label;
	}

	public int GetValue()
	{
		return -1;
	}

	public void OnSelect()
	{

	}

}

public class MenuItemCharacter : IMenuItemSelectorItem
{

	private BattleCharacterInstance _character;

	public BattleCharacterInstance Character
	{
		get
		{
			return _character;
		}
	}

	public MenuItemCharacter(BattleCharacterInstance inCharacter)
	{
		_character = inCharacter;
	}

	public string GetItemLabel()
	{
		return Character.Name;
	}

	public int GetValue()
	{
		return Character.ID;
	}

	public void OnSelect()
	{

	}

}

public class BattleStateSelectAttack : BattleState
{
	private MenuItemSelector _menuItemSelector;
	private BattleCharacterInstance _currentCharacter;

	public BattleStateSelectAttack() : base()
	{
		_menuItemSelector = new MenuItemSelector();
		_menuItemSelector.SetCaption("What will you do?");
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();
		
		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());

		if (!_currentCharacter.IsAIControlled)
		{
			_menuItemSelector.OnItemSelected += OnMenuItemSelected;

			int numActions = _currentCharacter.GetNumActions();
			for (int actionIndex = 0; actionIndex < numActions; ++actionIndex)
			{
				_menuItemSelector.AddItem(new MenuItemAction(_currentCharacter.GetAction(actionIndex)));
			}

			Context.RequestMenu(_menuItemSelector);
		}
	}

	private void OnMenuItemSelected(int index, int value=-1)
	{
		Context.SetCurrentAction(index);
		Owner.RequestState("SelectTarget");
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		if (!_currentCharacter.IsAIControlled)
		{
			_menuItemSelector.OnItemSelected -= OnMenuItemSelected;
			_menuItemSelector.Reset();
		}

		_currentCharacter = null;
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentCharacter.IsAIControlled)
		{
			// TODO - do something crazy to pick a good result
			int numActions = _currentCharacter.GetNumActions();
			OnMenuItemSelected(Random.Range(0, numActions));
		}
		else
		{
			_menuItemSelector.Update(deltaTime);
		}
	}
}

public class BattleStateSelectTarget : BattleState
{
	private MenuItemSelector _menuItemSelector;

	private List<int> _characterIDPool;
	private BattleCharacterInstance _currentCharacter;

	public BattleStateSelectTarget() : base()
	{
		_menuItemSelector = new MenuItemSelector();
		_menuItemSelector.SetCaption("Now choose a target...");

		_characterIDPool = new List<int>();
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_currentCharacter = Context.GetCharacterByID(Context.GetCurrentTurnCharacterID());

		int numCharacters = Context.GetNumCharacters();

		if (!_currentCharacter.IsAIControlled)
		{
			// TODO - action should have a way of returning valid targets - assume all targets viable for all actions for now
			_menuItemSelector.OnItemSelected += OnMenuItemSelected;

			for (int index = 0; index < numCharacters; ++index)
			{
				MenuItemCharacter characterItem = new MenuItemCharacter(Context.GetCharacterByIndex(index));
				_menuItemSelector.AddItem(characterItem);
			}

			Context.RequestMenu(_menuItemSelector);
		}
		else
		{
			for (int index = 0; index < numCharacters; ++index)
			{
				_characterIDPool.Add(Context.GetCharacterByIndex(index).ID);
			}
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		if (!_currentCharacter.IsAIControlled)
		{
			_menuItemSelector.Reset();
			_menuItemSelector.OnItemSelected -= OnMenuItemSelected;
		}
	}

	private void OnMenuItemSelected(int index, int value)
	{
		Context.SetCurrentTarget(value);
		Owner.RequestState("ResolveAttack");
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentCharacter.IsAIControlled)
		{
			List<int> targets = _characterIDPool.FindAll(x => x != _currentCharacter.ID);
			// do something crazy to pick a good result
			int index = Random.Range(0, targets.Count);
			Debug.Log(targets.Count + ", " + index);
			OnMenuItemSelected(index, targets[index]); // please don't hit yourself please
		}
		else
		{
			_menuItemSelector.Update(deltaTime);
		}
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
		
		_currentCharacter.OnAttributeChange += OnAttackerAttributeChange;
		_targetCharacter.OnAttributeChange += OnTargetAttributeChange;

		_currentAction = Context.GetCurrentAction();

		Context.ActionPerformed(_currentCharacter, _targetCharacter, _currentAction);
		_currentAction.OnBeginAction(_currentCharacter, _targetCharacter);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_currentAction.OnEndAction(_currentCharacter, _targetCharacter);

		_currentCharacter.OnAttributeChange -= OnAttackerAttributeChange;
		_targetCharacter.OnAttributeChange -= OnTargetAttributeChange;

		if (_targetCharacter.HP <= 0)
		{
			Context.CharacterLose(_targetCharacter);
		}
		else if (_currentCharacter.HP <= 0)
		{
			Context.CharacterLose(_currentCharacter);
		}
	}

	private void OnAttackerAttributeChange(string attribute, float oldValue, float newValue)
	{
		Context.AttributeChanged(_currentCharacter, attribute, oldValue, newValue);
	}

	private void OnTargetAttributeChange(string attribute, float oldValue, float newValue)
	{
		Context.AttributeChanged(_targetCharacter, attribute, oldValue, newValue);
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (_currentAction != null && _currentAction.ResolveAction())
		{
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
