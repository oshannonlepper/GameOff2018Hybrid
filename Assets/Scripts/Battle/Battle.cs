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