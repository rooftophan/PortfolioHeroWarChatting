using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelChatInformation
{
    #region Variables

    ChatNudgeNode _nudgeNode = new ChatNudgeNode();

    List<ChatMakingMessage> _channelChatMessages = new List<ChatMakingMessage>();

    #endregion

    #region Properties

    public ChatNudgeNode NudgeNode
    {
        get { return _nudgeNode; }
    }

    public List<ChatMakingMessage> ChannelChatMessages
    {
        get { return _channelChatMessages; }
    }

    #endregion

    #region Methods

    public void AddChannelChatMessage(ChatMakingMessage makingMessage)
    {
        if (_channelChatMessages.Count >= ChattingController.Instance.MaxChatLineCount) {
            _channelChatMessages.RemoveAt(0);
        }
        _channelChatMessages.Add(makingMessage);
    }

    public virtual void ReleaseChatMessages()
    {
        _nudgeNode.ConfirmNudge();
    }

    #endregion
}
