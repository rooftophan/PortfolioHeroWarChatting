using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MultiChatTextInfo
{
	#region Variables

	ChatDefinition.ChatViewTextType _chatViewType;
	string _chatPartKeyValue;
	string _chatPartMessage;
	ChatPartMessageInfo _partMessageInfo = null;
	Action<ChatPartMessageInfo> _onClickPartMessage = null;
	UIChatPartMessage _uiPartMessage = null;
	float _partTextWidth;
	int _lineCount;
    bool _isChatColor = false;
    Color _chatColor;

	#endregion

	#region Properties

	public ChatDefinition.ChatViewTextType ChatViewType
	{
		get{ return _chatViewType; }
		set{ _chatViewType = value; }
	}

	public string ChatPartKeyValue
	{
		get{ return _chatPartKeyValue; }
		set{ _chatPartKeyValue = value; }
	}

	public string ChatPartMessage
	{
		get{ return _chatPartMessage; }
		set{ _chatPartMessage = value; }
	}

	public ChatPartMessageInfo PartMessageInfo
	{
		get{ return _partMessageInfo; }
		set{ _partMessageInfo = value; }
	}

	public Action<ChatPartMessageInfo> OnClickPartMessage
	{
		get{ return _onClickPartMessage; }
		set{ _onClickPartMessage = value; }
	}

	public UIChatPartMessage UIPartMessage
	{
		get{ return _uiPartMessage; }
		set{ _uiPartMessage = value; }
	}

	public float PartTextWidth
	{
		get{ return _partTextWidth; }
		set{ _partTextWidth = value; }
	}

	public int LineCount
	{
		get{ return _lineCount; }
		set{ _lineCount = value; }
	}

    public bool IsChatColor
    {
        get { return _isChatColor; }
        set { _isChatColor = value; }
    }

    public Color ChatColor
    {
        get { return _chatColor; }
        set { _chatColor = value; }
    }

    #endregion

    #region Methods

    public void CopyChatTextInfo(MultiChatTextInfo chatTextInfo)
	{
		this._chatViewType = chatTextInfo._chatViewType;
		this._chatPartKeyValue = chatTextInfo._chatPartKeyValue;
		this._chatPartMessage = chatTextInfo._chatPartMessage;
		this._partMessageInfo = chatTextInfo._partMessageInfo;
		this._onClickPartMessage = chatTextInfo._onClickPartMessage;
		this._uiPartMessage = chatTextInfo._uiPartMessage;
		this._partTextWidth = chatTextInfo._partTextWidth;
		this._lineCount = chatTextInfo._lineCount;
        this._isChatColor = chatTextInfo._isChatColor;
        this._chatColor = chatTextInfo._chatColor;
    }

	#endregion
}

public class UIMultiChatMessage : MonoBehaviour, IScrollObjectInfo
{
	#region Definitions

	public enum ChatAlign
	{
		LeftAlign,
		CenterAlign,
		RightAlign,
	}

    #endregion

    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Transform _makedTextTrans = default(Transform);
	[SerializeField] float _textMaxWidth = default(float);
	[SerializeField] int _textHeight = default(int);
	[SerializeField] ChatAlign _chatAlign = default(ChatAlign);

	[SerializeField] UIChatPartMessage _normalPartMessage = default(UIChatPartMessage);
	[SerializeField] UIChatPartMessage _normalButtonPartMessage = default(UIChatPartMessage);
	[SerializeField] UIChatPartMessage _buttonBGPartMessage = default(UIChatPartMessage);
	[SerializeField] UIChatPartMessage _shortcutButtonPartMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _timeNormalPartMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _currencyImgPartMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _partyAcceptButtonPartMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _partyDenyButtonPartMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _partyRoleImgPartyMessage = default(UIChatPartMessage);

#pragma warning restore 649

    #endregion

    #region Variables

    List<MultiChatTextInfo> _multiChatTextInfos = null;
	float _messageWidth;
	float _messageHeight;

	MultiChatTextInfo _confirmMsgTextInfo = null;
	long _chatTimeStamp;
	float _leftLineWidth;
	int _lastLineIndex;

