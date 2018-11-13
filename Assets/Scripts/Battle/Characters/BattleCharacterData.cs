using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Battle/Battle Character")]
public class BattleCharacterData : ScriptableObject
{
	[SerializeField] public string Name;
	[SerializeField] public int Level;
	[SerializeField] public int HP;
	[SerializeField] public int Attack;
	[SerializeField] public int Defense;
	[SerializeField] public int Speed;
	[SerializeField] public BattleActionData[] Actions;

	public BattleCharacterInstance CreateInstance()
	{
		return new BattleCharacterInstance(this);
	}

}
