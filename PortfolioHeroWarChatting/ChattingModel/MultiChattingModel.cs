using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using LitJson;
using System.Linq;
using Framework.Controller;

public class ChatMessageFieldInfo
{
	public int startIndex;
	public int endIndex;
	public string fieldText;
	public string fieldKeyText;
	public string fieldRealText;
	public ChatDefinition.PartMessageType partMessageType = ChatDefinition.PartMessageType.None;
	public ChatPartMessageInfo fieldPartMessageInfo = null;
}

public class ChatMessageParsingInfo
{
	public int parsingType; // 0 : NormalText, 1: GroupInfo, 2: FieldInfo, 3 : NewLine NormalText
	public ChatDefinition.PartMessageType partMessageType = ChatDefinition.PartMessageType.None;
	public ChatPartMessageInfo partMessageInfo = null;
	public int startIndex;
	public int endIndex;
	public string fieldText;
	public string fieldKeyText;
	public string messageText;
    public bool isChatColor = false;
    public Color chatColor;
	public List<ChatMessageFieldInfo> includeGroupFieldInfos = new List<ChatMessageFieldInfo>();

	public void MakeGroupFieldMessage(ChatMessage chatMessage)
	{
		for (int i = 0; i < includeGroupFieldInfos.Count; i++) {
			if (includeGroupFieldInfos [i].fieldPartMessageInfo != null)
				partMessageInfo = includeGroupFieldInfos [i].fieldPartMessageInfo;

			if (chatMessage != null && chatMessage.prm.ContainsKey (includeGroupFieldInfos [i].fieldKeyText)) {
				messageText = messageText.Replace (includeGroupFieldInfos [i].fieldText, chatMessage.prm [includeGroupFieldInfos [i].fieldKeyText]);
			}

			if (includeGroupFieldInfos [i].partMessageType != ChatDefinition.PartMessageType.None) {
				partMessageType = includeGroupFieldInfos [i].partMessageType;
                fieldKeyText = includeGroupFieldInfos[i].fieldKeyText;
            }
		}
	}
}

public class MultiChattingModel : IChatReceiveMessageObserver, IChatChangePartyObserver
{
	#region Variables

	DataContext _context;
	int _chatGroupNum;
	long _chatUserId;
    int _langCode;

	int _limitLineCount = 3;

    ChannelChatInformation _channelChatInfo = new ChannelChatInformation();

    PartyBaseChatInformation _myPartyMessageInfo = new PartyBaseChatInformation();

    Dictionary<long /* Party Num */, GuildChatInformation> _guildMessageInfos = new Dictionary<long, GuildChatInformation>();

    ChatWhisperInformation _whisperInformation = new ChatWhisperInformation();

    List<ChatNoticeCurInfo> _chatCurNoticeListInfos = new List<ChatNoticeCurInfo>();

    ChatSaveInfo _chatSaveInfo = null;

    Dictionary<long /* Party Num */, List<ChatMakingMessage>> _guildChatMessage = new Dictionary<long, List<ChatMakingMessage>>();

    #endregion

    #region Properties

    public DataContext Context
	{
		get{ return _context; }
		set{ _context = value; }
	}

	public int ChatGroupNum
	{
		get{ return _chatGroupNum; }
		set{ _chatGroupNum = value; }
	}

	public long ChatUserId
	{
		get { return _chatUserId; }
		set { _chatUserId = value; }
	}

    public int LangCode
    {
        get { return _langCode; }
        set { _langCode = value; }
    }

    public ChannelChatInformation ChannelChatInfo
    {
        get { return _channelChatInfo; }
    }

    public List<ChatNoticeCurInfo> ChatCurNoticeListInfos
    {
        get { return _chatCurNoticeListInfos; }
    }

    public ChatSaveInfo ChatSaveInfo
    {
        get { return _chatSaveInfo; }
    }

    public PartyBaseChatInformation MyPartyMessageInfo
    {
        get { return _myPartyMessageInfo; }
    }

