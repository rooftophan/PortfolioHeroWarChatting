using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Linq;
using LitJson;
using System;
using System.IO;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChattingController : MonoBehaviour, IChatChangeCompanyNotice, IChatNudgeNotable
{
	private static ChattingController _instance = null;
	public static ChattingController Instance
	{
		get{ return _instance; }
	}

    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] UIMultiChatting _uiMultiChat = default(UIMultiChatting);
	[SerializeField] UIChatPartMessage _supportTextMessage = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _timeSupportText = default(UIChatPartMessage);
    [SerializeField] UIChatPartMessage _partyQuickViewText = default(UIChatPartMessage);

#pragma warning restore 649

    #endregion

    #region Variables

    bool _isUpdate = false;
	MultiChattingModel _chattingModel = null;
	ChattingSocketManager _chatSocketManager = new ChattingSocketManager ();
	DataContext _data;
	int _retryChatInfo = 0;
	bool _isRequestChatInfo = false;

	int _maxChatLineCount = 128;
	int _announceMaxCount = 2;
    int _maxWhisperChatLineCount = 50;

    int _checkEquipmentGrade = 6; // default : 6
    int _checkEquipmentRarity = 4; // default : 4

    string _chatWebUrl;

    PartyChattingModel _partyChatModel = null;

    List<IChatReceiveMessageObserver> _receiveMessageObservers = new List<IChatReceiveMessageObserver>();
	List<IChatSendMessageObserver> _sendMessageObservers = new List<IChatSendMessageObserver>();
    List<IChatPartyEventObserver> _partyEventObservers = new List<IChatPartyEventObserver>();
    List<IChatGuildRaidEventObserver> _guildRaidEventObservers = new List<IChatGuildRaidEventObserver>();

    bool _isBattleState = false;

	ChatDefinition.ChatMessageKind _curChatMessageKind;

	ChatPartyBaseInfo _curChatGuildInfoUI;

    Dictionary<long /* Party Num */, ChatGuildJoinedInfo> _chatGuildGroupInfos = new Dictionary<long, ChatGuildJoinedInfo>();
    Dictionary<long /* Party Num */, ChatPartyJoinedInfo> _chatPartyJoinedInfos = new Dictionary<long, ChatPartyJoinedInfo>();
    ChatPartyBaseInfo _myChatPartyInfo = new ChatPartyBaseInfo();

    bool _isChatGuildConnected = false;
    bool _isPartyGroupConnected = false;
    bool _isMyPartyConnected = false;
    bool _isPartyEventConnected = false;

	UICommonTopMenuPanel _uiCommonTopMenu = null;

	List<IChatServerMessage> _chatServerMessageObserver = new List<IChatServerMessage>();

	ChatPartyBaseInfo _curSelectGuildInfo = null;
	ChatPartyBaseInfo _addGuildBaseInfo = null;

	ChatEventManager _chatEventManager = new ChatEventManager ();
    ChatButtonEventManager _buttonEventManager = new ChatButtonEventManager();

    bool _isChatPauseState = false;
	long _pauseTime = 0;

    ChatShortcutData _shortcutData = null;
    int _chatTransitValue_1 = -1;
    int _totalNotConfirmCount = 0;
    bool _isPartyChatView = false;

    Action _onReleaseAllChatInfo = null;
    Action _onReleaseQuickPartyChat = null;

    ChatNudgeNode _rootNudgeNode = new ChatNudgeNode();

    long _lastSendTime = -1;
    int _checkSendCount = 0;

    ChatRepeatSendInfo _repeatSendNotify = new ChatRepeatSendInfo();

    bool _isOpenCloseState = false;
    GameObject _guildWarTopChatNudgeObj = null;

    #endregion

    #region Party Chat Server Log Variables

    int _curRequestPartyLogCount = 0;
	Queue<ChatPartyBaseInfo> _requestPartyServerLogChat = new Queue<ChatPartyBaseInfo>();
	ChatPartyBaseInfo _curRequestPartyBaseInfo = null;
	bool _isRequestPartyServerLog = false;
    Queue<ChatSendCardObtainInfo> _sendCardObatinInfos = new Queue<ChatSendCardObtainInfo>();

    Queue<ChatSendAchieveHeroLevelInfo> _sendAchieveHeroLevels = new Queue<ChatSendAchieveHeroLevelInfo>();

    PartyChatChecker _partyChattingChecker = new PartyChatChecker();

    #endregion

    #region Whisper Chat Server Log Variables

    bool _isRequestWhisperServerLog = false;

	#endregion

	#region Properties

	public bool IsUpdate
	{
		get{ return _isUpdate; }
		set{ _isUpdate = value; }
	}

	public MultiChattingModel ChattingModel
	{
		get{ return _chattingModel; }
	}

	public ChattingSocketManager ChatSocketManager
	{
		get{ return _chatSocketManager; }
	}

    public PartyChattingModel PartyChatModel
    {
        get { return _partyChatModel; }
    }

    public bool IsRequestChatInfo
	{
		get{ return _isRequestChatInfo; }
	}

	public bool IsBattleState
	{
		get{ return _isBattleState; }
		set{ _isBattleState = value; }
	}

    public string ChatWebUrl
    {
        get { return _chatWebUrl; }
    }

    public bool IsChattingPopup
	{
		get{ 
			return _uiMultiChat.UIMultiChatPopup.gameObject.activeSelf;
		}
	}

	public ChatDefinition.ChatMessageKind CurChatMessageKind
    {
		get{ return _curChatMessageKind; }
		set{ _curChatMessageKind = value; }
	}

	public UIMultiChatting UIMultiChat
	{
		get{ return _uiMultiChat; }
	}

	public DataContext Context
	{
		get{ return _data; }
	}

    public Dictionary<long /* Party Num */, ChatGuildJoinedInfo> ChatGuildGroupInfos
    {
        get { return _chatGuildGroupInfos; }
    }

    public Dictionary<long /* Party Num */, ChatPartyJoinedInfo> ChatPartyJoinedInfos
    {
        get { return _chatPartyJoinedInfos; }
    }

    public ChatPartyBaseInfo MyChatPartyInfo
    {
        get { return _myChatPartyInfo; }
    }

    public int MaxChatLineCount
	{
		get{ return _maxChatLineCount; }
	}

    public int MaxWhisperChatLineCount
    {
        get { return _maxWhisperChatLineCount; }
    }

    public int AnnounceMaxCount
	{
		get{ return _announceMaxCount; }
	}

	public ChatPartyBaseInfo CurChatGuildInfoUI
	{
		get{ return _curChatGuildInfoUI; }
	}

	public bool IsChatGuildConnected
	{
		get{ return _isChatGuildConnected; }
		set{ _isChatGuildConnected = value; }
	}

    public bool IsPartyGroupConnected
    {
        get { return _isPartyGroupConnected; }
        set { _isPartyGroupConnected = value; }
    }

    public bool IsMyPartyConnected
    {
        get { return _isMyPartyConnected; }
        set { _isMyPartyConnected = value; }
    }

    public bool IsPartyEventConnected
    {
        get { return _isPartyEventConnected; }
        set { _isPartyEventConnected = value; }
    }

    public UIChatPartMessage SupportTextMessage
	{
		get{ return _supportTextMessage; }
	}

    public UIChatPartMessage TimeSupportText
    {
        get { return _timeSupportText; }
    }

    public UIChatPartMessage PartyQuickViewText
    {
        get { return _partyQuickViewText; }
    }

    public UICommonTopMenuPanel UICommonTopMenu
	{
		get{ return _uiCommonTopMenu; }
		set{ _uiCommonTopMenu = value; }
	}

	public bool IsRequestPartyServerLog
	{
		get{ return _isRequestPartyServerLog; }
	}

	public ChatPartyBaseInfo CurSelectGuildInfo
	{
		get{ return _curSelectGuildInfo; }
		set{ _curSelectGuildInfo = value; }
	}

	public ChatEventManager ChatEventManager
	{
		get{ return _chatEventManager; }
	}

	public bool IsRequestWhisperServerLog
	{
		get{ return _isRequestWhisperServerLog; }
	}

    public ChatShortcutData ShortcutData
    {
        get { return _shortcutData; }
        set { _shortcutData = value; }
    }

    public int ChatTransitValue_1
	{
		get{ return _chatTransitValue_1; }
		set{ _chatTransitValue_1 = value; }
	}

    public int TotalNotConfirmCount
    {
        get { return _totalNotConfirmCount; }
        set { _totalNotConfirmCount = value; }
    }

    public bool IsPartyChatView
    {
        get { return _isPartyChatView; }
        set { _isPartyChatView = value; }
    }

    public Action OnReleaseAllChatInfo
    {
        get { return _onReleaseAllChatInfo; }
        set { _onReleaseAllChatInfo = value; }
    }

    public Action OnReleaseQuickPartyChat
    {
        get { return _onReleaseQuickPartyChat; }
        set { _onReleaseQuickPartyChat = value; }
    }

    public ChatButtonEventManager ButtonEventManager
    {
        get { return _buttonEventManager; }
    }

    public ChatNudgeNode RootNudgeNode
    {
        get { return _rootNudgeNode; }
    }

    public bool IsOpenCloseState
    {
        get { return _isOpenCloseState; }
        set { _isOpenCloseState = value; }
    }

    public GameObject GuildWarTopChatNudgeObj
    {
        get { return _guildWarTopChatNudgeObj; }
        set { _guildWarTopChatNudgeObj = value; }
    }

    #endregion

    #region MonoBehaviour Methods

    void Awake()
	{
		DontDestroyOnLoad (this.gameObject);
		_instance = this;
    }

	void OnEnable()
	{
		_isUpdate = true;
		StartCoroutine (UpdateChatting());
	}

	void OnDisable()
	{
		StopCoroutine (UpdateChatting ());
		_isUpdate = false;
	}

	void OnApplicationFocus(bool hasFocus)
	{
        if (hasFocus) {
			if ( _chatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected && _isChatPauseState) {
				long curTime = TimeUtil.GetTimeStamp ();
				long gapTime = curTime - _pauseTime;

				TimeSpan span = TimeSpan.FromMilliseconds ((double)gapTime);

				#if _CHATTING_LOG
				Debug.Log (string.Format ("ChattingController OnApplicationFocus span : {0}", span.TotalSeconds));
				#endif
				if (span.TotalSeconds >= 60f) {
                    if (_chatSocketManager.IsValidDisconnect()) {
                        _chatSocketManager.Disconnect();
                        _chatSocketManager.Reconnect();
                    }
				} else {
					if (_chatSocketManager.IsPingRequestPacket) {
						_chatSocketManager.PingStartTime = Time.realtimeSinceStartup;
					} else {
						_chatSocketManager.OnPingPacketRes (null);
					}
				}
			}

			_isChatPauseState = false;
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
			if (_chatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected && !_isChatPauseState) {
				_pauseTime = TimeUtil.GetTimeStamp();
				_isChatPauseState = true;

				#if _CHATTING_LOG
				Debug.Log (string.Format ("ChattingController OnApplicationPause _chatManager.ChatPacketManager.ChatCurState == ChatService.ChatCurrentState.Connected"));
				#endif
			}
		}
	}

	void OnDestroy()
	{
		_chatSocketManager.Disconnect ();
	}

	#endregion

	#region Methods

	public static void InitChattingController()
	{
		if (_instance != null)
			return;

        GameObject chatController = ResourceLoader.Instantiate(Res.PREFAB.ChattingController, null, true);

		chatController.SetActive (true);
		chatController.name = "[ChattingController]";

        LoadChattingSystemMessage();

        PartyChatEventManager.InitPartyChatEvent();
    }

    public static void LoadChattingSystemMessage()
    {
        GameObject chatSystemMessageUI = ResourceLoader.Instantiate(Res.PREFAB.UITopDepthNoticeMessage, null, true);
        chatSystemMessageUI.SetActive(true);
        chatSystemMessageUI.name = "[ChattingSystemMessage]";
    }

	public static void ReleaseChattingController()
	{
        UITopDepthNoticeMessage.ReleaseChatSystemMessage();

		if (_instance == null)
			return;

		#if UNITY_EDITOR
		Destroy (_instance.gameObject, 0f);
		#else
		DestroyImmediate (_instance.gameObject);
		#endif
		_instance = null;

        PartyChatEventManager.ReleasePartyChat();
    }

	public void InitChattingData(DataContext context)
	{
		_data = context;

		_chattingModel = new MultiChattingModel ();
		_chattingModel.Context = _data;

		_chattingModel.InitChattingModel ();

        _partyChatModel = new PartyChattingModel();
        _partyChatModel.Context = _data;

        _buttonEventManager.Context = _data;

        _chatEventManager.Context = context;
		_chatEventManager.AttachChangeCompanyNoticeOb ((IChatChangeCompanyNotice)this);
		_chatEventManager.AttachChangeCompanyNoticeOb ((IChatChangeCompanyNotice)_uiMultiChat);

		_chatSocketManager.InitChatSocket (_data, _chattingModel, this);
		_chatSocketManager.OnRequestNetChatInfo = OnRequestNetChatInfo;

		AttachChatReceiveMessageOb ((IChatReceiveMessageObserver)_chattingModel);
		AttachChatReceiveMessageOb ((IChatReceiveMessageObserver)_uiMultiChat);
        AttachChatReceiveMessageOb ((IChatReceiveMessageObserver)_partyChatModel);

        AttachChatSendMessageOb ((IChatSendMessageObserver)_chatSocketManager);

        _chatSocketManager.AttachChatChangePartyOb((IChatChangePartyObserver)_chattingModel);

        _myChatPartyInfo.party_num = _data.User.userData.userId;
        _myChatPartyInfo.party_type = (int)ChatPartyType.ChatUserParty;

        _retryChatInfo = 0;

#if UNITY_EDITOR
		_uiMultiChat.UIMultiChatPopup.SetInputFieldEndEditAction(OnChatInputMessage);
        _uiMultiChat.UIMultiChatPopup.SetChannelInputFieldEndEditAction(OnChatInputMessage);
        _uiMultiChat.UIMultiChatPopup.SetWhisperInputFieldEndEditAction(OnChatWhisperInputMessage);
#else
		_uiMultiChat.UIMultiChatPopup.OnInputKeyboardAction = SendChattingMessage;
		_uiMultiChat.UIMultiChatPopup.OnWhisperInputKeyboardAction = SendWhisperChatMessage;
#endif

        _uiMultiChat.UIMultiChatPopup.InitUIMultiChattingPopup ();

        _uiMultiChat.UIWhisperTargetPopup.InitWhisperTargetText(_data.Text);

        InitChatNudgeData();
    }

    void InitChatNudgeData()
    {
        _rootNudgeNode.AddNudgeNotable(this as IChatNudgeNotable);
        _rootNudgeNode.AddChildNode(_chattingModel.ChannelChatInfo.NudgeNode);
        _rootNudgeNode.AddChildNode(_chattingModel.WhisperInformation.WhisperRootNudge);
        _rootNudgeNode.AddChildNode(_chattingModel.MyPartyMessageInfo.NudgeNode);
    }

	//public void ReleaseChattingData()
	//{
	//	if (_chatSocketManager != null)
	//		_chatSocketManager.Disconnect ();
		
	//	ReleaseChatReceiveMessageOb ();
	//	ReleaseChatSendMessageOb ();

	//	_chatEventManager.ReleaseChangeCompanyNoticeOb ();

	//	_isUpdate = false;
	//}

	public void RefreshChatting()
	{
		_isUpdate = true;
		StartCoroutine (UpdateChatting());
	}

	public void RequestNetChatInfo(object objData = null)
	{
        if(_data == null || _data.User == null || _data.User.userData == null) {
            _isRequestChatInfo = false;
            _retryChatInfo = 0;
            return;
        }

        if(Application.internetReachability == NetworkReachability.NotReachable) {
            _chatSocketManager.EventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, RequestNetChatInfo, 3f, null,
                ChattingSocketManager.reserveChatInfoTimeID);
            return;
        }

        new ChatInfoTask(_data, OnChatInfoSuccess, OnChatInfoFail).Execute();
        _retryChatInfo++;

        _chatSocketManager.EventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnTimeoutRequestChatInfo, 15f, null,
            _chatSocketManager.ChatInfoTimeID);

        _isRequestChatInfo = true;
    }

	public void AttachChatReceiveMessageOb(IChatReceiveMessageObserver chatReceiveMessageOb)
	{
		if (_receiveMessageObservers.Contains (chatReceiveMessageOb))
			return;

		_receiveMessageObservers.Add (chatReceiveMessageOb);
	}

	public void DetachChatReceiveMessageOb(IChatReceiveMessageObserver chatReceiveMessageOb)
	{
		if (!_receiveMessageObservers.Contains (chatReceiveMessageOb))
			return;

		_receiveMessageObservers.Remove (chatReceiveMessageOb);
	}

	public void NotifyChatReceiveMessage(int packetType, ChatMessage recMessage)
	{
		SheetChatNoticeMessageRow messageRow = null;
		ChatMakingMessage makingMessage = null;
		if (recMessage.msgIdx != -1) {
			messageRow = _data.Sheet.SheetChatNoticeMessage [recMessage.msgIdx];

			makingMessage = new ChatMakingMessage ();
			//makingMessage.CopyChatBaseMessage (recMessage);
            makingMessage.chatMessageInfo = recMessage;

            if (recMessage.messageType == (int)ChatDefinition.ChatMessageType.GMChannelChat) {
                ChatGMSMessage chatGMSMsg = recMessage as ChatGMSMessage;
                makingMessage.messageColor = chatGMSMsg.chatColor;
            } else {
                makingMessage.messageColor = ChatHelper.GetChatMessageColor(_data, recMessage);
            }
			
			makingMessage.chatMessageKinds = messageRow.IndicateLocation;

            MultiChatTextInfo timeStampTextInfo = null;

            makingMessage.multiChatTextInfoList = _chattingModel.GetMultiChatTextInfoList (recMessage, out timeStampTextInfo);
            //makingMessage.multiChatTextOtherViewList = _partyChatModel.GetQuickMultiChatTextInfoList(recMessage);
            makingMessage.TimeStampTextInfo = timeStampTextInfo;

        } else {
			Debug.Log (string.Format ("NotifyChatReceiveMessage packetType : {0}, msgIdx == -1 !!!!!", packetType));
			return;
		}

		for (int i = 0; i < _receiveMessageObservers.Count; i++) {
			_receiveMessageObservers [i].OnChatReceiveMessage (packetType, makingMessage, recMessage);
		}

        SetChatReceiveMessage(packetType, makingMessage, recMessage);
    }

    void SetChatReceiveMessage(int packetType, ChatMakingMessage makingMessage, ChatMessage chatMsg)
    {
        if (ChattingController.Instance != null && ChattingController.Instance.CurSelectGuildInfo != null && chatMsg != null &&
            chatMsg.partyType == (int)ChatPartyType.ChatGuild && ChattingController.Instance.CurSelectGuildInfo.party_num == chatMsg.partyNum) {
            if (chatMsg.msgIdx == (int)ChatNoticeMessageKey.GuildWarMemberArrangeStart) {
                GuildWarChatting.editPlayerId = chatMsg.connectId;
            } else if(chatMsg.msgIdx == (int)ChatNoticeMessageKey.GuildWarMemberArrangeEnd) {
                GuildWarChatting.editPlayerId = -1;
            }
        }
    }

	public void ReleaseChatReceiveMessageOb()
	{
		_receiveMessageObservers.Clear ();
	}

	public void AttachChatSendMessageOb(IChatSendMessageObserver chatSendMessageOb)
	{
		if (_sendMessageObservers.Contains (chatSendMessageOb))
			return;

		_sendMessageObservers.Add (chatSendMessageOb);
	}

	public void DetachChatSendMessageOb(IChatSendMessageObserver chatSendMessageOb)
	{
		if (!_sendMessageObservers.Contains (chatSendMessageOb))
			return;

		_sendMessageObservers.Remove (chatSendMessageOb);
	}

	public void NotifyChatSendMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage sendMessage)
	{
        if(CheckRepeatChatMessage(chatMessageKind, sendMessage))
            return;

        for (int i = 0; i < _sendMessageObservers.Count; i++) {
			_sendMessageObservers [i].OnChatSendMessage (chatMessageKind, sendMessage);
		}

        if(_lastSendTime == -1) {
            _lastSendTime = TimeUtil.GetTimeStamp();
        } else {
            int checkTimeSec = 1;
            long curSendTime = TimeUtil.GetTimeStamp();
            long gapTime = (curSendTime - _lastSendTime)/1000;
            if(gapTime <= checkTimeSec) {
                _checkSendCount++;
                if(_checkSendCount >= 5) {
                    _chatSocketManager.LockDisconnectChatting();
                }
            } else {
                _lastSendTime = TimeUtil.GetTimeStamp();
                _checkSendCount = 0;
            }
        }
	}

    public void NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage sendMessage)
    {
        for (int i = 0; i < _sendMessageObservers.Count; i++) {
            _sendMessageObservers[i].OnChatSendMessage(chatMessageKind, sendMessage);
        }
    }

    public void NotifyChatSendPartyQuickMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage sendMessage)
    {
        if (CheckRepeatChatMessage(chatMessageKind, sendMessage))
            return;

        if (!_partyChattingChecker.CheckValidSendChat())
            return;

        for (int i = 0; i < _sendMessageObservers.Count; i++) {
            _sendMessageObservers[i].OnChatSendMessage(chatMessageKind, sendMessage);
        }
    }

    void SendRepeatWarningMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage chatMessage)
    {
        ChatMessage sendMessage = null;
        if (chatMessageKind == ChatDefinition.ChatMessageKind.WhisperChat) {
            sendMessage = new ChatWhisperMessage();
        } else {
            sendMessage = new ChatMessage();
        }

        sendMessage.CopyChatBaseMessage(chatMessage);
        sendMessage.isSelfNotify = true;
        sendMessage.sendType = chatMessageKind;
        sendMessage.timeStamp = TimeUtil.GetTimeStamp();
        if (chatMessageKind == ChatDefinition.ChatMessageKind.WhisperChat) {
            ChatWhisperMessage sendWhisperMessage = sendMessage as ChatWhisperMessage;
            ChatWhisperMessage whisperChat = chatMessage as ChatWhisperMessage;
            sendWhisperMessage.CopyWhisperMessage(whisperChat);
        }
        sendMessage.msgIdx = (int)ChatNoticeMessageKey.RepeatSameWordWriteWarningMessage;

        NotifyChatReceiveMessage(-1, sendMessage);
    }

    bool CheckRepeatChatMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage sendMessage)
    {
        if(!sendMessage.prm.ContainsKey("msg")) {
            _repeatSendNotify.ResetData();
            return false;
        }

        string sendMsg = sendMessage.prm["msg"];
        if(string.IsNullOrEmpty(sendMsg)) {
            return false;
        }

        if (_repeatSendNotify.messageKind == ChatDefinition.ChatMessageKind.None) {
            _repeatSendNotify.messageKind = chatMessageKind;
            _repeatSendNotify.sendTime = TimeUtil.GetTimeStamp();
            _repeatSendNotify.message = sendMsg;
            return false;
        } else {
            if(_repeatSendNotify.messageKind == chatMessageKind && _repeatSendNotify.message == sendMsg) {
                //long curTime = TimeUtil.GetTimeStamp();
                //long gapTime = (curTime - _repeatSendNotify.sendTime) / 1000;

                //Debug.Log(string.Format("_repeatSendNotify.message == sendMsg : {0}, gapTime : {1}, _repeatSendNotify.repeatCount : {2}", 
                //    sendMsg, gapTime, _repeatSendNotify.repeatCount));

                //if (gapTime > 5) {
                //    _repeatSendNotify.sendTime = TimeUtil.GetTimeStamp();
                //    _repeatSendNotify.repeatCount = 0;
                //    return false;
                //}

                if (_repeatSendNotify.repeatCount >= 2) {
                    SendRepeatWarningMessage(chatMessageKind, sendMessage);
                    //_repeatSendNotify.sendTime = TimeUtil.GetTimeStamp();
                    //_repeatSendNotify.repeatCount = 0;
                    return true;
                }

                _repeatSendNotify.repeatCount++;
            } else {
                _repeatSendNotify.messageKind = chatMessageKind;
                _repeatSendNotify.message = sendMsg;
                _repeatSendNotify.sendTime = TimeUtil.GetTimeStamp();
                _repeatSendNotify.repeatCount = 0;
                return false;
            }
        }

        return false;
    }

	public void ReleaseChatSendMessageOb()
	{
		_sendMessageObservers.Clear ();
	}

    //void CheckTempChangeNickname(string newNickName)
    //{
    //    TextModel _textModel = _data.Text;
    //    SystemTextModel _systemTextModel = GameSystem.Instance.SystemData.SystemText;

    //    int MinByte = 2;
    //    int MaxByte = 10;

    //    int wordByteLength = _textModel.GetWordByteLength(newNickName);
    //    Debug.Log(string.Format("CheckTempChangeNickname newNickName : {0}, wordByteLength : {1}", newNickName, wordByteLength));

    //    bool isValidateWord = _textModel.IsValidateWord(newNickName);
    //    bool isValidateLength = _textModel.IsValidateWordLength(newNickName, MinByte, MaxByte);
    //    if (!isValidateWord || !isValidateLength) {
    //        string popupText;
    //        if (!isValidateLength) {
    //            popupText = string.Format(_textModel.GetText(TextKey.UI_Text_22), MinByte, MaxByte);
    //        } else {
    //            popupText = _systemTextModel.GetText(SystemTextKey.Popup_Name_Error);
    //        }

    //        UIFloatingMessagePopup.Show(popupText);
    //        return;
    //    }
    //}

	void SendChattingMessage(string inputMessage)
	{
		if(string.IsNullOrEmpty(inputMessage))
			return;

        if(!CheckValidChatting()) {
            int chatProhibitionLevel = _data.Sheet.SheetGameConfig[1].ChatProhibitionLevel;
            UIFloatingMessagePopup.Show(string.Format(GameSystem.Instance.Data.Text.GetText(TextKey.CT_Text02), chatProhibitionLevel));
            return;
        }

        TextModel textModel = _data.Text;
		inputMessage = textModel.GetBannedFilterWords (inputMessage, BannedWordType.sentence);

		ChatMessage chattingMessage = new ChatMessage();
		chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

		MissionBaseData currentMission = _data.MissionListManager.CurrentMission;

        switch (_curChatMessageKind) {
            case ChatDefinition.ChatMessageKind.ChannelChat:
                chattingMessage.msgIdx = (int)ChatNoticeMessageKey.NormalChatting;
                chattingMessage.prm.Add("user", _data.User.Nickname);
                chattingMessage.prm.Add("msg", inputMessage);
                break;
            case ChatDefinition.ChatMessageKind.GuildChat:
                if (currentMission == null) {
                    chattingMessage.msgIdx = (int)ChatNoticeMessageKey.UserCompanyChatting;
                    chattingMessage.prm.Add("user", _data.User.Nickname);
                    chattingMessage.prm.Add("msg", inputMessage);
                } else {
                    chattingMessage.msgIdx = (int)ChatNoticeMessageKey.MissionChatting;
                    chattingMessage.prm.Add("user", _data.User.Nickname);
                    chattingMessage.prm.Add("msg", inputMessage);
                }

                chattingMessage.partyType = _curChatGuildInfoUI.party_type;
                chattingMessage.partyNum = _curChatGuildInfoUI.party_num;
                break;
        }

        NotifyChatSendMessage (_curChatMessageKind, chattingMessage);

        if(_curChatMessageKind == ChatDefinition.ChatMessageKind.ChannelChat) {
            _uiMultiChat.UIMultiChatPopup.ResetChannelInputMessage();
        } else {
            _uiMultiChat.UIMultiChatPopup.ResetInputMessage();
        }
	}

    //public void SendPartyChattingMessageOld(string inputMessage, int role)
    //{
    //    if (string.IsNullOrEmpty(inputMessage))
    //        return;

    //    Debug.Log(string.Format("SendPartyChattingMessage role : {0}", role));

    //    TextModel textModel = _data.Text;
    //    inputMessage = textModel.GetBannedFilterWords(inputMessage);

    //    ChatMessage chattingMessage = new ChatMessage();
    //    chattingMessage.userID = _data.User.userData.userId;

    //    chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID));

    //    chattingMessage.msgIdx = (int)ChatNoticeMessageKey.PartyChatting;
    //    chattingMessage.prm.Add("user", _data.User.Nickname);
    //    chattingMessage.prm.Add("msg", inputMessage);
    //    chattingMessage.prm.Add("roleIndex", role.ToString());

    //    chattingMessage.partyType = (int)ChatPartyType.ChatParty;
    //    chattingMessage.partyNum = _data.PartySystem.CurStartMissionInfo.party.partyId;

    //    NotifyChatSendMessage(ChatDefinition.ChatMessageKind.PartyChat, chattingMessage);
    //}

    public void SendPartyChattingMessage(string inputMessage)
    {
        if (string.IsNullOrEmpty(inputMessage))
            return;

        TextModel textModel = _data.Text;
        inputMessage = textModel.GetBannedFilterWords(inputMessage, BannedWordType.sentence);

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.PartyChatting;
        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("msg", inputMessage);

        chattingMessage.partyType = (int)ChatPartyType.ChatParty;
        chattingMessage.partyNum = _data.PartyMain.CurMission.missionId;

        NotifyChatSendMessage(ChatDefinition.ChatMessageKind.PartyChat, chattingMessage);
    }

    public void SendPartyChattingQuickMessage(int quickChatIndex)
    {
        //TextModel textModel = _data.Text;
        //inputMessage = textModel.GetBannedFilterWords(inputMessage);

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.PartyQuickChat;

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.PartyQuickChatting;
        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("partyquickindex", quickChatIndex.ToString());

        chattingMessage.partyType = (int)ChatPartyType.ChatParty;
        chattingMessage.partyNum = _data.PartyMain.CurMission.missionId;

        NotifyChatSendPartyQuickMessage(ChatDefinition.ChatMessageKind.PartyChat, chattingMessage);
    }

    void SendWhisperChatMessage(string inputMessage)
	{
		if(string.IsNullOrEmpty(inputMessage))
			return;

		if (_chattingModel.WhisperInformation.WhisperTargetUserInfo == null || _uiMultiChat.IsRequestWhisperServerLog) {
			_uiMultiChat.UIMultiChatPopup.ResetWhisperInputMessage ();
			return;
		}

		TextModel textModel = _data.Text;

		inputMessage = textModel.GetBannedFilterWords (inputMessage, BannedWordType.sentence);

		ChatWhisperMessage chattingMessage = new ChatWhisperMessage();
		chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);
        chattingMessage.sendUserName = _data.User.userData.nickname;
		chattingMessage.targetUserID = _chattingModel.WhisperInformation.WhisperTargetUserInfo.targetUserID;
        chattingMessage.targetConnectID = _chattingModel.WhisperInformation.WhisperTargetUserInfo.targetConnectID;
        chattingMessage.targetUserName = _chattingModel.WhisperInformation.WhisperTargetUserInfo.targetNickname;
        chattingMessage.whisperKind = _chattingModel.WhisperInformation.WhisperTargetUserInfo.whisperKind;
        chattingMessage.companyID = _chattingModel.WhisperInformation.WhisperTargetUserInfo.companyID;

        chattingMessage.prm.Add("msg", inputMessage);

        if (_chattingModel.WhisperInformation.WhisperTargetUserInfo.whisperKind == (int)ChatDefinition.WhisperKind.Friend) {
            chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.WhisperFriendChat;
        } else {
            chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.WhisperGuildChat;
        }

        NotifyChatSendMessage (ChatDefinition.ChatMessageKind.WhisperChat, chattingMessage);

		_uiMultiChat.UIMultiChatPopup.ResetWhisperInputMessage ();
	}

    public ChatPartMessageInfo GetUserInfoChatPartInfo(long userID, long connectId)
	{
		ChatPartMessageInfo retPartMessageInfo = new ChatPartMessageInfo ();
		retPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.UserInfoType;
		retPartMessageInfo.partValues = new string[2];
		retPartMessageInfo.partValues [0] = userID.ToString ();
        retPartMessageInfo.partValues[1] = connectId.ToString();

        return retPartMessageInfo;
	}

	public void SetChatGuildList(GuildJoinedInfo[] guildJoinedInfos)
	{
		if (guildJoinedInfos == null || guildJoinedInfos.Length == 0)
			return;

        _chatGuildGroupInfos.Clear();

        for (int i = 0; i < guildJoinedInfos.Length; i++) {
			#if _CHATTING_LOG
			Debug.Log (string.Format ("!!!====== SetChatGuildList guildId : {0}, notice : {1}, createTime : {2}, lastNotViewTime : {3}",
                 guildJoinedInfos[i].guildId, guildJoinedInfos[i].notice, guildJoinedInfos[i].createTime, guildJoinedInfos[i].lastChatViewTime));
			#endif
			if (_chatGuildGroupInfos.ContainsKey (guildJoinedInfos[i].guildId))
				continue;

            ChatGuildJoinedInfo inputGuildInfo = new ChatGuildJoinedInfo();
            inputGuildInfo.party_type = (int)ChatPartyType.ChatGuild;
            inputGuildInfo.party_num = guildJoinedInfos[i].guildId;
            inputGuildInfo.guildName = guildJoinedInfos[i].guildName;
            inputGuildInfo.notice = guildJoinedInfos[i].notice;
            inputGuildInfo.userIdList.Clear();
            if(guildJoinedInfos[i].userIdList != null && guildJoinedInfos[i].userIdList.Length > 0) {
                for(int j = 0;j< guildJoinedInfos[i].userIdList.Length;j++) {
                    inputGuildInfo.userIdList.Add(guildJoinedInfos[i].userIdList[j]);
                }
            }

            inputGuildInfo.playerIdList.Clear();

            if (guildJoinedInfos[i].playerIdList == null || guildJoinedInfos[i].playerIdList.Length == 0) {
                for (int j = 0; j < guildJoinedInfos[i].userIdList.Length; j++) {
                    inputGuildInfo.playerIdList.Add(guildJoinedInfos[i].userIdList[j]);
                }
            } else {
                for (int j = 0; j < guildJoinedInfos[i].playerIdList.Length; j++) {
                    if(guildJoinedInfos[i].playerIdList[j] > 0) {
                        inputGuildInfo.playerIdList.Add(guildJoinedInfos[i].playerIdList[j]);
                    } else {
                        inputGuildInfo.playerIdList.Add(guildJoinedInfos[i].userIdList[j]);
                    }
                }
            }

            if (string.IsNullOrEmpty(inputGuildInfo.notice)){
                inputGuildInfo.notice = _data.Text.GetText (TextKey.Guild_Announcement_Default_Text);	
			}

            //inputGuildInfo.guildNoticeMessages.Add (ChatHelper.GetMultiChatTextInfoList (inputGuildInfo.notice));

            AddChatGuildGropInfo(inputGuildInfo.party_num, inputGuildInfo);

            _chattingModel.AddGuildLastConfirmTimeInfo(guildJoinedInfos[i].guildId, guildJoinedInfos[i].createTime, guildJoinedInfos[i].lastChatViewTime);
		}
	}

    public void AddChatGuildGropInfo(long partyNum, ChatGuildJoinedInfo inputGuildInfo)
    {
        _chatGuildGroupInfos.Add(partyNum, inputGuildInfo);
    }

    public void SetChatPartyList(PartyJoinedInfo[] joinedPartys)
    {
        _chatPartyJoinedInfos.Clear();

        if (joinedPartys == null || joinedPartys.Length == 0)
            return;

        for (int i = 0; i < joinedPartys.Length; i++) {
#if _CHATTING_LOG
            Debug.Log(string.Format("!!!====== SetChatPartyList partyId : {0}, createTime : {1}, lastNotViewTime : {2}",
                joinedPartys[i].partyId, joinedPartys[i].createTime, joinedPartys[i].lastChatViewTime));
#endif
            if (_chatPartyJoinedInfos.ContainsKey(joinedPartys[i].partyId))
                continue;

            ChatPartyJoinedInfo inputPartyInfo = new ChatPartyJoinedInfo();
            inputPartyInfo.party_type = (int)ChatPartyType.ChatParty;
            inputPartyInfo.party_num = joinedPartys[i].partyId;
            inputPartyInfo.partyName = joinedPartys[i].partyName;
            inputPartyInfo.userIdList.Clear();
            if (joinedPartys[i].userIdList != null && joinedPartys[i].userIdList.Length > 0) {
                for (int j = 0; j < joinedPartys[i].userIdList.Length; j++) {
                    inputPartyInfo.userIdList.Add(joinedPartys[i].userIdList[j]);
                }
            }

            AddChatPartyJoinInfo(inputPartyInfo);

            PartyChatInformation partyChatInfo = _partyChatModel.AddPartyChatInformation(joinedPartys[i].partyId);
            partyChatInfo.PartyJoinTimeStamp = joinedPartys[i].createTime;
            partyChatInfo.LastConfirmTimeStamp = joinedPartys[i].lastChatViewTime;
        }
    }

    public void AddChatPartyJoinInfo(ChatPartyJoinedInfo inputPartyInfo)
    {
        _chatPartyJoinedInfos.Add(inputPartyInfo.party_num, inputPartyInfo);

        _uiMultiChat.AddPartyChattingButton(inputPartyInfo);
    }

    public void RemoveChatPartyJoinInfo(long partyId)
    {
        if (!_chatPartyJoinedInfos.ContainsKey(partyId))
            return;

        _chatPartyJoinedInfos.Remove(partyId);

        _uiMultiChat.RemovePartyChattingButton((int)ChatPartyType.ChatParty, partyId);
    }

    public ChatGuildJoinedInfo GetChatGuildInfo(long partyNum)
    {
        if(_chatGuildGroupInfos.ContainsKey(partyNum))
            return _chatGuildGroupInfos[partyNum];

        return null;
    }

    public ChatPartyJoinedInfo GetChatPartyInfo(long partyNum)
    {
        if (_chatPartyJoinedInfos.ContainsKey(partyNum))
            return _chatPartyJoinedInfos[partyNum];

        return null;
    }

    public List<ChatGuildJoinedInfo> GetChatGuildInfoList()
    {
        return _chatGuildGroupInfos.Values.ToList();
    }

    public List<ChatPartyJoinedInfo> GetChatPartyInfoList()
    {
        return _chatPartyJoinedInfos.Values.ToList();
    }

    public List<ChatGuildJoinedInfo> GetChatGuildJoinInfos()
    {
        List<ChatGuildJoinedInfo> retValue = new List<ChatGuildJoinedInfo>();

        List<long> guildIdKeys = _chatGuildGroupInfos.Keys.ToList();
        for(int i = 0;i< guildIdKeys.Count;i++) {
            retValue.Add(_chatGuildGroupInfos[guildIdKeys[i]]);
        }

        return retValue;
    }

    public bool ContainsGuildUser(long userID)
    {
        List<ChatGuildJoinedInfo> chatGuildInfo = GetChatGuildJoinInfos();

        if(chatGuildInfo == null || chatGuildInfo.Count == 0)
            return false;

        foreach(var each in chatGuildInfo)
        {
            if(each.userIdList == null || each.userIdList.Count == 0)
                continue;

            if(each.userIdList.Contains(userID))
                return true;
        }
        return false;
    }

    public bool CheckValidGuildUserId(long guildId, long userId)
    {
        if (_chatGuildGroupInfos.ContainsKey(guildId)) {
            ChatGuildJoinedInfo chatGuildInfo = _chatGuildGroupInfos[guildId];
            if (chatGuildInfo.userIdList.Contains(userId))
                return true;
        }

        return false;
    }
    public void CheckGuildUserID(long guildId, long userID, long playerId)
    {
        if(_chatGuildGroupInfos.ContainsKey(guildId)) {
            ChatGuildJoinedInfo chatGuildInfo = _chatGuildGroupInfos[guildId];

            if(chatGuildInfo.userIdList == null || chatGuildInfo.userIdList.Count == 0) {
                if(chatGuildInfo.userIdList == null)
                    chatGuildInfo.userIdList = new List<long>();
                chatGuildInfo.userIdList.Add(userID);
            } else {
                if (!chatGuildInfo.userIdList.Contains(userID)) {
                    chatGuildInfo.userIdList.Add(userID);
                }
            }

            if(playerId <= 0) {
                playerId = userID;
            }

            if (chatGuildInfo.playerIdList == null || chatGuildInfo.playerIdList.Count == 0) {
                if (chatGuildInfo.playerIdList == null)
                    chatGuildInfo.playerIdList = new List<long>();
                chatGuildInfo.playerIdList.Add(playerId);
            } else {
                if (!chatGuildInfo.playerIdList.Contains(playerId)) {
                    chatGuildInfo.playerIdList.Add(playerId);
                }
            }
        }
    }

    //public void RefreshGuildUserIDList(long guildId, CompanyMember[] guildMembers)
    //{
    //    if (_chatGuildGroupInfos.ContainsKey(guildId)) {
    //        ChatGuildJoinedInfo chatGuildInfo = _chatGuildGroupInfos[guildId];
    //        chatGuildInfo.userIdList.Clear();
    //        if (guildMembers != null && guildMembers.Length > 0) {
    //            for(int i = 0;i< guildMembers.Length;i++) {
    //                chatGuildInfo.userIdList.Add(guildMembers[i].userId);
    //            }
    //        }
    //    }
    //}

    public void RefreshGuildUserIDList(long guildId, GuildMember[] members)
    {
        if (_chatGuildGroupInfos.ContainsKey(guildId))
        {
            ChatGuildJoinedInfo chatGuildInfo = _chatGuildGroupInfos[guildId];
            chatGuildInfo.userIdList.Clear();
            chatGuildInfo.playerIdList.Clear();
            if (members != null && members.Length > 0)
            {
                for (int i = 0; i < members.Length; i++)
                {
                    chatGuildInfo.userIdList.Add(members[i].userId);
                    chatGuildInfo.playerIdList.Add(members[i].playerId <= 0 ? members[i].userId : members[i].playerId);
                }
            }
        }
    }

    public void SetCurChatGuildInfoUI(long partyNum)
	{
        _curChatGuildInfoUI = GetChatGuildInfo( partyNum);
	}

	public void SetDisableCurChatGuildInfo()
	{
        _curChatGuildInfoUI = null;
	}

    public void DisconnectCurPartyInfo()
    {
        _isPartyGroupConnected = false;
        _chatSocketManager.SendConnectCurPartyPacket(-1, null);
    }

	public void ResetChannelChatMessage()
	{
		if (_uiMultiChat.CurSelectChattingInput != null && _uiMultiChat.CurSelectChattingInput.ChatMessageKind == ChatDefinition.ChatMessageKind.ChannelChat) {
            _uiMultiChat.SetChannelChat(_chattingModel.ChatGroupNum);
        }

        //_uiMultiChat.ChangeChannelChatting (_chattingModel.ChatGroupNum);
	}

	public void ReleaseAllChatInfo()
	{
        _isChatGuildConnected = false;
        _isPartyGroupConnected = false;

        SetDisableCurChatGuildInfo();

		if(_chattingModel != null)
			_chattingModel.ReleaseAllChatMessageInfo ();

        if(_partyChatModel != null)
            _partyChatModel.ReleasePartyChatMessageInfo();
		
		_uiMultiChat.ReleaseAllUIChatInfo ();

		if (IsChattingPopup) {
			_uiMultiChat.CloseChattingPopup ();
		}

        if(_onReleaseAllChatInfo != null)
            _onReleaseAllChatInfo();

        if(_onReleaseQuickPartyChat != null)
            _onReleaseQuickPartyChat();
    }

	public void AddGuildChatGroup(long partyNum, string guildName, string guildNotice)
	{
		if (_chatGuildGroupInfos.Count > 0 && _chatGuildGroupInfos.ContainsKey (partyNum))
			return;

        long joinTimeStamp = TimeUtil.GetTimeStamp();
        _chattingModel.AddGuildLastConfirmTimeInfo(partyNum, joinTimeStamp, joinTimeStamp);

        ChatGuildJoinedInfo inputGuildInfo = new ChatGuildJoinedInfo();
        inputGuildInfo.party_type = (int)ChatPartyType.ChatGuild;
        inputGuildInfo.party_num = partyNum;

        inputGuildInfo.guildName = guildName;
        inputGuildInfo.notice = guildNotice;
		if(string.IsNullOrEmpty(inputGuildInfo.notice)){
            inputGuildInfo.notice = _data.Text.GetText (TextKey.Guild_Announcement_Default_Text);	
		}

        //inputGuildInfo.guildNoticeMessages.Add (ChatHelper.GetMultiChatTextInfoList (inputGuildInfo.notice));

        AddChatGuildGropInfo(partyNum, inputGuildInfo);

        _uiMultiChat.AddGuildChattingButton(inputGuildInfo);

        _addGuildBaseInfo = inputGuildInfo;
		_chatSocketManager.CheckCurChatPartyGroup(OnSuccessGuildUpdate);

	}

    public void AddGuildChatUserId(long partyNum, long userId, long playerId)
    {
        if (_chatGuildGroupInfos.ContainsKey(partyNum)) {
            ChatGuildJoinedInfo guildJoinedInfo = _chatGuildGroupInfos[partyNum];
            if (!guildJoinedInfo.userIdList.Contains(userId)) {
                guildJoinedInfo.userIdList.Add(userId);
            }

            if(playerId <= 0) {
                if (!guildJoinedInfo.playerIdList.Contains(userId)) {
                    guildJoinedInfo.playerIdList.Add(userId);
                }
            } else {
                if (!guildJoinedInfo.playerIdList.Contains(playerId)) {
                    guildJoinedInfo.playerIdList.Add(playerId);
                }
            }
        }
    }

    public void RemoveGuildChatGroup(long partyNum)
	{
		_chattingModel.RemoveGuildMessageInfo(partyNum);
		_uiMultiChat.RemovePartyChattingButton ((int)ChatPartyType.ChatGuild, partyNum);

        if(_chatGuildGroupInfos.ContainsKey(partyNum)) {
            _chatGuildGroupInfos.Remove(partyNum);
        }

        _uiMultiChat.UIWhisperTargetPopup.CheckRemoveGuildInfo(partyNum);

        _curSelectGuildInfo = null;
        _addGuildBaseInfo = null;
		_chatSocketManager.CheckCurChatPartyGroup(OnSuccessGuildUpdate);

        if(_chattingModel.WhisperInformation.WhisperTargetUserInfo != null) {
            if (!ContainsGuildUser(_chattingModel.WhisperInformation.WhisperTargetUserInfo.targetUserID)) {
                _chattingModel.WhisperInformation.WhisperTargetUserInfo = null;
                _uiMultiChat.UIMultiChatPopup.SetWhisperTargetText();
            }
        }
	}

    public void SetChannelNoticeInfo(NoticeListInfo[] noticeInfos)
	{
        _chattingModel.ChatCurNoticeListInfos.Clear();

        // TempCode
        //noticeInfos = null;
        //if (noticeInfos == null || noticeInfos.Length == 0) {
        //    noticeInfos = new NoticeListInfo[1];

        //    for (int i = 0; i < noticeInfos.Length; i++) {
        //        NoticeListInfo inputNoticeInfo = new NoticeListInfo();
        //        inputNoticeInfo.beginAt = DateTime.Now.ToString();
        //        inputNoticeInfo.finishAt = DateTime.Now.AddDays(1).ToString();
        //        //Debug.Log(string.Format("beginAt : {0}, finishAt : {1}", inputNoticeInfo.beginAt, inputNoticeInfo.finishAt));
        //        inputNoticeInfo.color = "#FFFFFFFF";
        //        inputNoticeInfo.market = -1;
        //        NoticeTextInfo[] textList = new NoticeTextInfo[10];
        //        for (int j = 0; j < textList.Length; j++) {
        //            NoticeTextInfo inputText = new NoticeTextInfo();
        //            if (j == 0) {
        //                inputText.id = 1;
        //                //string inputStr = string.Format("메시지 텍스트 채팅 메시지 테스트 메시지 테스트 메시지 테스트 메시지 테스트 메시지 테스트 메시지 테스트 메시지 테스트 ");
        //                string inputStr = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");

        //                inputText.text = inputStr;
        //            } else if (j == 1) {
        //                inputText.id = 2;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 2) {
        //                inputText.id = 3;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 3) {
        //                inputText.id = 4;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 4) {
        //                inputText.id = 5;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 5) {
        //                inputText.id = 6;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 6) {
        //                inputText.id = 7;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 7) {
        //                inputText.id = 8;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else if (j == 8) {
        //                inputText.id = 13;
        //                //inputText.text = string.Format("Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test Chat Message Test");
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            } else {
        //                inputText.id = 10;
        //                inputText.text = string.Format("แพทช์จะเริ่มอัปเดตในวันที่ 1 เม.ย. เวลา 23:00 น. (เวลาไทย)#กรุณาตรวจสอบประกาศเพื่อดูรายละเอียดเพิ่มเติม");
        //            }

        //            textList[j] = inputText;
        //        }
        //        inputNoticeInfo.textList = textList;

        //        noticeInfos[i] = inputNoticeInfo;
        //    }
        //}
        /////

        if (noticeInfos == null || noticeInfos.Length == 0)
			return;

		for (int i = 0; i < noticeInfos.Length; i++) {
            if(noticeInfos[i].textList != null && noticeInfos[i].textList.Length > 0) {
                for (int j = 0; j < noticeInfos[i].textList.Length; j++) {
                    if(!ChatEventManager.CheckValidChatMarket(noticeInfos[i].market)) continue;

                    if(noticeInfos[i].textList[j].id == (int)GameSystem.Instance.SystemData.SystemOption.Language) {
                        ChatNoticeCurInfo inputNoticeCurInfo = new ChatNoticeCurInfo();
                        inputNoticeCurInfo.beginTime = TimeUtil.KstTime.GetKstToKstDateTime(noticeInfos[i].beginAt);
                        inputNoticeCurInfo.endTime = TimeUtil.KstTime.GetKstToKstDateTime(noticeInfos[i].finishAt);
                        inputNoticeCurInfo.color = ChatEventManager.GetChatColor(noticeInfos[i].color);
                        inputNoticeCurInfo.noticeText = noticeInfos[i].textList[j].text;
                        _chattingModel.ChatCurNoticeListInfos.Add(inputNoticeCurInfo);
                        break;
                    }
                }
            }
        }
	}

	public void SetEnableScrollTouch(bool isEnable)
	{
		_uiMultiChat.UIMultiChatPopup.ObjScrollManager.IsTouch = isEnable;
#if _OLD_GUILD_CHAT
        _uiMultiChat.UIMultiChatPopup.ChatGuildMessageManager.SetTouchEnable(isEnable);
#endif
    }

    public void SetDisableWhisperInfo()
	{
		_uiMultiChat.UIMultiChatPopup.SetDisableUIWhisperChat ();
		_chattingModel.WhisperInformation.WhisperTargetUserInfo = null;
	}

	public int GetJoinedGuildCount()
	{
        return _chatGuildGroupInfos.Count;
    }

    public void SetChatObtainCompanyShopItemInfo(RewardData obtainRewardData, long partyNum, ChatDefinition.ChatRewardItemPlace rewardItemPlace)
    {
        //TextModel textModel = _data.Text;
        //string obtainShopName = "";
        //int itemPlaceIndex = 0;
        //switch (rewardItemPlace) {
        //    case ChatDefinition.ChatRewardItemPlace.CompanySupplyBox:
        //        obtainShopName = textModel.GetText(TextKey.Company_RivalryStore);
        //        itemPlaceIndex = (int)ChatDefinition.ChatRewardItemPlace.CompanySupplyBox;
        //        break;
        //    case ChatDefinition.ChatRewardItemPlace.CompanyTrophyStore:
        //        obtainShopName = textModel.GetText(TextKey.Company_BasicStore);
        //        itemPlaceIndex = (int)ChatDefinition.ChatRewardItemPlace.CompanyTrophyStore;
        //        break;
        //    case ChatDefinition.ChatRewardItemPlace.CompanySpecialStore:
        //        obtainShopName = textModel.GetText(TextKey.Company_SpecialStore);
        //        itemPlaceIndex = (int)ChatDefinition.ChatRewardItemPlace.CompanySpecialStore;
        //        break;
        //}

        //if(obtainRewardData.equipment != null && obtainRewardData.equipment.Length > 0) {
        //    for(int i = 0;i< obtainRewardData.equipment.Length;i++) {
        //        SendCompanyObtainEquipmentItemInfo(obtainRewardData.equipment[i], partyNum, obtainShopName, itemPlaceIndex);
        //    }
        //}

        //if (obtainRewardData.heroPOS != null && obtainRewardData.heroPOS.Length > 0) {
        //    for (int i = 0; i < obtainRewardData.heroPOS.Length; i++) {
        //        SendCompanyObtainPOSItemInfo(obtainRewardData.heroPOS[i], partyNum, obtainShopName, itemPlaceIndex);
        //    }
        //}
    }

    public string GetItemShopName(ChatDefinition.ChatRewardItemPlace rewardItemPlace)
    {
        TextModel textModel = _data.Text;
        string retValue = "";

        switch (rewardItemPlace) {
            case ChatDefinition.ChatRewardItemPlace.CompanySupplyBox:
                retValue = textModel.GetText(TextKey.Company_RivalryStore);
                break;
            case ChatDefinition.ChatRewardItemPlace.CompanyTrophyStore:
                retValue = textModel.GetText(TextKey.Company_BasicStore);
                break;
            case ChatDefinition.ChatRewardItemPlace.CompanySpecialStore:
                retValue = textModel.GetText(TextKey.Company_SpecialStore);
                break;
        }

        return retValue;
    }

    public void SendObtainEquipmentInfo(EquipmentData equipment)
    {
#if _CHATTING_LOG
        Debug.Log(string.Format("SendObtainEquipmentInfo equipment id : {0}", equipment.id));
#endif
        var grade = EquipmentModel.GetItemGrade(_data, equipment.index);
        var rarity = EquipmentModel.GetItemRarity(_data, equipment.index);
        if (grade < _checkEquipmentGrade || rarity < _checkEquipmentRarity)
            return;

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.ObtainEquipmentItemChat;
        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.UniqItemAcquisition;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetEquipmentDataChatPartInfo(equipment));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("grade", grade.ToString());
        chattingMessage.prm.Add("itemIndex", equipment.index.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);
    }

    public void SendEnhanceEquipmentInfo(EquipmentData equipment)
    {
        Debug.Log(string.Format("!!!!!!!! SendObtainEquipmentInfo equipment id : {0}", equipment.id));
#if _CHATTING_LOG
        Debug.Log(string.Format("SendObtainEquipmentInfo equipment id : {0}", equipment.id));
#endif

        //if (EquipmentModel.GetItemRank(_data, equipment) < _checkEquipmentRank || equipment.grade < _checkEquipmentGrade)
        //    return;

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.EnhanceEquipmentItemChat;
        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.UniqItemReinforcement;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetEquipmentDataChatPartInfo(equipment));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("grade", EquipmentModel.GetItemGrade(_data, equipment.index).ToString());
        chattingMessage.prm.Add("itemIndex", equipment.index.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);

        //List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();
        //for (int i = 0; i < chatGuildInfos.Count; i++) {
        //    SendCompanyEnhanceEquipmentInfo(equipment, chatGuildInfos[i].party_num);
        //}
    }

    void SendCompanyEnhanceEquipmentInfo(EquipmentData equipment, long companyID)
    {
#if _CHATTING_LOG
        Debug.Log(string.Format("SendObtainEquipmentInfo equipment id : {0}", equipment.id));
#endif

        //if (EquipmentModel.GetItemRank(_data, equipment) < _checkEquipmentRank || equipment.grade < _checkEquipmentGrade)
        //    return;

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.EnhanceEquipmentItemChat;
        //chattingMessage.msgIdx = (int)ChatNoticeMessageKey.CompanyMemberUniqItemReinforcement;

        chattingMessage.partyType = (int)ChatPartyType.ChatGuild;
        chattingMessage.partyNum = companyID;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetEquipmentDataChatPartInfo(equipment));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("grade", EquipmentModel.GetItemGrade(_data, equipment.index).ToString());
        chattingMessage.prm.Add("itemIndex", equipment.index.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.GuildChat, chattingMessage);
    }

    public void SendObtainCardItemtInfo(int itemIndex, int rewardGrade, CardType cardType)
    {
        if (rewardGrade < (int)CardGrade.SSR) {
            return;
        }

        ChatSendCardObtainInfo inputCardObtainInfo = new ChatSendCardObtainInfo();
        inputCardObtainInfo.itemIndex = itemIndex;
        inputCardObtainInfo.rewardGrade = rewardGrade;
        inputCardObtainInfo.cardType = cardType;
        _sendCardObatinInfos.Enqueue(inputCardObtainInfo);

        if (!_chatSocketManager.EventTimer.ExistTimerDataByID(ChattingSocketManager.sendObtainCardDelayTimeID)) {
            ExecuteSendObtainCardItem();
        }
        //ChatMessage chattingMessage = new ChatMessage();
        //chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.ObtainCardItemChat;

        //if (cardType == CardType.HeroCard) {
        //    chattingMessage.msgIdx = (int)ChatNoticeMessageKey.SkillCardAcquisition;
        //} else if (cardType == CardType.FuryCard) {
        //    chattingMessage.msgIdx = (int)ChatNoticeMessageKey.FuryCardAcquisition;
        //}

        //chattingMessage.userID = _data.User.userData.userId;

        //chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID));
        //chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetCardIndexChatPartInfo(itemIndex));

        //chattingMessage.prm.Add("user", _data.User.Nickname);
        //chattingMessage.prm.Add("itemIndex", itemIndex.ToString());
        //chattingMessage.prm.Add("tier", ChatHelper.GetCardGrade((CardGrade)rewardGrade));

        //NotifyChatSendMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);

        //List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();

        //for (int i = 0; i < chatGuildInfos.Count; i++) {
        //    SendCompanyObtainCardItemtInfo(itemIndex, chatGuildInfos[i].party_num);
        //}
    }

    void ExecuteSendObtainCardItem()
    {
        if (_sendCardObatinInfos.Count == 0)
            return;

        ChatSendCardObtainInfo cardObtainInfo = _sendCardObatinInfos.Dequeue();

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.ObtainCardItemChat;

        if (cardObtainInfo.cardType == CardType.HeroCard) {
            chattingMessage.msgIdx = (int)ChatNoticeMessageKey.SkillCardAcquisition;
        } else if (cardObtainInfo.cardType == CardType.FuryCard) {
            chattingMessage.msgIdx = (int)ChatNoticeMessageKey.FuryCardAcquisition;
        }

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetCardIndexChatPartInfo(cardObtainInfo.itemIndex));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("itemIndex", cardObtainInfo.itemIndex.ToString());
        chattingMessage.prm.Add("tier", ChatHelper.GetCardGrade((CardGrade)cardObtainInfo.rewardGrade));

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);

        //List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();

        //for (int i = 0; i < chatGuildInfos.Count; i++) {
        //    SendCompanyObtainCardItemtInfo(cardObtainInfo.itemIndex, chatGuildInfos[i].party_num);
        //}

        _chatSocketManager.EventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnSendObtainCardDelayTime, 1f, null, ChattingSocketManager.sendObtainCardDelayTimeID);
    }

    public void SendAchieveHeroLevel(HeroModel hero)
    {
        ChatSendAchieveHeroLevelInfo inputHeroLevelInfo = new ChatSendAchieveHeroLevelInfo();
        inputHeroLevelInfo.hero = hero;

        _sendAchieveHeroLevels.Enqueue(inputHeroLevelInfo);
        if (!_chatSocketManager.EventTimer.ExistTimerDataByID(ChattingSocketManager.sendAchieveHeroLevelDelayTimeID)) {
            ExecuteSendAchieveHeroLevel();
        }

    }

    void ExecuteSendAchieveHeroLevel()
    {
        if (_sendAchieveHeroLevels.Count == 0)
            return;

        ChatSendAchieveHeroLevelInfo achieveHeroLevel = _sendAchieveHeroLevels.Dequeue();

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.HeroLevelUpChat;

        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.AchieveHeroLevel;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("heronametext", achieveHeroLevel.hero.Info.NameText);
        chattingMessage.prm.Add("level", achieveHeroLevel.hero.Info.Level.ToString());
        chattingMessage.prm.Add("heroId", achieveHeroLevel.hero.ID.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);

        _chatSocketManager.EventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnSendAchieveHeroLevelDelayTime, 1f, null, ChattingSocketManager.sendAchieveHeroLevelDelayTimeID);
    }

    public void SendSuccessRefineryFloor(int floor)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.SuccessRefineryFloorChat;

        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.SuccessRefineryFloor;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("floor", floor.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);
    }

    public void SendHeroChallengeNewHighScore(int heroIndex)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.HeroChallengeNewHighScoreChat;

        chattingMessage.msgIdx = (int)ChatNoticeMessageKey.HeroChallengeNewHighScore;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        chattingMessage.partMessageInfos.Add("maxscore", ChatParsingUtil.GetHeroChallengeChatPartInfo(heroIndex));

        HeroModel challengeHeroModel = new HeroModel(_data, heroIndex);

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("heronametext", challengeHeroModel.Info.NameText);

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.ChannelChat, chattingMessage);
    }

    //void SendCompanyObtainCardItemtInfo(CardData cardItem, long companyID)
    void SendCompanyObtainCardItemtInfo(int itemIndex, long companyID)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.ObtainCardItemChat;
        //chattingMessage.msgIdx = (int)ChatNoticeMessageKey.CompanyMemberUniqItemAcquisition;

        chattingMessage.partyType = (int)ChatPartyType.ChatGuild;
        chattingMessage.partyNum = companyID;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        //chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetCardDataChatPartInfo(cardItem));
        chattingMessage.partMessageInfos.Add("Item", ChatParsingUtil.GetCardIndexChatPartInfo(itemIndex));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        //chattingMessage.prm.Add("itemIndex", cardItem.index.ToString());
        chattingMessage.prm.Add("itemIndex", itemIndex.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.GuildChat, chattingMessage);
    }

    public void SendTraceClear(int theme, int diff)
    {
        if(diff < 5)
            return;

        List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();

        for (int i = 0; i < chatGuildInfos.Count; i++) {
            SendCompanyTraceClear(theme, diff, chatGuildInfos[i].party_num);
        }
    }

    public void SendCompanyTraceClear(int theme, int diff, long companyID)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.TraceClearChat;
        //chattingMessage.msgIdx = (int)ChatNoticeMessageKey.TraceClear;

        chattingMessage.partyType = (int)ChatPartyType.ChatGuild;
        chattingMessage.partyNum = companyID;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        string traceKey = "";
        if(theme == 3) { // Isard
            traceKey = "CH_Isard";
        } else if(theme == 4) { // trivia
            traceKey = "CH_Trivia";
        }
        chattingMessage.prm.Add("tracenamekey", traceKey);
        chattingMessage.prm.Add("step", diff.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.GuildChat, chattingMessage);
    }

    public void SendBattleCenterClear(int theme, int diff)
    {
        if (diff < 5)
            return;

        List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();

        for (int i = 0; i < chatGuildInfos.Count; i++) {
            SendCompanyBattleCenterClear(theme, diff, chatGuildInfos[i].party_num);
        }
    }

    public void SendCompanyBattleCenterClear(int theme, int diff, long companyID)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.BattleCenterClearChat;
        //chattingMessage.msgIdx = (int)ChatNoticeMessageKey.BattleCenterClear;

        chattingMessage.partyType = (int)ChatPartyType.ChatGuild;
        chattingMessage.partyNum = companyID;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));
        
        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("theme", theme.ToString());
        chattingMessage.prm.Add("step", diff.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.GuildChat, chattingMessage);
    }

    public bool CheckValidUserLevelupChat(int preLevel, int level)
    {   
        int[] memberLevelUPs = _data.Sheet.SheetGameConfig[1].MemberLevelUp;
        for(int i = 0;i< memberLevelUPs.Length;i++) {
            if(memberLevelUPs[i] > preLevel && memberLevelUPs[i] <= level) {
                //Debug.Log(string.Format("CheckValidUserLevelupChat preLevel : {0}, level : {1}", preLevel, level));
                return true;
            }
        }

        return false;
    }

    public void SendCompanyUserLevelUp(int level, long companyID)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.messageType = (int)ChatDefinition.ChatMessageType.UserLevelUpChat;
        //chattingMessage.msgIdx = (int)ChatNoticeMessageKey.CompanyMemberLevelAchievement;

        chattingMessage.partyType = (int)ChatPartyType.ChatGuild;
        chattingMessage.partyNum = companyID;

        chattingMessage.userID = _data.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(_data);

        chattingMessage.partMessageInfos.Add("user", GetUserInfoChatPartInfo(chattingMessage.userID, chattingMessage.connectId));

        chattingMessage.prm.Add("user", _data.User.Nickname);
        chattingMessage.prm.Add("level", level.ToString());

        NotifyChatSendEventMessage(ChatDefinition.ChatMessageKind.GuildChat, chattingMessage);
    }

    public List<FriendModel> GetFriendList()
    {
        if(_data == null || _data.FriendContainer == null)
            return null;

        return _data.FriendContainer.Friends;
    }

    public bool CheckValidChatting()
    {
        int chatProhibitionLevel = _data.Sheet.SheetGameConfig[1].ChatProhibitionLevel;
        if(_data.User.userData.level < chatProhibitionLevel) {
            return false;
        }

        return true;
    }

