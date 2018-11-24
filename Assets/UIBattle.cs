using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattle : MonoBehaviour, IBattleListener {

	[SerializeField] private UIHealthDisplay _enemyHealthDisplay;
	[SerializeField] private UIHealthDisplay _playerHealthDisplay;
	[SerializeField] private UIBattleText _battleText;

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

	public void SetEnemy(BattleCharacterInstance character)
	{
		_enemyHealthDisplay.SetCharacter(character);
	}

	public void SetPlayer(BattleCharacterInstance character)
	{
		_playerHealthDisplay.SetCharacter(character);
	}

	public void OnActionPerformed(BattleCharacterInstance attacker, BattleCharacterInstance target, BattleActionInstance action)
	{
		_battleText.QueueText(attacker.Name + " used " + action.Label + " on " + ((target == attacker) ? "themselves." : target.Name + "."));
	}
}
