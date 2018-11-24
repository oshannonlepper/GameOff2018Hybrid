using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Primary state machine for handling the overall game flow. */
public class GameStateManager : MonoBehaviour
{
	private StateMachine _gameStateMachine;

	private BattleGameState _battleGameState;
	private OverworldGameState _overworldGameState;

	[SerializeField] private List<BattleCharacterData> _battleCharacterDatas;
	[SerializeField] private UIBattle _battleUI;
	[SerializeField] private GameObject _overworldPlayerPrefab;

	private void Awake()
	{
		_battleUI.gameObject.SetActive(false);

		_battleGameState = new BattleGameState();
		_battleGameState.SetCharacters(_battleCharacterDatas);
		_battleGameState.SetUI(_battleUI);

		_overworldGameState = new OverworldGameState();
		_overworldGameState.SetPlayerPrefab(_overworldPlayerPrefab);

		_gameStateMachine = new StateMachine();
		_gameStateMachine.RegisterState("MainMenu", new MainMenuGameState());
		_gameStateMachine.RegisterState("Overworld", _overworldGameState);
		_gameStateMachine.RegisterState("Battle", _battleGameState);
		_gameStateMachine.RegisterState("BattleResult", new BattleResultGameState());

		_gameStateMachine.RequestState("MainMenu");

		BattleStateMachine.OnSetContext += BattleStateMachine_OnSetContext;
	}

	private void BattleStateMachine_OnSetContext(BattleStateMachine machine, BattleContext context)
	{
		_battleUI.gameObject.SetActive(true);
		// TODO - Work this out a better way so we're not making assumptions about which ID corresponds to a playable character.
		_battleUI.SetPlayer(context.GetCharacterByID(0));
		_battleUI.SetEnemy(context.GetCharacterByID(1));
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_gameStateMachine.UpdateStateMachine(deltaTime);
	}
}