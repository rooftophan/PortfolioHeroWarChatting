using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventGuildMissionEntranceGift
{
    public static ChatServerMessageData GetGuildMissionEntranceGiftMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetGuildMissionEntranceGiftMessage() chatSubString : {0}", chatSubString));
#endif

        CompanyMissionEntranceGiftMessage chatResult = JsonMapper.ToObject<CompanyMissionEntranceGiftMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.GuildShopMissionEntranceOpen;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerGuildMissionEntranceGiftMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerGuildMissionEntranceGiftMessage(CompanyMissionEntranceGiftMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];
        inputValues[0] = resultMessage.nickname;
        inputValues[1] = resultMessage.missionKind.ToString();
        retMessage = ChatEventNotifyMessage.GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopAllEntranceCount, inputValues);

        return retMessage;
    }
}
