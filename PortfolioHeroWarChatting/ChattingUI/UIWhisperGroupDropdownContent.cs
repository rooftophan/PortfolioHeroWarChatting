using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWhisperGroupDropdownContent : MonoBehaviour, IChatNudgeNotable
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] GameObject _notCheckNumObject = default(GameObject);
    [SerializeField] Text _notCheckNumText = default(Text);

#pragma warning restore 649

    #endregion

    #region Properties

    public GameObject NotCheckNumObject
    {
        get { return _notCheckNumObject; }
    }

    public Text NotCheckNumText
    {
        get { return _notCheckNumText; }
    }

    #endregion

    #region IChatNudgeNotable

    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        return _notCheckNumObject;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        return _notCheckNumText;
    }

    #endregion
}
