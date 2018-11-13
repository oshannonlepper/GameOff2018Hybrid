using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActionInstance
{
	[SerializeField] public string Label { get; set; }

	public BattleActionInstance(BattleActionData data)
	{
		Label = data.Label;
	}

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
