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

	private GameObject _playerPrefab;

	private GameObject _playerInstance;

	public void SetPlayerPrefab(GameObject inPlayerPrefab)
	{
		_playerPrefab = inPlayerPrefab;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_playerInstance = GameObject.Instantiate(_playerPrefab);

		int LocationIndex = Random.Range(0, Locations.Length - 1);
		Debug.Log("You find yourself " + Locations[LocationIndex]);
		Debug.Log("If you hang around long enough, I'm sure some animal will attack you...");
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		GameObject.Destroy(_playerInstance);
		_playerInstance = null;
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);
		
		float xAxis = Input.GetAxisRaw("Horizontal");
		float yAxis = Input.GetAxisRaw("Vertical");
		bool hasMoved = xAxis != 0.0f || yAxis != 0.0f;

		Vector3 playerPosition = _playerInstance.transform.position;
		Vector3 direction = new Vector3(xAxis, 0.0f, yAxis) * deltaTime;
		playerPosition += direction.normalized;
		_playerInstance.transform.position = playerPosition;

		if (hasMoved && Random.Range(0.0f, 1.0f) > 0.995f)
		{
			Debug.Log("Oh boy here we go!");
			Owner.RequestState("Battle");
		}
	}
}

public class BattleGameState : GameState
{
	private Battle _battle;
	private UIBattle _battleUI;
	private List<BattleCharacterData> _characterPool;
	
	public BattleGameState()
	{
		_battle = new Battle();
	}

	public void SetCharacterPool(List<BattleCharacterData> inCharacters)
	{
		_characterPool = inCharacters;
	}

	public void SetUI(UIBattle inBattleUI)
	{
		_battleUI = inBattleUI;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_battleUI.gameObject.SetActive(true);
		_battle.OnBattleEnd += _battle_OnBattleEnd;

		List<BattleCharacterData> chosenCharacters = new List<BattleCharacterData>();
		for (int choice = 0; choice < 2; ++choice)
		{
			int index = Random.Range(0, _characterPool.Count);
			chosenCharacters.Add(_characterPool[index]);
		}

		_battle.BeginEncounter(chosenCharacters);
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_battleUI.gameObject.SetActive(false);
		_battle.OnBattleEnd -= _battle_OnBattleEnd;
	}

	private void _battle_OnBattleEnd()
	{
		Owner.RequestState("BattleResult");
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		_battle.UpdateBattle(deltaTime);
	}
}

public class BattleResultGameState : GameState
{

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		Debug.Log("Battle results");
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		Owner.RequestState("Overworld");
	}

}
