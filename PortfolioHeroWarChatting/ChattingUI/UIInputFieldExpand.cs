using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputFieldExpand : InputField
{
	#region Properties

	public TouchScreenKeyboard TouchScreenKeyboard
	{
		get{ return m_Keyboard; }
	}

    #endregion

    #region Methods

    public void ResetInputFieldExpand()
    {
        text = "";
        m_Text = "";
        m_TextComponent.text = "";
        textComponent.text = "";
    }

    #endregion
}
