using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using System;
using LitJson;
using NewBattleCore;

public enum UIMultiChattingState
{
    None = 0,
	SelectChatButton,
	InputChatting,
	NotifyConfirm,
}

public class UIMultiChatting : MonoBehaviour, IChatReceiveMessageObserver, IChatChangeCompanyNotice, IBackKeyMethod
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Camera _chatCamera = default(Camera);
    [SerializeField] UIChatPreviewMessage _chatPreviewMessage = default(UIChatPreviewMessage);
    [SerializeField] UIChatPreviewMessage _chatBattlePreviewMessage = default(UIChatPreviewMessage);
    [SerializeField] UIMultiChattingPopup _uiMultiChatPopup = default(UIMultiChattingPopup);
	[SerializeField] UIChatWhisperTargetPopup _uiWhisperTargetPopup = default(UIChatWhisperTargetPopup);
    [SerializeField] CanvasScaler _uiChatCanvasScaler = default(CanvasScaler);
    [SerializeField] UIPartyChattingView _uiPartyChattingPopup = default(UIPartyChattingView);

#pragma warning restore 649

    #endregion

    #region Variables

	MultiChatMessageTab _curChatPopupTab = MultiChatMessageTab.None;

	Dictionary<int /* ChatMessageKind */, ChatButtonObjInfo> _chatButtonObjInfos = new Dictionary<int, ChatButtonObjInfo> ();
    Dictionary<string /* PartyType + Party Num */, ChatButtonObjInfo> _chatPartyButtonObjInfos = new Dictionary<string /* PartyType + Party Num */, ChatButtonObjInfo> ();

	bool _isInitButtonState = false;

	UIMultiChattingState _curMultiChatState = UIMultiChattingState.None;

	int _chatListCount = 0;

	UIChattingButton _curSelectChattingInput = null;
	TouchScreenKeyboard _screenKeyboard = null;
    bool _isRequestWhisperServerLog = false;

    UIChattingButton _saveSelectChattingInput = null;

    List<ChatMakingMessage> _timeChatMessageList = new List<ChatMakingMessage>();

    Queue<ChatPreviewMessageInfo> _previewQueueMessageInfo = new Queue<ChatPreviewMessageInfo>();

    #endregion

    #region Properties

    public Camera ChatCamera
    {
        get { return _chatCamera; }
    }

    public UIMultiChattingPopup UIMultiChatPopup
	{
		get{ return _uiMultiChatPopup; }
	}

    public UIPartyChattingView UIPartyChattingPopup
    {
        get { return _uiPartyChattingPopup; }
    }

    public Dictionary<int /* ChatMessageKind */, ChatButtonObjInfo> ChatButtonObjInfos
	{
		get{ return _chatButtonObjInfos; }
	}

	public Dictionary<string /* PartyType + Party Num */, ChatButtonObjInfo> ChatPartyButtonObjInfos
	{
		get{ return _chatPartyButtonObjInfos; }
	}

	public UIMultiChattingState CurMultiChatState
	{
		get{ return _curMultiChatState; }
		set{ _curMultiChatState = value; }
	}

	public UIChattingButton CurSelectChattingInput
	{
		get{ return _curSelectChattingInput; }
	}

    public UIChatWhisperTargetPopup UIWhisperTargetPopup
    {
        get { return _uiWhisperTargetPopup; }
    }

    public bool IsRequestWhisperServerLog
    {
        get { return _isRequestWhisperServerLog; }
        set { _isRequestWhisperServerLog = value; }
    }

    #endregion

    #region MonoBehavior Methods

    void Awake()
	{
        if(UISafeAreaPanel.GetSafeArea().x > 0f) {
            _uiChatCanvasScaler.matchWidthOrHeight = 0.5f;
        }

		_uiMultiChatPopup.SetCloseAction (CloseChattingPopup);
		_uiMultiChatPopup.SetWhisperTargetButton (OnWhisperTargetButton);

		#if UNITY_EDITOR
		_uiMultiChatPopup.SetEditorChannelInputFieldEndEditAction(OnChannelInputText);
		#endif

		_uiWhisperTargetPopup.OnSelectUserInfoCell = OnSelectWhisperUserInfo;

        _uiMultiChatPopup.ChannelChangeButton.onClick.RemoveAllListeners();
        _uiMultiChatPopup.ChannelChangeButton.onClick.AddListener(() => OnClickChangeChannel());
    }

	void Start()
	{
		_uiMultiChatPopup.ObjScrollManager.InitScrollData ();
        _uiPartyChattingPopup.ObjScrollManager.InitScrollData();

#if _OLD_GUILD_CHAT
        SetChatGuildData();
#endif
    }

	void OnEnable()
	{
		SetCurUIChatPopup ();
    }

    private void OnDisable()
    {
    }

    #endregion

    #region Methods

#if _OLD_GUILD_CHAT

    void SetChatGuildData()
    {
        _uiMultiChatPopup.ChatGuildMessageManager.InitChatGuildUI();
        _uiMultiChatPopup.ChatGuildMessageManager.GuildChatScrollButton.onClick.AddListener(() => OnGuildChatOnly());
        _uiMultiChatPopup.ChatGuildMessageManager.SystemScrollButton.onClick.AddListener(() => OnGuildSystemChatOnly());
        _uiMultiChatPopup.ChatGuildMessageManager.LargeScrollButton.onClick.AddListener(() => OnGuildNormalChat());
    }

