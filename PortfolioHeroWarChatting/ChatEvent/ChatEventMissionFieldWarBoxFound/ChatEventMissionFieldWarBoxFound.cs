using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventMissionFieldWarBoxFound
{
    public static ChatServerMessageData GetMissionFieldWarBoxFoundMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetMissionFieldWarBoxFoundMessage() chatSubString : {0}", chatSubString));
#endif

        MissionFieldWarBoxFoundMessage chatResult = JsonMapper.ToObject<MissionFieldWarBoxFoundMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.findUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionFieldWarBoxChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerMissionFieldWarBoxFoundMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.findUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerMissionFieldWarBoxFoundMessage(MissionFieldWarBoxFoundMessage resultMessage)
    {
        ChatMessage retMessage = null;

        if (ChattingController.Instance.Context.MissionListManager.CurrentMission.guildId != resultMessage.companyId)
            return null;

        if (ChattingController.Instance.Context.MissionListManager.CurrentMission.missionKind != resultMessage.missionKind)
            return null;

        int missionKind = -1;
        missionKind = resultMessage.missionKind;

        string missionName = ChattingController.Instance.Context.Text.GetMissionKindName(missionKind);

        string[] inputValues = new string[3];
        inputValues[0] = missionName;
        inputValues[1] = ChattingController.Instance.Context.Text.GetMissionContentType(MissionManager.Instance.CurrentMissionType);

        inputValues[2] = resultMessage.nickname;

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.Raid, ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox, inputValues);

        return retMessage;
    }
}
