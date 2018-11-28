using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleMenu : MonoBehaviour
{

	[SerializeField] private Text _menuItemsText;

	private MenuItemSelector _currentMenu = null;
	private int _selectedIndex = 0;

	public void SetMenu(MenuItemSelector menu)
	{
		if (_currentMenu != null)
		{
			_currentMenu.OnItemSelected -= OnItemSelected;
			_currentMenu.OnSelectedItemChanged -= OnSelectedItemChanged;
		}

		_currentMenu = menu;

		gameObject.SetActive(_currentMenu != null);

		if (_currentMenu != null)
		{
			_currentMenu.OnItemSelected += OnItemSelected;
			_currentMenu.OnSelectedItemChanged += OnSelectedItemChanged;
			_selectedIndex = _currentMenu.GetSelectedIndex();

			UpdateMenuItems();
		}
	}
	
	void UpdateMenuItems()
	{
		int numItems = _currentMenu.GetNumItems();

		string text = "";
		for (int index = 0; index < numItems; ++index)
		{
			if (index == _selectedIndex)
			{
				text += ">";
			}
			text += "\t"+_currentMenu.GetLabel(index);
			if (index < numItems-1)
			{
				text += "\n";
			}
		}
		_menuItemsText.text = text;
	}

	void OnItemSelected(int index, int value)
	{
		SetMenu(null);
	}

	void OnSelectedItemChanged(int index, int value)
	{
		_selectedIndex = index;
		UpdateMenuItems();
	}

}
