using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildChatInformation : PartyBaseChatInformation
{
    #region Variables

    int _announceNotConfirmCount = 0;
    long _partyJoinTimeStamp = -1;

    List<ChatMakingMessage> _systemChatMessages = new List<ChatMakingMessage>();
    List<ChatMakingMessage> _guildAnnounceMessage = new List<ChatMakingMessage>();

    #endregion

    #region Properties

    public int AnnounceNotConfirmCount
    {
        get { return _announceNotConfirmCount; }
        set { _announceNotConfirmCount = value; }
    }

    public long PartyJoinTimeStamp
    {
        get { return _partyJoinTimeStamp; }
        set { _partyJoinTimeStamp = value; }
    }

    public List<ChatMakingMessage> SystemChatMessages
    {
        get { return _systemChatMessages; }
    }

    public List<ChatMakingMessage> GuildAnnounceMessage
    {
        get { return _guildAnnounceMessage; }
    }

    #endregion

    #region Methods

    public void AddSystemChatMakingMessage(ChatMakingMessage makingMessage)
    {
        if (_systemChatMessages.Count >= ChattingController.Instance.MaxChatLineCount) {
            _systemChatMessages.RemoveAt(0);
        }

        if (_lastChatTimeStamp < makingMessage.chatMessageInfo.timeStamp)
            _lastChatTimeStamp = makingMessage.chatMessageInfo.timeStamp;

        _systemChatMessages.Add(makingMessage);
    }

    public override void ReleaseChatMessages()
    {
        base.ReleaseChatMessages();
        _systemChatMessages.Clear();
        _guildAnnounceMessage.Clear();
    }

    #endregion
}