#endif

    void SetCurUIChatPopup()
	{

	}

	public void SetChatPopupTab(MultiChatMessageTab chatTab)
	{
		if (_curChatPopupTab == chatTab)
			return;

		_curChatPopupTab = chatTab;

        SetChatPopupData();
	}

    void SetChatPopupData()
    {
        if (!_isInitButtonState && ChattingController.Instance != null
		    && ChattingController.Instance.ChatSocketManager != null && ChattingController.Instance.ChatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected) {

			SetButtonList ();
		}
    }

	public void SetButtonList()
	{
		// Channel Chatting
		SetChannelChattingButton ();

        // Guild Chatting
        SetGuildChattingButton();

        // Party Chatting
        SetPartyChattingButton();

        // Whisper Chatting
        SetWhisperChattingButton();

		_isInitButtonState = true;

        OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.ChannelChat].UIChatButtonInfo);

        ChattingController.Instance.ChattingModel.WhisperInformation.FriendRootNudge.AddNudgeNotable(_uiWhisperTargetPopup.FriendTabButton as IChatNudgeNotable);
        ChattingController.Instance.ChattingModel.WhisperInformation.GuildRootNudge.AddNudgeNotable(_uiWhisperTargetPopup.GuildTabButton as IChatNudgeNotable);
    }

	void SetChannelChattingButton()
	{
		MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

		TextModel textModel = ChattingController.Instance.Context.Text;

		ChatButtonObjInfo buttonObjInfo = null;

		if (_chatButtonObjInfos.ContainsKey ((int)ChatDefinition.ChatMessageKind.ChannelChat)) {
			return;
		}

		UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo (ChatDefinition.ChatMessageKind.ChannelChat, OnClickChattingButton);
		uiChatButtonInfo.SetTitleText(textModel.GetText(TextKey.ChannelChatting));
        _uiMultiChatPopup.ChannelNumText.text = string.Format(textModel.GetText(TextKey.CT_Channel), chatModel.ChatGroupNum);

        chatModel.ChannelChatInfo.NudgeNode.AddNudgeNotable(uiChatButtonInfo as IChatNudgeNotable);

        buttonObjInfo = new ChatButtonObjInfo ();
		buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

		AddNormalButtonObjInfo ((int)ChatDefinition.ChatMessageKind.ChannelChat, buttonObjInfo);
	}

    void SetWhisperChattingButton()
	{
		MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

		TextModel textModel = ChattingController.Instance.Context.Text;

		ChatButtonObjInfo buttonObjInfo = null;

		if (_chatButtonObjInfos.ContainsKey ((int)ChatDefinition.ChatMessageKind.WhisperChat)) {
			return;
		}

		UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo (ChatDefinition.ChatMessageKind.WhisperChat, OnClickChattingButton);
		uiChatButtonInfo.SetTitleText(textModel.GetText (TextKey.CT_Whisper));

        chatModel.WhisperInformation.WhisperRootNudge.AddNudgeNotable(uiChatButtonInfo as IChatNudgeNotable);
        chatModel.WhisperInformation.WhisperRootNudge.AddNudgeNotable(_uiMultiChatPopup as IChatNudgeNotable);

        buttonObjInfo = new ChatButtonObjInfo ();
		buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

		AddNormalButtonObjInfo ((int)ChatDefinition.ChatMessageKind.WhisperChat, buttonObjInfo);
	}

	void AddNormalButtonObjInfo(int chatMessageKind, ChatButtonObjInfo buttonObjInfo)
	{
        _chatButtonObjInfos.Add (chatMessageKind, buttonObjInfo);
	}

	public void ChangeChannelChatting(int chatGroupNum)
	{
		TextModel textModel = ChattingController.Instance.Context.Text;
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
        _uiMultiChatPopup.ChannelNumText.text = string.Format(textModel.GetText(TextKey.CT_Channel), chatModel.ChatGroupNum);
    }

	void SetGuildChattingButton()
	{
		MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

		TextModel textModel = ChattingController.Instance.Context.Text;

		List<ChatGuildJoinedInfo> chatGuildInfos = ChattingController.Instance.GetChatGuildInfoList();

		for (int i = 0; i < chatGuildInfos.Count; i++) {
            ChatGuildJoinedInfo guildInfo = chatGuildInfos[i];
			UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo (ChatDefinition.ChatMessageKind.GuildChat, OnClickChattingButton);
			uiChatButtonInfo.PartyType = guildInfo.party_type;
			uiChatButtonInfo.PartyNum = guildInfo.party_num;
            uiChatButtonInfo.SetTitleText(string.Format ("{0} {1}", guildInfo.guildName, textModel.GetText(TextKey.UI_Text_57)));

            GuildChatInformation guildChatInfo = chatModel.GetGuildChatInformation(guildInfo.party_num);
            guildChatInfo.NudgeNode.AddNudgeNotable(uiChatButtonInfo as IChatNudgeNotable);

            ChatButtonObjInfo buttonObjInfo = new ChatButtonObjInfo ();
			buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

			AddChatPartyButtonObjInfo(guildInfo.party_type, guildInfo.party_num, buttonObjInfo);
		}
	}

    void SetPartyChattingButton()
    {
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

        TextModel textModel = ChattingController.Instance.Context.Text;

        List<ChatPartyJoinedInfo> chatPartyInfos = ChattingController.Instance.GetChatPartyInfoList();

        for (int i = 0; i < chatPartyInfos.Count; i++) {
            ChatPartyJoinedInfo guildInfo = chatPartyInfos[i];
            UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo(ChatDefinition.ChatMessageKind.PartyChat, OnClickChattingButton);
            uiChatButtonInfo.PartyType = guildInfo.party_type;
            uiChatButtonInfo.PartyNum = guildInfo.party_num;
            uiChatButtonInfo.SetTitleText(textModel.GetText(TextKey.Party_Main));

            GuildChatInformation guildChatInfo = chatModel.GetGuildChatInformation(guildInfo.party_num);
            guildChatInfo.NudgeNode.AddNudgeNotable(uiChatButtonInfo as IChatNudgeNotable);

            ChatButtonObjInfo buttonObjInfo = new ChatButtonObjInfo();
            buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

            AddChatPartyButtonObjInfo(guildInfo.party_type, guildInfo.party_num, buttonObjInfo);
        }
    }

    void AddChatPartyButtonObjInfo(int partyType, long partyNum, ChatButtonObjInfo buttonObjInfo)
	{
        string chatButtonKey = string.Format("{0}_{1}", partyType, partyNum);
        if (_chatPartyButtonObjInfos.ContainsKey (chatButtonKey))
			return;

		_chatPartyButtonObjInfos.Add (chatButtonKey, buttonObjInfo);
	}

    public void ConfirmChannelMessage()
    {
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
        chatModel.ChannelChatInfo.NudgeNode.ConfirmNudge();
    }

	public void ConfirmChatRoomMessage(int partyType, long partyNum)
	{
        if (partyType == (int)ChatPartyType.ChatGuild) {
            GuildChatInformation guildChatInfo = ChattingController.Instance.ChattingModel.GetGuildChatInformation(partyNum);
            if (guildChatInfo != null && guildChatInfo.NudgeNode.GetNodeNudgeCount() > 0) {
                guildChatInfo.NudgeNode.ConfirmNudge();

                long timeStamp =  TimeUtil.GetTimeStampAddSecond(1);
                new ProtocolGuildChatViewUpdate(ChattingController.Instance.Context, partyNum, timeStamp, null).Execute();

                ChattingController.Instance.ChattingModel.ChangeGuildChatConfirmTime(partyNum, timeStamp);
            }
        }
    }

    public void ConfirmMyPartyChatMessage()
    {
        if(ChattingController.Instance.ChattingModel.MyPartyMessageInfo.NudgeNode.GetNodeNudgeCount() > 0) {
            ChattingController.Instance.ChattingModel.MyPartyMessageInfo.NudgeNode.ConfirmNudge();

            long confirmTimeStamp = TimeUtil.GetTimeStampAddSecond(1);
            new ProtocolPartyMyChatViewUpdate(ChattingController.Instance.Context, confirmTimeStamp, null).Execute();
            ChattingController.Instance.ChattingModel.MyPartyMessageInfo.LastChatViewTime = confirmTimeStamp;
        }
    }

    public void ConfirmWhisperChatRoomMessage()
    {
        if(ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo == null)
            return;
        ChattingController.Instance.ChattingModel.WhisperInformation.SetWhisperLastConfirmTimeStamp(TimeUtil.GetTimeStamp());
    }

    public void AddGuildChattingButton(ChatGuildJoinedInfo partyInfo)
	{
		if (!_isInitButtonState)
			return;

        string chatButtonKey = string.Format("{0}_{1}", (int)ChatPartyType.ChatGuild, partyInfo.party_num);
        if (_chatPartyButtonObjInfos.ContainsKey (chatButtonKey))
			return;

        ChatButtonObjInfo whisperButton = _chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.WhisperChat];
        Transform buttonParent = whisperButton.UIChatButtonInfo.transform.parent;
        whisperButton.UIChatButtonInfo.transform.SetParent(buttonParent.parent);

        TextModel textModel = ChattingController.Instance.Context.Text;

		UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo (ChatDefinition.ChatMessageKind.GuildChat, OnClickChattingButton);
		uiChatButtonInfo.PartyType = partyInfo.party_type;
		uiChatButtonInfo.PartyNum = partyInfo.party_num;

        uiChatButtonInfo.SetTitleText(string.Format ("{0} {1}", partyInfo.guildName, textModel.GetText(TextKey.UI_Text_57)));

		ChatButtonObjInfo buttonObjInfo = new ChatButtonObjInfo ();
		buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

		AddChatPartyButtonObjInfo(partyInfo.party_type, partyInfo.party_num, buttonObjInfo);

        whisperButton.UIChatButtonInfo.transform.SetParent(buttonParent);
    }

    public void AddPartyChattingButton(ChatPartyJoinedInfo partyInfo)
    {
        if (!_isInitButtonState)
            return;

        string chatButtonKey = string.Format("{0}_{1}", partyInfo.party_type, partyInfo.party_num);
        if (_chatPartyButtonObjInfos.ContainsKey(chatButtonKey))
            return;

        ChatButtonObjInfo whisperButton = _chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.WhisperChat];
        Transform buttonParent = whisperButton.UIChatButtonInfo.transform.parent;
        whisperButton.UIChatButtonInfo.transform.SetParent(buttonParent.parent);

        TextModel textModel = ChattingController.Instance.Context.Text;

        UIChattingButton uiChatButtonInfo = _uiMultiChatPopup.AddChatButtonInfo(ChatDefinition.ChatMessageKind.PartyChat, OnClickChattingButton);
        uiChatButtonInfo.PartyType = partyInfo.party_type;
        uiChatButtonInfo.PartyNum = partyInfo.party_num;

        uiChatButtonInfo.SetTitleText(textModel.GetText(TextKey.Party_Main));

        ChatButtonObjInfo buttonObjInfo = new ChatButtonObjInfo();
        buttonObjInfo.UIChatButtonInfo = uiChatButtonInfo;

        AddChatPartyButtonObjInfo(partyInfo.party_type, partyInfo.party_num, buttonObjInfo);

        whisperButton.UIChatButtonInfo.transform.SetParent(buttonParent);
    }

    public void RemovePartyChattingButton(int partyType, long partyNum)
	{
        string chatButtonKey = string.Format("{0}_{1}", partyType, partyNum);
		if (!_chatPartyButtonObjInfos.ContainsKey (chatButtonKey))
			return;

		ChatButtonObjInfo buttonObjInfo = _chatPartyButtonObjInfos [chatButtonKey];

		Destroy (buttonObjInfo.UIChatButtonInfo.gameObject);
		_chatPartyButtonObjInfos.Remove (chatButtonKey);
	}

	public void ShowChattingPopup(ChatDefinition.ChatMessageKind chatKind = ChatDefinition.ChatMessageKind.None)
	{
        if(ChattingController.Instance.IsOpenCloseState)
            ChattingController.Instance.IsOpenCloseState = false;

        InitCommonChatPopup();

        if (chatKind == ChatDefinition.ChatMessageKind.None) {
            if (_saveSelectChattingInput != null) {
                OnClickChattingButton(_saveSelectChattingInput);
                _saveSelectChattingInput = null;
            } else {
                if (_chatButtonObjInfos.ContainsKey((int)ChatDefinition.ChatMessageKind.ChannelChat)) {
                    OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.ChannelChat].UIChatButtonInfo);
                }
            }
        } else {
            OnClickChattingButton(_chatButtonObjInfos[(int)chatKind].UIChatButtonInfo);
        }

        ChattingController.Instance.SetEnableScrollTouch(true);
    }

    void InitCommonChatPopup()
    {
        BackKeyService.Register(this);

        _uiMultiChatPopup.gameObject.SetActive(true);

        if (ChattingController.Instance.IsBattleState)
            BattleHandler.Instance.DisableTouchByPopupUI();

        if (_curChatPopupTab == MultiChatMessageTab.None) {
            SetChatPopupTab(MultiChatMessageTab.Chatting);
        }

        if (ChattingController.Instance.IsBattleState) {
            _chatBattlePreviewMessage.gameObject.SetActive(false);
        } else {
            _chatPreviewMessage.gameObject.SetActive(false);
        }

        _previewQueueMessageInfo.Clear();

        if (GameSystem.Instance != null)
            GameSystem.Instance.SetMainDrag(false);
    }

	public void ShowChattingWhisperPopup(int whisperKind, long targetID, long connectId, string targetNickname, long companyID = -1)
	{
        InitCommonChatPopup();

#if _CHATTING_LOG
        Debug.Log (string.Format ("targetNickname targetID : {0}, targetNickname : {1}", targetID, targetNickname));
#endif

        if (_chatButtonObjInfos.ContainsKey ((int)ChatDefinition.ChatMessageKind.WhisperChat)) {
			ChatWhisperTargetUserInfo whisperTarget = new ChatWhisperTargetUserInfo ();
			whisperTarget.targetUserID = targetID;
            whisperTarget.targetConnectID = connectId;
            whisperTarget.targetNickname = targetNickname;
            whisperTarget.whisperKind = whisperKind;
            whisperTarget.companyID = companyID;

            SetWhisperUserInfo(whisperTarget);

            MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
            if(!chatModel.WhisperInformation.ExistWhisperMessage(whisperTarget.targetConnectID)) {
                RequestHttpWhisperTargetMessage(whisperTarget);
            } else {
                OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.WhisperChat].UIChatButtonInfo);
            }
		}

        ChattingController.Instance.SetEnableScrollTouch(true);
    }

    public void ShowChattingPartyPopup(int partyType, long partyNum)
    {
        InitCommonChatPopup();

        string partyButtonKey = string.Format("{0}_{1}", partyType, partyNum);
        if (_chatPartyButtonObjInfos.ContainsKey(partyButtonKey)) {
            OnClickChattingButton(_chatPartyButtonObjInfos[partyButtonKey].UIChatButtonInfo);
        } else {
            if (_saveSelectChattingInput != null) {
                OnClickChattingButton(_saveSelectChattingInput);
                _saveSelectChattingInput = null;
            } else {
                if (_chatButtonObjInfos.ContainsKey((int)ChatDefinition.ChatMessageKind.ChannelChat)) {
                    OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.ChannelChat].UIChatButtonInfo);
                }
            }
        }
    }

    public void CloseChattingPopup()
	{
		_uiMultiChatPopup.gameObject.SetActive (false);
		_curChatPopupTab = MultiChatMessageTab.None;

		#if UNITY_IOS && !UNITY_EDITOR
		if(_screenKeyboard != null) _screenKeyboard.active = false;
		#endif
        
		_saveSelectChattingInput = _curSelectChattingInput;
        ReleaseCurSelectChattingInput();

        if (GameSystem.Instance != null)
            GameSystem.Instance.SetMainDrag(true);

        ChattingController.Instance.ButtonEventManager.ReleasePopup();

        if (ChattingController.Instance.IsBattleState)
            BattleHandler.Instance.EnableTouchByPopupUI();

        BackKeyService.UnRegister(this);
    }

    void RequestHttpWhisperTargetMessage(ChatWhisperTargetUserInfo whisperTarget)
    {
        ChatOtherUserListInfo[] otherUserListInfo = new ChatOtherUserListInfo[1];
        otherUserListInfo[0] = new ChatOtherUserListInfo();
        otherUserListInfo[0].other_user_id = whisperTarget.targetConnectID;
        otherUserListInfo[0].show_type = 1; // 1 : Show Count , 2 : Large begin_timestamp
        otherUserListInfo[0].begin_timestamp = 0;
        otherUserListInfo[0].show_count = 50;

        _isRequestWhisperServerLog = true;

        ChattingWhisperChatList whisperChatList = new ChattingWhisperChatList(ChattingController.Instance.Context, otherUserListInfo, OnSuccessChattingWhisperChatList, OnFailChattingWhisperChatList);
        whisperChatList.RequestHttpWeb();
    }

    void FinishRequestWhisperServerLog()
    {
        _isRequestWhisperServerLog = false;
    }


    void AddMultiChatMakingMessage(ChatMakingMessage makingMessage, Color inputChatColor)
	{
        bool isCurGuildChat = false;
        if(ChattingController.Instance.CurChatMessageKind == ChatDefinition.ChatMessageKind.GuildChat) {
            isCurGuildChat = true;
        }

        if(makingMessage.chatMessageInfo.sendType == ChatDefinition.ChatMessageKind.None) {
            for (int i = 0; i < makingMessage.chatMessageKinds.Length; i++) {
                if ((makingMessage.chatMessageKinds[i] == (int)ChattingController.Instance.CurChatMessageKind) ||
                    (isCurGuildChat && makingMessage.chatMessageKinds[i] == (int)ChatDefinition.ChatMessageKind.GuildSystemChat)) {
                    if (makingMessage.chatMessageKinds[i] == (int)ChatDefinition.ChatMessageKind.GuildChat ||
                        makingMessage.chatMessageKinds[i] == (int)ChatDefinition.ChatMessageKind.GuildSystemChat) {
                        if (makingMessage.chatMessageInfo.partyNum == ChattingController.Instance.CurChatGuildInfoUI.party_num) {
                            AddNormalChatMakeMessage(makingMessage, inputChatColor);
                        }
                    } else {
                        AddNormalChatMakeMessage(makingMessage, inputChatColor);
                    }
                }
            }
        } else {
            AddNormalChatMakeMessage(makingMessage, inputChatColor);
        }
    }

    void AddNormalChatMakeMessage(ChatMakingMessage makingMessage, Color inputChatColor)
    {
        DataContext context = ChattingController.Instance.Context;
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

        if (_chatListCount >= ChattingController.Instance.MaxChatLineCount) {
            _uiMultiChatPopup.RemoveMultiChatMessage(0);
        } else {
            _chatListCount++;
        }

#if _CHATTING_LOG
        Debug.Log("<color='red'>chat popup AddMultiChatMakingMessage</color>");
#endif
        Color chatColor = ColorPreset.CHAT_NORMAL;
        bool isUserChat = false;
        if (makingMessage.chatMessageInfo.messageType == (int)ChatDefinition.ChatMessageType.GMChannelChat) {
            chatColor = inputChatColor;
        } else {
            if (makingMessage.chatMessageInfo.userID == context.User.userData.userId) {
                isUserChat = true;
                if (ChatHelper.IsUserColorChatMessage(makingMessage.chatMessageInfo)) {
                    chatColor = ColorPreset.CHAT_MY_MESSAGE;
                } else {
                    chatColor = ChatHelper.GetChatMessageColor(chatModel.Context, makingMessage.chatMessageInfo);
                }
            } else {
                chatColor = ChatHelper.GetChatMessageColor(chatModel.Context, makingMessage.chatMessageInfo);
            }
        }

        UIMultiChatMessage multiChatMessage = _uiMultiChatPopup.AddMultiChatMessage();
        multiChatMessage.SetChatMessageList(makingMessage.multiChatTextInfoList, chatColor, makingMessage.chatMessageInfo.messageType);
        _uiMultiChatPopup.ObjScrollManager.AddScrollObject((IScrollObjectInfo)multiChatMessage, isUserChat);
    }

    public void RefreshMultiChattingPanel(List<ChatMakingMessage> chatMakingMessages, float topGapValue = 0f)
	{
		MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
		DataContext context = ChattingController.Instance.Context;

		_chatListCount = 0;
		_uiMultiChatPopup.ClearMultiChatMessage(true);

		_uiMultiChatPopup.ObjScrollManager.SetRectGapHeightValue (topGapValue);

		if (chatMakingMessages == null || chatMakingMessages.Count == 0)
			return;

		for(int i = 0; i < chatMakingMessages.Count; i++)
		{
			ChatMakingMessage chatMessage = chatMakingMessages[i];

            chatMessage.RefreshTimeStampText();

            if(chatMessage.TimeStampTextInfo != null) {
                _timeChatMessageList.Add(chatMessage);
            }

            Color chatColor = ChatHelper.GetChatMakingMessageColor(chatModel.Context, chatMessage);

            UIMultiChatMessage multiChatMessage = _uiMultiChatPopup.AddMultiChatMessage ();
			multiChatMessage.SetChatMessageList (chatMessage.multiChatTextInfoList, chatColor, chatMessage.chatMessageInfo.messageType);
			_uiMultiChatPopup.ObjScrollManager.AddScrollObject((IScrollObjectInfo)multiChatMessage, true);
		}

		_chatListCount = chatMakingMessages.Count;
	}

    void RefreshCurTimeChatList()
    {
        for(int i = 0; i < _timeChatMessageList.Count;i++) {
            _timeChatMessageList[i].RefreshTimeStampText();
            if(_timeChatMessageList[i].TimeStampTextInfo.UIPartMessage != null) {
                _timeChatMessageList[i].TimeStampTextInfo.UIPartMessage.MessageText.text = _timeChatMessageList[i].TimeStampTextInfo.ChatPartMessage;
            }
        }
    }

	public void RefreshNoticeMessages(List<MultiChatListInfo> chatNoticeMessages)
	{
        int noticeCount = 0;
		if (chatNoticeMessages.Count > _uiMultiChatPopup.NoticeMaxCount) {
			noticeCount = _uiMultiChatPopup.NoticeMaxCount;
		} else {
			noticeCount = chatNoticeMessages.Count;
		}

		for (int i = 0; i < noticeCount; i++) {
			_uiMultiChatPopup.AddNoticeChatMessage (chatNoticeMessages[i].ChatTextInfos, chatNoticeMessages[i].ChatColor);
		}
	}

	public void RefreshSubNoticeMessages(List<ChatMakingMessage> chatSubNoticeMessages, Color chatColor)
	{
		int noticeCount = 0;
		if (chatSubNoticeMessages.Count > _uiMultiChatPopup.SubNoticeMaxCount) {
			noticeCount = _uiMultiChatPopup.SubNoticeMaxCount;
		} else {
			noticeCount = chatSubNoticeMessages.Count;
		}

        for (int i = 0; i < noticeCount; i++) {
            if(chatSubNoticeMessages[i].TimeStampTextInfo != null) {
                _timeChatMessageList.Add(chatSubNoticeMessages[i]);
            }

            _uiMultiChatPopup.AddSubNoticeChatMessage (chatSubNoticeMessages[i].multiChatTextInfoList, chatColor);
		}
	}

	string GetSumLineMessage(int lineIndex, ChatMakingMessage makingMessage)
	{
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < makingMessage.multiChatTextInfoList.Count; i++) {
			if (makingMessage.multiChatTextInfoList [i].LineCount > lineIndex)
				break;
			
			sb.Append(makingMessage.multiChatTextInfoList [i].ChatPartMessage);
		}

		return sb.ToString();
	}

	void SetChannelInputKeyboard()
	{
#if UNITY_IOS
		_screenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false, false, false);
		_screenKeyboard.active = true;
