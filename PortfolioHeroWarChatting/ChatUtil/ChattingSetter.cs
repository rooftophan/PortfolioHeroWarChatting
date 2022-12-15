using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChattingSetter
{
    public static void SetMissionChatting(MissionContentType missionType, MissionKind missionKind, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    {
        if (ChattingController.Instance == null)
            return;

        if (ChattingController.Instance.CurSelectGuildInfo == null)
        {
            UnityEngine.Debug.Log(string.Format("SetMissionChatting ChattingController.Instance.CurSelectPartyInfo == null "));
            return;
        }

        var Context = ChattingController.Instance.Context;

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.msgIdx = (int)messageKey;
        chattingMessage.prm.Add("mission_kind_num", ((int)missionKind).ToString());

        chattingMessage.partyType = ChattingController.Instance.CurSelectGuildInfo.party_type;
        chattingMessage.partyNum = ChattingController.Instance.CurSelectGuildInfo.party_num;

        int curMissionType = (int)MissionManager.Instance.CurrentMissionType;
        string missionTypeName = Context.Text.GetMissionContentType(curMissionType);

        string userName = Context.User.userData.nickname;

        switch (missionType)
        {
            case MissionContentType.SpecialMission:
            case MissionContentType.EventMission:
            case MissionContentType.Raid:
                SetNormalMissionChatting(missionKind, chattingMessage, messageKey, userName, missionTypeName, curMissionType, inputValues);
                break;
        }

        chattingMessage.userID = Context.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(Context);

        ChatEventManager.SetUserIdChatMessage(chattingMessage, chattingMessage.userID, chattingMessage.connectId);

        ChattingController.Instance.ChatSocketManager.SendRequestPacket(ChattingPacketType.PartyChatV2Req, chattingMessage);
    }

    static void SetNormalMissionChatting(MissionKind missionKind, ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string userName, string missionTypeName, int curMissionType, string[] inputValues = null)
    {
        chattingMessage.prm.Add("missiontype_num", curMissionType.ToString());
        switch (messageKey) {
            case ChatNoticeMessageKey.CompanyNormalEliminationAzideWin:
            case ChatNoticeMessageKey.CompanyNormalEliminationAzideLose:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("contribution", inputValues[0]);
                break;

            case ChatNoticeMessageKey.CompanyNormalSecureBattleWin:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("enemy_user", inputValues[0]);
                chattingMessage.prm.Add("contribution", inputValues[1]);
                break;

            case ChatNoticeMessageKey.CompanyNormalRescueBattleWin:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("contribution", inputValues[0]);
                break;
            case ChatNoticeMessageKey.CompanyMissionParticipate:
                chattingMessage.prm.Add("user", userName);
                break;
        }
    }

    #region Party Chatting

    public static void SendPartyChatMessage(ChatNoticeMessageKey messageKey, ChatPartyType partyType, long partyId, params string[] values)
    {
        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.msgIdx = (int)messageKey;

        chattingMessage.partyType = (int)partyType;
        chattingMessage.partyNum = partyId;

        DataContext Context = ChattingController.Instance.Context;

        chattingMessage.userID = Context.User.userData.userId;
        chattingMessage.connectId = ChatHelper.GetChatChannelID(Context);

        SetPartyChatMessage(chattingMessage, messageKey, values);

        ChattingController.Instance.ChatSocketManager.SendRequestPacket(ChattingPacketType.PartyChatV2Req, chattingMessage);
    }

    static void SetPartyChatMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, params string[] values)
    {
        DataContext Context = ChattingController.Instance.Context;

        string userName = Context.User.userData.nickname;

        chattingMessage.prm.Add("roleIndex", values[0].ToString());

        switch (messageKey) {
            case ChatNoticeMessageKey.PartyMissionAttackSpot:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("explorespotbattle", values[1]);

                ChatEventManager.SetPartyHelpSpotChatMessage(chattingMessage, values[2]);
                break;
            case ChatNoticeMessageKey.PartyMissionDestinationSpot:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("explorespot", values[1]);
                break;
            case ChatNoticeMessageKey.PartyMissionOwnTreasure:
                chattingMessage.prm.Add("user", userName);
                chattingMessage.prm.Add("explorespot", values[1]);
                break;
            case ChatNoticeMessageKey.PartyMissionDestinationBossSpot:
                chattingMessage.prm.Add("user", userName);
                break;
        }

        ChatEventManager.SetUserIdChatMessage(chattingMessage, Context.User.userData.userId, ChatHelper.GetChatChannelID(Context));
    }

#endregion
}