    List<UIChatPartMessage> _previewPartMessages = new List<UIChatPartMessage>();

    #endregion

    #region Properties

    public GameObject ScrollGameObject
	{
		get{ return this.gameObject; }
	}

    public int TextHeight
    {
        get { return _textHeight; }
    }


    public float ObjectMaxWidth
	{
		get{ return _textMaxWidth; }
		set{ _textMaxWidth = value; }
	}

	public float ObjectWidth
	{
		get{ return _messageWidth; }
	}

	public float ObjectHeight
	{
		get{ return _messageHeight; }
	}

    public List<MultiChatTextInfo> MultiChatTextInfos
    {
        get { return _multiChatTextInfos; }
    }

    #endregion

    #region Methods

    public void SetChatMessageList(List<MultiChatTextInfo> chatTextInfos, Color chatColor, int messageType = -1)
	{
		if (chatTextInfos == null || chatTextInfos.Count == 0)
			return;

        _multiChatTextInfos = chatTextInfos;

        int heightCount = 0;

		_messageWidth = _textMaxWidth;

		int maxHeightCount = 0;
		maxHeightCount = chatTextInfos [chatTextInfos.Count - 1].LineCount + 1;

		_messageHeight = maxHeightCount * _textHeight;

		int quotientHeight = maxHeightCount / 2;
		int restHeight = maxHeightCount % 2;

		float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (_textHeight * quotientHeight) - (_textHeight * 0.5f);
        } else { // Odd
            startY = (_textHeight * quotientHeight);
        }

        float startX = -(_textMaxWidth * 0.5f);

