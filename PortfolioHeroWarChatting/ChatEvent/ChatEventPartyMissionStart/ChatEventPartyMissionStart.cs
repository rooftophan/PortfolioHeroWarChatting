using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyMissionStart
{
    public static ChatServerMessageData GetPartyStartMessageChatResult(JsonData subData, int partyType, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyStartMessageChatResult() chatSubString : {0}", chatSubString));
#endif

        ChatPartyMissionStartMessage chatResult = JsonMapper.ToObject<ChatPartyMissionStartMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionStart;
        serverMessageData.serverTypeData = chatResult;

        if (partyType == (int)ChatPartyType.ChatUserParty) {
            chatMsg = GetNotifyMyPartyStartMessage(chatResult);
        } else {
            chatMsg = GetNotifyPartyStartMessage(chatResult);
        }

        return serverMessageData;
    }

    static ChatMessage GetNotifyMyPartyStartMessage(ChatPartyMissionStartMessage chatResult)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[3];

        inputValues[0] = chatResult.partyName;
        inputValues[1] = chatResult.missionId.ToString();
        inputValues[2] = chatResult.partyId.ToString();

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.PartyFollowedMissionStartAlarm, inputValues);
        retMessage.missionID = chatResult.missionId;
        retMessage.missionContentType = chatResult.missionContentType;

        return retMessage;
    }

    static ChatMessage GetNotifyPartyStartMessage(ChatPartyMissionStartMessage chatResult)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[1];

        inputValues[0] = chatResult.partyName;

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.PartyMissionStart, inputValues);
        retMessage.missionID = chatResult.missionId;
        retMessage.missionContentType = chatResult.missionContentType;

        return retMessage;
    }
}
