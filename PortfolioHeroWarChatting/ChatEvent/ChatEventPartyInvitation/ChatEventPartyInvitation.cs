using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventPartyInvitation
{
    public static ChatServerMessageData GetPartyInvitationChat(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! SetPartyInvitationChat() chatSubString : {0}", chatSubString));
#endif

        PartyRequestChatInfo chatResult = JsonMapper.ToObject<PartyRequestChatInfo>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.senderUserId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyInvitationChat;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyPartyInvitationMessage(chatResult);

        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.senderUserId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyPartyInvitationMessage(PartyRequestChatInfo resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[8];
        inputValues[0] = resultMessage.requestType.ToString();
        inputValues[1] = resultMessage.difficulty.ToString();
        inputValues[2] = resultMessage.senderUserId.ToString();
        inputValues[3] = resultMessage.senderNickname;
        inputValues[4] = resultMessage.senderLevel.ToString();
        inputValues[5] = resultMessage.senderUsingKey.ToString();
        inputValues[6] = resultMessage.senderRepresentHeroIndex.ToString();
        inputValues[7] = resultMessage.senderReputationScore.ToString();

        retMessage = ChatEventNotifyMessage.GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyInvitationAlarm, inputValues);

        return retMessage;
    }
}
