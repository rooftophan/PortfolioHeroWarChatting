using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventGuildMemeberClassPromotion
{
    public static ChatServerMessageData GetGuildMemberClassPromotionMessage(JsonData subData, out ChatMessage chatMsg)
    {
        string chatSubString = JsonMapper.ToJson(subData);
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetGuildMemberClassPromotionMessage() chatSubString : {0}", chatSubString));
#endif

        CompanyMemberClassPromotionMessage chatResult = JsonMapper.ToObject<CompanyMemberClassPromotionMessage>(chatSubString);

        ChatServerMessageData serverMessageData = new ChatServerMessageData();
        serverMessageData.userId = chatResult.userId;

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.GuildMemberClassPromotionMessage;
        serverMessageData.serverTypeData = chatResult;

        chatMsg = GetNotifyServerGuildMemberClassPromotionMessage(chatResult);
        ChatEventManager.SetUserIdChatMessage(chatMsg, chatResult.userId, chatResult.connectId);

        return serverMessageData;
    }

    static ChatMessage GetNotifyServerGuildMemberClassPromotionMessage(CompanyMemberClassPromotionMessage resultMessage)
    {
        ChatMessage retMessage = null;

        string[] inputValues = new string[2];
        inputValues[0] = resultMessage.nickname;
        inputValues[1] = GameSystem.Instance.Data.Text.GetMemberClass(resultMessage.memberClass);

        retMessage = ChatEventNotifyMessage.GetNotifyChatEventMessage(ChatNoticeMessageKey.CompanyMemberPromotion, inputValues);

        return retMessage;
    }
}