		for (int i = 0; i < chatTextInfos.Count; i++) {
			MultiChatTextInfo chatTextInfo = chatTextInfos [i];

			if (chatTextInfo == null)
				continue;

            SetChatTextInfo(chatTextInfo, chatColor, messageType);

            if (heightCount != chatTextInfo.LineCount) {
				heightCount++;
				startY -= _textHeight;
				startX = -(_textMaxWidth * 0.5f);
			}

			startX += chatTextInfo.PartTextWidth * 0.5f;

            if(chatTextInfo.ChatViewType == ChatDefinition.ChatViewTextType.CurrencyImgText) {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX - (chatTextInfo.PartTextWidth * 0.5f) + 15f, startY, 0f);
            } else {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX, startY, 0f);
            }
			

			startX += chatTextInfo.PartTextWidth * 0.5f;
		}
	}

    public void SetPartyQuickChatMessageList(List<MultiChatTextInfo> chatTextInfos, Color chatColor, int messageType = -1)
    {
        if (chatTextInfos == null || chatTextInfos.Count == 0)
            return;

        _multiChatTextInfos = chatTextInfos;

        int heightCount = 0;

        _messageWidth = _textMaxWidth;

        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        _messageHeight = maxHeightCount * _textHeight;

        int quotientHeight = maxHeightCount / 2;
        int restHeight = maxHeightCount % 2;

        float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (_textHeight * quotientHeight) - (_textHeight * 0.5f);
        } else { // Odd
            startY = (_textHeight * quotientHeight);
        }

        float startX = -(_textMaxWidth * 0.5f);

        for (int i = 0; i < chatTextInfos.Count; i++) {
            MultiChatTextInfo chatTextInfo = chatTextInfos[i];

            if (chatTextInfo == null)
                continue;

            if (chatTextInfo.ChatPartKeyValue == "user") continue;

            SetChatTextInfo(chatTextInfo, chatColor, messageType);

            if (heightCount != chatTextInfo.LineCount) {
                heightCount++;
                startY -= _textHeight;
                startX = -(_textMaxWidth * 0.5f);
            }

            startX += chatTextInfo.PartTextWidth * 0.5f;

            if (chatTextInfo.ChatViewType == ChatDefinition.ChatViewTextType.CurrencyImgText) {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX - (chatTextInfo.PartTextWidth * 0.5f) + 15f, startY, 0f);
            } else {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX, startY, 0f);
            }

            startX += chatTextInfo.PartTextWidth * 0.5f;
        }
    }

    public void SetChatTextInfo(MultiChatTextInfo chatTextInfo, Color chatColor, int messageType, bool isButtonEnable = true)
    {
        RectTransform msgRectTrans = null;

        bool isColorSet = false;
        Color curColorChat = Color.white;
        switch (chatTextInfo.ChatViewType) {
            case ChatDefinition.ChatViewTextType.NormalText:
                UIChatPartMessage chatNormalPart = Instantiate(_normalPartMessage) as UIChatPartMessage;
                chatTextInfo.UIPartMessage = chatNormalPart;
                break;
            case ChatDefinition.ChatViewTextType.TimeNormalText:
                UIChatPartMessage chatTimeNormalPart = Instantiate(_timeNormalPartMessage) as UIChatPartMessage;
                chatTextInfo.UIPartMessage = chatTimeNormalPart;
                break;
            case ChatDefinition.ChatViewTextType.NormalButtonText: {
                    UIChatPartMessage normalButtonPart = Instantiate(_normalButtonPartMessage) as UIChatPartMessage;
                    msgRectTrans = normalButtonPart.GetComponent<RectTransform>();
                    msgRectTrans.sizeDelta = new Vector2(chatTextInfo.PartTextWidth, msgRectTrans.sizeDelta.y);
                    chatTextInfo.UIPartMessage = normalButtonPart;

                    ChatDefinition.PartMessageType partMessageType = ChatHelper.GetPartMessageType(chatTextInfo.ChatPartKeyValue);

                    switch (partMessageType) {
                        case ChatDefinition.PartMessageType.UserInfoType:
                            if (messageType != (int)ChatDefinition.ChatMessageType.WhisperFriendChat && messageType != (int)ChatDefinition.ChatMessageType.WhisperGuildChat) {
                                if (chatTextInfo.PartMessageInfo != null && chatTextInfo.PartMessageInfo.partValues != null && chatTextInfo.PartMessageInfo.partValues.Length > 2 && chatTextInfo.PartMessageInfo.partValues[2] == "enemyColor") {
                                    isColorSet = true;
                                    curColorChat = ColorPreset.CHAT_EnemyUserName;
                                } else if (chatTextInfo.ChatPartKeyValue == "user") {
                                    if (chatColor != ColorPreset.CHAT_MY_MESSAGE) {
                                        isColorSet = true;
                                        curColorChat = ColorPreset.CHAT_CharacterName;
                                    }
                                }
                            }
                            break;
                        case ChatDefinition.PartMessageType.ItemInfoType:
                            isColorSet = true;
                            curColorChat = ColorPreset.CHAT_ItemName;
                            break;
                        case ChatDefinition.PartMessageType.HeroChallengeMaxScore:
                            isColorSet = true;
                            curColorChat = ColorPreset.CHAT_HEROCHALLENGE_MAXSCORE;
                            break;
                        case ChatDefinition.PartMessageType.EnemyUserInfoType:
                            isColorSet = true;
                            curColorChat = ColorPreset.CHAT_CharacterName;
                            break;
                    }
                }
                break;
            case ChatDefinition.ChatViewTextType.ButtonBGText: {
                    UIChatPartMessage buttonBGPart = Instantiate(_buttonBGPartMessage) as UIChatPartMessage;
                    msgRectTrans = buttonBGPart.GetComponent<RectTransform>();
                    msgRectTrans.sizeDelta = new Vector2(chatTextInfo.PartTextWidth - 10, msgRectTrans.sizeDelta.y);
                    chatTextInfo.UIPartMessage = buttonBGPart;

                    ChatDefinition.PartMessageType partMessageType = ChatHelper.GetPartMessageType(chatTextInfo.ChatPartKeyValue);

                    switch (partMessageType) {
                        case ChatDefinition.PartMessageType.UserInfoType:
                            if (messageType != (int)ChatDefinition.ChatMessageType.WhisperFriendChat && messageType != (int)ChatDefinition.ChatMessageType.WhisperGuildChat) {
                                if (chatTextInfo.PartMessageInfo.partValues != null && chatTextInfo.PartMessageInfo.partValues.Length > 2 && chatTextInfo.PartMessageInfo.partValues[2] == "enemyColor") {
                                    isColorSet = true;
                                    curColorChat = ColorPreset.CHAT_EnemyUserName;
                                } else if (chatTextInfo.ChatPartKeyValue == "user") {
                                    if (chatColor != ColorPreset.CHAT_MY_MESSAGE) {
                                        isColorSet = true;
                                        curColorChat = ColorPreset.CHAT_CharacterName;
                                    }
                                }
                            }
                            break;
                        case ChatDefinition.PartMessageType.ItemInfoType:
                            isColorSet = true;
                            curColorChat = ColorPreset.CHAT_ItemName;
                            break;
                    }
                }
                break;
            case ChatDefinition.ChatViewTextType.ShortcutButton: {
                    UIChatPartMessage shortcutPart = Instantiate(_shortcutButtonPartMessage) as UIChatPartMessage;
                    msgRectTrans = shortcutPart.GetComponent<RectTransform>();
                    msgRectTrans.sizeDelta = new Vector2(chatTextInfo.PartTextWidth, msgRectTrans.sizeDelta.y);
                    chatTextInfo.UIPartMessage = shortcutPart;
                }
                break;
            case ChatDefinition.ChatViewTextType.CurrencyImgText: {
                    UIChatPartMessage currencyImgPart = Instantiate(_currencyImgPartMessage) as UIChatPartMessage;
                    if (chatTextInfo.PartMessageInfo != null && chatTextInfo.PartMessageInfo.partValues != null && chatTextInfo.PartMessageInfo.partValues.Length > 0) {
                        string iconPath = chatTextInfo.PartMessageInfo.partValues[0];
                        if (currencyImgPart.IconImg != null)
                            currencyImgPart.IconImg.sprite = ResourceLoader.Load<Sprite>(iconPath);
                    }

                    chatTextInfo.UIPartMessage = currencyImgPart;
                    isColorSet = true;
                    curColorChat = ColorPreset.CHAT_Currency;
                }
                break;
            case ChatDefinition.ChatViewTextType.PartyAcceptButton: {
                    UIChatPartMessage shortcutPart = Instantiate(_partyAcceptButtonPartMessage) as UIChatPartMessage;
                    msgRectTrans = shortcutPart.GetComponent<RectTransform>();
                    msgRectTrans.sizeDelta = new Vector2(chatTextInfo.PartTextWidth, msgRectTrans.sizeDelta.y);
                    chatTextInfo.UIPartMessage = shortcutPart;
                }
                break;
            case ChatDefinition.ChatViewTextType.PartyDenyButton: {
                    UIChatPartMessage shortcutPart = Instantiate(_partyDenyButtonPartMessage) as UIChatPartMessage;
                    msgRectTrans = shortcutPart.GetComponent<RectTransform>();
                    msgRectTrans.sizeDelta = new Vector2(chatTextInfo.PartTextWidth, msgRectTrans.sizeDelta.y);
                    chatTextInfo.UIPartMessage = shortcutPart;
                }
                break;
        }

        chatTextInfo.UIPartMessage.gameObject.SetActive(true);
        chatTextInfo.UIPartMessage.transform.SetParent(_makedTextTrans);
        chatTextInfo.UIPartMessage.transform.localScale = Vector3.one;

        if(isButtonEnable) {
            if (chatTextInfo.OnClickPartMessage != null && chatTextInfo.UIPartMessage.MessageButton != null) {
                chatTextInfo.UIPartMessage.MessageButton.onClick.AddListener(() => chatTextInfo.OnClickPartMessage(chatTextInfo.PartMessageInfo));
            }
        } else {
            if (chatTextInfo.OnClickPartMessage != null && chatTextInfo.UIPartMessage.MessageButton != null) {
                chatTextInfo.UIPartMessage.MessageButton.enabled = false;
            }
        }

        if (chatTextInfo.UIPartMessage.MessageText != null) {
            chatTextInfo.UIPartMessage.MessageText.text = chatTextInfo.ChatPartMessage;

            if(chatTextInfo.IsChatColor) {
                chatTextInfo.UIPartMessage.MessageText.color = chatTextInfo.ChatColor;
            } else {
                if (!isColorSet)
                    chatTextInfo.UIPartMessage.MessageText.color = chatColor;
                else
                    chatTextInfo.UIPartMessage.MessageText.color = curColorChat;
            }
        }
    }

    public List<MultiChatTextInfo> SetPreviewChatMessageList(List<MultiChatTextInfo> chatTextInfos, Color chatColor, int messageType)
    {
        if (chatTextInfos == null || chatTextInfos.Count == 0)
            return null;

        List<MultiChatTextInfo> previewTextInfos = new List<MultiChatTextInfo>();

        _messageWidth = _textMaxWidth;

        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        _messageHeight = maxHeightCount * _textHeight;

        for (int i = 0; i < chatTextInfos.Count; i++) {
            MultiChatTextInfo previewChatInfo = new MultiChatTextInfo();
            previewChatInfo.CopyChatTextInfo(chatTextInfos[i]);

            if (previewChatInfo == null)
                continue;

            if (previewChatInfo.LineCount > 0) {
                MultiChatTextInfo lastChatInfo = previewTextInfos[previewTextInfos.Count - 1];
                char[] chatChars = lastChatInfo.ChatPartMessage.ToCharArray();
                if (chatChars != null && chatChars.Length > 1) {
                    char[] newChars = new char[chatChars.Length - 1];
                    for (int j = 0; j < newChars.Length; j++) {
                        newChars[j] = chatChars[j];
                    }

                    lastChatInfo.ChatPartMessage = string.Format("{0}...", new string(newChars));
                    lastChatInfo.UIPartMessage.MessageText.text = lastChatInfo.ChatPartMessage;
                }
                break;
            }

            SetChatTextInfo(previewChatInfo, chatColor, messageType, false);

            previewTextInfos.Add(previewChatInfo);
        }

        float startY = 0f;
        float startX = _textMaxWidth * 0.5f;

        for (int i = previewTextInfos.Count - 1; i>= 0 ;i--) {
            MultiChatTextInfo chatTextInfo = previewTextInfos[i];
            float chatScaleWidth = chatTextInfo.PartTextWidth * 0.8f;
            startX -= chatScaleWidth * 0.5f;

            if (chatTextInfo.ChatViewType == ChatDefinition.ChatViewTextType.CurrencyImgText) {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX - (chatTextInfo.PartTextWidth * 0.5f) + 15f, startY, 0f);
            } else {
                chatTextInfo.UIPartMessage.transform.localPosition = new Vector3(startX, startY, 0f);
            }

            chatTextInfo.UIPartMessage.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            startX -= chatScaleWidth * 0.5f;

            chatTextInfo.UIPartMessage.SetTweenAlpha();

            _previewPartMessages.Add(chatTextInfo.UIPartMessage);
        }

        return previewTextInfos;
    }

    public void ResetChatMessage()
	{
		if (_multiChatTextInfos == null || _multiChatTextInfos.Count == 0)
			return;

		for (int i = 0; i < _multiChatTextInfos.Count; i++) {
			if (_multiChatTextInfos [i].UIPartMessage != null) {
				Destroy (_multiChatTextInfos [i].UIPartMessage.gameObject);
                _multiChatTextInfos[i].UIPartMessage = null;
            }
		}
			
		_multiChatTextInfos = null;
	}

    public void ResetPreviewChatMessage()
    {
        if (_previewPartMessages == null || _previewPartMessages.Count == 0)
            return;

        for (int i = 0; i < _previewPartMessages.Count; i++) {
            if (_previewPartMessages[i] != null) {
                Destroy(_previewPartMessages[i].gameObject);
                _previewPartMessages[i] = null;
            }
        }

        _previewPartMessages.Clear();
    }

    #endregion
}