#endregion

#region Coroutine Methods

	IEnumerator UpdateChatting()
	{
		while (_isUpdate) {
			_chatSocketManager.UpdateChatSocket ();
			yield return null;
		}
	}

#endregion

#region CallBack Methods

	void OnRequestNetChatInfo()
	{
		_retryChatInfo = 0;
		RequestNetChatInfo ();
	}

	void OnTimeoutRequestChatInfo(object objData)
	{
		if (_retryChatInfo >= 3) {
			Debug.Log ("OnTimeoutRequestChatInfo");
			_isRequestChatInfo = false;
		} else {
			RequestNetChatInfo ();
		}
	}

	void OnChatInputMessage(string inputMessage)
	{
//		Debug.Log (string.Format ("OnChatInputMessage inputMessage : {0}", inputMessage));
		if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
			SendChattingMessage(inputMessage);
	}

    //public void OnChatPartyInputMessage(string inputMessage)
    //{
    //    if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
    //        SendPartyChattingMessage(inputMessage);
    //}

    void OnChatWhisperInputMessage(string inputMessage)
	{
		if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
			SendWhisperChatMessage(inputMessage);
	}

    void OnSendObtainCardDelayTime(object objData)
    {
        ExecuteSendObtainCardItem();
    }

    void OnSendAchieveHeroLevelDelayTime(object objData)
    {
        ExecuteSendAchieveHeroLevel();
    }

    #endregion

    #region Protocol CallBack Methods

    private void OnChatInfoSuccess(ChatInfoResponse resParam)
	{
        _chatSocketManager.ChattingServerInfo = resParam.chat;
		_chatSocketManager.Connect ();

		_chatSocketManager.EventTimer.RemoveTimeEventByID (_chatSocketManager.ChatInfoTimeID);

		_retryChatInfo = 0;

		_isRequestChatInfo = false;

        _chatWebUrl = resParam.chat.chatWebUrl;
    }

	private void OnChatInfoFail(ChatInfoResponse resParam)
	{
		_chatSocketManager.EventTimer.RemoveTimeEventByID (_chatSocketManager.ChatInfoTimeID);

		if (_retryChatInfo >= 3) {
			Debug.Log ("OnChatInfoFail");
			_isRequestChatInfo = false;
		} else {
			RequestNetChatInfo ();
		}
	}

