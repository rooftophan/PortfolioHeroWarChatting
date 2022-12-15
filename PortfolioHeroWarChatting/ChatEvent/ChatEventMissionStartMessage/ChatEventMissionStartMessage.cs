using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventMissionStartMessage
{
    public static ChatServerMessageData GetMissionStartMessageChatResult(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
		Debug.Log (string.Format ("Chatting!!! SetMissionStartMessageChatResult() chatSubString : {0}", chatSubString));
#endif

        ChatMissionStartMessage chatResult = JsonMapper.ToObject<ChatMissionStartMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionStartMessage;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyMissionStartMessage(chatResult);

        return serverMessageData;
    }

    static ChatMessage GetNotifyMissionStartMessage(ChatMissionStartMessage chatResult)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];

        inputValues[0] = ChattingController.Instance.Context.Text.GetMissionKindName(chatResult.missionKind);
        inputValues[1] = ChattingController.Instance.Context.Text.GetMissionContentType((MissionContentType)chatResult.missionContentType);

        if (chatResult.missionContentType == (int)MissionContentType.SpecialMission) {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionStart, inputValues);
        } else {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.CompanyMissionStart, inputValues);
        }
        retMessage.missionID = chatResult.missionId;
        retMessage.missionContentType = chatResult.missionContentType;

        return retMessage;
    }
}
