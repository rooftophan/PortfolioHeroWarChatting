using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventGuildNewMemberMessage
{
    public static ChatServerMessageData GetGuildNewMemberMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetGuildNewMemberMessage() chatSubString : {0}", chatSubString));
#endif

        CompanyNewMemberMessage chatResult = JsonMapper.ToObject<CompanyNewMemberMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.userId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.GuildNewMemberMessage;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerGuildNewMemberMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerGuildNewMemberMessage(CompanyNewMemberMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[1];
        inputValues[0] = resultMessage.nickname;

        retMessage = ChatEventNotifyMessage.GetNotifyChatEventMessage(ChatNoticeMessageKey.CompanyNewMemberEnter, inputValues);

        return retMessage;
    }
}
