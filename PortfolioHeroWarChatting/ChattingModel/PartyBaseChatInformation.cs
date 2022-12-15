using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParytChatMessageInfo
{
    public int messageType;
    public long partyId;
    public long missionId;
    public ChatMakingMessage chatMessage;
}

public class PartyBaseChatInformation
{
    #region Variables

    long _partyNum = -1;
    bool _isRequestChatServer = false;
    long _lastChatViewTime = -1;

    protected long _lastChatTimeStamp = 0;

    ChatNudgeNode _nudgeNode = new ChatNudgeNode();

    List<ChatMakingMessage> _chatMessages = new List<ChatMakingMessage>();
    List<ParytChatMessageInfo> _partyChatMessagesInfos = new List<ParytChatMessageInfo>();

    #endregion

    #region Properties

    public long PartyNum
    {
        get { return _partyNum; }
        set { _partyNum = value; }
    }

    public bool IsRequestChatServer
    {
        get { return _isRequestChatServer; }
        set { _isRequestChatServer = value; }
    }

    public long LastChatViewTime
    {
        get { return _lastChatViewTime; }
        set { _lastChatViewTime = value; }
    }

    public long LastChatTimeStamp
    {
        get { return _lastChatTimeStamp; }
        set { _lastChatTimeStamp = value; }
    }

    public ChatNudgeNode NudgeNode
    {
        get { return _nudgeNode; }
    }

    public List<ChatMakingMessage> ChatMessages
    {
        get { return _chatMessages; }
    }

    public List<ParytChatMessageInfo> PartyChatMessagesInfos
    {
        get { return _partyChatMessagesInfos; }
    }

    #endregion

    #region Methods

    public virtual void ReleaseChatMessages()
    {
        _isRequestChatServer = false;
        _chatMessages.Clear();
        _partyChatMessagesInfos.Clear();
        _nudgeNode.ConfirmNudge();
    }

    public void AddPartyChatMessageInfo(int messageType, long partyId, long missionId, ChatMakingMessage chatMessage)
    {
        for (int i = 0; i < _partyChatMessagesInfos.Count; i++) {
            if (_partyChatMessagesInfos[i].messageType == messageType && _partyChatMessagesInfos[i].partyId == partyId &&
                _partyChatMessagesInfos[i].missionId == missionId) {
                return;
            }
        }

        ParytChatMessageInfo inputPartyChat = new ParytChatMessageInfo();
        inputPartyChat.messageType = messageType;
        inputPartyChat.partyId = partyId;
        inputPartyChat.missionId = missionId;
        inputPartyChat.chatMessage = chatMessage;
        _partyChatMessagesInfos.Add(inputPartyChat);
    }

    public void RemovePrePartyChatMessage(int messageType, long partyId, long missionId)
    {
        for(int i = 0;i< _partyChatMessagesInfos.Count;i++) {
            if(_partyChatMessagesInfos[i].messageType == messageType && _partyChatMessagesInfos[i].partyId == partyId &&
                _partyChatMessagesInfos[i].missionId == missionId) {
                _chatMessages.Remove(_partyChatMessagesInfos[i].chatMessage);
                _partyChatMessagesInfos.RemoveAt(i);
                break;
            }
        }
    }

    public void RemovePartyChatMessageInfo(int messageType, long partyId, long missionId)
    {
        for (int i = 0; i < _partyChatMessagesInfos.Count; i++) {
            if (_partyChatMessagesInfos[i].messageType == messageType && _partyChatMessagesInfos[i].partyId == partyId &&
                _partyChatMessagesInfos[i].missionId == missionId) {
                _partyChatMessagesInfos.RemoveAt(i);
                break;
            }
        }
    }

    public void AddChatMakingMessage(ChatMakingMessage makingMessage)
    {
        if (_chatMessages.Count >= ChattingController.Instance.MaxChatLineCount) {
            _chatMessages.RemoveAt(0);
        }

        if(_lastChatTimeStamp < makingMessage.chatMessageInfo.timeStamp) {
            _lastChatTimeStamp = makingMessage.chatMessageInfo.timeStamp;
            Debug.Log(string.Format("_lastChatTimeStamp : {0}", _lastChatTimeStamp));
        }

        _chatMessages.Add(makingMessage);
    }

    #endregion
}
