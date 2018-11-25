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
	private TypewriterText _typewriter;

	private void Awake()
	{
		_stringQueue = new List<string>();
		_typewriter = new TypewriterText(_text, _updateRate);
	}

	public void QueueText(string input)
	{
		_stringQueue.Add(input);
		if (_stringQueue.Count == 1)
		{
			_typewriter.SetText(_stringQueue[0]);
		}
	}

	private void Update()
	{
		if (_stringQueue.Count == 0)
		{
			return;
		}

		_typewriter.Update(Time.deltaTime);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (_typewriter.IsComplete())
			{
				_stringQueue.RemoveAt(0);
				if (_stringQueue.Count > 0)
				{
					_typewriter.SetText(_stringQueue[0]);
				}
				else if (OnTextQueueExhausted != null)
				{
					OnTextQueueExhausted();
				}
			}
			else
			{
				_typewriter.ForceComplete();
			}
		}
	}

}
