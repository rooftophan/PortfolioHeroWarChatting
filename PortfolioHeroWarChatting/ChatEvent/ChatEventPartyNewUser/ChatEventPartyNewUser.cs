using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyNewUser
{
    public static ChatServerMessageData GetPartyNewUserChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyMissionNewUserChat() chatSubString : {0}", chatSubString));
#endif

        PartyNewUserChatMessage chatResult = JsonMapper.ToObject<PartyNewUserChatMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.userId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyNewUserChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyPartyNewUserMessage(chatResult);

        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyPartyNewUserMessage(PartyNewUserChatMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];
        inputValues[0] = resultMessage.nickname;
        inputValues[1] = resultMessage.partyName;

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyJoinChat, inputValues);

        return retMessage;
    }
}
