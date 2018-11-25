using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBattleOutcome
{
	Win,
	Lose
}

public class Battle
{

	public delegate void BattleEvent(EBattleOutcome result);

	public event BattleEvent OnBattleStart;
	public event BattleEvent OnBattleEnd;

	private BattleStateMachine _battleStateMachine;
	private BattleContext _context;

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

	public void BeginRandomEncounter(List<BattleCharacterInstance> playerBattleCharacters, BattleCharacterInstance enemy)
	{
		_battleStateMachine.OnStateChanged += _battleStateMachine_OnStateChanged;

		_context = new BattleContext();
		_battleStateMachine.SetContext(_context);

		foreach (BattleCharacterInstance playerCharacter in playerBattleCharacters)
		{
			_context.AddCharacter(playerCharacter, 0);
		}

		_context.AddCharacter(enemy, 1);

		_battleStateMachine.RequestState("Start");
	}

	public void BeginTrainerBattle(List<BattleCharacterInstance> playerBattleCharacters, List<BattleCharacterInstance> enemyBattleCharacters)
	{
		_battleStateMachine.OnStateChanged += _battleStateMachine_OnStateChanged;

		BattleContext context = new BattleContext();
		_battleStateMachine.SetContext(context);

		foreach (BattleCharacterInstance playerCharacter in playerBattleCharacters)
		{
			context.AddCharacter(playerCharacter, 0);
		}

		foreach (BattleCharacterInstance enemyCharacter in enemyBattleCharacters)
		{
			context.AddCharacter(enemyCharacter, 1);
		}

		_battleStateMachine.RequestState("Start");
	}

	private void _battleStateMachine_OnStateChanged(string oldState, string newState)
	{
		if (newState.Equals("End"))
		{
			EndEncounter(_context.GetBattleResult());
		}
	}

	public void EndEncounter(EBattleOutcome result)
	{
		if (OnBattleEnd != null)
		{
			OnBattleEnd(result);
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