    public ChatWhisperInformation WhisperInformation
    {
        get { return _whisperInformation; }
    }

    #endregion

    #region Methods

    public void InitChattingModel()
	{
        LoadChatSaveData();
    }

    public List<MultiChatListInfo> GetNoticeListMessages()
    {
        List<MultiChatListInfo> retValue = new List<MultiChatListInfo>();

        for(int i = 0;i< _chatCurNoticeListInfos.Count;i++) {
            MultiChatListInfo inputChatListInfo = new MultiChatListInfo();
            ChatNoticeCurInfo noticeInfo = _chatCurNoticeListInfos[i];

            DateTime curServerTime = TimeUtil.KstTime.CurrentServerTime;
            TimeSpan beginGapTime = noticeInfo.beginTime - curServerTime;
            TimeSpan endGapTime = noticeInfo.endTime - curServerTime;
            inputChatListInfo.ChatColor = noticeInfo.color;

            if (beginGapTime.TotalSeconds < 0 && endGapTime.TotalSeconds > 0) {
                if (!string.IsNullOrEmpty(noticeInfo.noticeText)) {
                    inputChatListInfo.ChatTextInfos = ChatHelper.GetMultiChatTextInfoList(noticeInfo.noticeText);
                    retValue.Add(inputChatListInfo);
                }
            }
        }

        return retValue;
    }

	void AddGuildMakingMessage(ChatMakingMessage makingMessage)
	{
        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[makingMessage.chatMessageInfo.msgIdx];
#if _OLD_GUILD_CHAT
        for(int i = 0;i< messageRow.IndicateLocation.Length;i++) {
            switch((ChatDefinition.ChatMessageKind)messageRow.IndicateLocation[i]) {
                case ChatDefinition.ChatMessageKind.GuildChat: 
                    {
                        GuildChatInformation guildChatInfo = GetGuildChatInformation(makingMessage.partyNum);
                        guildChatInfo.AddChatMakingMessage(makingMessage);
                    }
                    break;
                case ChatDefinition.ChatMessageKind.GuildSystemChat: 
                    {
                        GuildChatInformation guildChatInfo = GetGuildChatInformation(makingMessage.partyNum);
                        guildChatInfo.AddSystemChatMakingMessage(makingMessage);
                    }
                    break;
            }
        }
#else
        List<ChatMakingMessage> partyMakingmessages = GetGuildChatInformation(makingMessage.chatMessageInfo.partyNum).ChatMessages;

        if (partyMakingmessages.Count >= ChattingController.Instance.MaxChatLineCount) {
			partyMakingmessages.RemoveAt (0);
		}

		partyMakingmessages.Add (makingMessage);
#endif

        AddGuildMessageInfo(makingMessage.chatMessageInfo.partyNum, makingMessage);
	}

    void AddMyPartyNotConfirmCount(int addValue)
    {
        _myPartyMessageInfo.NudgeNode.AddNudgeCount(addValue);
    }

    public GuildChatInformation GetGuildChatInformation(long partyNum)
    {
        GuildChatInformation chatInfo = null;

        if(_guildMessageInfos.ContainsKey(partyNum)) {
            chatInfo = _guildMessageInfos[partyNum];
        } else {
            GuildChatInformation inputGuildMessage = new GuildChatInformation();
            inputGuildMessage.PartyNum = partyNum;
            chatInfo = inputGuildMessage;

            AddChatGuildChatInfo(partyNum, inputGuildMessage);
        }

        return chatInfo;
    }

    public List<ChatMakingMessage> GetMyPartyMakingMesssage()
    {
        return _myPartyMessageInfo.ChatMessages;
    }

