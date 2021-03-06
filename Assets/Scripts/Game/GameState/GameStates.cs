﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateContext
{

	private List<BattleCharacterInstance> _playerCharacters;
	public UICutscenes Cutscenes { get; set; }
	private EBattleOutcome _lastBattleResult;

	public void SetBattleResult(EBattleOutcome result)
	{
		_lastBattleResult = result;
	}

	public EBattleOutcome GetBattleResult()
	{
		return _lastBattleResult;
	}

}

public class GameState : State
{

	protected GameStateContext Context
	{
		get
		{
			return (Owner as GameStateMachine).GetContext();
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
			Owner.RequestState("Intro");
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
	private List<BattleCharacterInstance> _playerCharacters;

	public BattleGameState()
	{
		_battle = new Battle();

		_playerCharacters = new List<BattleCharacterInstance>();
	}

	public void SetCharacterPool(List<BattleCharacterData> inCharacters)
	{
		_characterPool = inCharacters;
		BattleCharacterInstance character = MakeRandomCharacterInstance();
		character.IsAIControlled = false;
		_playerCharacters.Add(character);
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

		_battle.BeginRandomEncounter(_playerCharacters, MakeRandomCharacterInstance());
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_battleUI.gameObject.SetActive(false);
		_battle.OnBattleEnd -= _battle_OnBattleEnd;
	}

	private void _battle_OnBattleEnd(EBattleOutcome result)
	{
		Context.SetBattleResult(result);
		Owner.RequestState("BattleResult");
	}

	private BattleCharacterInstance MakeRandomCharacterInstance()
	{
		if (_characterPool.Count <= 0)
		{
			Debug.LogError("Trying to make character instance without a character data pool.");
			return null;
		}

		int index = Random.Range(0, _characterPool.Count);
		return _characterPool[index].CreateInstance();
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		_battle.UpdateBattle(deltaTime);
	}
}

public class CutsceneGameState : GameState
{
	private CutsceneData _cutscene = null;
	private bool _autoPlay = true;
	private bool _cutsceneIsPlaying = false;
	private string _nextState = "";

	public CutsceneGameState() : base()
	{
	}

	public CutsceneGameState(CutsceneData inData, string nextState, bool autoPlay=true) : base()
	{
		_cutscene = inData;
		_nextState = nextState;
		_autoPlay = autoPlay;
	}

	public void SetCutscene(CutsceneData inData)
	{
		_cutscene = inData;
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		_cutsceneIsPlaying = false;

		UICutscenes.OnCutsceneStart += OnCutsceneStart;
		UICutscenes.OnCutsceneEnd += OnCutsceneEnd;

		if (_autoPlay)
		{
			Context.Cutscenes.RequestCutscene(_cutscene);
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();

		_cutsceneIsPlaying = false;

		UICutscenes.OnCutsceneStart -= OnCutsceneStart;
		UICutscenes.OnCutsceneEnd -= OnCutsceneEnd;
	}

	protected void StartCutscene()
	{
		Context.Cutscenes.RequestCutscene(_cutscene);
	}

	protected virtual void OnCutsceneStart()
	{
		_cutsceneIsPlaying = true;
	}

	protected virtual void OnCutsceneEnd()
	{
		if (_nextState.Length > 0)
		{
			Owner.RequestState(_nextState);
		}
	}

	protected bool IsCutscenePlaying()
	{
		return _cutsceneIsPlaying;
	}
}

public class BattleResultGameState : CutsceneGameState
{

	public BattleResultGameState() : base()
	{
	}

	public BattleResultGameState(CutsceneData data) : base(data, "Overworld", false)
	{
	}

	public override void OnStateEnter()
	{
		base.OnStateEnter();

		if (Context.GetBattleResult() == EBattleOutcome.Lose)
		{
			StartCutscene();
		}
	}

	public override void UpdateState(float deltaTime)
	{
		base.UpdateState(deltaTime);

		if (!IsCutscenePlaying())
		{
			OnCutsceneEnd();
		}
	}

}
