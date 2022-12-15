using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDropdownExpandItem
{
    public GameObject itemGameObject;
    public Text itemText;
    public UIWhisperGroupDropdownContent groupDropdownContent;
}

public class UIDropdownExpand : Dropdown
{
    #region Variables

    int _dropdownItemCount = 0;

    Action<UIDropdownExpandItem> _onDropdownAddItem = null;
    Action<UIDropdownExpandItem> _onDropdownRemoveItem = null;

    Action _onCreateComplete = null;
    List<UIDropdownExpandItem> _dropdownItems = new List<UIDropdownExpandItem>();

    #endregion

    #region Properties

    public int DropdownItemCount
    {
        get { return _dropdownItemCount; }
        set { _dropdownItemCount = value; }
    }

    public Action<UIDropdownExpandItem> OnDropdownAddItem
    {
        get { return _onDropdownAddItem; }
        set { _onDropdownAddItem = value; }
    }

    public Action<UIDropdownExpandItem> OnDropdownRemoveItem
    {
        get { return _onDropdownRemoveItem; }
        set { _onDropdownRemoveItem = value; }
    }

    public Action OnCreateComplete
    {
        get { return _onCreateComplete; }
        set { _onCreateComplete = value; }
    }

    public List<UIDropdownExpandItem> DropdownItems
    {
        get { return _dropdownItems; }
    }

    #endregion

    #region Methods

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        DropdownItem retValue = base.CreateItem(itemTemplate);

        UIDropdownExpandItem inputExpandItem = new UIDropdownExpandItem();
        inputExpandItem.itemGameObject = retValue.gameObject;
        inputExpandItem.itemText = retValue.text;
        inputExpandItem.groupDropdownContent = retValue.gameObject.GetComponent< UIWhisperGroupDropdownContent >();
        _dropdownItems.Add(inputExpandItem);

        if(_onDropdownAddItem != null) {
            _onDropdownAddItem(inputExpandItem);
        }

        if(_dropdownItemCount == _dropdownItems.Count) {
            if(_onCreateComplete != null)
                _onCreateComplete();
        }

        return retValue;
    }

    protected override void DestroyItem(DropdownItem item)
    {
        for(int i = 0;i< _dropdownItems.Count;i++) {
            if(_dropdownItems[i].itemGameObject == item.gameObject) {
                if(_onDropdownRemoveItem != null) {
                    _onDropdownRemoveItem(_dropdownItems[i]);
                }

                _dropdownItems.RemoveAt(i);
                break;
            }
        }
        base.DestroyItem(item);
    }

    #endregion
}
