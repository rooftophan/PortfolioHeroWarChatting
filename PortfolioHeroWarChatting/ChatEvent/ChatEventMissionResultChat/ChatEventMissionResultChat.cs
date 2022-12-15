using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventMissionResultChat
{
    public static ChatServerMessageData GetMissionResultChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetMissionResultChat() chatSubString : {0}", chatSubString));
#endif

        MissionResultMessage chatResult = JsonMapper.ToObject<MissionResultMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.clearUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionResultChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerMissionResultMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.clearUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerMissionResultMessage(MissionResultMessage resultMessage)
    {
#if _CHATTING_LOG
        Debug.Log(string.Format("GetNotifyServerMissionResultMessage resultMessage companyName : {0}", resultMessage.companyName));
#endif

        ChatMessage retMessage = null;

        bool isMissionSuccess = false;
        int missionKind = -1;
        if (resultMessage.result == (int)MissionResult.Success) {
            isMissionSuccess = true;
        } else if (resultMessage.result == (int)MissionResult.Fail) {
            isMissionSuccess = false;
        }
        missionKind = resultMessage.missionKind;

        string[] inputValues = new string[2];
        inputValues[0] = ChattingController.Instance.Context.Text.GetMissionKindName(missionKind);
        inputValues[1] = ChattingController.Instance.Context.Text.GetMissionContentType((MissionContentType)resultMessage.missionContentType);

        if (isMissionSuccess) {
            if ((MissionContentType)resultMessage.missionContentType == MissionContentType.SpecialMission) {
                retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionSuccess, inputValues);
            } else {
                retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionSuccess, inputValues);
            }
        } else {
            if ((MissionContentType)resultMessage.missionContentType == MissionContentType.SpecialMission) {
                retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionFail, inputValues);
            } else {
                retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionFail, inputValues);
            }
        }

        retMessage.missionID = resultMessage.missionId;
        retMessage.missionContentType = resultMessage.missionContentType;

        return retMessage;
    }
}
