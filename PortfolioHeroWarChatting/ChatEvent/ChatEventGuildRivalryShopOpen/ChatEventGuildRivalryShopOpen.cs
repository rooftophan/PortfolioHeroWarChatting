using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventGuildRivalryShopOpen
{
    public static ChatServerMessageData GetGuildRivalryShopOpenMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetGuildRivalryShopOpenMessage() chatSubString : {0}", chatSubString));
#endif

        GuildRivalryShopOpenMessage chatResult = JsonMapper.ToObject<GuildRivalryShopOpenMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.GuildShopRivalryOpenMessage;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerGuildRivalryShopOpenMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerGuildRivalryShopOpenMessage(GuildRivalryShopOpenMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[1];
        inputValues[0] = resultMessage.nickname;
        retMessage = ChatEventNotifyMessage.GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopSuppliesShopCall, inputValues);

        return retMessage;
    }
}
