using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICutsceneImageEvents : MonoBehaviour {

	[SerializeField] private UICutscenes _cutscenes;

	public void OnPostFadeIn()
	{
		_cutscenes.OnPostCutsceneFadeIn();
	}

	public void OnPreFadeOut()
	{
		_cutscenes.OnPreCutsceneFadeOut();
	}

	public void OnPostFadeOut()
	{
		_cutscenes.OnPostCutsceneFadeOut();
	}

}
