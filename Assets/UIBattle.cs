using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattle : MonoBehaviour {

	[SerializeField] private UIHealthDisplay _enemyHealthDisplay;
	[SerializeField] private UIHealthDisplay _playerHealthDisplay;

	public void SetEnemy(BattleCharacterInstance character)
	{
		_enemyHealthDisplay.SetCharacter(character);
	}

	public void SetPlayer(BattleCharacterInstance character)
	{
		_playerHealthDisplay.SetCharacter(character);
	}

}
