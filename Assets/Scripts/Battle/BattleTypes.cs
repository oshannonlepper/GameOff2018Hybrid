﻿using System.Collections;
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

	public BattleActionInstance GetCurrentAction()
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

public class BattleCharacterInstance
{
	public string Name { get; set; }
	public int ID { get; set; }
	public bool IsAIControlled = true;

	private int _currentHP = 0;
	private AttributesContainer _attributes;
	private List<BattleActionInstance> _actions;
	private int _numActions = 0;

	public int HP
	{
		get
		{
			return _currentHP;
		}
	}

	public BattleCharacterInstance(BattleCharacterData data)
	{
		Name = data.Name;

		_attributes = new AttributesContainer();
		_attributes.AddContribution("Level", "Level", AttributeContributionType.Additive, data.Level);
		_attributes.AddContribution("MaxHP", "BaseValue", AttributeContributionType.Additive, data.HP);
		_attributes.AddContribution("Attack", "BaseValue", AttributeContributionType.Additive, data.Attack);
		_attributes.AddContribution("Defense", "BaseValue", AttributeContributionType.Additive, data.Defense);
		_attributes.AddContribution("Speed", "BaseValue", AttributeContributionType.Additive, data.Speed);
		_currentHP = _attributes.GetValueAsInt("MaxHP");

		_actions = new List<BattleActionInstance>();
		foreach (BattleActionData action in data.Actions)
		{
			_actions.Add(action.CreateInstance());
		}
		_numActions = _actions.Count;

	}

	public AttributesContainer GetAttributes()
	{
		return _attributes;
	}

	public void SetActions(List<BattleActionInstance> inActions)
	{
		_actions = inActions;
	}

	public void AddAction(BattleActionInstance action)
	{
		_actions.Add(action);
		++_numActions;
	}

	public int GetNumActions()
	{
		return _numActions;
	}

	public BattleActionInstance GetAction(int index)
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
		BattleActionInstance action = GetAction(index);
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
