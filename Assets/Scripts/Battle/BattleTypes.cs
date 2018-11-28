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
			// TODO - Don't iterate all character IDs anymore, just get the active ones and process them
			for (int characterID = 0; characterID < context.GetNumCharacters(); ++characterID)
			{
				BattleCharacterInstance character = context.GetCharacterByID(characterID);
				if (character.HP <= 0.0f || !context.IsActiveCharacter(characterID))
				{
					continue;
				}

				_characterSpeedValues[characterID] -= character.GetSpeed();
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

			// only count non-out, active characters for the turn total
			if (character.HP > 0 && context.IsActiveCharacter(characterID))
			{
				_turnTotal += character.GetSpeed();
			}
		}
	}
}

public interface IBattleListener
{

	void OnActionPerformed(BattleCharacterInstance attacker, BattleCharacterInstance target, BattleActionInstance action);
	void OnAttributeChanged(BattleCharacterInstance character, string attribute, float oldValue, float newValue);
	void OnCharacterLose(BattleCharacterInstance character);
	void OnActiveCharacterChange(BattleCharacterInstance character, int team);
	void OnPlayerMenuRequested(MenuItemSelector menu);

}

public class BattleEventDispatcher
{

	private List<IBattleListener> _listeners;

	public BattleEventDispatcher()
	{
		_listeners = new List<IBattleListener>();
	}

	public void AddListener(IBattleListener listener)
	{
		_listeners.Add(listener);
	}

	public void RemoveListener(IBattleListener listener)
	{
		_listeners.Remove(listener);
	}

	public void ActionPerformed(BattleCharacterInstance attacker, BattleCharacterInstance target, BattleActionInstance action)
	{
		foreach (IBattleListener listener in _listeners)
		{
			listener.OnActionPerformed(attacker, target, action);
		}
	}

	public void AttributeChanged(BattleCharacterInstance character, string attribute, float oldValue, float newValue)
	{
		foreach (IBattleListener listener in _listeners)
		{
			listener.OnAttributeChanged(character, attribute, oldValue, newValue);
		}
	}

	public void CharacterLose(BattleCharacterInstance character)
	{
		foreach (IBattleListener listener in _listeners)
		{
			listener.OnCharacterLose(character);
		}
	}

	public void ActiveCharacterChange(BattleCharacterInstance character, int team)
	{
		foreach (IBattleListener listener in _listeners)
		{
			listener.OnActiveCharacterChange(character, team);
		}
	}

	public void RequestMenu(MenuItemSelector menu)
	{
		foreach (IBattleListener listener in _listeners)
		{
			listener.OnPlayerMenuRequested(menu);
		}
	}

}

public class BattleContext
{
	private BattleEventDispatcher _battleEventDispatcher;
	private List<BattleCharacterInstance> _characters;
	private List<int> _characterTeamIDs;
	private List<int> _outCharacterIDs;
	private int _currentTurn = 0;
	private int _currentAction = 0;
	private int _currentTarget = 0;

	private int _activePlayerCharacter = -1;
	private int _activeEnemyCharacter = -1;
	private EBattleOutcome _result = EBattleOutcome.Win;

	public BattleContext()
	{
		_characters = new List<BattleCharacterInstance>();
		_characterTeamIDs = new List<int>();
		_outCharacterIDs = new List<int>();
		_battleEventDispatcher = new BattleEventDispatcher();
	}

	public EBattleOutcome GetBattleResult()
	{
		return _result;
	}

	public void SetActivePlayerCharacter(int characterID)
	{
		// TODO: Sanity check - make sure is valid ID and is aligned with player team
		_activePlayerCharacter = characterID;
		_battleEventDispatcher.ActiveCharacterChange(GetCharacterByID(characterID), 0);
	}

	public void SetActiveEnemyCharacter(int characterID)
	{
		// TODO: Sanity check - make sure is valid ID and is aligned with enemy team
		_activeEnemyCharacter = characterID;
		_battleEventDispatcher.ActiveCharacterChange(GetCharacterByID(characterID), 1);
	}

	public bool IsActiveCharacter(int characterID)
	{
		return _activePlayerCharacter == characterID || _activeEnemyCharacter == characterID;
	}

	public int AddCharacter(BattleCharacterInstance character, int team)
	{
		_result = EBattleOutcome.Win;
		int id = _characters.Count;
		character.ID = id;
		_characters.Add(character);
		_characterTeamIDs.Add(team);

		if (team == 0 && _activePlayerCharacter == -1)
		{
			SetActivePlayerCharacter(id);
		}
		else if (team == 1 && _activeEnemyCharacter == -1)
		{
			SetActiveEnemyCharacter(id);
		}

		return id;
	}

	public bool RemoveCharacter(int characterID)
	{
		int index = IndexOfCharacter(GetCharacterByID(characterID));

		if (index == -1)
		{
			return false;
		}

		_characters.RemoveAt(index);
		_characterTeamIDs.RemoveAt(index);
		return true;
	}

	public int GetNumCharacters()
	{
		return _characters.Count;
	}

