using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleText : MonoBehaviour
{

	public delegate void UIBattleTextEvent();
	public static event UIBattleTextEvent OnTextQueueExhausted;

	[SerializeField] private Text _text;
	[SerializeField] private float _updateRate;

	private List<string> _stringQueue;
	private int _currentIndex = 0;
	private float _updateTimer = 0.0f;

	private void Awake()
	{
		_stringQueue = new List<string>();
	}

	public void QueueText(string input)
	{
		_stringQueue.Add(input);
	}

	private void Update()
	{
		if (_stringQueue.Count == 0)
		{
			return;
		}

		string currentStr = _stringQueue[0];

		_updateTimer += Time.deltaTime;
		if (_updateTimer >= _updateRate)
		{
			++_currentIndex;
			_updateTimer -= _updateRate;

			_text.text = currentStr.Substring(0, Mathf.Min(_currentIndex, currentStr.Length));
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (_currentIndex < currentStr.Length)
			{
				_currentIndex = currentStr.Length;
			}
			else
			{
				_stringQueue.RemoveAt(0);
				_currentIndex = 0;

				if (_stringQueue.Count == 0 && OnTextQueueExhausted != null)
				{
					OnTextQueueExhausted();
				}
			}
		}
	}

}
