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
		_battleStateMachine.RegisterState("ResolveAttack", new BattleStateResolveAttack());
		_battleStateMachine.RegisterState("AttackAnimation", new BattleStateAttackAnimation());
		_battleStateMachine.RegisterState("DetermineNextTurn", new BattleStateDetermineNextTurn());
		_battleStateMachine.RegisterState("End", new BattleStateEnd());
	}

	public void BeginEncounter(/* TODO - params, probably want participants */)
	{
		_battleStateMachine.OnStateChanged += _battleStateMachine_OnStateChanged;

		BattleContext context = new BattleContext();

		BattleCharacterInstance charA = new BattleCharacterInstance("Snake", 1, 30, 11, 23, 21);
		charA.IsAIControlled = false;

		BattleActionAttack actionWriggle = ScriptableObject.CreateInstance<BattleActionAttack>();// (0, 100);
		actionWriggle.Init(0, 100);
		actionWriggle.Label = "Wriggle";

		BattleActionAttack actionBite = ScriptableObject.CreateInstance<BattleActionAttack>();// (50, 100);
		actionBite.Init(50, 100);
		actionBite.Label = "Bite";

		BattleActionAttack actionHiss = ScriptableObject.CreateInstance<BattleActionAttack>();// (0, 100);
		actionHiss.Init(0, 100);
		actionHiss.Label = "Hiss";

		charA.AddAction(actionWriggle);
		charA.AddAction(actionBite);
		charA.AddAction(actionHiss);

		BattleCharacterInstance charB = new BattleCharacterInstance("Bear", 4, 80, 20, 18, 14);
		charB.Name = "Bear";

		BattleActionAttack actionRoar = ScriptableObject.CreateInstance<BattleActionAttack>();// (0, 100);
		actionRoar.Init(0, 100);
		actionRoar.Label = "Roar";

		BattleActionAttack actionSlash = ScriptableObject.CreateInstance<BattleActionAttack>();// (70, 50);
		actionSlash.Init(70, 50);
		actionSlash.Label = "Slash";

		BattleActionAttack actionStomp = ScriptableObject.CreateInstance<BattleActionAttack>();// (60, 70);
		actionStomp.Init(60, 70);
		actionStomp.Label = "Stomp";

		BattleActionAttack actionSleep = ScriptableObject.CreateInstance<BattleActionAttack>();// (0, 100);
		actionSleep.Init(0, 100);
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