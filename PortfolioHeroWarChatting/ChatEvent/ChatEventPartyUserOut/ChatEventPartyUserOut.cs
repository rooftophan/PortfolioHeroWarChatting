using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyUserOut
{
    public static ChatServerMessageData GetPartyUserOutChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyUserOutChat() chatSubString : {0}", chatSubString));
#endif

        PartyUserOutChatMessage chatResult = JsonMapper.ToObject<PartyUserOutChatMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.userId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyUserOutChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyPartyUserOutChatMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyPartyUserOutChatMessage(PartyUserOutChatMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];
        inputValues[0] = resultMessage.nickname;
        inputValues[1] = resultMessage.partyName;

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyWithdrawal, inputValues);

        return retMessage;
    }
}