#endregion

#region IChatServerMessage Observer

	public void AttachChatServerMessageOb(IChatServerMessage inputChatServerOb)
	{
		if(_chatServerMessageObserver.Contains(inputChatServerOb))
			return;

		_chatServerMessageObserver.Add (inputChatServerOb);
	}

	public void DetachChatServerMessageOb(IChatServerMessage removeChatServerOb)
	{
		if(!_chatServerMessageObserver.Contains(removeChatServerOb))
			return;

		_chatServerMessageObserver.Remove (removeChatServerOb);
	}

	public void NotifyChatServerMessage(ChatServerMessageData serverMessageData)
	{
		for (int i = 0; i < _chatServerMessageObserver.Count; i++) {
			_chatServerMessageObserver [i].OnChatServerMessageData (serverMessageData);
		}
	}

	public void ReleaseChatServerMessageOb()
	{
		_chatServerMessageObserver.Clear ();
	}

    #endregion

    #region IChatPartyEventObserver

    public void AttachChatPartyEventOb(IChatPartyEventObserver inputChatPartyEventOb)
    {
        if (_partyEventObservers.Contains(inputChatPartyEventOb))
            return;

        _partyEventObservers.Add(inputChatPartyEventOb);
    }

    public void DetachChatPartyEventOb(IChatPartyEventObserver removeChatPartyEventOb)
    {
        if (!_partyEventObservers.Contains(removeChatPartyEventOb))
            return;

        _partyEventObservers.Remove(removeChatPartyEventOb);
    }

    public void NotifyChatPartyEvent(ChatDefinition.PartyChatEventType eventType, PartyChatEventData partyChatEvent)
    {
        for (int i = 0; i < _partyEventObservers.Count; i++) {
            _partyEventObservers[i].OnChatPartyEvent(eventType, partyChatEvent);
        }
    }

    public void ReleaseChatPartyEventOb()
    {
        _partyEventObservers.Clear();
    }

    #endregion

    #region IChatGuildRaidEventObserver

    public void AttachChatGuildRaidEventOb(IChatGuildRaidEventObserver inputChatPartyEventOb)
    {
        if (_guildRaidEventObservers.Contains(inputChatPartyEventOb))
            return;

        _guildRaidEventObservers.Add(inputChatPartyEventOb);
    }

    public void DetachChatGuildRaidEventOb(IChatGuildRaidEventObserver removeChatPartyEventOb)
    {
        if (!_guildRaidEventObservers.Contains(removeChatPartyEventOb))
            return;

        _guildRaidEventObservers.Remove(removeChatPartyEventOb);
    }

    public void NotifyChatGuildRaidEvent(ChatDefinition.GuildRaidEventType eventType, RaidChatEventData raidChatEvent)
    {
        for (int i = 0; i < _partyEventObservers.Count; i++) {
            _guildRaidEventObservers[i].OnChatGuildRaidEvent(eventType, raidChatEvent);
        }
    }

    public void ReleaseChatGuildRaidEventOb()
    {
        _guildRaidEventObservers.Clear();
    }

    #endregion

    #region Party Chat Server Log Methods

    public void SetGuildServerChatLog()
    {
        if (!_isChatGuildConnected)
            return;

        List<ChatGuildJoinedInfo> chatGuildInfos = GetChatGuildInfoList();

        if (chatGuildInfos != null && chatGuildInfos.Count > 0) {
            for (int i = 0; i < chatGuildInfos.Count; i++) {
                ChatGuildJoinedInfo guildInfo = chatGuildInfos[i];
                _requestPartyServerLogChat.Enqueue(guildInfo);
            }
        }

        _isRequestPartyServerLog = true;

        _curRequestPartyLogCount = 0;
        RequestPartyServerChatLog();
    }

    public bool CheckRequestPartyMessage(int partyType, long partyNum)
    {
        bool retValue = false;

        switch((ChatPartyType)partyType) {
            case ChatPartyType.ChatGuild:
                retValue = _chattingModel.CheckRequestGuildChatMessage(partyNum);
                break;
            case ChatPartyType.ChatParty:
                retValue = _partyChatModel.CheckRequestPartyChatMessage(partyNum);
                break;
            case ChatPartyType.ChatUserParty:
                retValue = _chattingModel.CheckRequestMyPartyChatMessage();
                break;
        }

        return retValue;
    }

    public void RequestAddGuildServerChatLog(ChatPartyBaseInfo partyBaseInfo)
	{
		if (!_isChatGuildConnected)
			return;

		_curRequestPartyBaseInfo = partyBaseInfo;
		ChattingPartyChatList partyChatList = new ChattingPartyChatList (_data, _curRequestPartyBaseInfo, OnSuccessChattingGuildChatList, OnFailChattingPartyChatList);
		partyChatList.RequestHttpWeb ();
	}

    //public void RequestAddPartyServerChatLog(ChatPartyBaseInfo partyBaseInfo)
    //{
    //    if (!_isPartyGroupConnected)
    //        return;

    //    _curRequestPartyBaseInfo = partyBaseInfo;
    //    ChattingPartyChatList partyChatList = new ChattingPartyChatList(_data, _curRequestPartyBaseInfo, OnSuccessChattingPartyChatList, OnFailChattingPartyChatList);
    //    partyChatList.RequestHttpWeb();
    //}

    void FinishRequestPartyServerLog()
	{
		_isRequestPartyServerLog = false;

		//CheckTotalNotConfirmMsgCount ();
	}

	public void RequestPartyServerChatLog()
	{
        if (_requestPartyServerLogChat.Count == 0) {
			FinishRequestPartyServerLog();
			return;
		}

		_curRequestPartyBaseInfo = _requestPartyServerLogChat.Dequeue ();
		ChattingPartyChatList partyChatList = new ChattingPartyChatList (_data, _curRequestPartyBaseInfo, OnSuccessChattingGuildChatList, OnFailChattingPartyChatList);
		partyChatList.RequestHttpWeb ();

        //SendTempRequestMissionStartChat();
    }

    //public void SendTempRequestMissionStartChat(long companyID, long missionID, int missionContentType)
    //{
    //    ChatMessage retMessage = null;

    //    string[] inputValues = new string[2];

    //    inputValues[0] = Context.Text.GetMissionKindName((int)MissionKind.Explore);
    //    inputValues[1] = Context.Text.GetMissionContentType(MissionContentType.Raid);

    //    retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.Raid, ChatNoticeMessageKey.CompanyMissionStart, inputValues);

    //    retMessage.partyType = (int)ChatPartyType.ChatGuild;
    //    retMessage.partyNum = companyID;
    //    retMessage.missionID = missionID;
    //    retMessage.missionContentType = missionContentType;

    //    retMessage.timeStamp = TimeUtil.GetTimeStamp();

    //    NotifyChatReceiveMessage((int)ChattingPacketType.PartyChatV2Notify, retMessage);
    //}

	void OnSuccessChattingGuildChatList(ChattingPartyChatResponse chatResponse)
	{
		_curRequestPartyLogCount = 0;
        RequestPartyServerChatLog ();
        _curRequestPartyBaseInfo = null;

        //		Debug.Log (string.Format ("============== OnSuccessChattingPartyChatList partyType : {0}, partyNum : {1} ==================", chatResponse.partyInfo.party_type, chatResponse.partyInfo.party_num));
        if (chatResponse != null && chatResponse.message_list != null && chatResponse.message_list.Length > 0) {
			for (int i = 0; i < chatResponse.message_list.Length; i++) {
//				Debug.Log (string.Format ("OnSuccessChattingPartyChatList msg timeStamp : {0}, msg : {1}", chatResponse.message_list[i].timestamp, chatResponse.message_list[i].chat_msg));
				byte[] decbuf = System.Convert.FromBase64String(chatResponse.message_list[i].chat_msg);
				string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
                Debug.Log(string.Format("OnSuccessChattingPartyChatList Decoding msg : {0}", msg));
#endif

                ChatMessage chatMessage = ChatHelper.GetPartyChatMessage(_data, _chatEventManager, chatResponse.message_list[i], msg);

                if(chatMessage != null) {
                    _chattingModel.AddServerGuildChatMessage(chatMessage);
                }
			}
		}
	}

    void OnFailChattingPartyChatList(ChattingPartyChatResponse chatResponse)
	{
		if (_curRequestPartyLogCount < 3) {
			_curRequestPartyLogCount++;
            if(_curRequestPartyBaseInfo == null) {
                if (_requestPartyServerLogChat.Count == 0) {
                    FinishRequestPartyServerLog();
                    return;
                }

                _curRequestPartyBaseInfo = _requestPartyServerLogChat.Dequeue();
            }

            ChattingPartyChatList partyChatList = new ChattingPartyChatList (_data, _curRequestPartyBaseInfo, OnSuccessChattingGuildChatList, OnFailChattingPartyChatList);
			partyChatList.RequestHttpWeb ();
		} else {
			_curRequestPartyLogCount = 0;
            _curRequestPartyBaseInfo = null;
            RequestPartyServerChatLog ();
		}
	}

	void OnSuccessGuildUpdate()
	{
		if (_addGuildBaseInfo != null) {
            _curSelectGuildInfo = _addGuildBaseInfo;
            RequestAddGuildServerChatLog(_addGuildBaseInfo);
            _addGuildBaseInfo = null;
		}
	}

