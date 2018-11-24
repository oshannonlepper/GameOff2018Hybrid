using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleComponent : MonoBehaviour {

	[SerializeField] private List<BattleCharacterData> _testData;

	private Battle _battle;

	private void Awake()
	{
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_battle.UpdateBattle(deltaTime);
	}

}
