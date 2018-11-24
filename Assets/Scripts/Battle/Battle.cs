using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle
{

	public delegate void BattleEvent();

	public event BattleEvent OnBattleStart;
	public event BattleEvent OnBattleEnd;

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

	public void BeginEncounter(List<BattleCharacterData> participants)
	{
		_battleStateMachine.OnStateChanged += _battleStateMachine_OnStateChanged;

		BattleContext context = new BattleContext();

		foreach (BattleCharacterData data in participants)
		{
			BattleCharacterInstance instance = new BattleCharacterInstance(data);
			context.AddCharacter(instance);
		}
		
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
		if (OnBattleEnd != null)
		{
			OnBattleEnd();
		}
	}

	public void UpdateBattle(float deltaTime)
	{
		_battleStateMachine.UpdateStateMachine(deltaTime);
	}

}

public class BattleStateMachine : StateMachine
{

	public delegate void BattleStateMachineEvent(BattleStateMachine machine, BattleContext context);
	public static event BattleStateMachineEvent OnSetContext;

	private BattleContext _context;

	public void SetContext(BattleContext inContext)
	{
		_context = inContext;
		if (OnSetContext != null)
		{
			OnSetContext(this, _context);
		}
	}

}