#endregion

#region Whisper Chat Server Log Methods

	public void SetWhisperServerChatLog()
	{
		_isRequestWhisperServerLog = true;
		RequestWhisperChatCountLog();
	}

	public void RequestWhisperChatCountLog()
	{
		ChattingWhisperChatLastCount whisperChatLastCount = new ChattingWhisperChatLastCount (_data, OnSuccessChattingWhisperChatLastCount, OnFailChattingWhisperChatLastCount);
		whisperChatLastCount.RequestHttpWeb ();
	}

	void FinishRequestWhisperServerLog()
	{
		_isRequestWhisperServerLog = false;
	}

	void OnSuccessChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
	{
		if (chatResponse != null && chatResponse.info != null && chatResponse.info.Length > 0) {
			List<IChatTimeStamp> chatMessageInfos = new List<IChatTimeStamp> ();
			for (int i = 0; i < chatResponse.info.Length; i++) {
				ChatWhisperOtherUserResponseInfo whisperChatInfo = chatResponse.info [i];

				if (whisperChatInfo.message_list != null && whisperChatInfo.message_list.Length > 0) {
					for (int j = 0; j < whisperChatInfo.message_list.Length; j++) {
						chatMessageInfos.Add (whisperChatInfo.message_list [j]);
					}
				}
			}

			chatMessageInfos.Sort(new ChatTimeStampComparer());

			if (chatMessageInfos.Count > _maxChatLineCount) {
				int gapValue = chatMessageInfos.Count - _maxChatLineCount;
				for (int i = 0; i < gapValue; i++) {
					chatMessageInfos.RemoveAt (0);
				}
			}

			for (int i = 0; i < chatMessageInfos.Count; i++) {
				ChatWhisperUserMessageInfo whisperMessageInfo = chatMessageInfos[i] as ChatWhisperUserMessageInfo;
				byte[] decbuf = System.Convert.FromBase64String(whisperMessageInfo.chat_msg);
				string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
				Debug.Log (string.Format ("OnSuccessChattingWhisperChatList timestamp : {0}, msg : {1}", whisperMessageInfo.timestamp, msg));
#endif

				JsonData msgJson = ChatParsingUtil.GetChatMessageJsonData (msg);
				ChatWhisperMessage chatMessage = ChatParsingUtil.GetChatWhisperMessageParsingByJson (msgJson);
				chatMessage.timeStamp = whisperMessageInfo.timestamp;

				long whisperUserID = 0;
                long connectId = 0;
				string whisperUserName = "";

                //if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.WhisperPartyInviteChat) {
                //    if (_data.User.userData.userId == chatMessage.userID) {
                //        continue;
                //    }

                //    chatMessage.msgIdx = (int)ChatNoticeMessageKey.PartyInvitationAlarm;
                //    whisperUserID = chatMessage.userID;
                //    whisperUserName = chatMessage.sendUserName;
                //} else if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.WhisperPartyMissionStartAlarm) {
                //    if (_data.User.userData.userId == chatMessage.userID) {
                //        continue;
                //    }

                //    chatMessage.msgIdx = (int)ChatNoticeMessageKey.PartyFollowedMissionStartAlarm;
                //    whisperUserID = chatMessage.userID;
                //    whisperUserName = chatMessage.sendUserName;
                //} else {
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
                //}

                if (chatMessage.partMessageInfos.ContainsKey ("user")) {
					chatMessage.partMessageInfos.Remove ("user");
				}

				chatMessage.partMessageInfos.Add ("user", ChattingController.Instance.GetUserInfoChatPartInfo (whisperUserID, connectId));

				if(!chatMessage.prm.ContainsKey("user"))
					chatMessage.prm.Add ("user", whisperUserName);

				_chattingModel.AddServerWhisperChatMessage (chatMessage);
			}
		}

		FinishRequestWhisperServerLog ();
	}

	void OnFailChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
	{
		_isRequestWhisperServerLog = false;
	}

	void OnSuccessChattingWhisperChatLastCount(ChattingWhisperChatLastCountResponse chatResponse)
	{
		if (chatResponse != null && chatResponse.info != null && chatResponse.info.Length > 0) {
            HashSet<long> connectIDList = new HashSet<long>();
            List<FriendModel> friendList = GetFriendList();
            if(friendList != null && friendList.Count > 0) {
                for(int i = 0; i < friendList.Count; i++) {
                    FriendModel friendInfo = friendList[i];
                    long connectId = friendInfo.playerId <= 0 ? friendInfo.userId : friendInfo.playerId;
                    if (!connectIDList.Contains(connectId)) {
                        connectIDList.Add(connectId);
                    }
                }
            }

            if(_chatGuildGroupInfos != null && _chatGuildGroupInfos.Count > 0) {
                List<long> guildKeys = _chatGuildGroupInfos.Keys.ToList();
                for(int i = 0;i< guildKeys.Count; i++) {
                    ChatGuildJoinedInfo guildJoinedInfo = _chatGuildGroupInfos[guildKeys[i]];
                    List<long> guildConnectIdList = null;
                    if(guildJoinedInfo.playerIdList != null && guildJoinedInfo.playerIdList.Count > 0) {
                        guildConnectIdList = guildJoinedInfo.playerIdList;
                    } else if (guildJoinedInfo.userIdList != null && guildJoinedInfo.userIdList.Count > 0) {
                        guildConnectIdList = guildJoinedInfo.userIdList;
                    }

                    if(guildConnectIdList != null && guildConnectIdList.Count > 0) {
                        for(int j = 0;j< guildConnectIdList.Count; j++) {
                            long guildConnectId = guildConnectIdList[j];
                            if (!connectIDList.Contains(guildConnectId)) {
                                connectIDList.Add(guildConnectId);
                            }
                        }
                    }
                }
            }
            
            for(int i = 0;i< chatResponse.info.Length;i++) {
                ChatWhisperLastCountResponseInfo whisperLastInfo = chatResponse.info[i];
                if (connectIDList.Contains(whisperLastInfo.other_user_id)) {
                    _chattingModel.WhisperInformation.AddWhisperConfirmTimeInfo(whisperLastInfo.other_user_id, whisperLastInfo.message_count, whisperLastInfo.read_timestamp, whisperLastInfo.last_timestamp);
                }
            }

            FinishRequestWhisperServerLog();
        } else {
			_isRequestWhisperServerLog = false;
		}
	}

	void OnFailChattingWhisperChatLastCount(ChattingWhisperChatLastCountResponse chatResponse)
	{
		_isRequestWhisperServerLog = false;
	}

