using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyChattingModel : IChatReceiveMessageObserver, IChatChangePartyObserver
{
    #region Variables

    DataContext _context;

    long _curPartyId = -1;

    Action _onReceivedPartyChat = null;

    Dictionary<long /* Party Num */, PartyChatInformation> _partyChatInfos = new Dictionary<long, PartyChatInformation>();

    #endregion

    #region Properties

    public DataContext Context
    {
        get { return _context; }
        set { _context = value; }
    }

    public long CurPartyId
    {
        get { return _curPartyId; }
        set { _curPartyId = value; }
    }

    public Action OnReceivedPartyChat
    {
        get { return _onReceivedPartyChat; }
        set { _onReceivedPartyChat = value; }
    }

    #endregion

    #region Methods

    public PartyChatInformation AddPartyChatInformation(long partyNum)
    {
        if(_partyChatInfos.ContainsKey(partyNum))
            return _partyChatInfos[partyNum];

        PartyChatInformation inputPartyChatInfo = new PartyChatInformation();

        _partyChatInfos.Add(partyNum, inputPartyChatInfo);

        return inputPartyChatInfo;
    }

    public void RemovePartyChatInformation(long partyNum)
    {
        if(!_partyChatInfos.ContainsKey(partyNum))
            return;

        _partyChatInfos[partyNum].ReleaseChatMessages();

        _partyChatInfos.Remove(partyNum);
    }

    public List<ChatMakingMessage> GetPartyMakingMesssage(long partyNum)
    {
        List<ChatMakingMessage> partyMakingmessages = null;

        if(_partyChatInfos.ContainsKey(partyNum)) {
            partyMakingmessages = _partyChatInfos[partyNum].ChatMessages;
        } else {
            PartyChatInformation inputChatInfo = GetAddPartyChatInfo(partyNum);
            partyMakingmessages = inputChatInfo.ChatMessages;

            _partyChatInfos.Add(partyNum, inputChatInfo);
        }

        return partyMakingmessages;
    }

    public bool CheckRequestPartyChatMessage(long partyNum)
    {
        if (_partyChatInfos.ContainsKey(partyNum)) {
            return _partyChatInfos[partyNum].IsRequestChatServer;
        }

        return false;
    }

    public void ReleasePartyMakingMessage(long partyNum)
    {
        if(_partyChatInfos.ContainsKey(partyNum)) {
            _partyChatInfos[partyNum].ReleaseChatMessages();
        }
    }

    public void AddPartyChatInfo(long partyNum)
    {
        if(_partyChatInfos.ContainsKey(partyNum)) {
            return;
        }

        PartyChatInformation inputChatInfo = GetAddPartyChatInfo(partyNum);
        _partyChatInfos.Add(partyNum, inputChatInfo);
    }

    public PartyChatInformation GetAddPartyChatInfo(long partyNum)
    {
        PartyChatInformation inputChatInfo = new PartyChatInformation();
        inputChatInfo.PartyNum = partyNum;
        inputChatInfo.PartyJoinTimeStamp = TimeUtil.GetTimeStamp();

        return inputChatInfo;
    }

    public void AddServerPartyChatMessage(ChatMessage chatMessage)
    {
        if (chatMessage.msgIdx == -1)
            return;

        SheetChatNoticeMessageRow messageRow = _context.Sheet.SheetChatNoticeMessage[chatMessage.msgIdx];

        if(!_partyChatInfos.ContainsKey(chatMessage.partyNum)) {
            return;
        }

        PartyChatInformation partyChatInfo = _partyChatInfos[chatMessage.partyNum];
        if(partyChatInfo.PartyJoinTimeStamp > chatMessage.timeStamp) {
            return;
        }

        List<ChatMakingMessage> partyMakingmessages = GetPartyMakingMesssage(chatMessage.partyNum);
        if (partyMakingmessages != null && partyMakingmessages.Count > 0) {
            long lastTimeStamp = -1;

            if (partyMakingmessages.Count > 0) {
                lastTimeStamp = partyMakingmessages[partyMakingmessages.Count - 1].chatMessageInfo.timeStamp;
            }

#if _CHATTING_LOG
            Debug.Log(string.Format("AddServerPartyChatMessage cur TimeStamp : {0}, message TimeStamp : {1}", TimeUtil.GetTimeStamp(), chatMessage.timeStamp));
#endif

            if (lastTimeStamp >= chatMessage.timeStamp) {
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

    void SetChatMessageInfo(ChatMakingMessage makingMessage, ChatMessage chatMsg, bool isRefresh = true)
    {
        if (ChattingController.Instance == null)
            return;

        if(chatMsg.sendType == ChatDefinition.ChatMessageKind.None) {
            for (int i = 0; i < makingMessage.chatMessageKinds.Length; i++) {
                switch ((ChatDefinition.ChatMessageKind)makingMessage.chatMessageKinds[i]) {
                    case ChatDefinition.ChatMessageKind.GuildChat:
                    case ChatDefinition.ChatMessageKind.GuildSystemChat:
                    case ChatDefinition.ChatMessageKind.PartyChat:
                        AddPartyMakingMessage(makingMessage);
                        break;
                }
            }
        } else {
            if(chatMsg.sendType == ChatDefinition.ChatMessageKind.PartyChat) {
                AddPartyMakingMessage(makingMessage);
            }
        }
    }

    public List<MultiChatTextInfo> GetMultiChatTextInfoList(ChatMessage chatMessage, out MultiChatTextInfo timeStampTextInfo)
    {
        timeStampTextInfo = null;

        if (ChattingController.Instance == null)
            return null;

        var sheetChatNoticeMessage = _context.Sheet.SheetChatNoticeMessage;
        TextModel textModel = _context.Text;

        string chattingMessageText = null;

        chattingMessageText = textModel.GetChatNoticeSheetText(chatMessage.msgIdx);

        List<MultiChatTextInfo> addMultiTextInfo = new List<MultiChatTextInfo>();

        return ChatHelper.GetMultiChatTextInfoList(chattingMessageText, chatMessage, addMultiTextInfo);
    }

    public List<MultiChatTextInfo> GetQuickMultiChatTextInfoList(ChatMessage chatMessage)
    {
        if (ChattingController.Instance == null)
            return null;

        var sheetChatNoticeMessage = _context.Sheet.SheetChatNoticeMessage;
        TextModel textModel = _context.Text;

        string chattingMessageText = null;

        chattingMessageText = textModel.GetChatNoticeSheetText(chatMessage.msgIdx);

        List<MultiChatTextInfo> addMultiTextInfo = new List<MultiChatTextInfo>();

        return ChatHelper.GetQuickMultiChatTextInfoList(chattingMessageText, chatMessage, addMultiTextInfo);
    }

    public List<MultiChatTextInfo> GetQuickMultiChatTextInfoList(ChatMessage chatMessage, UIChatPartMessage supportTextMessage, float messageMaxWidth)
    {
        if (ChattingController.Instance == null)
            return null;

        var sheetChatNoticeMessage = _context.Sheet.SheetChatNoticeMessage;
        TextModel textModel = _context.Text;

        string chattingMessageText = null;

        chattingMessageText = textModel.GetChatNoticeSheetText(chatMessage.msgIdx);

        return ChatHelper.GetQuickMultiChatTextInfoList(chattingMessageText, chatMessage, supportTextMessage, messageMaxWidth);
    }

    void AddPartyMakingMessage(ChatMakingMessage makingMessage)
    {
        List<ChatMakingMessage> partyMakingmessages = GetPartyMakingMesssage(makingMessage.chatMessageInfo.partyNum);

        if (partyMakingmessages.Count >= ChattingController.Instance.MaxChatLineCount) {
            partyMakingmessages.RemoveAt(0);
        }

        partyMakingmessages.Add(makingMessage);
    }

    public void ReleasePartyChatMessageInfo()
    {
        List<long> _partyKeys = _partyChatInfos.Keys.ToList();
        for(int i = 0;i< _partyKeys.Count;i++) {
            _partyChatInfos[_partyKeys[i]].ReleaseChatMessages();
        }
    }

    void SetPartyChatServerMessage(ChattingPartyChatResponse chatResponse)
    {
        PartyChatInformation partyMessageInfo = null;
        if (_partyChatInfos.ContainsKey(chatResponse.partyInfo.party_num)) {
            partyMessageInfo = _partyChatInfos[chatResponse.partyInfo.party_num];
            if (partyMessageInfo.IsRequestChatServer)
                return;
        } else {
            partyMessageInfo = GetAddPartyChatInfo(chatResponse.partyInfo.party_num);
            _partyChatInfos.Add(chatResponse.partyInfo.party_num, partyMessageInfo);
        }

        partyMessageInfo.IsRequestChatServer = true;

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
                    AddServerPartyChatMessage(chatMessage);
                }
            }
        }
    }

    #endregion

    #region IChatReceiveMessageObserver

    void IChatReceiveMessageObserver.OnChatReceiveMessage(int packetType, ChatMakingMessage makingMessage, ChatMessage chatMsg)
    {
        if(makingMessage.chatMessageInfo.partyType == (int)ChatPartyType.ChatParty) {
            SetChatMessageInfo(makingMessage, chatMsg);
        }
    }

    #endregion

    #region IChatChangePartyObserver

    void IChatChangePartyObserver.OnPartyChatRes(ChattingPartyChatResponse chatResponse)
    {
        switch ((ChatPartyType)chatResponse.partyInfo.party_type) {
            case ChatPartyType.ChatParty:
                SetPartyChatServerMessage(chatResponse);
                if(_onReceivedPartyChat != null) {
                    _onReceivedPartyChat();
                }
                break;
        }
    }

    #endregion
}