    void AddAnnounceGuildMakingMessage(int partyType, ChatMakingMessage makingMessage)
	{
		List<ChatMakingMessage> guildMakingMessages = null;

        if(_guildMessageInfos.ContainsKey(makingMessage.chatMessageInfo.partyNum)) {
            guildMakingMessages = _guildMessageInfos[makingMessage.chatMessageInfo.partyNum].GuildAnnounceMessage;
        } else {
            GuildChatInformation guildChatInfo = new GuildChatInformation();
            guildChatInfo.PartyNum = makingMessage.chatMessageInfo.partyNum;
            AddChatGuildChatInfo(guildChatInfo.PartyNum, guildChatInfo);
            guildMakingMessages = guildChatInfo.GuildAnnounceMessage;
        }

        if (guildMakingMessages.Count >= ChattingController.Instance.AnnounceMaxCount) {
            guildMakingMessages.RemoveAt(0);
        }

        guildMakingMessages.Add (makingMessage);

        AddAnnounceGuildMessageInfo(makingMessage.chatMessageInfo.partyNum, makingMessage);
	}

    public List<ChatMakingMessage> GetAnnounceGuildMultiChatText(long partyNum)
    {
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            for(int i = 0;i< _guildMessageInfos[partyNum].GuildAnnounceMessage.Count;i++) {
                _guildMessageInfos[partyNum].GuildAnnounceMessage[i].RefreshTimeStampText();
            }
            return _guildMessageInfos[partyNum].GuildAnnounceMessage;
        }
        