#endregion

#region IChatChangeCompanyNotice Methods

	void IChatChangeCompanyNotice.OnChangeCompanyNotice(int partyType, long partyNum, string companyNotice)
	{
        if(partyType == (int)ChatPartyType.ChatGuild) {
            if (_chatGuildGroupInfos.ContainsKey(partyNum)){
                _chatGuildGroupInfos[partyNum].notice = companyNotice;
            }
        }
	}

    #endregion

    #region UnityWebRequest Methods

    Action<string, bool> _onReceiveUnityWeb;
    List<ChatRequestWebInfo> _chatRequestWebInfos = new List<ChatRequestWebInfo>();
    ChatRequestWebInfo _curRequestWebInfo = null;

    public void RequestChatUnityWeb(string url, string requestJson, Action<string, bool> onReceiveWeb)
    {
        ChatRequestWebInfo inputWebInfo = new ChatRequestWebInfo();
        inputWebInfo.Url = url;
        inputWebInfo.RequestJson = requestJson;
        inputWebInfo.OnReceiveWeb = onReceiveWeb;

        if (_curRequestWebInfo == null) {
            _curRequestWebInfo = inputWebInfo;
            RequestOnlyChatUnityWeb(inputWebInfo.Url, inputWebInfo.RequestJson, inputWebInfo.OnReceiveWeb);
        } else {
            _chatRequestWebInfos.Add(inputWebInfo);
        }
    }

    void RequestOnlyChatUnityWeb(string url, string requestJson, Action<string, bool> onReceiveWeb)
    {
        _onReceiveUnityWeb = onReceiveWeb;
        StartCoroutine(UploadUnityWeb(url, requestJson));
    }

    IEnumerator UploadUnityWeb(string url, string requestJson)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(requestJson);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
        webRequest.SetRequestHeader("Pragma", "no-cache");

        webRequest.timeout = 10;
        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError) {
#if _CHATTING_LOG
            Debug.Log(webRequest.error);
#endif
            if (_onReceiveUnityWeb != null)
                _onReceiveUnityWeb(webRequest.error, false);

            webRequest.Dispose();
        } else {
            string receiveText = webRequest.downloadHandler.text;

#if _CHATTING_LOG
            Debug.Log(string.Format("Form upload complete! receiveText : {0}", receiveText));
#endif
            if (_onReceiveUnityWeb != null)
                _onReceiveUnityWeb(receiveText, true);

            webRequest.Dispose();
        }

        if(_chatRequestWebInfos.Count == 0) {
            _curRequestWebInfo = null;
        } else {
            _curRequestWebInfo = _chatRequestWebInfos[0];

            RequestOnlyChatUnityWeb(_curRequestWebInfo.Url, _curRequestWebInfo.RequestJson, _curRequestWebInfo.OnReceiveWeb);

            _chatRequestWebInfos.RemoveAt(0);
        }
    }

    #endregion

    #region IChatNudgeNotable

    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        if(_isBattleState) {
            if (UIBattle.This != null) {
                return UIBattle.This.UICommonNudgeCountObj;
            }
        } else {
            if(_guildWarTopChatNudgeObj != null) {
                return _guildWarTopChatNudgeObj;
            } else if(_uiCommonTopMenu != null) {
                return _uiCommonTopMenu.ChattingNotConfirmMsgObj;
            }
        }

        return null;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        if (_isBattleState) {
            if (UIBattle.This != null) {
                return UIBattle.This.NotCheckChatNumText;
            }
        } else {
            if (_uiCommonTopMenu != null) {
                return _uiCommonTopMenu.ChattingNotConfirmMsgCountText;
            }
        }

        return null;
    }

    #endregion
}
