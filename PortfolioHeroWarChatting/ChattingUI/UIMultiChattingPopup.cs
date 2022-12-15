using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UIMultiChattingPopup : MonoBehaviour, IChatNudgeNotable /* Whisper */
{
	#region Definitions

	public enum ChatInputFieldType
	{
        None                    = -1,
		NormalInputField        = 0,
        ChannelInputField       = 1,
		WhisperInputField       = 2,
        QuickParty              = 3,
	}

    #endregion

    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Button _buttonClose = default(Button);

	[SerializeField] UIMultiChatMessage _uiMultiChatMessage = default(UIMultiChatMessage);

	[SerializeField] Transform _cacheChatParentTrans = default(Transform);

	[SerializeField] float _chatScrollMaxWidth = default(float);

	[SerializeField] InputField _editorChannelInputField = default(InputField);
	[SerializeField] Text _editorChannelPlaceholderText = default(Text);

	[Header("Chatting Button List")]
	[SerializeField] UIChattingButtonList _chatButtonList = default(UIChattingButtonList);

	[Header("Chatting Input Objects")]
	[SerializeField] UIInputFieldExpand _inputField = default(UIInputFieldExpand);
    [SerializeField] Text _inputFieldPlaceholder = default(Text);
	[SerializeField] ObjectScrollManager _objScrollManager = default(ObjectScrollManager);
	[SerializeField] Transform _chatNoticeRootTrans = default(Transform);
    [SerializeField] GameObject _normalMessageScrollObj = default(GameObject);
    [SerializeField] UIChatGuildMessageManager _chatGuildMessageManager = default(UIChatGuildMessageManager);
    [SerializeField] Button _bottomMoveButton;
    [SerializeField] Transform _normalBottomPosTrans;
    [SerializeField] Transform _partyChatBottomPosTrans;

    [Header("Channel Input Objects")]
    [SerializeField] GameObject _channelInputObject = default(GameObject);
	[SerializeField] UIInputFieldExpand _channelInputField = default(UIInputFieldExpand);
    [SerializeField] Text _channelInputFieldPlaceholder = default(Text);
	[SerializeField] Button _channelChangeButton = default(Button);
	[SerializeField] Text _channelNumText = default(Text);

	[Header("Whisper Objects")]
	[SerializeField] GameObject _whisperObject = default(GameObject);
	[SerializeField] UIInputFieldExpand _whisperInputField = default(UIInputFieldExpand);
    [SerializeField] Text _whisperInputFieldPlaceholder = default(Text);
	[SerializeField] Button _whisperTargetButton = default(Button);
	[SerializeField] Text _whisperTargetText = default(Text);
    [SerializeField] GameObject _whisperNotCheckRootObj = default(GameObject);
    [SerializeField] GameObject _whisperNotConfirmObj = default(GameObject);
    [SerializeField] Text _whisperNotConfirmText = default(Text);

    [Header("Party Objects")]
    [SerializeField] GameObject _partyQuickChatObjs = default(GameObject);
    [SerializeField] Button _partyQuickChatButton = default(Button);
    [SerializeField] UIPartyQuickChatMsgList _quickChatMsgList = default(UIPartyQuickChatMsgList);

    [Header("Cheat Buttons")]
    [SerializeField] Button _cheatTestButton = default(Button);

#pragma warning restore 649

    #endregion

    #region Variables

    UIMultiChatMessage[] _multiChatCacheMessages;
	int _multiCacheIndex = 0;

	UIMultiChatMessage[] _noticeCacheMessages;
	int _noticeCacheIndex = 0;
	int _noticeMaxCount = 3;
	float _curNoticeHeight = 0f;
	List<UIMultiChatMessage> _noticeMessageInfos = new List<UIMultiChatMessage>();
	float _noticeGapValue = 0f;

	UIMultiChatMessage[] _subNoticeCacheMessages;
	int _subNoticeCacheIndex = 0;
	int _subNoticeMaxCount = 2;
	List<UIMultiChatMessage> _subNoticeMsgInfos = new List<UIMultiChatMessage>();

	ChatInputFieldType _inputFieldType;

	Action<string> _onInputKeyboardAction = null;
	Action<string> _onWhisperInputKeyboardAction = null;

	#endregion

	#region Properties

	public UIChattingButtonList UIChatButtonList
	{
		get{ return _chatButtonList; }
	}

	public ObjectScrollManager ObjScrollManager
	{
		get{ return _objScrollManager; }
	}

	public float ChatScrollMaxWidth
	{
		get{ return _chatScrollMaxWidth; }
		set{ _chatScrollMaxWidth = value; }
	}

	public InputField EditorChannelInputField
	{
		get{ return _editorChannelInputField; }
	}

	public Text EditorChannelPlaceholderText
	{
		get{ return _editorChannelPlaceholderText; }
	}

	public Transform ChatNoticeRootTrans
	{
		get{ return _chatNoticeRootTrans; }
	}

	public int NoticeMaxCount
	{
		get{ return _noticeMaxCount; }
	}

	public int SubNoticeMaxCount
	{
		get{ return _subNoticeMaxCount; }
	}

	public float CurNoticeHeight
	{
		get{ return _curNoticeHeight; }
	}

	public UIInputFieldExpand ChatInputField
	{
		get{ return _inputField; }
	}

    public GameObject WhisperNotConfirmObj
    {
        get { return _whisperNotConfirmObj; }
    }

    public Text WhisperNotConfirmText
    {
        get { return _whisperNotConfirmText; }
    }

    public GameObject NormalMessageScrollObj
    {
        get { return _normalMessageScrollObj; }
    }

    public UIChatGuildMessageManager ChatGuildMessageManager
    {
        get { return _chatGuildMessageManager; }
    }

    public ChatInputFieldType InputFieldType
	{
		get{ return _inputFieldType; }
		set{ _inputFieldType = value; }
	}

	public Action<string> OnInputKeyboardAction
	{
		get{ return _onInputKeyboardAction; }
		set{ _onInputKeyboardAction = value; }
	}

	public Action<string> OnWhisperInputKeyboardAction
	{
		get{ return _onWhisperInputKeyboardAction; }
		set{ _onWhisperInputKeyboardAction = value; }
	}

    public Transform CacheChatParentTrans
    {
        get { return _cacheChatParentTrans; }
    }

    public Button ChannelChangeButton
    {
        get { return _channelChangeButton; }
    }

    public Text ChannelNumText
    {
        get { return _channelNumText; }
    }

    public GameObject PartyQuickChatObjs
    {
        get { return _partyQuickChatObjs; }
    }

    public Button PartyQuickChatButton
    {
        get { return _partyQuickChatButton; }
    }

    public UIPartyQuickChatMsgList QuickChatMsgList
    {
        get { return _quickChatMsgList; }
    }

    public Button BottomMoveButton
    {
        get { return _bottomMoveButton; }
    }

    #endregion

    #region MonoBehaviour Methods

    void Awake()
	{
		MakeMultiCacheMessageList ();
		MakeNoticeCacheMessageList ();

        SetInputFieldType(ChatInputFieldType.ChannelInputField);

        _channelNumText.text = "";

        _partyQuickChatButton.onClick.RemoveAllListeners();
        _partyQuickChatButton.onClick.AddListener(() => OnQuickChatListButton());

#if USE_CHEAT
        if (_cheatTestButton != null) {
            _cheatTestButton.gameObject.SetActive(false);
            _cheatTestButton.onClick.RemoveAllListeners();
            _cheatTestButton.onClick.AddListener(() => OnCheatTestButton());
        }
#else
        if (_cheatTestButton != null) {
            _cheatTestButton.gameObject.SetActive(false);
        }
#endif

        _bottomMoveButton.gameObject.SetActive(false);
        _bottomMoveButton.onClick.RemoveAllListeners();
        _bottomMoveButton.onClick.AddListener(OnButtonBottomMoveChat);

        _objScrollManager.OnBottomScrollState = OnBottomScrollState;
    }

    void Update()
	{
#if !UNITY_EDITOR
        switch (_inputFieldType) {
            case ChatInputFieldType.NormalInputField:
                if (_inputField.TouchScreenKeyboard != null) {
                    if (_inputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Done) {
                        if (_onInputKeyboardAction != null) {
                            _onInputKeyboardAction(_inputField.TouchScreenKeyboard.text);
                        }
                    } else if (_inputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled) {
                        //_inputField.text = "";
                        _inputField.ResetInputFieldExpand();
                    }
                }
                break;
            case ChatInputFieldType.ChannelInputField:
                if (_channelInputField.TouchScreenKeyboard != null) {
                    if (_channelInputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Done) {
                        if (_onInputKeyboardAction != null) {
                            _onInputKeyboardAction(_channelInputField.TouchScreenKeyboard.text);
                        }
                    } else if (_channelInputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled) {
                        //_channelInputField.text = "";
                        _channelInputField.ResetInputFieldExpand();
                    }
                }
                break;
            case ChatInputFieldType.WhisperInputField:
                if (_whisperInputField.TouchScreenKeyboard != null) {
                    if (_whisperInputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Done) {
                        if (_onWhisperInputKeyboardAction != null) {
                            _onWhisperInputKeyboardAction(_whisperInputField.TouchScreenKeyboard.text);
                        }
                    } else if (_whisperInputField.TouchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled) {
                        //_whisperInputField.text = "";
                        _whisperInputField.ResetInputFieldExpand();
                    }
                }
                break;
        }
#endif
    }

    #endregion

    #region Methods

    public void InitUIMultiChattingPopup()
	{
		SetTextObject ();
	}

    public void SetInputFieldType(ChatInputFieldType inputFieldType)
    {
        _inputFieldType = inputFieldType;
        switch(_inputFieldType) {
            case ChatInputFieldType.None:
                _inputField.gameObject.SetActive(false);
                _channelInputObject.SetActive(false);
                _whisperObject.SetActive(false);
                _chatGuildMessageManager.gameObject.SetActive(false);
                _normalMessageScrollObj.SetActive(true);
                _partyQuickChatObjs.SetActive(false);
                _objScrollManager.OnValidTouchArea = null;
                _bottomMoveButton.transform.SetParent(_normalBottomPosTrans);
                _bottomMoveButton.transform.localPosition = Vector3.zero;
                break;
            case ChatInputFieldType.NormalInputField:
                _inputField.gameObject.SetActive(true);
                _channelInputObject.SetActive(false);
                _whisperObject.SetActive(false);
                _partyQuickChatObjs.SetActive(false);
                _objScrollManager.OnValidTouchArea = null;
                _bottomMoveButton.transform.SetParent(_normalBottomPosTrans);
                _bottomMoveButton.transform.localPosition = Vector3.zero;
                break;
            case ChatInputFieldType.ChannelInputField:
                _inputField.gameObject.SetActive(false);
                _channelInputObject.SetActive(true);
                _whisperObject.SetActive(false);
                _partyQuickChatObjs.SetActive(false);
                _objScrollManager.OnValidTouchArea = null;
                _bottomMoveButton.transform.SetParent(_normalBottomPosTrans);
                _bottomMoveButton.transform.localPosition = Vector3.zero;
                break;
            case ChatInputFieldType.WhisperInputField:
                _inputField.gameObject.SetActive(false);
                _channelInputObject.SetActive(false);
                _whisperObject.SetActive(true);
                _partyQuickChatObjs.SetActive(false);
                _objScrollManager.OnValidTouchArea = null;
                _bottomMoveButton.transform.SetParent(_normalBottomPosTrans);
                _bottomMoveButton.transform.localPosition = Vector3.zero;
                break;
            case ChatInputFieldType.QuickParty:
                _inputField.gameObject.SetActive(false);
                _channelInputObject.SetActive(false);
                _whisperObject.SetActive(false);
                _chatGuildMessageManager.gameObject.SetActive(false);
                _normalMessageScrollObj.SetActive(true);
                _partyQuickChatObjs.SetActive(true);
                _quickChatMsgList.gameObject.SetActive(false);
                _objScrollManager.OnValidTouchArea = OnPartyValidTouchArea;
                _bottomMoveButton.transform.SetParent(_partyChatBottomPosTrans);
                _bottomMoveButton.transform.localPosition = Vector3.zero;
                break;
        }
    }

	void SetTextObject()
	{
		SetWhisperTargetText ();
        TextModel textModel = ChattingController.Instance.Context.Text;
        _inputFieldPlaceholder.text = textModel.GetText(TextKey.UI_Text_9);
        _whisperInputFieldPlaceholder.text = textModel.GetText(TextKey.UI_Text_9);
        _channelInputFieldPlaceholder.text = textModel.GetText(TextKey.UI_Text_9);
    }

	public void SetWhisperTargetText(string targetName = "")
	{
        if(ChattingController.Instance == null || ChattingController.Instance.Context == null)
            return;

		TextModel textModel = ChattingController.Instance.Context.Text;

		if (string.IsNullOrEmpty (targetName)) {
			_whisperTargetText.text = textModel.GetText (TextKey.CT_Whisper_Target);
            _whisperNotCheckRootObj.SetActive(true);
        } else {
			_whisperTargetText.text = targetName;
            _whisperNotCheckRootObj.SetActive(false);
        }
	}

	public void SetWhisperTargetButton(Action onWhisperTargetButton)
	{
        _whisperTargetButton.onClick.RemoveAllListeners();
        _whisperTargetButton.onClick.AddListener(() => onWhisperTargetButton());
	}

	void MakeMultiCacheMessageList()
	{
		if (ChattingController.Instance == null)
			return;

		int maxCount = ChattingController.Instance.MaxChatLineCount;
        _multiChatCacheMessages = new UIMultiChatMessage[maxCount];

		for (int i = 0; i < maxCount; i++) {
            _multiChatCacheMessages[i] = GameObject.Instantiate<UIMultiChatMessage>(_uiMultiChatMessage);

			var rectTransform = _multiChatCacheMessages[i].GetComponent<RectTransform>();

			rectTransform.SetParent(_cacheChatParentTrans.transform, false);
            _multiChatCacheMessages[i].gameObject.SetActive(false);
		}
	}

	void MakeNoticeCacheMessageList()
	{
		if (ChattingController.Instance == null)
			return;

        _noticeCacheMessages = new UIMultiChatMessage[_noticeMaxCount];

		for (int i = 0; i < _noticeMaxCount; i++) {
            _noticeCacheMessages[i] = GameObject.Instantiate<UIMultiChatMessage>(_uiMultiChatMessage);

			var rectTransform = _noticeCacheMessages[i].GetComponent<RectTransform>();

			rectTransform.SetParent(_cacheChatParentTrans.transform, false);
            _noticeCacheMessages[i].gameObject.SetActive(false);
		}

        _subNoticeCacheMessages = new UIMultiChatMessage[_subNoticeMaxCount];

		for (int i = 0; i < _subNoticeMaxCount; i++) {
            _subNoticeCacheMessages[i] = GameObject.Instantiate<UIMultiChatMessage>(_uiMultiChatMessage);

			var rectTransform = _subNoticeCacheMessages[i].GetComponent<RectTransform>();

			rectTransform.SetParent(_cacheChatParentTrans.transform, false);
            _subNoticeCacheMessages[i].gameObject.SetActive(false);
		}
	}

	public void SetCloseAction(Action closeAction)
	{
		_buttonClose.onClick.RemoveAllListeners();
		_buttonClose.onClick.AddListener(() => closeAction());
	}

	public void SetInputFieldEndEditAction(Action<string> endEditAction)
	{
		_inputField.onEndEdit.RemoveAllListeners();
		_inputField.onEndEdit.AddListener((inputText) => endEditAction(inputText));
	}

    public void SetChannelInputFieldEndEditAction(Action<string> endEditAction)
    {
        _channelInputField.onEndEdit.RemoveAllListeners();
        _channelInputField.onEndEdit.AddListener((inputText) => endEditAction(inputText));
    }

    public void SetWhisperInputFieldEndEditAction(Action<string> endEditAction)
	{
		_whisperInputField.onEndEdit.RemoveAllListeners ();
		_whisperInputField.onEndEdit.AddListener((inputText) => endEditAction(inputText));
	}

	public void ResetInputMessage()
	{
        _inputField.ResetInputFieldExpand();
    }

    public void ResetChannelInputMessage()
    {
        _channelInputField.ResetInputFieldExpand();
    }

    public void ResetWhisperInputMessage()
	{
        _whisperInputField.ResetInputFieldExpand();
    }

	public void ClearMultiChatMessage(bool isResize)
	{
		if (_multiChatCacheMessages != null) {
			for (int i = 0; i < _multiChatCacheMessages.Length; i++) {
				if (_multiChatCacheMessages[i].transform.parent == _objScrollManager.ObjListTrans) {
                    _multiChatCacheMessages[i].transform.SetParent (_cacheChatParentTrans.transform, false);
				}

                _multiChatCacheMessages[i].ResetChatMessage ();
                _multiChatCacheMessages[i].gameObject.SetActive (false);
			}
		}

		_multiCacheIndex = 0;

		_objScrollManager.ReleaseScrollData (isResize);
	}

	public void ClearNoticeMessage()
	{
		if (_noticeCacheMessages != null) {
			for (int i = 0; i < _noticeCacheMessages.Length; i++) {
				if (_noticeCacheMessages[i].transform.parent == _chatNoticeRootTrans) {
                    _noticeCacheMessages[i].transform.SetParent (_cacheChatParentTrans.transform, false);
				}

                _noticeCacheMessages[i].ResetChatMessage ();
                _noticeCacheMessages[i].gameObject.SetActive (false);
			}
		}

		_noticeCacheIndex = 0;
		_curNoticeHeight = 0f;
		_noticeMessageInfos.Clear ();

		ClearSubNoticeMessage ();
	}

	public void ClearSubNoticeMessage()
	{
		if (_subNoticeCacheMessages != null) {
			for (int i = 0; i < _subNoticeCacheMessages.Length; i++) {
				if (_subNoticeCacheMessages[i].transform.parent == _chatNoticeRootTrans) {
                    _subNoticeCacheMessages[i].transform.SetParent (_cacheChatParentTrans.transform, false);
				}

                _subNoticeCacheMessages[i].ResetChatMessage ();
                _subNoticeCacheMessages[i].gameObject.SetActive (false);
			}
		}

		_subNoticeCacheIndex = 0;
		_subNoticeMsgInfos.Clear ();
	}

	public UIMultiChatMessage AddMultiChatMessage()
	{
		UIMultiChatMessage message = _multiChatCacheMessages[_multiCacheIndex];
		if (message.transform.parent == _objScrollManager.ObjListTrans) {
			message.transform.SetParent (_cacheChatParentTrans.transform, false);
		}

		if (_multiCacheIndex == ChattingController.Instance.MaxChatLineCount - 1) {
			_multiCacheIndex = 0;
		} else {
			_multiCacheIndex++;
		}

		message.gameObject.SetActive(true);

		return message;
	}

	public UIMultiChatMessage AddNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor)
	{
		UIMultiChatMessage message = _noticeCacheMessages[_noticeCacheIndex];
		message.transform.SetParent (_chatNoticeRootTrans, false);

        if (message.MultiChatTextInfos != null)
            message.ResetChatMessage();

        if (_noticeCacheIndex == _noticeMaxCount - 1) {
			_noticeCacheIndex = 0;
		} else {
			_noticeCacheIndex++;
		}

		message.gameObject.SetActive(true);
		message.SetChatMessageList (chatTextInfos, chatColor);

        // Calc StartPosY
        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        float textHeight = (float)message.TextHeight;

        int quotientHeight = maxHeightCount / 2;
        int restHeight = maxHeightCount % 2;

        float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (textHeight * quotientHeight) - (textHeight * 0.5f);
        } else { // Odd
            startY = (textHeight * quotientHeight);
        }
        /////

        float startPosY = -startY;
		if (_noticeMessageInfos.Count == 0) {
			_curNoticeHeight += message.ObjectHeight;
		} else {
			_curNoticeHeight += (message.ObjectHeight + _noticeGapValue);

			UIMultiChatMessage lastNoticeMsg = _noticeMessageInfos [_noticeMessageInfos.Count - 1];

            // Calc StartPosY
            int lastHeightCount = 0;
            lastHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

            float lastTextHeight = (float)message.TextHeight;

            int lastQuotientHeight = lastHeightCount / 2;
            int lastRestHeight = lastHeightCount % 2;

            float lastStartY = 0f;
            if (lastRestHeight == 0) { // Even
                lastStartY = (lastTextHeight * lastQuotientHeight) - (lastTextHeight * 0.5f);
            } else { // Odd
                lastStartY = (lastTextHeight * lastQuotientHeight);
            }
            /////

            float lastObjPosY = lastNoticeMsg.transform.localPosition.y + lastStartY;
            startPosY += lastObjPosY - (lastNoticeMsg.ObjectHeight * 0.5f) - _noticeGapValue - (message.ObjectHeight * 0.5f);
        }

		message.transform.localPosition = new Vector3 (0f, startPosY, 0f);

		_noticeMessageInfos.Add (message);

		return message;
	}

	public UIMultiChatMessage AddSubNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor)
	{
		UIMultiChatMessage message = _subNoticeCacheMessages[_subNoticeCacheIndex];
		message.transform.SetParent (_chatNoticeRootTrans, false);
        if(message.MultiChatTextInfos != null)
            message.ResetChatMessage();

        if (_subNoticeCacheIndex == _subNoticeMaxCount - 1) {
			_subNoticeCacheIndex = 0;
		} else {
			_subNoticeCacheIndex++;
		}

		message.gameObject.SetActive(true);
		message.SetChatMessageList (chatTextInfos, chatColor);

        // Calc StartPosY
        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        float textHeight = (float)message.TextHeight;

        int quotientHeight = maxHeightCount / 2;
        int restHeight = maxHeightCount % 2;

        float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (textHeight * quotientHeight) - (textHeight * 0.5f);
        } else { // Odd
            startY = (textHeight * quotientHeight);
        }
        /////

        float startPosY = -startY;
		if (_subNoticeMsgInfos.Count == 0 && _noticeMessageInfos.Count == 0) {
			_curNoticeHeight += message.ObjectHeight;
		} else {
			_curNoticeHeight += (message.ObjectHeight + _noticeGapValue);

			UIMultiChatMessage lastNoticeMsg = null;

			if (_subNoticeMsgInfos.Count == 0) {
				lastNoticeMsg = _noticeMessageInfos [_noticeMessageInfos.Count - 1];
			} else {
				lastNoticeMsg = _subNoticeMsgInfos [_subNoticeMsgInfos.Count - 1];
			}

            // Calc StartPosY
            int lastHeightCount = 0;
            lastHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

            float lastTextHeight = (float)message.TextHeight;

            int lastQuotientHeight = lastHeightCount / 2;
            int lastRestHeight = lastHeightCount % 2;

            float lastStartY = 0f;
            if (lastRestHeight == 0) { // Even
                lastStartY = (lastTextHeight * lastQuotientHeight) - (lastTextHeight * 0.5f);
            } else { // Odd
                lastStartY = (lastTextHeight * lastQuotientHeight);
            }
            /////

            float lastObjPosY = lastNoticeMsg.transform.localPosition.y + lastStartY;
            startPosY += lastObjPosY - (lastNoticeMsg.ObjectHeight * 0.5f) - _noticeGapValue - (message.ObjectHeight * 0.5f);

        }

		message.transform.localPosition = new Vector3 (0f, startPosY, 0f);

		_subNoticeMsgInfos.Add (message);

		return message;
	}

	public void RemoveMultiChatMessage(int posIndex)
	{
		UIMultiChatMessage multiChatMessage = _objScrollManager.GetScrollObj (posIndex).ScrollObjInfo as UIMultiChatMessage;

		if (multiChatMessage.transform.parent == _objScrollManager.ObjListTrans) {
			multiChatMessage.transform.SetParent (_cacheChatParentTrans.transform, false);
		}

		multiChatMessage.ResetChatMessage ();
		multiChatMessage.gameObject.SetActive (false);

		_objScrollManager.RemoveScrollObj (posIndex);
	}

	public UIChattingButton AddChatButtonInfo(ChatDefinition.ChatMessageKind chatKind, Action<UIChattingButton, bool> onClickChatButton)
	{
		UIChattingButton retValue = null;

        retValue = Instantiate(_chatButtonList.UIChattingButtonInfo);

        retValue.gameObject.transform.SetParent (_chatButtonList.ChatContentsTrans);
		retValue.gameObject.transform.localScale = Vector3.one;
		retValue.gameObject.SetActive (true);
        retValue.SelectObj.SetActive(false);

        retValue.ChatMessageKind = chatKind;
		retValue.ChatButton.onClick.AddListener (() => onClickChatButton (retValue, false));

		return retValue;
	}

    public void SetDisableUIWhisperChat()
	{
        SetInputFieldType(ChatInputFieldType.ChannelInputField);
    }

	public void SetEnableEditorChannelInputObj(bool isEnable)
	{
		_editorChannelInputField.gameObject.SetActive (isEnable);
	}

	public void SetEditorChannelInputFieldEndEditAction(Action<string> endEditAction)
	{
		_editorChannelInputField.onEndEdit.RemoveAllListeners();
		_editorChannelInputField.onEndEdit.AddListener((inputText) => endEditAction(inputText));
	}

	public void ResetEditorChannelInputMessage()
	{
		_editorChannelInputField.text = "";
		_editorChannelInputField.gameObject.SetActive (false);
	}

    public void AddPartyQuickChatListInfo(TextModel textModel, int[] missionQuickChatList, Action<UIPartyQuickChatBoard> OnClickQuickChatMsg)
    {
        _quickChatMsgList.ReleaseChatList();

        for (int i = 0; i < missionQuickChatList.Length; i++) {
            string addQuickChat = textModel.GetText(string.Format("Party_QuickChat_{0}", missionQuickChatList[i]));
            UIPartyQuickChatBoard quickChatBoard = _quickChatMsgList.AddQuickChatMsg(addQuickChat, missionQuickChatList[i]);
            quickChatBoard.ChatButton.onClick.RemoveAllListeners();
            quickChatBoard.ChatButton.onClick.AddListener(() => OnClickQuickChatMsg(quickChatBoard));
        }
    }

    #endregion

    #region CallBack Methods

    void OnQuickChatListButton()
    {
        if (_quickChatMsgList.gameObject.activeSelf) {
            _quickChatMsgList.gameObject.SetActive(false);
        } else {
            _quickChatMsgList.gameObject.SetActive(true);
        }
    }

    bool OnPartyValidTouchArea(float posX, float posY)
    {
        Ray ray = ChattingController.Instance.UIMultiChat.ChatCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.transform.gameObject == ChattingController.Instance.UIMultiChat.UIMultiChatPopup.QuickChatMsgList.gameObject) {
                return false;
            }
        }

        return true;
    }

    void OnButtonBottomMoveChat()
    {
        _objScrollManager.MoveBottomScrollObj();
    }

    void OnBottomScrollState(bool isBottomState)
    {
        _bottomMoveButton.gameObject.SetActive(!isBottomState);
    }

    #endregion

    #region Whisper Input IChatNudgeNotable

    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        return _whisperNotConfirmObj;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        return _whisperNotConfirmText;
    }

    #endregion

#if USE_CHEAT

    void OnCheatTestButton()
    {
        ChatGMSMessage chatMessage = ChatHelper.GetChatGMSMessage((int)ChatNoticeMessageKey.NoticeMessage, ChatDefinition.ChatMessageType.GMSystemChatMessage, "Test GMS Message", Color.white);
        ChattingController.Instance.NotifyChatReceiveMessage((int)ChattingPacketType.ServerChatNotify, chatMessage);
    }

#endif
}
