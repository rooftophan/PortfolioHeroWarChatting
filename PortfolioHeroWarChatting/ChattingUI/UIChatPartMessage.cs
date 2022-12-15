using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatPartMessage : MonoBehaviour 
{

    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] ChatDefinition.ChatViewTextType _messageType = default(ChatDefinition.ChatViewTextType);
	[SerializeField] Text _messageText = default(Text);
	[SerializeField] Button _messageButton = default(Button);
	[SerializeField] Image _messageBGImg = default(Image);
    [SerializeField] Image _iconImg = default(Image);

#pragma warning restore 649

    #endregion

    #region Properties

    public ChatDefinition.ChatViewTextType MessageType
	{
		get{ return _messageType; }
	}

	public Text MessageText
	{
		get{ return _messageText; }
	}

	public Button MessageButton
	{
		get{ return _messageButton; }
	}

	public Image MessageBGImg
	{
		get{ return _messageBGImg; }
	}

    public Image IconImg
    {
        get { return _iconImg; }
    }

    #endregion

    #region Methods

    public float GetTextWidth(string textValue)
	{
		_messageText.text = textValue;

		return _messageText.preferredWidth;
	}

	public float GetTextWidthByChar(char textChar)
	{
		_messageText.text = new string(textChar, 1);

		return _messageText.preferredWidth;
	}

    public void SetTweenAlpha()
    {
        List<UGUITweenAlpha> _tweenerList = new List<UGUITweenAlpha>();
        if(_messageText != null) {
            _tweenerList.Add(_messageText.gameObject.AddComponent<UGUITweenAlpha>());
        }

        if(_messageButton != null) {
            if(_messageText == null || _messageText.gameObject != _messageButton.gameObject)
                _tweenerList.Add(_messageButton.gameObject.AddComponent<UGUITweenAlpha>());
        }

        if (_messageBGImg != null) {
            _tweenerList.Add(_messageBGImg.gameObject.AddComponent<UGUITweenAlpha>());
        }

        if(_iconImg != null) {
            _tweenerList.Add(_iconImg.gameObject.AddComponent<UGUITweenAlpha>());
        }

        for(int i = 0;i< _tweenerList.Count;i++) {
            _tweenerList[i].from = 0f;
            _tweenerList[i].to = 1f;

            _tweenerList[i].duration = 0.5f;
            _tweenerList[i].ResetToBeginning();
            _tweenerList[i].PlayForward();
        }
    }

	#endregion
}