        return null;
    }

	public ChatMakingMessage GetLastChatMakingMessage(long partyNum)
	{
		List<ChatMakingMessage> chatMakingList = null;
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            chatMakingList = _guildMessageInfos[partyNum].ChatMessages;
        }

        if (chatMakingList != null && chatMakingList.Count > 0) {
			return chatMakingList [chatMakingList.Count - 1];
		}

		return null;
	}

	public List<MultiChatTextInfo> GetMultiChatTextInfoList(ChatMessage chatMessage, out MultiChatTextInfo timeStampTextInfo)
	{
        timeStampTextInfo = null;

        if (ChattingController.Instance == null)
            return null;

		TextModel textModel = _context.Text;

        string chattingMessageText = textModel.GetChatNoticeSheetText(chatMessage.msgIdx);

        List<MultiChatTextInfo> addMultiTextInfo = ChatHelper.AddChatMessageInfo(textModel, chatMessage, out timeStampTextInfo);

        return ChatHelper.GetMultiChatTextInfoList (chattingMessageText, chatMessage, addMultiTextInfo);
	}

    public void AddGuildLastConfirmTimeInfo(long guildNum, long guildJoinTime, long lastChatViewTime)
    {
        GuildChatInformation guildChatInfo = GetGuildChatInformation(guildNum);
        guildChatInfo.PartyJoinTimeStamp = guildJoinTime;
        guildChatInfo.LastChatViewTime = lastChatViewTime;
    }

	public void ChangeGuildChatConfirmTime(long partyNum, long timeStamp)
	{
        GuildChatInformation guildChatInfo = GetGuildChatInformation(partyNum);
        guildChatInfo.LastChatViewTime = timeStamp;
    }

    public long GetLastGuildChatMessageTimeStamp(long partyNum)
    {
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            return _guildMessageInfos[partyNum].LastChatTimeStamp;
        }

        return 0;
    }

	public void AddServerGuildChatMessage(ChatMessage chatMessage)
	{
		if (chatMessage.msgIdx == -1)
			return;

        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMessage.msgIdx];

        GuildChatInformation guildChatInfo = GetGuildChatInformation(chatMessage.partyNum);
        if(guildChatInfo.PartyJoinTimeStamp > chatMessage.timeStamp)
            return;

        List<ChatMakingMessage> partyMakingmessages = GetGuildChatInformation(chatMessage.partyNum).ChatMessages;
        if (partyMakingmessages != null && partyMakingmessages.Count > 0) {
            long lastTimeStamp = -1;

			if (partyMakingmessages.Count > 0) {
				lastTimeStamp = partyMakingmessages [partyMakingmessages.Count - 1].chatMessageInfo.timeStamp;
			}

#if _CHATTING_LOG
			Debug.Log (string.Format ("AddServerPartyChatMessage cur TimeStamp : {0}, message TimeStamp : {1}", TimeUtil.GetTimeStamp (), chatMessage.timeStamp));
#endif

			if (lastTimeStamp >= chatMessage.timeStamp) {
				return;
			}
		}

        ChatMakingMessage makingMessage = new ChatMakingMessage ();
        makingMessage.chatMessageInfo = chatMessage;

        makingMessage.messageColor = ChatHelper.GetChatMessageColor (_context, chatMessage);
		makingMessage.chatMessageKinds = messageRow.IndicateLocation;

        MultiChatTextInfo timeStampTextInfo = null;
        makingMessage.multiChatTextInfoList = GetMultiChatTextInfoList (chatMessage, out timeStampTextInfo);
        makingMessage.TimeStampTextInfo = timeStampTextInfo;

        SetChatMessageInfo (makingMessage, chatMessage);
	}

    public void AddServerMyPartyChatMessage(ChatMessage chatMessage)
    {
        if (chatMessage.msgIdx == -1)
            return;

        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMessage.msgIdx];

        List<ChatMakingMessage> partyMakingmessages = _myPartyMessageInfo.ChatMessages;
        if (partyMakingmessages != null && partyMakingmessages.Count > 0) {
            long lastTimeStamp = -1;

            if (partyMakingmessages.Count > 0) {
                lastTimeStamp = partyMakingmessages[partyMakingmessages.Count - 1].chatMessageInfo.timeStamp;
            }

#if _CHATTING_LOG
            Debug.Log(string.Format("AddServerMyPartyChatMessage cur TimeStamp : {0}, message TimeStamp : {1}", TimeUtil.GetTimeStamp(), chatMessage.timeStamp));
#endif

            if (lastTimeStamp >= chatMessage.timeStamp) {
                return;
            }
        }

        if(chatMessage.messageType == (int)ChatDefinition.ChatMessageType.PartyMissionStart) {
            long gapTimeStamp = (TimeUtil.GetTimeStamp() - chatMessage.timeStamp) / 1000;
#if _CHATTING_LOG
            Debug.Log(string.Format("!!!!!!!! ----------------- gapTimeStamp : {0}", gapTimeStamp));
#endif
            SheetMissionSettingRow settingRow = MissionHelpers.GetMissionSettingRow(MissionContentType.PartyMission);
            if (gapTimeStamp > settingRow.ExpireSec) {
                return;
            }
        }

        ChatMakingMessage makingMessage = new ChatMakingMessage();
        makingMessage.chatMessageInfo = chatMessage;

        makingMessage.messageColor = ChatHelper.GetChatMessageColor(_context, chatMessage);
        makingMessage.chatMessageKinds = messageRow.IndicateLocation;

        MultiChatTextInfo timeStampTextInfo = null;
        makingMessage.multiChatTextInfoList = GetMultiChatTextInfoList(chatMessage, out timeStampTextInfo);
        makingMessage.TimeStampTextInfo = timeStampTextInfo;

        SetChatMessageInfo(makingMessage, chatMessage);
    }

    public void AddServerWhisperChatMessage(ChatWhisperMessage chatMessage, bool isRefresh = true)
	{
		if (chatMessage.msgIdx == -1)
			return;

		SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMessage.msgIdx];

		ChatMakingMessage makingMessage = new ChatMakingMessage ();
        makingMessage.chatMessageInfo = chatMessage;

        makingMessage.messageColor = ChatHelper.GetChatMessageColor (_context, chatMessage);
		makingMessage.chatMessageKinds = messageRow.IndicateLocation;

        MultiChatTextInfo timeStampTextInfo = null;
        makingMessage.multiChatTextInfoList = GetMultiChatTextInfoList (chatMessage, out timeStampTextInfo);
        makingMessage.TimeStampTextInfo = timeStampTextInfo;

        SetChatMessageInfo (makingMessage, chatMessage, isRefresh);
	}

	void SetChatMessageInfo(ChatMakingMessage makingMessage, ChatMessage chatMsg, bool isRefresh = true)
	{
        if(ChattingController.Instance == null)
            return;

        if(makingMessage.chatMessageInfo.sendType == ChatDefinition.ChatMessageKind.None) {
            for (int i = 0; i < makingMessage.chatMessageKinds.Length; i++) {
                switch ((ChatDefinition.ChatMessageKind)makingMessage.chatMessageKinds[i]) {
                    case ChatDefinition.ChatMessageKind.ChannelChat:
                        AddChannelMakingMessage(makingMessage);
                        break;
                    case ChatDefinition.ChatMessageKind.GuildChat:
                    case ChatDefinition.ChatMessageKind.GuildSystemChat:
                        if (makingMessage.chatMessageInfo.partyType == (int)ChatPartyType.ChatGuild)
                            AddGuildMakingMessage(makingMessage);
                        break;
                    case ChatDefinition.ChatMessageKind.PartyChat:

                        break;
                    case ChatDefinition.ChatMessageKind.WhisperChat:
                        if (makingMessage.chatMessageInfo.messageType == (int)ChatDefinition.ChatMessageType.WhisperFriendChat) {
                            _whisperInformation.AddWhisperUserMessage(makingMessage, chatMsg as ChatWhisperMessage, ChatDefinition.WhisperKind.Friend, isRefresh);
                        } else if (makingMessage.chatMessageInfo.messageType == (int)ChatDefinition.ChatMessageType.WhisperGuildChat) {
                            _whisperInformation.AddWhisperUserMessage(makingMessage, chatMsg as ChatWhisperMessage, ChatDefinition.WhisperKind.Guild, isRefresh);
                        }
                        break;
                    case ChatDefinition.ChatMessageKind.AnnouncementChat:
                        if (makingMessage.chatMessageInfo.partyType == (int)ChatPartyType.ChatGuild) {
                            AddAnnounceGuildMakingMessage(makingMessage.chatMessageInfo.partyType, makingMessage);
                        }
                        break;
                }
            }
        } else {
            switch(makingMessage.chatMessageInfo.sendType) {
                case ChatDefinition.ChatMessageKind.ChannelChat:
                    AddChannelMakingMessage(makingMessage);
                    break;
                case ChatDefinition.ChatMessageKind.GuildChat:
                    AddGuildMakingMessage(makingMessage);
                    break;
                case ChatDefinition.ChatMessageKind.WhisperChat:
                    _whisperInformation.AddWhisperUserMessage(makingMessage, chatMsg as ChatWhisperMessage, ChatDefinition.WhisperKind.Friend, isRefresh);
                    break;
            }
        }
        
    }

    void AddChannelMakingMessage(ChatMakingMessage makingMessage)
    {
        _channelChatInfo.AddChannelChatMessage(makingMessage);

        AddChannelMessageInfo(makingMessage);
    }

    public void AddGuildMessageInfo(long partyNum, ChatMakingMessage chatMakingMsg)
	{
        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMakingMsg.chatMessageInfo.msgIdx];
        if(messageRow.NudgeIndicate == 0)
            return;

        GuildChatInformation guildChatInfo = GetGuildChatInformation(partyNum);
        if(chatMakingMsg.chatMessageInfo.timeStamp > guildChatInfo.LastChatViewTime) {
            RefreshGuildChatRoomNotConfirmMsgInfo(partyNum);
        }
    }

    public void AddChannelMessageInfo(ChatMakingMessage chatMakingMsg)
    {
        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMakingMsg.chatMessageInfo.msgIdx];
        if (messageRow.NudgeIndicate == 0)
            return;

        AddChannelNudgeCount();
    }

    public void AddMyPartyMessageInfo(ChatMakingMessage chatMakingMsg)
    {
        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMakingMsg.chatMessageInfo.msgIdx];
        if (messageRow.NudgeIndicate == 0)
            return;

        if (chatMakingMsg.chatMessageInfo.timeStamp > _myPartyMessageInfo.LastChatViewTime) {
            RefreshMyPartyChatRoomNotConfirmMsgInfo();
        }
    }

    public void AddAnnounceGuildMessageInfo(long partyNum, ChatMakingMessage chatMakingMsg)
	{
        GuildChatInformation guildChatInfo = GetGuildChatInformation(partyNum);
        if (guildChatInfo.AnnounceNotConfirmCount >= ChattingController.Instance.AnnounceMaxCount) {
            return;
        } else {
            guildChatInfo.AnnounceNotConfirmCount++;
            SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMakingMsg.chatMessageInfo.msgIdx];

            if (messageRow.NudgeIndicate == 1)
                RefreshGuildChatRoomNotConfirmMsgInfo(partyNum);
        }
    }

    public bool CheckRequestGuildChatMessage(long partyNum)
    {
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            return _guildMessageInfos[partyNum].IsRequestChatServer;
        }

        return false;
    }

    public bool CheckRequestMyPartyChatMessage()
    {
        return _myPartyMessageInfo.IsRequestChatServer;
    }

    void AddChannelNudgeCount(int addValue = 1)
    {
        UIMultiChatting uiChatting = ChattingController.Instance.UIMultiChat;
        if (uiChatting.CurMultiChatState == UIMultiChattingState.InputChatting && uiChatting.CurSelectChattingInput != null) {
            if (uiChatting.CurSelectChattingInput.ChatMessageKind != ChatDefinition.ChatMessageKind.ChannelChat) {
                _channelChatInfo.NudgeNode.AddNudgeCount(addValue);
            }
        } else {
            _channelChatInfo.NudgeNode.AddNudgeCount(addValue);
        }
    }

    void RefreshGuildChatRoomNotConfirmMsgInfo(long partyNum)
	{
		UIMultiChatting uiChatting = ChattingController.Instance.UIMultiChat;
		if (uiChatting.CurMultiChatState == UIMultiChattingState.InputChatting && uiChatting.CurSelectChattingInput != null) {
			if (uiChatting.CurSelectChattingInput.PartyNum != partyNum) {
                AddNotConfirmGuildMessageCount(partyNum);
			}
		} else {
            AddNotConfirmGuildMessageCount(partyNum);
		}
	}

    void RefreshMyPartyChatRoomNotConfirmMsgInfo(int addValue = 1)
    {
        UIMultiChatting uiChatting = ChattingController.Instance.UIMultiChat;
        if (uiChatting.CurMultiChatState == UIMultiChattingState.InputChatting && uiChatting.CurSelectChattingInput != null) {
            if (uiChatting.CurSelectChattingInput.ChatMessageKind != ChatDefinition.ChatMessageKind.MyPartyChat) {
                AddMyPartyNotConfirmCount(addValue);
            }
        } else {
            AddMyPartyNotConfirmCount(addValue);
        }
    }

    public void AddWhisperNudgeCount(ChatWhisperUserData whisperUserData, long timeStamp)
	{
        bool isRefresh = false;
        UIMultiChatting uiChatting = ChattingController.Instance.UIMultiChat;
        if (uiChatting.CurSelectChattingInput != null) {
            if (uiChatting.CurSelectChattingInput.ChatMessageKind == ChatDefinition.ChatMessageKind.WhisperChat) {
                if (ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo != null &&
                     ChattingController.Instance.ChattingModel.WhisperInformation.WhisperTargetUserInfo.targetConnectID == whisperUserData.ConnectID) {
                    whisperUserData.ConfirmTimeStamp = timeStamp;
                    RequestWhisperLastConfirmHttp(whisperUserData.ConnectID);
                } else {
                    isRefresh = true;
                }
            } else {
                isRefresh = true;
            }
        } else {
            isRefresh = true;
        }

        if (isRefresh && whisperUserData.NudgeNode.GetNodeNudgeCount() < ChattingController.Instance.MaxWhisperChatLineCount) {
            whisperUserData.NudgeNode.AddNudgeCount(1);
        }
    }

    public static void RequestWhisperLastConfirmHttp(long connectId)
    {
        ChatOtherUserListInfo[] otherUserListInfo = new ChatOtherUserListInfo[1];
        otherUserListInfo[0] = new ChatOtherUserListInfo();
        otherUserListInfo[0].other_user_id = connectId;
        otherUserListInfo[0].show_type = 1; // 1 : Show Count , 2 : Large begin_timestamp
        otherUserListInfo[0].begin_timestamp = 0;
        otherUserListInfo[0].show_count = 50;

        ChattingWhisperChatList whisperChatList = new ChattingWhisperChatList(ChattingController.Instance.Context, otherUserListInfo, null, null);
        whisperChatList.RequestHttpWeb();
    }

    public void RemoveGuildMessageInfo(long partyNum)
    {
        RemoveChatGuildChatInfo(partyNum);
    }

	public void AddNotConfirmGuildMessageCount(long partyNum)
	{
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            _guildMessageInfos[partyNum].NudgeNode.AddNudgeCount(1);
        } else {
            GuildChatInformation inputGuildChat = new GuildChatInformation();
            inputGuildChat.PartyNum = partyNum;

            AddChatGuildChatInfo(partyNum, inputGuildChat);
            inputGuildChat.NudgeNode.AddNudgeCount(1);
        }
	}

    public void ReleaseAllChatMessageInfo()
	{
        _myPartyMessageInfo.ReleaseChatMessages();

        RemoveAllChatGuildChatInfo();

        _whisperInformation.ReleaseWhisperData();
    }

    private string GetChatFileName()
    {
        if (GameSystem.Instance.SystemData.SystemOption.LocalServer == LocalServerType.TH) {
            return string.Format("ChatInfo_{0}.dat", Context.User.userData.nickname);
        } else {
            return string.Format("ChatInfo_{0}_{1}.dat", Context.User.userData.nickname, GameSystem.Instance.SystemData.SystemOption.LocalServer.ToString());
        }
    }

    private string GetChatFileNameByUserId()
    {
        if (GameSystem.Instance.SystemData.SystemOption.LocalServer == LocalServerType.TH) {
            return string.Format("ChatInfo_{0}.dat", ChatHelper.PlayerId);
        } else {
            return string.Format("ChatInfo_{0}_{1}.dat", ChatHelper.PlayerId, GameSystem.Instance.SystemData.SystemOption.LocalServer.ToString());
        }
    }

    public void SaveChatSaveData()
    {
        string localData = LitJson.JsonMapper.ToJson(_chatSaveInfo);
        FileUtility.SaveAESEncryptData(GetChatFileNameByUserId(), localData);
    }

    public void LoadChatSaveData()
    {
        string jsonString = "";
        jsonString = FileUtility.LoadAESEncryptData(GetChatFileNameByUserId());
        if (string.IsNullOrEmpty(jsonString)) {
            jsonString = FileUtility.LoadAESEncryptData(GetChatFileName());
        }
        if (!string.IsNullOrEmpty(jsonString)) {
            _chatSaveInfo = LitJson.JsonMapper.ToObject<ChatSaveInfo>(jsonString);
        } else {
            _chatSaveInfo = new ChatSaveInfo();
            _chatSaveInfo.guildUIType = (int)ChatDefinition.GuildUIType.Normal;
        }
    }

    void SetGuildChatServerMessage(ChattingPartyChatResponse chatResponse)
    {
        GuildChatInformation guildMessageInfo = null;
        if (_guildMessageInfos.ContainsKey(chatResponse.partyInfo.party_num)) {
            guildMessageInfo = _guildMessageInfos[chatResponse.partyInfo.party_num];
            if(guildMessageInfo.IsRequestChatServer)
                return;
        } else {
            guildMessageInfo = new GuildChatInformation();
            guildMessageInfo.PartyNum = chatResponse.partyInfo.party_num;
            AddChatGuildChatInfo(chatResponse.partyInfo.party_num, guildMessageInfo);
        }

        guildMessageInfo.IsRequestChatServer = true;

        if (chatResponse != null && chatResponse.message_list != null && chatResponse.message_list.Length > 0) {
            for (int i = 0; i < chatResponse.message_list.Length; i++) {
                byte[] decbuf = System.Convert.FromBase64String(chatResponse.message_list[i].chat_msg);
                string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
                Debug.Log(string.Format("OnSuccessChattingPartyChatList Decoding msg : {0}", msg));
#endif

                ChatEventManager chatEvent = ChattingController.Instance.ChatEventManager;

                ChatMessage chatMessage = ChatHelper.GetPartyChatMessage(_context, chatEvent, chatResponse.message_list[i], msg);

                if (chatMessage != null) {
                    AddServerGuildChatMessage(chatMessage);
                }
            }
        }
    }

    public void AddChatGuildChatInfo(long partyNum, GuildChatInformation addGuildChatInfo)
    {
        ChattingController.Instance.RootNudgeNode.AddChildNode(addGuildChatInfo.NudgeNode);
        _guildMessageInfos.Add(partyNum, addGuildChatInfo);
    }

    public void RemoveChatGuildChatInfo(long partyNum)
    {
        if(_guildMessageInfos.ContainsKey(partyNum)) {
            _guildMessageInfos[partyNum].NudgeNode.DeLinkAllNode();
            _guildMessageInfos.Remove(partyNum);
        }
    }

    public void RemoveAllChatGuildChatInfo()
    {
        List<long> guildKeys = _guildMessageInfos.Keys.ToList();
        for(int i = 0;i< guildKeys.Count;i++) {
            _guildMessageInfos[guildKeys[i]].ReleaseChatMessages();
        }
    }

    void SetMyPartyChatServerMessage(ChattingPartyChatResponse chatResponse)
    {
        if(_myPartyMessageInfo.IsRequestChatServer)
            return;

        _myPartyMessageInfo.IsRequestChatServer = true;

        if (chatResponse != null && chatResponse.message_list != null && chatResponse.message_list.Length > 0) {
            for (int i = 0; i < chatResponse.message_list.Length; i++) {
                byte[] decbuf = System.Convert.FromBase64String(chatResponse.message_list[i].chat_msg);
                string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
                Debug.Log(string.Format("OnSuccessChattingPartyChatList Decoding msg : {0}", msg));
#endif

                ChatEventManager chatEvent = ChattingController.Instance.ChatEventManager;

                ChatMessage chatMessage = ChatHelper.GetPartyChatMessage(_context, chatEvent, chatResponse.message_list[i], msg);

                if (chatMessage != null) {
                    AddServerMyPartyChatMessage(chatMessage);
                }
            }
        }
    }

    #endregion

    #region IChatReceiveMessageObserver

    void IChatReceiveMessageObserver.OnChatReceiveMessage (int packetType, ChatMakingMessage makingMessage, ChatMessage chatMsg)
	{
		SetChatMessageInfo (makingMessage, chatMsg);
	}

    #endregion

    #region CallBack Methods

    #endregion

    #region IChatChangePartyObserver

    void IChatChangePartyObserver.OnPartyChatRes(ChattingPartyChatResponse chatResponse)
    {
        switch((ChatPartyType)chatResponse.partyInfo.party_type) {
            case ChatPartyType.ChatGuild:
                SetGuildChatServerMessage(chatResponse);
                break;
            case ChatPartyType.ChatUserParty:
                SetMyPartyChatServerMessage(chatResponse);
                break;
        }
    }

    #endregion
}
