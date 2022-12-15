using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventMissionNewUser
{
    public static ChatServerMessageData GetMissionNewUserChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetMissionNewUserChat() chatSubString : {0}", chatSubString));
#endif

        MissionNewUserChatMessage chatResult = JsonMapper.ToObject<MissionNewUserChatMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.newUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionNewUserChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyMissionNewUserMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.newUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyMissionNewUserMessage(MissionNewUserChatMessage resultMessage)
    {
        ChatMessage retMessage = null;

        int missionKind = resultMessage.missionKind;

        string missionName = ChattingController.Instance.Context.Text.GetMissionKindName(missionKind);

        string[] inputValues = new string[3];
        inputValues[0] = missionName;
        inputValues[1] = resultMessage.nickname;
        inputValues[2] = ChattingController.Instance.Context.Text.GetMissionContentType((MissionContentType)resultMessage.missionContentType);

        if (resultMessage.missionContentType == (int)MissionContentType.SpecialMission) {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionParticipate, inputValues);
        } else {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionParticipate, inputValues);
        }

        return retMessage;
    }
}
