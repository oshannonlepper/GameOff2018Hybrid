using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleActionData : ScriptableObject
{

	[SerializeField] public string Label;

	public virtual BattleActionInstance CreateInstance()
	{
		return new BattleActionInstance(this);
	}

}