#elif UNITY_ANDROID
        _screenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false, false, false);
#endif
		StartCoroutine (UpdateTouchScreenKeyboard());
	}

	void SetEditorChannelInputObject()
	{
#if UNITY_EDITOR
		if(_uiMultiChatPopup.EditorChannelInputField.gameObject.activeSelf){
			_uiMultiChatPopup.SetEnableEditorChannelInputObj(false);
		} else {
			_uiMultiChatPopup.SetEnableEditorChannelInputObj(true);
		}
#endif
	}

	void ChangeChannelNumber(string channelNum)
	{
#if UNITY_EDITOR
		_uiMultiChatPopup.ResetEditorChannelInputMessage();
#endif

		if (string.IsNullOrEmpty (channelNum))
			return;

		int resultChannelNum = 0;
		if (!int.TryParse (channelNum, out resultChannelNum)) {
            string notValidNumText = string.Format(GameSystem.Instance.Data.Text.GetText(TextKey.CT_Channel_Change), 1,9999);
            UIFloatingMessagePopup.Show(notValidNumText);
            return;
		}

		if (resultChannelNum == ChattingController.Instance.ChattingModel.ChatGroupNum)
			return;

		if (resultChannelNum < 1 || resultChannelNum > 9999) {
            string notValidNumText = string.Format(GameSystem.Instance.Data.Text.GetText(TextKey.CT_Channel_Change), 1, 9999);
            UIFloatingMessagePopup.Show(notValidNumText);
            return;
        }

		ChattingController.Instance.ChatSocketManager.SendRequestChangeGroupPacket (resultChannelNum);
        GameSystem.Instance.AudioController.PlayUI(Res.WAV.Menu_Ok01);
	}

    void ReleaseButtonList()
    {
        List<int> chatObjKeys = _chatButtonObjInfos.Keys.ToList();
        for (int i = 0; i < chatObjKeys.Count; i++) {
            if (_chatButtonObjInfos[chatObjKeys[i]].UIChatButtonInfo != null) {
                Destroy(_chatButtonObjInfos[chatObjKeys[i]].UIChatButtonInfo.gameObject);
            }
        }

        _chatButtonObjInfos.Clear();

        List<string> chatPartyObjKeys = _chatPartyButtonObjInfos.Keys.ToList();
        for (int i = 0; i < chatPartyObjKeys.Count; i++) {
            if (_chatPartyButtonObjInfos[chatPartyObjKeys[i]].UIChatButtonInfo != null) {
                Destroy(_chatPartyButtonObjInfos[chatPartyObjKeys[i]].UIChatButtonInfo.gameObject);
            }
        }

        _chatPartyButtonObjInfos.Clear();
    }

	public void ReleaseAllUIChatInfo()
	{
        ReleaseButtonList();

        _chatListCount = 0;
		_uiMultiChatPopup.ClearMultiChatMessage (true);

#if UNITY_EDITOR
		_uiMultiChatPopup.SetEnableEditorChannelInputObj(false);
#endif

		_isInitButtonState = false;

        ReleaseCurSelectChattingInput();

        if (ChattingController.Instance.CurChatMessageKind == ChatDefinition.ChatMessageKind.WhisperChat) {
			ChattingController.Instance.SetDisableWhisperInfo ();
		}

		ChattingController.Instance.CurChatMessageKind = ChatDefinition.ChatMessageKind.None;

		_curMultiChatState = UIMultiChattingState.None;

		_uiMultiChatPopup.SetWhisperTargetText ();
		_uiWhisperTargetPopup.ReleaseWhisperTargetPopup ();
	}

	public void CompletePreviewTextTimer()
	{
		ActionEventTimer eventTimer = ChattingController.Instance.ChatSocketManager.EventTimer;
		eventTimer.CompleteTimeEventByTimeID (ChattingController.Instance.ChatSocketManager.PreviewUITimeID);
	}

    public void SetChannelChat(int channelNum)
    {
        TextModel textModel = ChattingController.Instance.Context.Text;
        _uiMultiChatPopup.ChannelNumText.text = string.Format(textModel.GetText(TextKey.CT_Channel), channelNum);
    }

    public void ExecuteBackKey()
    {
        CloseChattingPopup();
    }

    public void SetGuildUIType(ChatDefinition.GuildUIType guildUIType)
    {
        if(GetGuildUIType() != guildUIType) {
            _uiMultiChatPopup.ChatGuildMessageManager.SetGuildUIType(guildUIType);
            ChattingController.Instance.ChattingModel.ChatSaveInfo.guildUIType = (int)guildUIType;
            ChattingController.Instance.ChattingModel.SaveChatSaveData();
        }
    }

    public ChatDefinition.GuildUIType GetGuildUIType()
    {
        return _uiMultiChatPopup.ChatGuildMessageManager.GuildUIType;
    }

    void ReleaseCurSelectChattingInput()
    {
        if(_curSelectChattingInput != null) {
            _curSelectChattingInput.SelectObj.SetActive(false);
            _curSelectChattingInput = null;
        }
    }

    public void RefreshChatMessage()
    {
        if(_curSelectChattingInput != null)
            OnClickChattingButton(_curSelectChattingInput, true);
    }

    void ShowPreviewMessage()
    {
        ActionEventTimer eventTimer = ChattingController.Instance.ChatSocketManager.EventTimer;

        if (_previewQueueMessageInfo.Count == 0)
            return;

        if (eventTimer.ExistTimerDataByID(ChattingSocketManager.previewNextCheckTimeID))
            return;

        float noticeTime = 5f;
        float nextCheckTime = 1.5f;
        int previewTimeID = ChattingController.Instance.ChatSocketManager.PreviewUITimeID;

        eventTimer.CompleteTimeEventByTimeID(previewTimeID);

        ChatPreviewMessageInfo previewInfo = _previewQueueMessageInfo.Dequeue();

        if (ChattingController.Instance.IsBattleState) {
            if (!IsMessageBlockOnBattle()) {
                _chatBattlePreviewMessage.SetPreviewMultiChatText(previewInfo.multiChatInfos, previewInfo.chatTextColor, previewInfo.messageType);
                eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnSetOffBattlePreviewText, noticeTime, null, previewTimeID);
            }
        } else {
            if (previewInfo.packetType != -1 && !IsMessageBlock()) {
                _chatPreviewMessage.SetPreviewMultiChatText(previewInfo.multiChatInfos, previewInfo.chatTextColor, previewInfo.messageType);
                eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnSetOffPreviewText, noticeTime, null, previewTimeID);
            }
        }

        eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnCheckNextPreview, nextCheckTime, null, ChattingSocketManager.previewNextCheckTimeID);
    }

    #endregion

    #region Party Chatting Methods

    public void ShowPartyChatting()
    {
        _uiPartyChattingPopup.ShowView();
    }

    public void ClosePartyChatting()
    {
        _uiPartyChattingPopup.CloseView();
    }

    #endregion

    #region Conroutine Methods

    IEnumerator UpdateTouchScreenKeyboard()
	{
		while (_screenKeyboard != null) {
            if (_screenKeyboard.status == TouchScreenKeyboard.Status.Done) {
				ChangeChannelNumber (_screenKeyboard.text);
				_screenKeyboard = null;
                break;
			}

            if (_screenKeyboard != null && _screenKeyboard.status == TouchScreenKeyboard.Status.Canceled) {
				_screenKeyboard = null;
				break;
			}

            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    if(_screenKeyboard != null) {
                        _screenKeyboard.active = false;
                        _screenKeyboard = null;
                    }                   
                    break;
                }
            }

            yield return null;
		}
	}

