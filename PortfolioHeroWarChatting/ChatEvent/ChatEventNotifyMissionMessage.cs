using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatEventNotifyMessage
{
    public  static ChatMessage GetNotifyChatEventMessage(ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        ChatMessage chattingMessage = new ChatMessage();

        chattingMessage.msgIdx = (int)messageKey;

        switch (messageKey) {
            case ChatNoticeMessageKey.CompanyNewMemberEnter:
                chattingMessage.prm.Add("user", inputValues[0]);
                break;
            case ChatNoticeMessageKey.CompanyMemberPromotion:
                chattingMessage.prm.Add("user", inputValues[0]);
                chattingMessage.prm.Add("MemberClass", inputValues[1]);
                break;
            case ChatNoticeMessageKey.GuildCashShopPresent:
                chattingMessage.prm.Add("user", inputValues[0]);
                chattingMessage.prm.Add("shopindex", inputValues[1]);
                break;
            case ChatNoticeMessageKey.GuildCashShopAllEntranceCount:
                chattingMessage.prm.Add("user", inputValues[0]);
                chattingMessage.prm.Add("mission_kind_num", inputValues[1]);
                break;
            case ChatNoticeMessageKey.GuildCashShopSuppliesShopCall:
                chattingMessage.prm.Add("user", inputValues[0]);
                break;
        }

        return chattingMessage;
    }

    public static ChatMessage GetNotifyMissionMessage(MissionContentType missionType, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        ChatMessage chattingMessage = new ChatMessage();

        chattingMessage.msgIdx = (int)messageKey;

        switch (missionType) {
            case MissionContentType.EventMission:
            case MissionContentType.Raid:
                SetNotifyNormalMissionMessage(chattingMessage, messageKey, inputValues);
                break;
            case MissionContentType.CompetitionMission:
                SetNotifyCompetitionMissionMessage(chattingMessage, messageKey, inputValues);
                break;
            case MissionContentType.PartyMission:
                SetNotifyPartyMissionMessage(chattingMessage, messageKey, inputValues);
                break;
        }

        return chattingMessage;
    }

    static void SetNotifyNormalMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        chattingMessage.prm.Add("mission_kind", inputValues[0]);
        switch (messageKey) {
            case ChatNoticeMessageKey.CompanySpecialMissionStart:
                chattingMessage.prm.Add("missiontype", inputValues[1]);
                break;
            case ChatNoticeMessageKey.CompanyMissionSuccess:
                break;
            case ChatNoticeMessageKey.CompanySpecialMissionSuccess:
                chattingMessage.prm.Add("missiontype", inputValues[1]);
                break;
            case ChatNoticeMessageKey.CompanySpecialMissionFail:
                chattingMessage.prm.Add("missiontype", inputValues[1]);
                break;
            case ChatNoticeMessageKey.CompanyMissionParticipate:
                chattingMessage.prm.Add("user", inputValues[1]);
                break;
            case ChatNoticeMessageKey.CompanySpecialMissionParticipate:
                chattingMessage.prm.Add("user", inputValues[1]);
                chattingMessage.prm.Add("missiontype", inputValues[2]);
                break;
            case ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox:
                chattingMessage.prm.Add("missiontype", inputValues[1]);
                chattingMessage.prm.Add("user", inputValues[2]);
                break;
        }
    }

    static void SetNotifyPartyMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        switch (messageKey) {
            case ChatNoticeMessageKey.PartyJoinChat:
                chattingMessage.prm.Add("user", inputValues[0]);
                chattingMessage.prm.Add("partyname", inputValues[1]);
                break;
            case ChatNoticeMessageKey.PartyInvitationAlarm:
                chattingMessage.prm.Add("requestType", inputValues[0]);
                chattingMessage.prm.Add("difficulty", inputValues[1]);
                chattingMessage.prm.Add("senderUserId", inputValues[2]);
                chattingMessage.prm.Add("senderNickname", inputValues[3]);
                chattingMessage.prm.Add("senderLevel", inputValues[4]);
                chattingMessage.prm.Add("senderUsingKey", inputValues[5]);
                chattingMessage.prm.Add("senderRepresentHeroIndex", inputValues[6]);
                chattingMessage.prm.Add("senderReputationScore", inputValues[7]);
                break;
            case ChatNoticeMessageKey.PartyMissionStart:
                chattingMessage.prm.Add("partyname", inputValues[0]);
                break;
        }
    }

    static void SetNotifyCompetitionMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        chattingMessage.prm.Add("mission_kind", inputValues[0]);
        switch (messageKey) {
            case ChatNoticeMessageKey.CompanyMissionParticipate:
                chattingMessage.prm.Add("user", inputValues[1]);
                break;
        }
    }
}
