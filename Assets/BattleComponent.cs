using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleComponent : MonoBehaviour {

	private Battle _battle;

	private void Awake()
	{
		_battle = new Battle();
		_battle.BeginEncounter();
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		_battle.UpdateBattle(deltaTime);
	}

}
