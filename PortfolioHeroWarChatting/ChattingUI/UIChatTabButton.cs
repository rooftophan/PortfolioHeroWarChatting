using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatTabButton : MonoBehaviour, IChatNudgeNotable
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] GameObject _selectObj = default(GameObject);
    [SerializeField] Button _tabButton = default(Button);
    [SerializeField] Text _titleText = default(Text);
    [SerializeField] GameObject _nudgeObject = default(GameObject);
    [SerializeField] Text _nudgeText = default(Text);

#pragma warning restore 649

    #endregion

    #region Variables

    WhisperTabButtonType _tabButtonType;

    #endregion

    #region Properties

    public GameObject SelectObj
    {
        get { return _selectObj; }
    }

    public Button TabButton
    {
        get { return _tabButton; }
    }

    public Text TitleText
    {
        get { return _titleText; }
    }

    public GameObject NudgeObject
    {
        get { return _nudgeObject; }
        set { _nudgeObject = value; }
    }

    public WhisperTabButtonType TabButtonType
    {
        get { return _tabButtonType; }
        set { _tabButtonType = value; }
    }

    #endregion

    #region IChatNudgeNotable

    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        return _nudgeObject;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        return _nudgeText;
    }

    #endregion
}
