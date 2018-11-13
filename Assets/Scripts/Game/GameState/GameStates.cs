using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : State
{
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

public class MainMenuGameState : GameState
{
	public override void OnStateEnter()
	{
		base.OnStateEnter();

		Debug.Log("Press Space to start the game.");
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Owner.RequestState("Overworld");
		}
	}
}

public class OverworldGameState : GameState
{
	private string[] Locations = { "in a forest.", "in some woods...", "in a marsh.", "out on the street...", "in... a jungle?", "on the beach!", "out in the middle of the ocean...", "in a desert!?" };

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		int LocationIndex = Random.Range(0, Locations.Length - 1);
		Debug.Log("You find yourself " + Locations[LocationIndex]);
		Debug.Log("If you hang around long enough, I'm sure some animal will attack you...");
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (Random.Range(0.0f, 1.0f) > 0.995f)
		{
			Debug.Log("Oh boy here we go!");
			Owner.RequestState("Battle");
		}
	}
}

public class BattleGameState : GameState
{
	private Battle _battle;
	private List<BattleCharacterData> _battleCharacters;

	public BattleGameState()
	{
		_battle = new Battle();
	}

	public void SetCharacters(List<BattleCharacterData> inCharacters)
	{
		_battleCharacters = inCharacters;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_battle.BeginEncounter(_battleCharacters);
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);
		_battle.UpdateBattle(deltaTime);
	}
}

public class BattleResultGameState : GameState
{

}
