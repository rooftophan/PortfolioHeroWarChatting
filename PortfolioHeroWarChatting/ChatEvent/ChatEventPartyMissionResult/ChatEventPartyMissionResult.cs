using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyMissionResult
{
    public static ChatServerMessageData SetPartyMissionResultChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyMissionResultChat() chatSubString : {0}", chatSubString));
#endif

        PartyMissionResultMessage chatResult = JsonMapper.ToObject<PartyMissionResultMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.clearUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionResultChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerPartyMissionResultMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.clearUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerPartyMissionResultMessage(PartyMissionResultMessage resultMessage)
    {
#if _CHATTING_LOG
        Debug.Log(string.Format("GetNotifyServerPartyMissionResultMessage resultMessage partyName : {0}", resultMessage.partyName));
#endif

        ChatMessage retMessage = null;

        bool isMissionSuccess = false;
        if (resultMessage.result == (int)MissionResult.Success) {
            isMissionSuccess = true;
        } else if (resultMessage.result == (int)MissionResult.Fail) {
            isMissionSuccess = false;
        }

        string missionName = ChattingController.Instance.Context.Text.GetMissionKindName(resultMessage.missionKind);

        string[] inputValues = new string[2];
        inputValues[0] = missionName;
        inputValues[1] = resultMessage.partyName;

        if (isMissionSuccess) {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.PartyMissionEndSuccess, inputValues);
        } else {
            retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.PartyMissionEndFail, inputValues);
        }

        retMessage.missionID = resultMessage.missionId;
        retMessage.missionContentType = resultMessage.missionContentType;

        return retMessage;
    }
}
