using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** TODO - Most of this can be worked out by single line formulae - I did it once before, but this is fine for now. */
public class BattleTurnCalculator
{
	private Dictionary<int, float> _characterSpeedValues;
	private float _turnTotal = 0.0f;

	public bool TryInit(BattleContext context)
	{
		if (_characterSpeedValues == null)
		{
			_characterSpeedValues = new Dictionary<int, float>();

			UpdateTurnTotal(context);
			for (int characterID = 0; characterID < context.GetNumCharacters(); ++characterID)
			{
				_characterSpeedValues[characterID] = _turnTotal;
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	public int GetNextTurn(BattleContext context)
	{
		UpdateTurnTotal(context);

		int nextTurn = -1;

		while (nextTurn == -1)
		{
			for (int characterID = 0; characterID < context.GetNumCharacters(); ++characterID)
			{
				BattleCharacterInstance character = context.GetCharacterByID(characterID);
				if (character.HP <= 0.0f)
				{
					continue;
				}

				_characterSpeedValues[characterID] -= character.GetAttributeValue("Speed");
				if (_characterSpeedValues[characterID] <= 0.0f)
				{
					_characterSpeedValues[characterID] += _turnTotal;
					nextTurn = characterID;
					break;
				}
			}
		}

		return nextTurn;
	}

	private void UpdateTurnTotal(BattleContext context)
	{
		for (int characterID = 0; characterID < context.GetNumCharacters(); ++characterID)
		{
			BattleCharacterInstance character = context.GetCharacterByID(characterID);

			// only count non-out characters for the turn total
			if (character.HP > 0)
			{
				_turnTotal += character.GetAttributeValue("Speed");
			}
		}
	}
}

public class BattleContext
{
	private List<BattleCharacterInstance> _characters;
	private List<int> _outCharacterIDs;
	private int _currentTurn = 0;
	private int _currentAction = 0;
	private int _currentTarget = 0;

	public BattleContext()
	{
		_characters = new List<BattleCharacterInstance>();
		_outCharacterIDs = new List<int>();
	}

	public int AddCharacter(BattleCharacterInstance character)
	{
		int id = _characters.Count;
		character.ID = id;
		_characters.Add(character);
		return id;
	}

	public bool RemoveCharacter(int characterID)
	{
		return _characters.Remove(GetCharacterByID(characterID));
	}

	public int GetNumCharacters()
	{
		return _characters.Count;
	}

	public BattleCharacterInstance GetCharacterByID(int id)
	{
		foreach (BattleCharacterInstance character in _characters)
		{
			if (character.ID == id)
			{
				return character;
			}
		}

		Debug.LogError("Unable to find character with id = " + id);
		return null;
	}

	public BattleCharacterInstance GetCharacterByIndex(int index)
	{
		if (index >= 0 && index < GetNumCharacters())
		{
			return _characters[index];
		}

		Debug.LogError("Invalid index specified: " + index + ", (max = " + GetNumCharacters() + ")");
		return null;
	}

	public BattleAction GetCurrentAction()
	{
		return GetCharacterByID(_currentTurn).GetAction(_currentAction);
	}

	public int GetCurrentTarget()
	{
		return _currentTarget;
	}

	public int GetCurrentTurnCharacterID()
	{
		return _currentTurn;
	}

	public void SetCurrentCharacter(int currentCharacter)
	{
		_currentTurn = currentCharacter;
	}

	public void SetCurrentAction(int actionIndex)
	{
		_currentAction = actionIndex;
	}

	public void SetCurrentTarget(int currentTarget)
	{
		_currentTarget = currentTarget;
	}

	public int MarkCharacterOut(int characterID)
	{
		if (_outCharacterIDs.Contains(characterID))
		{
			Debug.LogWarning("Marking a character out who was already marked out, this shouldn't happen.");
		}
		else
		{
			_outCharacterIDs.Add(characterID);
		}
		return GetActiveCharacterCount();
	}

	// return number of characters who aren't marked as out
	public int GetActiveCharacterCount()
	{
		return _characters.Count - _outCharacterIDs.Count;
	}

}

[System.Serializable]
public class BattleCharacterData : ScriptableObject
{
	public string Name { get; set; }
	public int Level { get; set; }
	public int HP { get; set; }
	public int Attack { get; set; }
	public int Defense { get; set; }
	public int Speed { get; set; }

	public BattleCharacterInstance CreateInstance()
	{
		return new BattleCharacterInstance(Name, Level, HP, Attack, Defense, Speed);
	}

}

public class BattleCharacterInstance
{
	public string Name { get; set; }
	public int ID { get; set; }
	public bool IsAIControlled = true;

	private int _currentHP = 0;
	private AttributesContainer _attributes;
	private List<BattleAction> _actions;
	private int _numActions = 0;

	public int HP
	{
		get
		{
			return _currentHP;
		}
	}

	public BattleCharacterInstance(string inName, int lvl, int hp, int atk, int def, int spd)
	{
		Name = inName;

		_attributes = new AttributesContainer();
		_attributes.AddContribution("Level", "Level", AttributeContributionType.Override, lvl);
		_attributes.AddContribution("MaxHP", "BaseValue", AttributeContributionType.Override, hp);
		_attributes.AddContribution("Attack", "BaseValue", AttributeContributionType.Override, atk);
		_attributes.AddContribution("Defense", "BaseValue", AttributeContributionType.Override, def);
		_attributes.AddContribution("Speed", "BaseValue", AttributeContributionType.Override, spd);
		_currentHP = Mathf.RoundToInt(_attributes.GetValue("MaxHP"));

		_actions = new List<BattleAction>();
	}

	public AttributesContainer GetAttributes()
	{
		return _attributes;
	}

	public void SetActions(List<BattleAction> inActions)
	{
		_actions = inActions;
	}

	public void AddAction(BattleAction action)
	{
		_actions.Add(action);
		++_numActions;
	}

	public int GetNumActions()
	{
		return _numActions;
	}

	public BattleAction GetAction(int index)
	{
		if (index < 0 || index >= GetNumActions())
		{
			Debug.LogError("Invalid action index (id = " + index + ", max = " + GetNumActions() + ")");
			return null;
		}
		else
		{
			return _actions[index];
		}
	}

	public string GetActionLabel(int index)
	{
		BattleAction action = GetAction(index);
		return action == null ? "[ Null Action ]" : action.ToString();
	}

	public override string ToString()
	{
		return Name;
	} 

	public float GetAttributeValue(string attribute)
	{
		if (_attributes == null)
		{
			Debug.LogError("Need to set attributes first.");
			return 0.0f;
		}

		return _attributes.GetValue(attribute);
	}

	/** Reduce the character's health by the given damage value, return true if their health is <= 0, making this a fatal attack. */
	public bool TakeDamage(int damage)
	{
		_currentHP -= damage;
		return _currentHP <= 0;
	}

}

[System.Serializable]
public class BattleAction : ScriptableObject
{
	[SerializeField] public string Label { get; set; }

	public override string ToString()
	{
		return Label;
	}

	public virtual void OnBeginAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{
	}

	public virtual void OnEndAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{

	}

	public virtual bool ResolveAction()
	{
		return true;
	}

}

[System.Serializable]
[CreateAssetMenu(menuName = "Actions/Basic Attack")]
public class BattleActionAttack : BattleAction
{

	[SerializeField] private int _power = 40;
	[SerializeField] private int _accuracy = 100;
	[SerializeField] private List<AttributeData> _customUserAttackAttributes;
	[SerializeField] private List<AttributeData> _customTargetAttackAttributes;
	[SerializeField] private List<AttributeData> _customUserBattleAttributes;
	[SerializeField] private List<AttributeData> _customTargetBattleAttributes;

	private BattleCharacterInstance _currentTarget = null;
	private AttributesContainer _userAttributes = null;
	private AttributesContainer _targetAttributes = null;

	public BattleActionAttack()
	{
		Label = "Attack";
	}

	public void Init(int power, int accuracy)
	{
		_power = power;
		_accuracy = accuracy;

		_customUserAttackAttributes = new List<AttributeData>();
		_customTargetAttackAttributes = new List<AttributeData>();
		_customUserBattleAttributes = new List<AttributeData>();
		_customTargetBattleAttributes = new List<AttributeData>();
	}

	public override void OnBeginAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{
		_userAttributes = user.GetAttributes();
		_currentTarget = target;
		_targetAttributes = _currentTarget.GetAttributes();

		// add the power and accuracy attributes to the user

		_userAttributes.AddContribution("Power", "User"+Label, AttributeContributionType.Additive, _power);
		_userAttributes.AddContribution("Accuracy", "User"+Label, AttributeContributionType.Additive, _accuracy);

		// custom attributes applied to the user of the attack, these attributes will last only for the duration of the attack.

		foreach (var contribution in _customUserAttackAttributes)
		{
			_userAttributes.AddContribution(contribution.Attribute, "User"+contribution.Category, contribution.ContributionType, contribution.Value);
		}

		// custom attributes applied to the target of the attack, these attributes will last only for the duration of the attack.

		foreach (var contribution in _customTargetAttackAttributes)
		{
			_targetAttributes.AddContribution(contribution.Attribute, "Target"+contribution.Category, contribution.ContributionType, contribution.Value);
		}

		// custom attributes to be removed at the end of battle.

		foreach (var contribution in _customUserBattleAttributes)
		{
			_userAttributes.AddContribution(contribution.Attribute, "BattleStat", contribution.ContributionType, contribution.Value);
		}

		foreach (var contribution in _customTargetBattleAttributes)
		{
			_targetAttributes.AddContribution(contribution.Attribute, "BattleStat", contribution.ContributionType, contribution.Value);
		}
	}

	public override void OnEndAction(BattleCharacterInstance user, BattleCharacterInstance target)
	{
		// remove the power and accuracy attributes from the user

		_userAttributes.RemoveContribution("Power", Label);
		_userAttributes.RemoveContribution("Accuracy", Label);

		// remove the custom attack attributes, as we are now done with the attack and they should no longer apply

		foreach (var contribution in _customUserAttackAttributes)
		{
			_userAttributes.RemoveContribution(contribution.Attribute, "User"+contribution.Category);
		}

		foreach (var contribution in _customTargetAttackAttributes)
		{
			_targetAttributes.RemoveContribution(contribution.Attribute, "Target"+contribution.Category);
		}

		// the battle stats will be removed by removing all "BattleStat" contributions at the end of battle.

	}

	public override bool ResolveAction()
	{
		float Attack = _userAttributes.GetValue("Attack");
		float Defense = _targetAttributes.GetValue("Defense", 1.0f);
		float AttackDefenseRatio = Attack / Defense;

		float UserLevel = _userAttributes.GetValue("Level", 1.0f);
		float LevelModifier = ((2 * UserLevel) / 5.0f) + 2;

		float Power = _userAttributes.GetValue("Power");

		Debug.LogWarning("Attack = " + Attack + ", Defense = " + Defense + ", ADR = " + AttackDefenseRatio + ", Level = " + UserLevel + ", LevelModifier = " + LevelModifier);

		// this is the pokemon attack formula for now
		// there's potential for adding elemental strengths/weaknesses
		// but for now this is a game about hybridising animals, so
		// for the most part their attacks and moves are intended to be
		// grounded in reality (assume everything is normal type)
		int damage = Mathf.RoundToInt((LevelModifier * Power * AttackDefenseRatio / 50.0f) + 2);
		_currentTarget.TakeDamage(damage);

		Debug.Log(_currentTarget.Name + " took " + damage + " damage, it is now at " + _currentTarget.HP + "/" + ((int)_currentTarget.GetAttributeValue("MaxHP"))+"HP");

		return true;
	}
}
