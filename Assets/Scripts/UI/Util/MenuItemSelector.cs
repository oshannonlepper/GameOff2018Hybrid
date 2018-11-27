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
	public delegate void MenuItemSelectorEvent(int index, int value);
	public event MenuItemSelectorEvent OnItemSelected;

	private List<IMenuItemSelectorItem> _itemList;
	private bool _wrapAround = false;
	private int _index = 0;

	private KeyCode _previousItemKey;
	private KeyCode _nextItemKey;

	public MenuItemSelector()
	{
		_itemList = new List<IMenuItemSelectorItem>();
		SetNavigateMode(EMenuItemNavigateMode.TopToBottom);
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

			Debug.Log("MenuItemSelector Changed: " + _index + ", " + _itemList[_index].GetItemLabel());
		}

		if (Input.GetKeyDown(_previousItemKey))
		{
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

			Debug.Log("MenuItemSelector Changed: " + _index + ", " + _itemList[_index].GetItemLabel());
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

}
