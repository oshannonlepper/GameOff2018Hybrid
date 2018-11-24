using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleText : MonoBehaviour
{

	[SerializeField] private Text _text;
	[SerializeField] private float _updateRate;

	private string _finalDisplayString = "";
	private int _currentIndex = 0;
	private float _updateTimer = 0.0f;

	private void OnEnable()
	{

	}

	public void SetText(string input)
	{
		_finalDisplayString = input;
		_currentIndex = 0;
	}

	private void Update()
	{
		_updateTimer += Time.deltaTime;
		if (_updateTimer >= _updateRate)
		{
			++_currentIndex;
			_updateTimer -= _updateRate;

			_text.text = _finalDisplayString.Substring(0, Mathf.Min(_currentIndex, _finalDisplayString.Length));
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			_currentIndex = _finalDisplayString.Length;
		}
	}

}