	public int IndexOfCharacter(BattleCharacterInstance character)
	{
		for (int index = 0; index < _characters.Count; ++index)
		{
			if (_characters[index] == character)
			{
				return index;
			}
		}

		return -1;
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
			if (!IsActiveCharacter(characterID))
			{
				Debug.LogError("Inactive character marked as out somehow");
			}

			_outCharacterIDs.Add(characterID);
			
			// find out which team the character was on, and try to replace them with a new active character
			int index = IndexOfCharacter(GetCharacterByID(characterID));
			int teamIndex = _characterTeamIDs[index];
			bool foundReplacement = false;

			foreach (BattleCharacterInstance character in _characters)
			{
				if (character.HP > 0 && !IsActiveCharacter(character.ID))
				{
					int characterTeam = _characterTeamIDs[IndexOfCharacter(character)];
					if (characterTeam == teamIndex)
					{
						if (characterTeam == 0)
						{
							SetActivePlayerCharacter(character.ID);
						}
						else
						{
							SetActiveEnemyCharacter(character.ID);
						}
						foundReplacement = true;
					}
				}
			}

			if (!foundReplacement)
			{
				if (teamIndex == 0)
				{
					SetActivePlayerCharacter(-1);
					_result = EBattleOutcome.Lose;
				}
				else
				{
					SetActiveEnemyCharacter(-1);
				}
			}
		}
		return GetActiveCharacterCount();
	}

	// return number of characters in play, typically returns 2, will return 1 if all of one team's animals are out.
	public int GetActiveCharacterCount()
	{
		int count = 2;

		if (_activePlayerCharacter == -1)
		{
			--count;
		}
		if (_activeEnemyCharacter == -1)
		{
			--count;
		}

		return count;
	}

	// event dispatcher interface

	public void AddListener(IBattleListener listener)
	{
		_battleEventDispatcher.AddListener(listener);
	}

	public void RemoveListener(IBattleListener listener)
	{
		_battleEventDispatcher.RemoveListener(listener);
	}

	public void ActionPerformed(BattleCharacterInstance attacker, BattleCharacterInstance target, BattleActionInstance action)
	{
		_battleEventDispatcher.ActionPerformed(attacker, target, action);
	}

	public void AttributeChanged(BattleCharacterInstance character, string attribute, float oldValue, float newValue)
	{
		_battleEventDispatcher.AttributeChanged(character, attribute, oldValue, newValue);
	}
	
	public void CharacterLose(BattleCharacterInstance character)
	{
		_battleEventDispatcher.CharacterLose(character);
	}

	public void RequestMenu(MenuItemSelector menu)
	{
		_battleEventDispatcher.RequestMenu(menu);
	}

}

public class BattleCharacterInstance
{
	public delegate void HealthChangeEvent(int oldHP, int newHP);
	public event HealthChangeEvent OnHealthChange;
	public event AttributesContainer.AttributeChangeEvent OnAttributeChange;

	private float[] _modifiers = { 0.25f, 0.28f, 0.33f, 0.4f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f };

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
		_attributes.OnAttributeChanged += OnAttributesChanged;

		_attributes.AddContribution("Level", "Level", AttributeContributionType.Additive, data.Level);
		_attributes.AddContribution("MaxHP", "BaseValue", AttributeContributionType.Additive, data.HP);
		_attributes.AddContribution("Attack", "BaseValue", AttributeContributionType.Additive, data.Attack);
		_attributes.AddContribution("Defense", "BaseValue", AttributeContributionType.Additive, data.Defense);
		_attributes.AddContribution("Speed", "BaseValue", AttributeContributionType.Additive, data.Speed);
		_currentHP = _attributes.GetValueAsInt("MaxHP");

		_actions = new List<BattleActionInstance>();
		foreach (BattleActionData action in data.Actions)
		{
			if (action == null)
			{
				Debug.LogError(data.Name + " had a null action.");
				continue;
			}
			_actions.Add(action.CreateInstance());
		}
		_numActions = _actions.Count;

	}

	private void OnAttributesChanged(string attribute, float oldValue, float newValue)
	{
		if (OnAttributeChange != null)
		{
			OnAttributeChange(attribute, oldValue, newValue);
		}
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

	public float GetAttack()
	{
		return GetStat("Attack");
	}

	public float GetDefense()
	{
		return GetStat("Defense");
	}

	public float GetSpeed()
	{
		return GetStat("Speed");
	}

	public int GetMaxHP()
	{
		return Mathf.RoundToInt(GetAttributeValue("MaxHP"));
	}

	private float GetStat(string attributeLabel)
	{
		return GetAttributeValue(attributeLabel) * GetModifier(attributeLabel);
	}

	private float GetModifier(string attribute)
	{
		int modifier = _attributes.GetValueAsInt(attribute + "Modifier");
		return _modifiers[Mathf.Clamp(modifier + 5, 0, _modifiers.Length-1)];
	}

	private float GetAttributeValue(string attribute, float defaultValue = 0.0f)
	{
		if (_attributes == null)
		{
			Debug.LogError("Need to set attributes first.");
			return defaultValue;
		}

		return _attributes.GetValue(attribute, defaultValue);
	}

	/** Reduce the character's health by the given damage value, return true if their health is <= 0, making this a fatal attack. */
	public bool TakeDamage(int damage)
	{
		int oldHP = _currentHP;
		_currentHP -= damage;
		_currentHP = Mathf.Clamp(_currentHP, 0, GetMaxHP());

		if (OnHealthChange != null)
		{
			OnHealthChange(oldHP, _currentHP);
		}

		return _currentHP <= 0;
	}

}
