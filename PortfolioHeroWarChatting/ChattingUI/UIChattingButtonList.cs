using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIChattingButtonList : MonoBehaviour 
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Transform _chatContentsTrans = default(Transform);
	[SerializeField] UIChattingButton _chattingButtonInfo = default(UIChattingButton);
    [SerializeField] UIChattingButton _chattingChannelButtonInfo = default(UIChattingButton);

#pragma warning restore 649

    #endregion

    #region Variables

    #endregion

    #region Properties

    public Transform ChatContentsTrans
	{
		get{ return _chatContentsTrans; }
	}

	public UIChattingButton UIChattingButtonInfo
	{
		get{ return _chattingButtonInfo; }
	}

    public UIChattingButton UIChattingChannelButtonInfo
    {
        get { return _chattingChannelButtonInfo; }
    }

    #endregion
}
