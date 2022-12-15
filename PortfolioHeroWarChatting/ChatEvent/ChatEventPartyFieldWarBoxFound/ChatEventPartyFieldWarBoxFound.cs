using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyFieldWarBoxFound
{
    public static ChatServerMessageData GetPartyFieldWarBoxFoundMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyFieldWarBoxFoundMessage() chatSubString : {0}", chatSubString));
#endif

        PartyFieldWarBoxFoundMessage chatResult = JsonMapper.ToObject<PartyFieldWarBoxFoundMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.findUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionFieldWarBoxChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerPartyFieldWarBoxFoundMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.findUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerPartyFieldWarBoxFoundMessage(PartyFieldWarBoxFoundMessage resultMessage)
    {
        ChatMessage retMessage = null;

        int missionKind = -1;
        missionKind = resultMessage.missionKind;

        string missionName = ChattingController.Instance.Context.Text.GetMissionKindName(missionKind);

        string[] inputValues = new string[3];
        inputValues[0] = missionName;
        inputValues[1] = ChattingController.Instance.Context.Text.GetMissionContentType(MissionManager.Instance.CurrentMissionType);

        inputValues[2] = resultMessage.nickname;

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox, inputValues);

        return retMessage;
    }
}
