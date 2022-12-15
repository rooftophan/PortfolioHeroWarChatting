using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventGuildShopGiftMessage
{
    public static ChatServerMessageData GetGuildShopGiftMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetGuildShopGiftMessage() chatSubString : {0}", chatSubString));
#endif

        CompanyShopGiftMessage chatResult = JsonMapper.ToObject<CompanyShopGiftMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.GuildShopGiftMessage;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerGuildShopGiftMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerGuildShopGiftMessage(CompanyShopGiftMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];
        inputValues[0] = resultMessage.nickname;
        inputValues[1] = resultMessage.shopIndex.ToString();
        retMessage = ChatEventNotifyMessage.GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopPresent, inputValues);

        return retMessage;
    }
}
