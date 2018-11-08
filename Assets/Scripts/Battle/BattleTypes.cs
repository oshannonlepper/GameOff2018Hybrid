using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleTurnCalculator
{
	public int GetNextTurn()
	{
		return 0;
	}
}

public class BattleContext
{
	private List<BattleCharacter> _characters;
	private int _currentTurn = 0;
	private int _currentAction = 0;
	private int _currentTarget = 0;

	public BattleContext()
	{
		_characters = new List<BattleCharacter>();
	}

	public int AddCharacter(BattleCharacter character)
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

	public BattleCharacter GetCharacterByID(int id)
	{
		foreach (BattleCharacter character in _characters)
		{
			if (character.ID == id)
			{
				return character;
			}
		}

		Debug.LogError("Unable to find character with id = " + id);
		return null;
	}

	public BattleCharacter GetCharacterByIndex(int index)
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

}

public class BattleCharacter
{
	public string Name { get; set; }
	public int ID { get; set; }
	public bool IsAIControlled = false;

	private List<BattleAction> _actions;
	private int _numActions = 0;

	public BattleCharacter()
	{
		_actions = new List<BattleAction>();
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

}

public class BattleAction
{
	public string Label { get; set; }

	public override string ToString()
	{
		return Label;
	}
}
