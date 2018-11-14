using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActionInstanceAttack : BattleActionInstance
{

	[SerializeField] private BattleActionDataAttack _data;

	private BattleCharacterInstance _currentUser = null;
	private BattleCharacterInstance _currentTarget = null;
	private AttributesContainer _userAttributes = null;
	private AttributesContainer _targetAttributes = null;

	public BattleActionInstanceAttack(BattleActionDataAttack data) : base(data)
	{
		_data = data;
	}

	public override void OnBeginAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{
		_currentUser = user;
		_userAttributes = user.GetAttributes();
		_currentTarget = target;
		_targetAttributes = _currentTarget.GetAttributes();

		// add the power and accuracy attributes to the user

		_userAttributes.AddContribution("Power", Label, AttributeContributionType.Additive, _data.Power);
		_userAttributes.AddContribution("Accuracy", Label, AttributeContributionType.Additive, _data.Accuracy);

		// custom attributes applied to the user of the attack, these attributes will last only for the duration of the attack.

		foreach (var contribution in _data.CustomUserAttackAttributes)
		{
			_userAttributes.AddContribution(contribution.Attribute, "User" + contribution.Category, contribution.ContributionType, contribution.Value);
		}

		// custom attributes applied to the target of the attack, these attributes will last only for the duration of the attack.

		foreach (var contribution in _data.CustomTargetAttackAttributes)
		{
			_targetAttributes.AddContribution(contribution.Attribute, "Target" + contribution.Category, contribution.ContributionType, contribution.Value);
		}

		// custom attributes to be removed at the end of battle.

		foreach (var contribution in _data.CustomUserBattleAttributes)
		{
			float oldAttr = _userAttributes.GetValue(contribution.Attribute);
			_userAttributes.AddContribution(contribution.Attribute, "BattleStat", contribution.ContributionType, contribution.Value);
			float newAttr = _userAttributes.GetValue(contribution.Attribute);
			if (newAttr > oldAttr)
			{
				Debug.Log(user.Name + "'s " + contribution.Attribute + " was raised. ("+oldAttr+" -> "+newAttr+")");
			}
			else
			{
				Debug.Log(user.Name + "'s " + contribution.Attribute + " was lowered. (" + oldAttr + " -> " + newAttr + ")");
			}
		}

		foreach (var contribution in _data.CustomTargetBattleAttributes)
		{
			float oldAttr = _targetAttributes.GetValue(contribution.Attribute);
			_targetAttributes.AddContribution(contribution.Attribute, "BattleStat", contribution.ContributionType, contribution.Value);
			float newAttr = _targetAttributes.GetValue(contribution.Attribute);
			if (newAttr > oldAttr)
			{
				Debug.Log(target.Name + "'s " + contribution.Attribute + " was raised. (" + oldAttr + " -> " + newAttr + ")");
			}
			else
			{
				Debug.Log(target.Name + "'s " + contribution.Attribute + " was lowered. ("+oldAttr+" -> "+newAttr+")");
			}
		}
	}

	public override void OnEndAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{
		// remove the power and accuracy attributes from the user

		_userAttributes.RemoveContribution("Power", Label);
		_userAttributes.RemoveContribution("Accuracy", Label);

		// remove the custom attack attributes, as we are now done with the attack and they should no longer apply

		foreach (var contribution in _data.CustomUserAttackAttributes)
		{
			_userAttributes.RemoveContribution(contribution.Attribute, "User" + contribution.Category);
		}

		foreach (var contribution in _data.CustomTargetAttackAttributes)
		{
			_targetAttributes.RemoveContribution(contribution.Attribute, "Target" + contribution.Category);
		}

		// the battle stats will be removed by removing all "BattleStat" contributions at the end of battle.

	}

	public override bool ResolveAction()
	{
		if (_data.DamageOpponent)
		{
			float Attack = _currentUser.GetAttack();
			float Defense = _currentTarget.GetDefense();
			float AttackDefenseRatio = Attack / Defense;

			float UserLevel = _userAttributes.GetValue("Level", 1.0f);
			float LevelModifier = ((2.0f * UserLevel) / 5.0f) + 2.0f;

			float Power = _userAttributes.GetValue("Power");

			Debug.LogWarning("Attack = " + Attack + ", Defense = " + Defense + ", ADR = " + AttackDefenseRatio + ", Level = " + UserLevel + ", LevelModifier = " + LevelModifier);

			// this is the pokemon attack formula for now
			// there's potential for adding elemental strengths/weaknesses
			// but for now this is a game about hybridising animals, so
			// for the most part their attacks and moves are intended to be
			// grounded in reality (assume everything is normal type)
			int damage = Mathf.RoundToInt((LevelModifier * Power * AttackDefenseRatio / 50.0f) + 2.0f);
			_currentTarget.TakeDamage(damage);

			Debug.Log(_currentTarget.Name + " took " + damage + " damage, it is now at " + _currentTarget.HP + "/" + _currentTarget.GetMaxHP() + "HP");
		}

		return true;
	}
}
