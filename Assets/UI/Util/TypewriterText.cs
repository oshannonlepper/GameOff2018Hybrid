using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterText
{

	public delegate void TypewriterEvent();
	public event TypewriterEvent OnComplete;

	private Text _textOutput;
	private float _typeRate = 0.2f;
	private float _time = 0.0f;
	private string _textString = "";
	private int _textIndex = -1;
	private bool _complete = false;

	public TypewriterText(Text inText, float typeRate=0.2f)
	{
		_textOutput = inText;
		_typeRate = typeRate;
	}

	public void SetText(string text)
	{
		_textString = text;
		_textIndex = -1;
		_textOutput.text = "";
	}

	public void Update(float delta)
	{
		_time += delta;
		if (_time >= _typeRate)
		{
			_time -= _typeRate;
			++_textIndex;

			if (IsComplete())
			{
				_textOutput.text = _textString;
				Completed();
			}
			else
			{
				_textOutput.text = _textString.Substring(0, _textIndex);
			}
		}
	}

	public bool IsComplete()
	{
		return _textIndex >= _textString.Length;
	}

	public void ForceComplete()
	{
		_textIndex = _textString.Length;
		_textOutput.text = _textString;
		Completed();
	}

	private void Completed()
	{
		if (OnComplete != null)
		{
			OnComplete();
		}
	}

}
