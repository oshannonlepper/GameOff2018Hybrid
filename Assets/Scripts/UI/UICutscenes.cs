﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICutscenes : MonoBehaviour {

	public delegate void CutsceneEvent();
	public static event CutsceneEvent OnCutsceneStart;
	public static event CutsceneEvent OnCutsceneEnd;

	[SerializeField] private Animator _backgroundAnimator;
	[SerializeField] private Animator _cutsceneImageAnimator;
	[SerializeField] private Image _cutsceneImage;
	[SerializeField] private Text _text;

	private TypewriterText _typewriter;
	private CutsceneData _currentData = null;
	private int _numItems = 0;
	private int _currentItem = -1;
	private bool _readyToContinue = false;

	private void Awake()
	{
		_backgroundAnimator.StopPlayback();
		_cutsceneImageAnimator.StopPlayback();
		_typewriter = new TypewriterText(_text, 0.01f);
	}

	public void RequestCutscene(CutsceneData data)
	{
		if (OnCutsceneStart != null)
		{
			OnCutsceneStart();
		}

		_currentData = data;
		_numItems = _currentData.Data.Length;
		_backgroundAnimator.Play("UICutscenes_FadeIn");
	}

	public void EndCutscene()
	{
		_backgroundAnimator.Play("UICutscenes_FadeOut");
		_cutsceneImageAnimator.Play("UICutsceneImage_FadeOut");
		_currentData = null;
		_numItems = 0;
		_currentItem = -1;

		if (OnCutsceneEnd != null)
		{
			OnCutsceneEnd();
		}
	}

	private void NextScene()
	{
		++_currentItem;

		if (_currentItem < _numItems)
		{
			_typewriter.SetText(_currentData.Data[_currentItem].CutsceneText);
			_cutsceneImage.sprite = _currentData.Data[_currentItem].CutsceneSprite;
			_cutsceneImageAnimator.Play("UICutsceneImage_FadeIn");
		}
		else
		{
			EndCutscene();
		}
	}

	private void Update()
	{
		if (_currentData == null)
		{
			return;
		}

		_typewriter.Update(Time.deltaTime);

		if (_currentItem > -1 && _readyToContinue)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (!_typewriter.IsComplete())
				{
					_typewriter.ForceComplete();
				}
				else
				{
					_cutsceneImageAnimator.Play("UICutsceneImage_FadeOut");
				}
			}
		}
	}

	public void OnCutscenesFadeIn()
	{
		NextScene();
	}

	public void OnPostCutsceneFadeIn()
	{
		_readyToContinue = true;
	}

	public void OnPreCutsceneFadeOut()
	{
		_readyToContinue = false;
		_typewriter.SetText("");
	}

	public void OnPostCutsceneFadeOut()
	{
		NextScene();
	}

}