#endregion

    #region IChatReceiveMessageObserver

    bool IsMessageBlockOnBattle()
    {
        if (BattleManager.Instance.battleType == BattleType.TutorialStartBattle )
            return true;
        else
            return false;
    }

    bool IsMessageBlock()
    {
        if (!GameSystem.Instance.Data.User.Tasks.IsComplete(UserTaskTag.Tut_ClearTutBattle) )
            return true;
        else
            return false;
    }

    void IChatReceiveMessageObserver.OnChatReceiveMessage (int packetType, ChatMakingMessage makingMessage, ChatMessage chatMsg)
	{
        if(ChattingController.Instance == null)
            return;

		if (makingMessage == null)
			return;

		ActionEventTimer eventTimer = ChattingController.Instance.ChatSocketManager.EventTimer;

        bool isPreviewNotify = true;
        if(chatMsg.messageType == (int)ChatDefinition.ChatMessageType.ObtainCardItemChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.ObtainEquipmentItemChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.EnhanceEquipmentItemChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.UserLevelUpChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.HeroLevelUpChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.SuccessRefineryFloorChat ||
            chatMsg.messageType == (int)ChatDefinition.ChatMessageType.HeroChallengeNewHighScoreChat) {
            UserModel userModel = ChattingController.Instance.Context.User;
            if(chatMsg.userID == userModel.userData.userId) {
                isPreviewNotify = false;
            }
        } else if(chatMsg.messageType == (int)ChatDefinition.ChatMessageType.GMSystemChatMessage) {
            isPreviewNotify = false;
        }

        if(isPreviewNotify && !ChattingController.Instance.IsPartyChatView) {
            ChatPreviewMessageInfo inputPreviewInfo = new ChatPreviewMessageInfo();
            inputPreviewInfo.packetType = packetType;
            inputPreviewInfo.messageType = makingMessage.chatMessageInfo.messageType;
            inputPreviewInfo.chatTextColor = makingMessage.messageColor;
            inputPreviewInfo.multiChatInfos = makingMessage.multiChatTextInfoList;
            _previewQueueMessageInfo.Enqueue(inputPreviewInfo);

            ShowPreviewMessage();
        }

        bool isValidSelectChat = false;

        if(makingMessage.chatMessageInfo.sendType == ChatDefinition.ChatMessageKind.None) {
            for (int i = 0; i < makingMessage.chatMessageKinds.Length; i++) {
                switch ((ChatDefinition.ChatMessageKind)makingMessage.chatMessageKinds[i]) {
                    case ChatDefinition.ChatMessageKind.ChannelChat:
                        if (_curSelectChattingInput != null) {
                            isValidSelectChat = true;
                        }
                        break;
                    case ChatDefinition.ChatMessageKind.GuildChat:
                        if (_curSelectChattingInput != null) {
                            if (_curSelectChattingInput.PartyType == (int)ChatPartyType.ChatGuild && _curSelectChattingInput.PartyNum == makingMessage.chatMessageInfo.partyNum) {
                                isValidSelectChat = true;
                                if (_curMultiChatState == UIMultiChattingState.InputChatting) {
                                    long confirmTimeStamp = TimeUtil.GetTimeStampAddSecond(1);
                                    GuildChatInformation guildChatInfo = ChattingController.Instance.ChattingModel.GetGuildChatInformation(_curSelectChattingInput.PartyNum);
                                    if (guildChatInfo != null) {
                                        guildChatInfo.LastChatViewTime = confirmTimeStamp;
                                    }

                                    if (!makingMessage.chatMessageInfo.isSelfNotify) {
                                        if (_curSelectChattingInput.PartyType == (int)ChatPartyType.ChatGuild) {
                                            new ProtocolGuildChatViewUpdate(ChattingController.Instance.Context, _curSelectChattingInput.PartyNum, confirmTimeStamp, null).Execute();
                                        } else if (_curSelectChattingInput.PartyType == (int)ChatPartyType.ChatParty) {
                                            new ProtocolPartyChatViewUpdate(ChattingController.Instance.Context, _curSelectChattingInput.PartyNum, confirmTimeStamp, null).Execute();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ChatDefinition.ChatMessageKind.WhisperChat:
                        if (_curSelectChattingInput != null) {
                            if (ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo != null) {
                                ChatWhisperMessage whisperMessage = chatMsg as ChatWhisperMessage;
                                if (ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo.targetUserID == whisperMessage.targetUserID ||
                                    ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo.targetUserID == whisperMessage.userID) {
                                    isValidSelectChat = true;
                                    if (_curMultiChatState == UIMultiChattingState.InputChatting) {
                                        long connectId = ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo.targetConnectID;
                                        ChatWhisperUserData whisperUserTime = ChattingController.Instance.ChattingModel.WhisperInformation.GetWhisperUserData(connectId);
                                        if (whisperUserTime != null)
                                            whisperUserTime.ConfirmTimeStamp = TimeUtil.GetTimeStampAddSecond(1);
                                        MultiChattingModel.RequestWhisperLastConfirmHttp(connectId);
                                    }
                                }
                            }
                        }
                        break;
                    case ChatDefinition.ChatMessageKind.MyPartyChat:
                        if (_curSelectChattingInput != null) {
                            if (_curSelectChattingInput.PartyNum == makingMessage.chatMessageInfo.partyNum) {
                                isValidSelectChat = true;
                                ChattingController.Instance.ChattingModel.MyPartyMessageInfo.LastChatTimeStamp = TimeUtil.GetTimeStampAddSecond(1);
                                long confirmTimeStamp = ChattingController.Instance.ChattingModel.MyPartyMessageInfo.LastChatTimeStamp;
                                new ProtocolPartyMyChatViewUpdate(ChattingController.Instance.Context, confirmTimeStamp, null).Execute();
                            }
                        }
                        break;
                    case ChatDefinition.ChatMessageKind.PartyChat:
                        if (_curSelectChattingInput != null && _curSelectChattingInput.PartyType == (int)ChatPartyType.ChatParty) {
                            if (_curSelectChattingInput.PartyNum == makingMessage.chatMessageInfo.partyNum) {
                                isValidSelectChat = true;
                            }
                        }
                        break;
                }
            }
        } else {
            isValidSelectChat = true;
        }

        if (_curMultiChatState == UIMultiChattingState.InputChatting && isValidSelectChat) {
            Color chatColor = ColorPreset.CHAT_NORMAL;
            if(chatMsg.messageType == (int)ChatDefinition.ChatMessageType.GMChannelChat) {
                ChatGMSMessage gmMessage = chatMsg as ChatGMSMessage;
                chatColor = gmMessage.chatColor;
            }
			AddMultiChatMakingMessage (makingMessage, chatColor);

            RefreshCurTimeChatList();
            if(makingMessage.TimeStampTextInfo != null) {
                _timeChatMessageList.Add(makingMessage);
            }

            if (_curSelectChattingInput != null) {
                switch (_curSelectChattingInput.ChatMessageKind) {
                    case ChatDefinition.ChatMessageKind.ChannelChat:
                        break;
                    case ChatDefinition.ChatMessageKind.GuildChat:
                        ChattingController.Instance.ChattingModel.ChangeGuildChatConfirmTime(_curSelectChattingInput.PartyNum, TimeUtil.GetTimeStampAddSecond(1));
                        break;
                    case ChatDefinition.ChatMessageKind.WhisperChat:
                        ChattingController.Instance.ChattingModel.WhisperInformation.SetWhisperLastConfirmTimeStamp(TimeUtil.GetTimeStampAddSecond(1));
                        break;
                }
            }
        }
	}

    #endregion

    #region Helper Methods

	public static string GetConfirmMessageTimeText(long lastMsgTimeStamp)
	{
		if (ChattingController.Instance == null)
			return "";
		
		string retValue = "";

		TextModel textModel = ChattingController.Instance.Context.Text;

		long gapTimeStamp = TimeUtil.GetTimeStamp () - lastMsgTimeStamp;

		TimeSpan span = TimeSpan.FromMilliseconds ((double)gapTimeStamp);
		if (span.Days > 1) {
            retValue = string.Format(textModel.GetText (TextKey.Days_Ago), span.Days);
		} else if (span.Hours > 1) {
            retValue = string.Format(textModel.GetText (TextKey.Hours_Ago), span.Hours);
		} else {
            if(span.Minutes >= 1) {
                retValue = string.Format(textModel.GetText(TextKey.Minutes_Ago), span.Minutes);
            } else {
                retValue = textModel.GetText(TextKey.Just_Ago);
            }
		}

		return retValue;
	}

    public void SetWhisperUserInfo(ChatWhisperTargetUserInfo whisperTargetInfo)
    {
        MultiChattingModel chattingModel = ChattingController.Instance.ChattingModel;
        chattingModel.WhisperInformation.WhisperTargetUserInfo = whisperTargetInfo;
        _uiMultiChatPopup.SetWhisperTargetText(chattingModel.WhisperInformation.WhisperTargetUserInfo.targetNickname);
    }

    #endregion

    #region CallBack Methods

    void OnSetOffPreviewText(object objData)
	{
        _chatPreviewMessage.gameObject.SetActive (false);
	}

	void OnSetOffBattlePreviewText(object objData)
	{
        _chatBattlePreviewMessage.gameObject.SetActive (false);
	}

    void OnCheckNextPreview(object objData)
    {
        ShowPreviewMessage();
    }

    void OnClickChattingButton(UIChattingButton chatButtonInfo, bool refresh = false)
	{
		if (chatButtonInfo.ChatMessageKind == ChatDefinition.ChatMessageKind.GuildChat) {
			if (!ChattingController.Instance.IsChatGuildConnected)
				return;
		}

        if (!refresh && _curSelectChattingInput == chatButtonInfo)
            return;

        _timeChatMessageList.Clear();

#if UNITY_EDITOR
        _uiMultiChatPopup.SetEnableEditorChannelInputObj(false);
#endif

        if(_curSelectChattingInput != null) {
            _curSelectChattingInput.SelectObj.SetActive(false);
        }

        chatButtonInfo.SelectObj.SetActive(true);

		_curSelectChattingInput = chatButtonInfo;

		MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

		ChattingController.Instance.CurChatMessageKind = chatButtonInfo.ChatMessageKind;

        List<MultiChatListInfo> noticeMessageList = null;
        List<ChatMakingMessage> chatMakingMessages = null;

        List<ChatMakingMessage> subNoticeMessageList = null;

        switch (chatButtonInfo.ChatMessageKind) {
            case ChatDefinition.ChatMessageKind.ChannelChat:
                chatMakingMessages = chatModel.ChannelChatInfo.ChannelChatMessages;
                noticeMessageList = chatModel.GetNoticeListMessages();
                _uiMultiChatPopup.SetInputFieldType(UIMultiChattingPopup.ChatInputFieldType.ChannelInputField);

                ConfirmChannelMessage();
                break;
            case ChatDefinition.ChatMessageKind.GuildChat:
                ChattingController.Instance.SetCurChatGuildInfoUI(chatButtonInfo.PartyNum);
                ConfirmChatRoomMessage((int)ChatPartyType.ChatGuild, chatButtonInfo.PartyNum);

                chatMakingMessages = chatModel.GetGuildChatInformation(chatButtonInfo.PartyNum).ChatMessages;

                noticeMessageList = chatModel.GetNoticeListMessages();

                ChatGuildJoinedInfo guildPartyInfo = ChattingController.Instance.GetChatGuildInfo(chatButtonInfo.PartyNum);
                if (guildPartyInfo != null) {
                    if (noticeMessageList == null) {
                        noticeMessageList = new List<MultiChatListInfo>();
                    }

                    for (int i = 0; i < guildPartyInfo.guildNoticeMessages.Count; i++) {
                        MultiChatListInfo inputChatListInfo = new MultiChatListInfo();
                        inputChatListInfo.ChatColor = ColorPreset.CHAT_COMPANY_NOTICE;
                        inputChatListInfo.ChatTextInfos = guildPartyInfo.guildNoticeMessages[i];
                        noticeMessageList.Add(inputChatListInfo);
                    }
                }

                subNoticeMessageList = chatModel.GetAnnounceGuildMultiChatText(chatButtonInfo.PartyNum);
                _uiMultiChatPopup.SetInputFieldType(UIMultiChattingPopup.ChatInputFieldType.NormalInputField);
                break;
            case ChatDefinition.ChatMessageKind.PartyChat:
                chatMakingMessages = ChattingController.Instance.PartyChatModel.GetPartyMakingMesssage(chatButtonInfo.PartyNum);

                noticeMessageList = chatModel.GetNoticeListMessages();

                _uiMultiChatPopup.SetInputFieldType(UIMultiChattingPopup.ChatInputFieldType.QuickParty);
                break;
            case ChatDefinition.ChatMessageKind.WhisperChat:
                _uiMultiChatPopup.SetInputFieldType(UIMultiChattingPopup.ChatInputFieldType.WhisperInputField);
                noticeMessageList = chatModel.GetNoticeListMessages();

                if (ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo != null) {
                    ChatWhisperTargetUserInfo whisperTargetUserInfo = ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo;
                    chatMakingMessages = chatModel.WhisperInformation.GetWhisperUserMessages(whisperTargetUserInfo.targetConnectID);
                }

                ConfirmWhisperChatRoomMessage();
                break;
            case ChatDefinition.ChatMessageKind.MyPartyChat:
                chatMakingMessages = chatModel.MyPartyMessageInfo.ChatMessages;
                noticeMessageList = chatModel.GetNoticeListMessages();

                _uiMultiChatPopup.SetInputFieldType(UIMultiChattingPopup.ChatInputFieldType.None);

                ConfirmMyPartyChatMessage();
                break;
        }

        _uiMultiChatPopup.ClearNoticeMessage ();

        if (noticeMessageList != null && noticeMessageList.Count > 0) {
			RefreshNoticeMessages(noticeMessageList);
		}

        if (subNoticeMessageList != null && subNoticeMessageList.Count > 0) {
            Color noticeColor = ColorPreset.CHAT_COMPANY_SUBNOTICE;
			RefreshSubNoticeMessages(subNoticeMessageList, noticeColor);
        }

		RefreshMultiChattingPanel (chatMakingMessages, _uiMultiChatPopup.CurNoticeHeight);

        if (_curMultiChatState != UIMultiChattingState.InputChatting) {
            _curMultiChatState = UIMultiChattingState.InputChatting;
        }

        ChattingController.Instance.ButtonEventManager.ReleasePopup();
    }

	public void OnClickChangeChannel()
	{
#if UNITY_EDITOR
        SetEditorChannelInputObject();
#else
        SetChannelInputKeyboard();
#endif
    }

    void OnChannelInputText(string inputText)
	{
		if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
			ChangeChannelNumber(inputText);
	}

	void OnWhisperTargetButton()
	{
        _uiWhisperTargetPopup.InitWhisperTargetPopup();
        _uiWhisperTargetPopup.gameObject.SetActive(true);
    }

    void OnSelectWhisperUserInfo(UIWhisperUserInfoCell userInfoCell)
	{
        SetWhisperUserInfo(userInfoCell.WhisperUserInfo);

        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
        if (!chatModel.WhisperInformation.ExistWhisperMessage(userInfoCell.WhisperUserInfo.targetConnectID)) {
            RequestHttpWhisperTargetMessage(userInfoCell.WhisperUserInfo);
        } else {
            ReleaseCurSelectChattingInput();
            OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.WhisperChat].UIChatButtonInfo);
        }
    }

    void OnSuccessChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
    {
        if (chatResponse != null && chatResponse.info != null && chatResponse.info.Length > 0) {
            List<IChatTimeStamp> chatMessageInfos = new List<IChatTimeStamp>();
            for (int i = 0; i < chatResponse.info.Length; i++) {
                ChatWhisperOtherUserResponseInfo whisperChatInfo = chatResponse.info[i];

                if (whisperChatInfo.message_list != null && whisperChatInfo.message_list.Length > 0) {
                    for (int j = 0; j < whisperChatInfo.message_list.Length; j++) {
                        chatMessageInfos.Add(whisperChatInfo.message_list[j]);
                    }
                }
            }

            int chatCount = 50;
            if (chatMessageInfos.Count > chatCount) {
                int gapValue = chatMessageInfos.Count - chatCount;
                for (int i = 0; i < gapValue; i++) {
                    chatMessageInfos.RemoveAt(0);
                }
            }

            for (int i = 0; i < chatMessageInfos.Count; i++) {
                ChatWhisperUserMessageInfo whisperMessageInfo = chatMessageInfos[i] as ChatWhisperUserMessageInfo;
                byte[] decbuf = System.Convert.FromBase64String(whisperMessageInfo.chat_msg);
                string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
                Debug.Log(string.Format("OnSuccessChattingWhisperChatList timestamp : {0}, msg : {1}", whisperMessageInfo.timestamp, msg));
#endif

                JsonData msgJson = ChatParsingUtil.GetChatMessageJsonData(msg);
                ChatWhisperMessage chatMessage = ChatParsingUtil.GetChatWhisperMessageParsingByJson(msgJson);
                chatMessage.timeStamp = whisperMessageInfo.timestamp;

                long whisperUserID = 0;
                long connectId = 0;
                string whisperUserName = "";

                if (whisperMessageInfo.type == 1) { // Send
                    chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperToChatting;
                    whisperUserID = chatMessage.targetUserID;
                    connectId = chatMessage.targetConnectID;
                    whisperUserName = chatMessage.targetUserName;
                } else if (whisperMessageInfo.type == 2) { // Receive
                    chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperFromChatting;
                    whisperUserID = chatMessage.userID;
                    connectId = chatMessage.connectId;
                    whisperUserName = chatMessage.sendUserName;
                }

                if (chatMessage.partMessageInfos.ContainsKey("user")) {
                    chatMessage.partMessageInfos.Remove("user");
                }

                chatMessage.partMessageInfos.Add("user", ChattingController.Instance.GetUserInfoChatPartInfo(whisperUserID, connectId));

                if (!chatMessage.prm.ContainsKey("user"))
                    chatMessage.prm.Add("user", whisperUserName);

                MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

                chatModel.AddServerWhisperChatMessage(chatMessage);
            }
        }

        FinishRequestWhisperServerLog();

        ReleaseCurSelectChattingInput();
        OnClickChattingButton(_chatButtonObjInfos[(int)ChatDefinition.ChatMessageKind.WhisperChat].UIChatButtonInfo);
    }
    
    void OnFailChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
    {
        _isRequestWhisperServerLog = false;
    }

    #endregion

    #region IChatChangeCompanyNotice Methods

    void IChatChangeCompanyNotice.OnChangeCompanyNotice(int partyType, long partyNum, string companyNotice)
	{
		if (_curMultiChatState == UIMultiChattingState.InputChatting && _curSelectChattingInput != null && _curSelectChattingInput.ChatMessageKind == ChatDefinition.ChatMessageKind.GuildChat) {
			if (_curSelectChattingInput.PartyType == partyType && _curSelectChattingInput.PartyNum == partyNum) {
				
			}
		}
	}

    #endregion
}
