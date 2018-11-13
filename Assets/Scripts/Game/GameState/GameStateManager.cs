using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Primary state machine for handling the overall game flow. */
public class GameStateManager : MonoBehaviour
{
	private StateMachine _gameStateMachine;

	[SerializeField] private List<BattleCharacterData> _battleCharacterDatas;

	private void Awake()
	{
		BattleGameState battleGameState = new BattleGameState();
		battleGameState.SetCharacters(_battleCharacterDatas);

		_gameStateMachine = new StateMachine();
		_gameStateMachine.RegisterState("MainMenu", new MainMenuGameState());
		_gameStateMachine.RegisterState("Overworld", new OverworldGameState());
		_gameStateMachine.RegisterState("Battle", battleGameState);
		_gameStateMachine.RegisterState("BattleResult", new BattleResultGameState());

		_gameStateMachine.RequestState("MainMenu");
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_gameStateMachine.UpdateStateMachine(deltaTime);
	}
}