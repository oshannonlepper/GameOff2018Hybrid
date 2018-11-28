using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenuItemSelectorItem
{

	string GetItemLabel();
	int GetValue();
	void OnSelect();

}

public enum EMenuItemNavigateMode
{
	TopToBottom,
	LeftToRight,
}

// List of items that are navigable + selectable via arrow keys + space bar
public class MenuItemSelector
{
	// events for selection + navigation
	public delegate void MenuItemSelectorEvent(int index, int value);
	public event MenuItemSelectorEvent OnItemSelected;
	public event MenuItemSelectorEvent OnSelectedItemChanged;

	// list of elements added to the menu
	private List<IMenuItemSelectorItem> _itemList;

	// If true, navigating to the end of the menu will send you back to the start and vice versa
	// If false, trying to navigate past either end does nothing
	private bool _wrapAround = false;

	// Index of the current selected item
	private int _index = 0;

	// Message to appear to the player when faced with this menu
	private string _menuCaption = "";

	// Keys to be pressed for navigating the menu, updated by SetNavigateMode
	private KeyCode _previousItemKey;
	private KeyCode _nextItemKey;

	public MenuItemSelector()
	{
		_itemList = new List<IMenuItemSelectorItem>();
		SetNavigateMode(EMenuItemNavigateMode.TopToBottom);
	}

	public void SetCaption(string caption)
	{
		_menuCaption = caption;
	}

	public void SetNavigateMode(EMenuItemNavigateMode inNavigateMode)
	{
		if (inNavigateMode == EMenuItemNavigateMode.LeftToRight)
		{
			_previousItemKey = KeyCode.LeftArrow;
			_nextItemKey = KeyCode.RightArrow;
		}
		else if (inNavigateMode == EMenuItemNavigateMode.TopToBottom)
		{
			_previousItemKey = KeyCode.UpArrow;
			_nextItemKey = KeyCode.DownArrow;
		}
	}

	public void Update(float deltaTime)
	{
		if (_itemList.Count == 0)
		{
			return;
		}

		if (Input.GetKeyDown(_nextItemKey))
		{
			int oldIndex = _index;

			if (_index >= _itemList.Count - 1)
			{
				if (_wrapAround)
				{
					_index = 0;
				}
			}
			else
			{
				++_index;
			}

			if (oldIndex != _index && OnSelectedItemChanged != null)
			{
				OnSelectedItemChanged(_index, _itemList[_index].GetValue());
			}
		}

		if (Input.GetKeyDown(_previousItemKey))
		{
			int oldIndex = _index;

			if (_index <= 0)
			{
				if (_wrapAround)
				{
					_index = _itemList.Count - 1;
				}
			}
			else
			{
				--_index;
			}

			if (oldIndex != _index && OnSelectedItemChanged != null)
			{
				OnSelectedItemChanged(_index, _itemList[_index].GetValue());
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			_itemList[_index].OnSelect();
			
			if (OnItemSelected != null)
			{
				OnItemSelected(_index, _itemList[_index].GetValue());
			}
		}
	}

	public void AddItem(IMenuItemSelectorItem item)
	{
		_itemList.Add(item);
	}

	public bool RemoveItem(IMenuItemSelectorItem item)
	{
		return _itemList.Remove(item);
	}

	public void Reset()
	{
		_itemList.Clear();
		_index = 0;
	}

	public int GetNumItems()
	{
		return _itemList.Count;
	}

	public string GetLabel(int index)
	{
		return _itemList[index].GetItemLabel();
	}

	public int GetSelectedIndex()
	{
		return _index;
	}

	public string GetCaption()
	{
		return _menuCaption;
	}

}
