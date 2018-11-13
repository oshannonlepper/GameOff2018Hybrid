using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Battle/Actions/Attack")]
public class BattleActionDataAttack : BattleActionData
{

	public bool DamageOpponent = true;
	public int Power = 40;
	public int Accuracy = 100;
	public List<AttributeData> CustomUserAttackAttributes;
	public List<AttributeData> CustomTargetAttackAttributes;
	public List<AttributeData> CustomUserBattleAttributes;
	public List<AttributeData> CustomTargetBattleAttributes;

	public override BattleActionInstance CreateInstance()
	{
		return new BattleActionInstanceAttack(this);
	}

}
