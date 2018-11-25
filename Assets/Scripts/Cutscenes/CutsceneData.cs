using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "UI/Cutscenes/Cutscene Data")]
public class CutsceneData : ScriptableObject
{

	[System.Serializable]
	public struct CutsceneDataElement
	{
		[SerializeField] public Sprite CutsceneSprite;
		[SerializeField] public string CutsceneText;
	}

	[SerializeField] public CutsceneDataElement[] Data;

}