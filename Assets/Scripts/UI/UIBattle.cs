﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattle : MonoBehaviour, IBattleListener {

	[SerializeField] private UIHealthDisplay _enemyHealthDisplay;
	[SerializeField] private UIHealthDisplay _playerHealthDisplay;
	[SerializeField] private UIBattleText _battleText;
	[SerializeField] private UIBattleMenu _battleMenu;

	private BattleContext _currentContext = null;

	void OnEnable()
	{
		BattleStateMachine.OnSetContext += BattleStateMachine_OnSetContext;
	}

	void OnDisable()
	{
		BattleStateMachine.OnSetContext -= BattleStateMachine_OnSetContext;
	}

	private void BattleStateMachine_OnSetContext(BattleStateMachine machine, BattleContext context)
	{
		if (_currentContext != null)
		{
			_currentContext.RemoveListener(this);
		}

		_currentContext = context;

		if (_currentContext != null)
		{
			_currentContext.AddListener(this);
		}
	}

	public void OnActionPerformed(BattleCharacterInstance attacker, BattleCharacterInstance target, BattleActionInstance action)
	{
		_battleText.QueueText(attacker.Name + " used " + action.Label + " on " + ((target == attacker) ? "themselves." : target.Name + "."));
	}

	public void OnAttributeChanged(BattleCharacterInstance character, string attribute, float oldValue, float newValue)
	{
		if (attribute.Contains("Modifier"))
		{
			string affectedStat = attribute.Substring(0, attribute.IndexOf("Modifier"));
			_battleText.QueueText(character.Name + "'s " + affectedStat + " " + ((newValue > oldValue) ? "increased." : "decreased."));
		}
	}

	public void OnCharacterLose(BattleCharacterInstance character)
	{
		_battleText.QueueText(character.Name + " fainted.");
	}

	public void OnActiveCharacterChange(BattleCharacterInstance character, int team)
	{
		if (team == 0)
		{
			_playerHealthDisplay.gameObject.SetActive(character != null);
			if (character != null)
			{
				_playerHealthDisplay.SetCharacter(character);
			}
		}
		else
		{
			_enemyHealthDisplay.gameObject.SetActive(character != null);
			if (character != null)
			{
				_enemyHealthDisplay.SetCharacter(character);
			}
		}
	}

	public void OnPlayerMenuRequested(MenuItemSelector menu)
	{
		_battleMenu.SetMenu(menu);
		_battleText.QueueText(menu.GetCaption());
	}

}
