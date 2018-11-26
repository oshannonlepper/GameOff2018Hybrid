using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Primary state machine for handling the overall game flow. */
public class GameStateManager : MonoBehaviour
{
	private StateMachine _gameStateMachine;

	private BattleGameState _battleGameState;
	private BattleResultGameState _battleResultGameState;
	private OverworldGameState _overworldGameState;

	[SerializeField] private List<BattleCharacterData> _battleCharacterPool;
	[SerializeField] private UIBattle _battleUI;
	[SerializeField] private UICutscenes _cutscenesUI;
	[SerializeField] private GameObject _overworldPlayerPrefab;
	[SerializeField] private CutsceneData _introCutsceneData;
	[SerializeField] private CutsceneData _loseGameCutsceneData;

	private GameStateContext _gameStateContext;

	private void Awake()
	{
		_battleUI.gameObject.SetActive(false);

		_battleGameState = new BattleGameState();
		_battleGameState.SetUI(_battleUI);
		_battleGameState.SetCharacterPool(_battleCharacterPool);

		_overworldGameState = new OverworldGameState();
		_overworldGameState.SetPlayerPrefab(_overworldPlayerPrefab);

		_gameStateContext = new GameStateContext();
		_gameStateContext.Cutscenes = _cutscenesUI;
		_gameStateContext.SetBattleResult(EBattleOutcome.Lose);

		_gameStateMachine = new GameStateMachine(_gameStateContext);
		_gameStateMachine.RegisterState("MainMenu", new MainMenuGameState());
		_gameStateMachine.RegisterState("Intro", new CutsceneGameState(_introCutsceneData, "Overworld"));
		_gameStateMachine.RegisterState("Overworld", _overworldGameState);
		_gameStateMachine.RegisterState("Battle", _battleGameState);
		_gameStateMachine.RegisterState("BattleResult", new BattleResultGameState(_loseGameCutsceneData));

		_gameStateMachine.RequestState("MainMenu");

		BattleStateMachine.OnSetContext += BattleStateMachine_OnSetContext;
	}

	private void BattleStateMachine_OnSetContext(BattleStateMachine machine, BattleContext context)
	{
		_battleUI.gameObject.SetActive(true);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_gameStateMachine.UpdateStateMachine(deltaTime);
	}

}

public class GameStateMachine : StateMachine
{

	private GameStateContext _gameStateContext = null;

	public GameStateMachine(GameStateContext inContext)
	{
		_gameStateContext = inContext;
	}

	public GameStateContext GetContext()
	{
		return _gameStateContext;
	}

}