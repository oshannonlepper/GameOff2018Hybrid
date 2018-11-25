using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthDisplay : MonoBehaviour {

	[SerializeField] private Text NameText;
	[SerializeField] private Text HealthText;
	[SerializeField] private RectTransform HealthBar;
	[SerializeField] private RectTransform HealthBarBackground;

	private BattleCharacterInstance _target = null;
	private float _defaultWidth = 0.0f;
	private int _maxHP = 0;

	private void Awake()
	{
		_defaultWidth = HealthBarBackground.rect.width;
	}

	public void SetCharacter(BattleCharacterInstance characterInstance)
	{
		if (_target != null)
		{
			_target.OnHealthChange -= _target_OnHealthChange;
		}
		_target = characterInstance;
		if (_target != null)
		{
			NameText.text = _target.Name;
			_maxHP = _target.GetMaxHP();
			SetHealth(_maxHP);
			_target.OnHealthChange += _target_OnHealthChange;
		}
	}

	private void _target_OnHealthChange(int oldHP, int newHP)
	{
		SetHealth(newHP);
	}

	void SetHealth(int health)
	{
		float ratio = 1.0f * health / _maxHP;
		Vector2 size = HealthBar.sizeDelta;
		size.x = ratio * _defaultWidth;
		HealthBar.sizeDelta = size;
		HealthText.text = health + " / " + _maxHP;
	}

}
