using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class ChatEventManager
{
	#region Variables

	DataContext _context;

	List<IChatChangeCompanyNotice> _changeCompanyNoticeObs = new List<IChatChangeCompanyNotice>();

	#endregion

	#region Properties

	public DataContext Context
	{
		get{ return _context; }
		set{ _context = value; }
	}

	#endregion

	#region Methods

	public ChatMessage GetChatServerMessage(string serverMsg, int partyType = -1, long partyNum = -1, long serverTimeStamp = -1, bool isNotify = false)
	{
		return GetChatServerMessageJson (JsonMapper.ToObject (serverMsg), partyType, partyNum, serverTimeStamp, isNotify);
	}

	public ChatMessage GetChatServerMessageJson(JsonData jsonRoot, int partyType = -1, long partyNum = -1, long serverTimeStamp = -1, bool isNotify = false)
	{
		ChatMessage retMessage = null;
		if (jsonRoot == null)
			return null;

		if (!(jsonRoot as IDictionary).Contains ("messageType"))
			return null;

		int chatServerMessageType = (int)jsonRoot ["messageType"];

		#if _CHATTING_LOG
		Debug.Log (string.Format ("GetChatServerMessageJson chatServerMessageType : {0}", chatServerMessageType));
		#endif

		if (partyType == -1) {
			int partyTypeValue = (int)jsonRoot ["partyType"];
			long partyNumValue = -1;
			if (jsonRoot ["partyNum"].IsInt) {
				partyNumValue = (long)(int)jsonRoot ["partyNum"];
			} else if (jsonRoot ["partyNum"].IsLong) {
				partyNumValue = (long)jsonRoot ["partyNum"];
			}

			partyType = partyTypeValue;
			partyNum = partyNumValue;
		}

		long timeStamp = 0;
		if ((jsonRoot as IDictionary).Contains ("timestamp")) {
			if (jsonRoot ["timestamp"].IsInt) {
				timeStamp = (long)(int)jsonRoot ["timestamp"];
			} else if (jsonRoot ["timestamp"].IsLong) {
				timeStamp = (long)jsonRoot ["timestamp"];
			} else if (jsonRoot ["timestamp"].IsDouble) {
				timeStamp = (long)(double)jsonRoot ["timestamp"];
			}
		}

		JsonData subData = jsonRoot ["data"];

		ChatServerMessageData chatServerMsgData = null;

        switch ((ChatDefinition.ChatMessageType)chatServerMessageType) {
            //case ChatDefinition.ChatMessageType.MissionStartMessage:
            //    chatServerMsgData = ChatEventMissionStartMessage.GetMissionStartMessageChatResult(subData, out retMessage);
            //    break;
            //case ChatDefinition.ChatMessageType.CompetitionMissionStart:
            //    chatServerMsgData = SetCompetitionMissionStartChatResult(subData, out retMessage);
            //    break;
            case ChatDefinition.ChatMessageType.PartyMissionStart:
                chatServerMsgData = ChatEventPartyMissionStart.GetPartyStartMessageChatResult(subData, partyType, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.MissionResultChat:
                //chatServerMsgData = ChatEventMissionResultChat.GetMissionResultChat(subData, out retMessage);
                break;
            //case ChatDefinition.ChatMessageType.CompetitionMissionResultChat:
            //    chatServerMsgData = SetCompetitionMissionResultChat(subData, out retMessage);
            //    break;
            //case ChatDefinition.ChatMessageType.CompetitionMissionUnearnedWinChat:
            //    chatServerMsgData = SetCompetitionMissionUnearedWinChat(subData, out retMessage);
            //    break;
            case ChatDefinition.ChatMessageType.PartyMissionResultChat:
                chatServerMsgData = ChatEventPartyMissionResult.SetPartyMissionResultChat(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.MissionNewUserChat:
                //chatServerMsgData = ChatEventMissionNewUser.GetMissionNewUserChat(subData, out retMessage);
                break;
            //case ChatDefinition.ChatMessageType.CompetitionMissionNewUserChat:
            //    chatServerMsgData = SetCompetitionMissionNewUserChat(subData, out retMessage);
            //    break;
            case ChatDefinition.ChatMessageType.PartyInvitationChat:
                chatServerMsgData = ChatEventPartyInvitation.GetPartyInvitationChat(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.PartyNewUserChat:
                //chatServerMsgData = ChatEventPartyNewUser.GetPartyNewUserChat(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.PartyUserOutChat:
                chatServerMsgData = ChatEventPartyUserOut.GetPartyUserOutChat(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.MissionFieldWarBoxChat:
                //chatServerMsgData = ChatEventMissionFieldWarBoxFound.GetMissionFieldWarBoxFoundMessage(subData, out retMessage);
                break;
            //case ChatDefinition.ChatMessageType.PartyMissionFieldWarBoxChat:
            //    chatServerMsgData = ChatEventPartyFieldWarBoxFound.GetPartyFieldWarBoxFoundMessage(subData, out retMessage);
            //    break;
            case ChatDefinition.ChatMessageType.GuildNewMemberMessage:
                //chatServerMsgData = ChatEventGuildNewMemberMessage.GetGuildNewMemberMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.GuildMemberClassPromotionMessage:
                //chatServerMsgData = ChatEventGuildMemeberClassPromotion.GetGuildMemberClassPromotionMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.GuildBasicShopInitMessage:
                //chatServerMsgData = ChatEventGuildBasicShopInit.GetGuildBasicShopInitMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.GuildShopGiftMessage:
                //chatServerMsgData = ChatEventGuildShopGiftMessage.GetGuildShopGiftMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.GuildShopRivalryOpenMessage:
                //chatServerMsgData = ChatEventGuildRivalryShopOpen.GetGuildRivalryShopOpenMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.GuildShopMissionEntranceOpen:
                //chatServerMsgData = ChatEventGuildMissionEntranceGift.GetGuildMissionEntranceGiftMessage(subData, out retMessage);
                break;
            case ChatDefinition.ChatMessageType.ChangeGuildNotice:
                chatServerMsgData = GetChangeGuildNoticeMessage(subData);
                break;
        }

        if (retMessage != null) {
            retMessage.messageType = chatServerMessageType;
        }

        if (chatServerMsgData != null) {
            chatServerMsgData.partyType = partyType;
            chatServerMsgData.partyNum = partyNum;

            if (retMessage != null) {
				retMessage.partyType = partyType;
				retMessage.partyNum = partyNum;
			}

            if (chatServerMsgData.partyType == (int)ChatPartyType.ChatUserParty) {
                if (chatServerMsgData.serverMesssageType == ChatDefinition.ChatMessageType.PartyInvitationChat) {
                    if (isNotify) {
                        PartyRequestChatInfo partyInvitationInfo = chatServerMsgData.serverTypeData as PartyRequestChatInfo;
                        SetPartyInviteChatMessage(partyInvitationInfo);
                    }
                    return null;
                } else if (chatServerMsgData.serverMesssageType == ChatDefinition.ChatMessageType.PartyMissionStart) {
                    //if(isNotify) {
                    //    ChatPartyMissionStartMessage partyMissionStart = chatServerMsgData.serverTypeData as ChatPartyMissionStartMessage;
                    //    SetPartyMissionStartAlarmChatMessage(partyMissionStart);
                    //}
                }
            }

            if (isNotify) {
                ChattingController.Instance.NotifyChatServerMessage(chatServerMsgData);
            }
		}

		if (retMessage != null) {
			retMessage.timeStamp = timeStamp;
		}

		return retMessage;
	}

    void SetPartyInviteChatMessage(PartyRequestChatInfo partyInvitationInfo)
    {
        if (ChattingController.Instance.IsBattleState)
            return;

        if (MissionManager.Instance.CurrentMissionType == MissionContentType.PartyMission)
            return;

        if (UITopDepthNoticeMessage.Instance.IsEffectState)
            return;

        if(partyInvitationInfo.requestType == (int)PartyDefinitions.ChatRequestType.Invitation) {
            if (!GameSystem.Instance.Data.User.IsPartyInviteAlarmOn)
                return;
        }

        int limitTime = ChattingController.Instance.Context.Sheet.SheetPartyConfig[1].PartyAcceptLimitTime;
        int penaltyReputation = ChattingController.Instance.Context.Sheet.SheetPartyConfig[1].PartyReputationScoreRefusal;
        UITopDepthNoticeMessage.Instance.PartyNotifyPopupManager.ShowPartyNotifyPopup(ChattingController.Instance.Context.Text, partyInvitationInfo, (float)limitTime, penaltyReputation);
        UITopDepthNoticeMessage.Instance.PartyTopNoticeInfo.CloseTopNotice();
    }

    void SetPartyMissionStartAlarmChatMessage(ChatPartyMissionStartMessage partyMissionStart)
    {
        long partyId = partyMissionStart.partyId;

        int missionKind = partyMissionStart.missionKind;

        TextModel textModel = ChattingController.Instance.Context.Text;
        //PartySystemModel partyModel = ChattingController.Instance.Context.PartySystem;
        SheetPartyConfigRow configRow = GameSystem.Instance.Sheet.SheetPartyConfig[1];
        //UITopDepthNoticeMessage.Instance.PartyInviteMessage.ShowPartyStartAlarm(textModel, partyId, missionKind,
        //    (float)configRow.PartyPopupOffTime, (float)configRow.TimeLimitForFollowers);
    }

    public static void SetUserIdChatMessage(ChatMessage chatMessage, long userId, long connectId)
	{
		if (chatMessage == null)
			return;

		chatMessage.userID = userId;
        chatMessage.connectId = connectId;

        if (chatMessage.prm.ContainsKey ("user")) {
			if (!chatMessage.partMessageInfos.ContainsKey ("user")) {
                ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
                inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.UserInfoType;
                inputPartMessageInfo.partValues = new string[2];
                inputPartMessageInfo.partValues[0] = userId.ToString();
                inputPartMessageInfo.partValues[1] = connectId.ToString();

                chatMessage.partMessageInfos.Add ("user", inputPartMessageInfo);
			}
		}
	}

    public static void SetPartyHelpSpotChatMessage(ChatMessage chatMessage, string spotLayer)
    {
        if (chatMessage == null)
            return;

        if (chatMessage.prm.ContainsKey("explorespotbattle")) {
            if (!chatMessage.partMessageInfos.ContainsKey("explorespotbattle")) {
                ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
                inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.PartyExploreHelpSpot;
                inputPartMessageInfo.partValues = new string[2];
                inputPartMessageInfo.partValues[0] = spotLayer;
                inputPartMessageInfo.partValues[1] = chatMessage.userID.ToString();

                chatMessage.partMessageInfos.Add("explorespotbattle", inputPartMessageInfo);
            }
        }
    }

    public static void SetPartyRoleChatMessage(ChatMessage chatMessage, int role)
    {
        if (chatMessage == null)
            return;

        if (chatMessage.prm.ContainsKey("user")) {
            ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
            inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.PartyRoleImgType;
            inputPartMessageInfo.partValues = new string[1];
            inputPartMessageInfo.partValues[0] = role.ToString();

            if (!chatMessage.partMessageInfos.ContainsKey("role")) {
                chatMessage.partMessageInfos.Add("role", inputPartMessageInfo);
            }
        }
    }

    //   public ChatMessage GetNotifyChatEventMessage(ChatNoticeMessageKey messageKey, string[] inputValues = null)
    //   {
    //       ChatMessage chattingMessage = new ChatMessage();

    //       chattingMessage.msgIdx = (int)messageKey;

    //       switch (messageKey) {
    //           case ChatNoticeMessageKey.CompanyNewMemberEnter:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyMemberPromotion:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("MemberClass", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyTrophyBoxInitialization:
    //               break;
    //           case ChatNoticeMessageKey.GuildCashShopPresent:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("shopindex", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.GuildCashShopAllEntranceCount:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("mission_kind_num", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.GuildCashShopSuppliesShopCall:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               break;
    //       }

    //       return chattingMessage;
    //   }

    //   public ChatMessage GetNotifyMissionMessage(MissionContentType missionType, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    //{
    //	ChatMessage chattingMessage = new ChatMessage();

    //	chattingMessage.msgIdx = (int)messageKey;

    //       switch (missionType) {
    //           case MissionContentType.EventMission:
    //           case MissionContentType.Raid:
    //               SetNotifyNormalMissionMessage(chattingMessage, messageKey, inputValues);
    //               break;
    //           case MissionContentType.CompetitionMission:
    //               SetNotifyCompetitionMissionMessage(chattingMessage, messageKey, inputValues);
    //               break;
    //           case MissionContentType.PartyMission:
    //               SetNotifyPartyMissionMessage(chattingMessage, messageKey, inputValues);
    //               break;
    //       }

    //       return chattingMessage;
    //}

    //void SetNotifyNormalMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    //{
    //       chattingMessage.prm.Add("mission_kind", inputValues[0]);
    //       switch (messageKey) {
    //           case ChatNoticeMessageKey.CompanyMissionStart:
    //               break;
    //           case ChatNoticeMessageKey.CompanySpecialMissionStart:
    //               chattingMessage.prm.Add("missiontype", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyMissionSuccess:
    //               break;
    //           case ChatNoticeMessageKey.CompanySpecialMissionSuccess:
    //               chattingMessage.prm.Add("missiontype", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyMissionFail:
    //               break;
    //           case ChatNoticeMessageKey.CompanySpecialMissionFail:
    //               chattingMessage.prm.Add("missiontype", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyMissionParticipate:
    //               chattingMessage.prm.Add("user", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanySpecialMissionParticipate:
    //               chattingMessage.prm.Add("user", inputValues[1]);
    //               chattingMessage.prm.Add("missiontype", inputValues[2]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox:
    //               chattingMessage.prm.Add("missiontype", inputValues[1]);
    //               chattingMessage.prm.Add("user", inputValues[2]);
    //               break;
    //       }
    //   }

    //   void SetNotifyPartyMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    //   {
    //       switch (messageKey) {
    //           case ChatNoticeMessageKey.PartyJoinChat:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("partyname", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.PartyInvitationAlarm:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("partyname", inputValues[1]);
    //               chattingMessage.prm.Add("missionId", inputValues[2]);
    //               chattingMessage.prm.Add("partyId", inputValues[3]);
    //               chattingMessage.prm.Add("difficulty", inputValues[4]);
    //               chattingMessage.prm.Add("missionkind", inputValues[5]);
    //               break;
    //           case ChatNoticeMessageKey.PartyFollowedMissionStartAlarm:
    //               chattingMessage.prm.Add("partyname", inputValues[0]);
    //               chattingMessage.prm.Add("missionId", inputValues[1]);
    //               chattingMessage.prm.Add("partyId", inputValues[2]);
    //               break;
    //           case ChatNoticeMessageKey.PartyMissionStart:
    //               chattingMessage.prm.Add("partyname", inputValues[0]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox:
    //               chattingMessage.prm.Add("mission_kind", inputValues[0]);
    //               chattingMessage.prm.Add("missiontype", inputValues[1]);
    //               chattingMessage.prm.Add("user", inputValues[2]);
    //               break;
    //           case ChatNoticeMessageKey.PartyWithdrawal:
    //               chattingMessage.prm.Add("user", inputValues[0]);
    //               chattingMessage.prm.Add("partyname", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.PartyMissionEndSuccess:
    //               chattingMessage.prm.Add("mission_kind", inputValues[0]);
    //               chattingMessage.prm.Add("partyname", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.PartyMissionEndFail:
    //               chattingMessage.prm.Add("mission_kind", inputValues[0]);
    //               chattingMessage.prm.Add("partyname", inputValues[1]);
    //               break;
    //       }
    //   }

    //   void SetNotifyCompetitionMissionMessage(ChatMessage chattingMessage, ChatNoticeMessageKey messageKey, string[] inputValues = null)
    //{
    //       chattingMessage.prm.Add("mission_kind", inputValues[0]);
    //       switch (messageKey) {
    //           case ChatNoticeMessageKey.CompanyMissionStart:
    //               break;
    //           case ChatNoticeMessageKey.CompanyCompetitionWin:
    //               chattingMessage.prm.Add("enemy_company", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyCompetitionUnearnedWin:
    //               break;
    //           case ChatNoticeMessageKey.CompanyCompetitionLose:
    //               chattingMessage.prm.Add("enemy_company", inputValues[1]);
    //               break;
    //           case ChatNoticeMessageKey.CompanyMissionParticipate:
    //               chattingMessage.prm.Add("user", inputValues[1]);
    //               break;
    //       }
    //   }

    //    #region MissionStartMessage

    //    ChatServerMessageData SetMissionStartMessageChatResult(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetMissionStartMessageChatResult() chatSubString : {0}", chatSubString));
    //		#endif

    //		ChatMissionStartMessage chatResult = JsonMapper.ToObject<ChatMissionStartMessage> (chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData ();

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionStartMessage;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyMissionStartMessage(chatResult);

    //        return serverMessageData;
    //	}

    //	ChatMessage GetNotifyMissionStartMessage(ChatMissionStartMessage chatResult)
    //	{
    //		ChatMessage retMessage = null;

    //		string[] inputValues = new string[2];

    //		inputValues [0] = Context.Text.GetMissionKindName (chatResult.missionKind);
    //		inputValues [1] = Context.Text.GetMissionContentType((MissionContentType)chatResult.missionContentType);

    //        if(chatResult.missionContentType == (int)MissionContentType.SpecialMission) {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionStart, inputValues);
    //        } else {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.CompanyMissionStart, inputValues);
    //        }
    //        retMessage.missionID = chatResult.missionId;
    //        retMessage.missionContentType = chatResult.missionContentType;

    //        return retMessage;
    //	}

    //    #endregion

    //    #region PartyMissionStart

    //    ChatServerMessageData SetPartyStartMessageChatResult(JsonData subData, int partyType, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyStartMessageChatResult() chatSubString : {0}", chatSubString));
    //#endif

    //        ChatPartyMissionStartMessage chatResult = JsonMapper.ToObject<ChatPartyMissionStartMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionStart;
    //        serverMessageData.serverTypeData = chatResult;

    //        if(partyType == (int)ChatPartyType.ChatUserParty) {
    //            chatMsg = GetNotifyMyPartyStartMessage(chatResult);
    //        } else {
    //            chatMsg = GetNotifyPartyStartMessage(chatResult);
    //        }

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyMyPartyStartMessage(ChatPartyMissionStartMessage chatResult)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[3];

    //        inputValues[0] = chatResult.partyName;
    //        inputValues[1] = chatResult.missionId.ToString();
    //        inputValues[2] = chatResult.partyId.ToString();

    //        retMessage = GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.PartyFollowedMissionStartAlarm, inputValues);
    //        retMessage.missionID = chatResult.missionId;
    //        retMessage.missionContentType = chatResult.missionContentType;

    //        return retMessage;
    //    }

    //    ChatMessage GetNotifyPartyStartMessage(ChatPartyMissionStartMessage chatResult)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[1];

    //        inputValues[0] = chatResult.partyName;

    //        retMessage = GetNotifyMissionMessage((MissionContentType)chatResult.missionContentType, ChatNoticeMessageKey.PartyMissionStart, inputValues);
    //        retMessage.missionID = chatResult.missionId;
    //        retMessage.missionContentType = chatResult.missionContentType;

    //        return retMessage;
    //    }

    //    #endregion

    //    #region CompetitionMissionStartChat

    //    ChatServerMessageData SetCompetitionMissionStartChatResult(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetCompetitionMissionStartChatResult() chatSubString : {0}", chatSubString));
    //		#endif

    //		ChatCompetitionMissionStart chatResult = JsonMapper.ToObject<ChatCompetitionMissionStart> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompetitionMissionStart;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyCompetitionMissionStart(chatResult);

    //        return serverMessageData;
    //	}

    //	ChatMessage GetNotifyCompetitionMissionStart(ChatCompetitionMissionStart chatResult)
    //	{
    //		ChatMessage retMessage = null;

    //        string[] inputValues = new string[1];
    //        inputValues[0] = Context.Text.GetMissionKindName(chatResult.missionKind);

    //        retMessage = GetNotifyMissionMessage (MissionContentType.CompetitionMission, ChatNoticeMessageKey.CompanyMissionStart, inputValues);

    //        retMessage.missionID = chatResult.competitionMissionId;
    //        retMessage.missionContentType = (int)MissionContentType.CompetitionMission;

    //        return retMessage;
    //	}

    //    #endregion

    //    #region MissionResultChat

    //    ChatServerMessageData SetMissionResultChat(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetMissionResultChat() chatSubString : {0}", chatSubString));
    //		#endif

    //		MissionResultMessage chatResult = JsonMapper.ToObject<MissionResultMessage> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();
    //		serverMessageData.userId = chatResult.clearUserId;

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionResultChat;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerMissionResultMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.clearUserId);

    //        return serverMessageData;
    //	}

    //	ChatMessage GetNotifyServerMissionResultMessage(MissionResultMessage resultMessage)
    //	{
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("GetNotifyServerMissionResultMessage resultMessage companyName : {0}", resultMessage.companyName));
    //		#endif

    //		ChatMessage retMessage = null;

    //		bool isMissionSuccess = false;
    //		int missionKind = -1;
    //		if (resultMessage.result == (int)MissionResult.Success) {
    //			isMissionSuccess = true;
    //		} else if (resultMessage.result == (int)MissionResult.Fail) {
    //			isMissionSuccess = false;
    //		}
    //		missionKind = resultMessage.missionKind;

    //		string[] inputValues = new string[2];
    //		inputValues [0] = Context.Text.GetMissionKindName(missionKind);
    //        inputValues [1] = Context.Text.GetMissionContentType((MissionContentType)resultMessage.missionContentType);

    //		if (isMissionSuccess) {
    //            if((MissionContentType)resultMessage.missionContentType == MissionContentType.SpecialMission) {
    //                retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionSuccess, inputValues);
    //            } else {
    //                retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionSuccess, inputValues);
    //            }
    //		} else {
    //            if ((MissionContentType)resultMessage.missionContentType == MissionContentType.SpecialMission) {
    //                retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionFail, inputValues);
    //            } else {
    //                retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionFail, inputValues);
    //            }
    //		}

    //        retMessage.missionID = resultMessage.missionId;
    //        retMessage.missionContentType = resultMessage.missionContentType;

    //        return retMessage;
    //	}

    //    #endregion

    //    #region CompetitionMissionResultChat

    //    ChatServerMessageData SetCompetitionMissionResultChat(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetCompetitionMissionResultChat() chatSubString : {0}", chatSubString));
    //		#endif

    //		CompetitionMissionResultMessage chatResult = JsonMapper.ToObject<CompetitionMissionResultMessage> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();
    //		serverMessageData.userId = chatResult.clearUserId;

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompetitionMissionResultChat;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompetitionMissionResultMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.clearUserId);

    //        return serverMessageData;
    //	}

    //	ChatMessage GetNotifyServerCompetitionMissionResultMessage(CompetitionMissionResultMessage resultMessage)
    //	{
    //		ChatMessage retMessage = null;

    //		bool isMissionSuccess = false;
    //		int missionKind = -1;
    //		if (resultMessage.winCompanyId == resultMessage.companyId) {
    //			isMissionSuccess = true;
    //		} else {
    //			isMissionSuccess = false;
    //		}
    //		missionKind = resultMessage.missionKind;

    //		string missionName = Context.Text.GetMissionKindName (missionKind);

    //		string[] inputValues = new string[2];
    //		inputValues [0] = missionName;
    //        inputValues[1] = resultMessage.enemyCompanyName;

    //        if (isMissionSuccess) {
    //			retMessage = GetNotifyMissionMessage (MissionContentType.CompetitionMission, ChatNoticeMessageKey.CompanyCompetitionWin, inputValues);
    //		} else {
    //			retMessage = GetNotifyMissionMessage (MissionContentType.CompetitionMission, ChatNoticeMessageKey.CompanyCompetitionLose, inputValues);
    //		}

    //        retMessage.missionID = resultMessage.competitionMissionId;
    //        retMessage.missionContentType = (int)MissionContentType.CompetitionMission;

    //        return retMessage;
    //	}

    //    #endregion

    //    #region CompetitionMissionUnearnedWinChat

    //    ChatServerMessageData SetCompetitionMissionUnearedWinChat(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompetitionMissionResultChat() chatSubString : {0}", chatSubString));
    //#endif

    //        CompetitionMissionUnearnedWinMessage chatResult = JsonMapper.ToObject<CompetitionMissionUnearnedWinMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompetitionMissionUnearnedWinChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompetitionMissionUnearedWinMessage(chatResult);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompetitionMissionUnearedWinMessage(CompetitionMissionUnearnedWinMessage resultMessage)
    //    {
    //        Debug.Log(string.Format("GetNotifyServerCompetitionMissionUnearedWinMessage resultMessage missionKind : {0}", resultMessage.missionKind));

    //        ChatMessage retMessage = null;

    //        int missionKind = resultMessage.missionKind;

    //        string missionName = Context.Text.GetMissionKindName(missionKind);

    //        string[] inputValues = new string[1];
    //        inputValues[0] = missionName;

    //        retMessage = GetNotifyMissionMessage(MissionContentType.CompetitionMission, ChatNoticeMessageKey.CompanyCompetitionUnearnedWin, inputValues);
    //        retMessage.missionID = resultMessage.competitionMissionId;
    //        retMessage.missionContentType = (int)MissionContentType.CompetitionMission;

    //        return retMessage;
    //    }

    //    #endregion

    //    #region PartyMissionResultChat

    //    ChatServerMessageData SetPartyMissionResultChat(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyMissionResultChat() chatSubString : {0}", chatSubString));
    //#endif

    //        PartyMissionResultMessage chatResult = JsonMapper.ToObject<PartyMissionResultMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.clearUserId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionResultChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerPartyMissionResultMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.clearUserId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerPartyMissionResultMessage(PartyMissionResultMessage resultMessage)
    //    {
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("GetNotifyServerPartyMissionResultMessage resultMessage partyName : {0}", resultMessage.partyName));
    //#endif

    //        ChatMessage retMessage = null;

    //        bool isMissionSuccess = false;
    //        if (resultMessage.result == (int)MissionResult.Success) {
    //            isMissionSuccess = true;
    //        } else if (resultMessage.result == (int)MissionResult.Fail) {
    //            isMissionSuccess = false;
    //        }

    //        string missionName = Context.Text.GetMissionKindName(resultMessage.missionKind);

    //        string[] inputValues = new string[2];
    //        inputValues[0] = missionName;
    //        inputValues[1] = resultMessage.partyName;

    //        if (isMissionSuccess) {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.PartyMissionEndSuccess, inputValues);
    //        } else {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.PartyMissionEndFail, inputValues);
    //        }

    //        retMessage.missionID = resultMessage.missionId;
    //        retMessage.missionContentType = resultMessage.missionContentType;

    //        return retMessage;
    //    }

    //    #endregion

    //    #region MissionFieldWarBoxFound

    //    ChatServerMessageData SetMissionFieldWarBoxFoundMessage(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetMissionFieldWarBoxFoundMessage() chatSubString : {0}", chatSubString));
    //		#endif

    //		MissionFieldWarBoxFoundMessage chatResult = JsonMapper.ToObject<MissionFieldWarBoxFoundMessage> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();
    //		serverMessageData.userId = chatResult.findUserId;

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionFieldWarBoxChat;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerMissionFieldWarBoxFoundMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.findUserId);

    //        return serverMessageData;
    //	}

    //	ChatMessage GetNotifyServerMissionFieldWarBoxFoundMessage(MissionFieldWarBoxFoundMessage resultMessage)
    //	{
    //		ChatMessage retMessage = null;

    //		if (Context.MissionListManager.CurrentMission.guildId != resultMessage.companyId)
    //			return null;

    //		if (Context.MissionListManager.CurrentMission.missionKind != resultMessage.missionKind)
    //			return null;

    //		int missionKind = -1;
    //		missionKind = resultMessage.missionKind;

    //		string missionName = Context.Text.GetMissionKindName (missionKind);

    //		string[] inputValues = new string[3];
    //		inputValues [0] = missionName;
    //		inputValues [1] = Context.Text.GetMissionContentType(MissionManager.Instance.CurrentMissionType);

    //		inputValues [2] = resultMessage.nickname;

    //		retMessage = GetNotifyMissionMessage (MissionContentType.Raid, ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox, inputValues);

    //		return retMessage;
    //	}

    //    #endregion

    //    #region PartyFieldWarBoxFound

    //    ChatServerMessageData SetPartyFieldWarBoxFoundMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyFieldWarBoxFoundMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        PartyFieldWarBoxFoundMessage chatResult = JsonMapper.ToObject<PartyFieldWarBoxFoundMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.findUserId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyMissionFieldWarBoxChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerPartyFieldWarBoxFoundMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.findUserId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerPartyFieldWarBoxFoundMessage(PartyFieldWarBoxFoundMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        int missionKind = -1;
    //        missionKind = resultMessage.missionKind;

    //        string missionName = Context.Text.GetMissionKindName(missionKind);

    //        string[] inputValues = new string[3];
    //        inputValues[0] = missionName;
    //        inputValues[1] = Context.Text.GetMissionContentType(MissionManager.Instance.CurrentMissionType);

    //        inputValues[2] = resultMessage.nickname;

    //        retMessage = GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.CompanyNormalFieldWarTreasureBox, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region Guild NewMember

    //    ChatServerMessageData SetGuildNewMemberMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyNewMemberMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyNewMemberMessage chatResult = JsonMapper.ToObject<CompanyNewMemberMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.userId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyNewMemberMessage;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerGuildNewMemberMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerGuildNewMemberMessage(CompanyNewMemberMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[1];
    //        inputValues[0] = resultMessage.nickname;

    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.CompanyNewMemberEnter, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region GuildMemeberClassPromotion

    //    ChatServerMessageData SetCompanyMemberClassPromotionMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyMemberClassPromotionMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyMemberClassPromotionMessage chatResult = JsonMapper.ToObject<CompanyMemberClassPromotionMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.userId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyMemberClassPromotionMessage;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompanyMemberClassPromotionMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompanyMemberClassPromotionMessage(CompanyMemberClassPromotionMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[2];
    //        inputValues[0] = resultMessage.nickname;
    //        inputValues[1] = GameSystem.Instance.Data.Text.GetMemberClass(resultMessage.memberClass);

    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.CompanyMemberPromotion, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region GuildBasicShopInit

    //    ChatServerMessageData SetCompanyBasicShopInitMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyBasicShopInitMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyBasicShopInitMessage chatResult = JsonMapper.ToObject<CompanyBasicShopInitMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyBasicShopInitMessage;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompanyBasicShopInitMessage(chatResult);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompanyBasicShopInitMessage(CompanyBasicShopInitMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.CompanyTrophyBoxInitialization);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region CompanyShopGiftMessage

    //    ChatServerMessageData SetCompanyShopGiftMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyShopGiftMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyShopGiftMessage chatResult = JsonMapper.ToObject<CompanyShopGiftMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyShopGiftMessage;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompanyShopGiftMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompanyShopGiftMessage(CompanyShopGiftMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[2];
    //        inputValues[0] = resultMessage.nickname;
    //        inputValues[1] = resultMessage.shopIndex.ToString();
    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopPresent, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region CompanyRivalryShopOpen

    //    ChatServerMessageData SetCompanyRivalryShopOpenMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyRivalryShopOpenMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyRivalryShopOpenMessage chatResult = JsonMapper.ToObject<CompanyRivalryShopOpenMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyShopRivalryOpenMessage;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompanyRivalryShopOpenMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompanyRivalryShopOpenMessage(CompanyRivalryShopOpenMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[1];
    //        inputValues[0] = resultMessage.nickname;
    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopSuppliesShopCall, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region CompanyMissionEntranceGift

    //    ChatServerMessageData SetCompanyMissionEntranceGiftMessage(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetCompanyMissionEntranceGiftMessage() chatSubString : {0}", chatSubString));
    //#endif

    //        CompanyMissionEntranceGiftMessage chatResult = JsonMapper.ToObject<CompanyMissionEntranceGiftMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompanyShopMissionEntranceOpen;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyServerCompanyMissionEntranceGiftMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyServerCompanyMissionEntranceGiftMessage(CompanyMissionEntranceGiftMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[2];
    //        inputValues[0] = resultMessage.nickname;
    //        inputValues[1] = resultMessage.missionKind.ToString();
    //        retMessage = GetNotifyChatEventMessage(ChatNoticeMessageKey.GuildCashShopAllEntranceCount, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    #region ChangeGuildNotice

    ChatServerMessageData GetChangeGuildNoticeMessage(JsonData subData)
    {
        string chatSubString = (string)subData;
#if _CHATTING_LOG
        Debug.Log(string.Format("Chatting!!! GetChangeGuildNoticeMessage() chatSubString : {0}", chatSubString));
#endif

        ChatServerMessageData serverMessageData = new ChatServerMessageData();

        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.ChangeGuildNotice;
        serverMessageData.serverTypeData = chatSubString;

        NotifyChangeCompanyNotice(serverMessageData.partyType, serverMessageData.partyNum, chatSubString);

        return serverMessageData;
    }

    void NotifyChangeCompanyNotice(int partyType, long partyNum, string companyNotice)
    {
        for (int i = 0; i < _changeCompanyNoticeObs.Count; i++) {
            _changeCompanyNoticeObs[i].OnChangeCompanyNotice(partyType, partyNum, companyNotice);
        }
    }

    #endregion

    //    #region Mission NewUser

    //    ChatServerMessageData SetMissionNewUserChat(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetMissionNewUserChat() chatSubString : {0}", chatSubString));
    //		#endif

    //		MissionNewUserChatMessage chatResult = JsonMapper.ToObject<MissionNewUserChatMessage> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();
    //		serverMessageData.userId = chatResult.newUserId;

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.MissionNewUserChat;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyMissionNewUserMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.newUserId);

    //        return serverMessageData;
    //	}

    //    ChatMessage GetNotifyMissionNewUserMessage(MissionNewUserChatMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        int missionKind = resultMessage.missionKind;

    //        string missionName = Context.Text.GetMissionKindName(missionKind);

    //        string[] inputValues = new string[3];
    //        inputValues[0] = missionName;
    //        inputValues[1] = resultMessage.nickname;
    //        inputValues[2] = Context.Text.GetMissionContentType((MissionContentType)resultMessage.missionContentType);

    //        if(resultMessage.missionContentType == (int)MissionContentType.SpecialMission) {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanySpecialMissionParticipate, inputValues);
    //        } else {
    //            retMessage = GetNotifyMissionMessage((MissionContentType)resultMessage.missionContentType, ChatNoticeMessageKey.CompanyMissionParticipate, inputValues);
    //        }

    //        return retMessage;
    //    }

    //    #endregion

    //    #region Party Invitation Chat

    //    ChatServerMessageData SetPartyInvitationChat(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyInvitationChat() chatSubString : {0}", chatSubString));
    //#endif

    //        PartyRequestChatInfo chatResult = JsonMapper.ToObject<PartyRequestChatInfo>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.senderUserId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyInvitationChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyPartyInvitationMessage(chatResult);

    //        SetUserIdChatMessage(chatMsg, chatResult.senderUserId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyPartyInvitationMessage(PartyRequestChatInfo resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[8];
    //        inputValues[0] = resultMessage.requestType.ToString();
    //        inputValues[1] = resultMessage.difficulty.ToString();
    //        inputValues[2] = resultMessage.senderUserId.ToString();
    //        inputValues[3] = resultMessage.senderNickname;
    //        inputValues[4] = resultMessage.senderLevel.ToString();
    //        inputValues[5] = resultMessage.senderUsingKey.ToString();
    //        inputValues[6] = resultMessage.senderRepresentHeroIndex.ToString();
    //        inputValues[7] = resultMessage.senderReputationScore.ToString();

    //        retMessage = GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyInvitationAlarm, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region Party NewUser

    //    ChatServerMessageData SetPartyNewUserChat(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyMissionNewUserChat() chatSubString : {0}", chatSubString));
    //#endif

    //        PartyNewUserChatMessage chatResult = JsonMapper.ToObject<PartyNewUserChatMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.userId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyNewUserChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyPartyNewUserMessage(chatResult);

    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyPartyNewUserMessage(PartyNewUserChatMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[2];
    //        inputValues[0] = resultMessage.nickname;
    //        inputValues[1] = resultMessage.partyName;

    //        retMessage = GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyJoinChat, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region Party User Out

    //    ChatServerMessageData SetPartyUserOutChat(JsonData subData, out ChatMessage chatMsg)
    //    {
    //        string chatSubString = JsonMapper.ToJson(subData);
    //#if _CHATTING_LOG
    //        Debug.Log(string.Format("Chatting!!! SetPartyUserOutChat() chatSubString : {0}", chatSubString));
    //#endif

    //        PartyUserOutChatMessage chatResult = JsonMapper.ToObject<PartyUserOutChatMessage>(chatSubString);

    //        ChatServerMessageData serverMessageData = new ChatServerMessageData();
    //        serverMessageData.userId = chatResult.userId;

    //        serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.PartyUserOutChat;
    //        serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyPartyUserOutChatMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.userId);

    //        return serverMessageData;
    //    }

    //    ChatMessage GetNotifyPartyUserOutChatMessage(PartyUserOutChatMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        string[] inputValues = new string[2];
    //        inputValues[0] = resultMessage.nickname;
    //        inputValues[1] = resultMessage.partyName;

    //        retMessage = GetNotifyMissionMessage(MissionContentType.PartyMission, ChatNoticeMessageKey.PartyWithdrawal, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    //    #region CompetitionMission NewUser

    //    ChatServerMessageData SetCompetitionMissionNewUserChat(JsonData subData, out ChatMessage chatMsg)
    //	{
    //		string chatSubString = JsonMapper.ToJson (subData);
    //		#if _CHATTING_LOG
    //		Debug.Log (string.Format ("Chatting!!! SetMissionNewUserChat() chatSubString : {0}", chatSubString));
    //		#endif

    //		CompetitionMissionNewUserChatMessage chatResult = JsonMapper.ToObject<CompetitionMissionNewUserChatMessage> (chatSubString);

    //		ChatServerMessageData serverMessageData = new ChatServerMessageData ();
    //		serverMessageData.userId = chatResult.newUserId;

    //		serverMessageData.serverMesssageType = ChatDefinition.ChatMessageType.CompetitionMissionNewUserChat;
    //		serverMessageData.serverTypeData = chatResult;

    //        chatMsg = GetNotifyCompetitionMissionNewUserMessage(chatResult);
    //        SetUserIdChatMessage(chatMsg, chatResult.newUserId);

    //        return serverMessageData;
    //	}

    //    ChatMessage GetNotifyCompetitionMissionNewUserMessage(CompetitionMissionNewUserChatMessage resultMessage)
    //    {
    //        ChatMessage retMessage = null;

    //        int missionKind = resultMessage.competitionMissionKind;

    //        string missionName = Context.Text.GetMissionKindName(missionKind);

    //        string[] inputValues = new string[2];
    //        inputValues[0] = missionName;
    //        inputValues[1] = resultMessage.nickname;

    //        retMessage = GetNotifyMissionMessage(MissionContentType.CompetitionMission, ChatNoticeMessageKey.CompanyMissionParticipate, inputValues);

    //        return retMessage;
    //    }

    //    #endregion

    #endregion

    #region Change Company Notice Observer Methods

    public void AttachChangeCompanyNoticeOb(IChatChangeCompanyNotice inputChangeCompanyNoticeOb)
	{
		if(_changeCompanyNoticeObs.Contains(inputChangeCompanyNoticeOb))
			return;

		_changeCompanyNoticeObs.Add (inputChangeCompanyNoticeOb);
	}

	public void DetachChangeCompanyNoticeOb(IChatChangeCompanyNotice removeChangeCompanyNoticeOb)
	{
		if(!_changeCompanyNoticeObs.Contains(removeChangeCompanyNoticeOb))
			return;

		_changeCompanyNoticeObs.Remove (removeChangeCompanyNoticeOb);
	}

	public void ReleaseChangeCompanyNoticeOb()
	{
		_changeCompanyNoticeObs.Clear ();
	}

    #endregion

    #region Helper Methods

    public static Color GetChatColor(string colorText)
    {
        if(string.IsNullOrEmpty(colorText))
            return Color.white;

        string htmlColor = "";
        if (colorText[0] == '#') {
            if(colorText.Length > 7) {
                htmlColor = colorText;
            } else {
                htmlColor = string.Format("{0}FF", colorText);
            }
            
        } else {
            if (colorText.Length > 6) {
                htmlColor = string.Format("#{0}", colorText); ;
            } else {
                htmlColor = string.Format("#{0}FF", colorText);
            }
        }

        Color chatColor = Color.white;
        if (ColorUtility.TryParseHtmlString(htmlColor, out chatColor)) {
            Debug.Log(string.Format("!!!!!! SetChannelNoticeInfo noticeColor : {0}", chatColor));
        }

        return chatColor;
    }

    public static bool CheckValidChatMarket(int market)
    {
        if (market != -1 && Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor) {
            if (market != GameSystem.Instance.MarketIndex) return false;
        }

        return true;
    }

    public static void SetChatMissionCommonPrmInfo(DataContext data, ChatMessage chatMessage)
    {
        if (chatMessage.prm.ContainsKey("mission_kind_num")) {
            if (chatMessage.prm.ContainsKey("mission_kind")) {
                chatMessage.prm.Remove("mission_kind");
            }
            int chatMissionKind = int.Parse(chatMessage.prm["mission_kind_num"]);
            string missionKindName = data.Text.GetMissionKindName(chatMissionKind);
            chatMessage.prm.Add("mission_kind", missionKindName);
        }

        if (chatMessage.prm.ContainsKey("missiontype_num")) {
            if (chatMessage.prm.ContainsKey("missiontype")) {
                chatMessage.prm.Remove("missiontype");
            }
            int chatMissionType = int.Parse(chatMessage.prm["missiontype_num"]);
            string missionTypeName = data.Text.GetMissionContentType((MissionContentType)chatMissionType);
            chatMessage.prm.Add("missiontype", missionTypeName);
        }
    }

    public static void SetChatCompanyShopPrmInfo(DataContext data, ChatMessage chatMessage)
    {
        if (chatMessage.prm.ContainsKey("shopindex")) {
            int shopIndex = int.Parse(chatMessage.prm["shopindex"]);
            int currencyValue = data.Sheet.SheetCompanyShopList[shopIndex].GuildItemCount;
            int itemIndex = data.Sheet.SheetCompanyShopList[shopIndex].ItemIndex;
            string iconPath = data.Sheet.SheetCurrency[itemIndex].IconPath;
            chatMessage.prm.Add("Currency", currencyValue.ToString());

            if (chatMessage.partMessageInfos == null) {
                chatMessage.partMessageInfos = new Dictionary<string, ChatPartMessageInfo>();
            }

            if (!chatMessage.partMessageInfos.ContainsKey("Currency")) {
                ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
                inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.CurrencyImgType;
                inputPartMessageInfo.partValues = new string[1];
                inputPartMessageInfo.partValues[0] = iconPath;

                chatMessage.partMessageInfos.Add("Currency", inputPartMessageInfo);
            }
        }
    }

    public static void SetChatItemPrmInfo(ChatMessage chatMessage)
    {
        if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.ObtainEquipmentItemChat) {
            if (!chatMessage.prm.ContainsKey("Item") && chatMessage.prm.ContainsKey("grade") && chatMessage.prm.ContainsKey("itemIndex")) {
                int grade = int.Parse(chatMessage.prm["grade"]);
                int itemIndex = int.Parse(chatMessage.prm["itemIndex"]);

                chatMessage.prm.Add("Item", EquipmentModel.GetItemNameByIndex(ChattingController.Instance.Context, grade, itemIndex));
            }

            if (chatMessage.prm.ContainsKey("itemPlaceIndex")) {
                int itemPlaceIndex = int.Parse(chatMessage.prm["itemPlaceIndex"]);
                chatMessage.prm.Add("Place", ChattingController.Instance.GetItemShopName((ChatDefinition.ChatRewardItemPlace)itemPlaceIndex));
            }
        } else if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.ObtainCardItemChat) {
            if (!chatMessage.prm.ContainsKey("Item") && chatMessage.prm.ContainsKey("itemIndex")) {
                int itemIndex = int.Parse(chatMessage.prm["itemIndex"]);
                chatMessage.prm.Add("Item", CardModel.GetItemNameByCardIndex(ChattingController.Instance.Context, itemIndex));
            }

            if (chatMessage.prm.ContainsKey("itemPlaceIndex")) {
                int itemPlaceIndex = int.Parse(chatMessage.prm["itemPlaceIndex"]);
                chatMessage.prm.Add("Place", ChattingController.Instance.GetItemShopName((ChatDefinition.ChatRewardItemPlace)itemPlaceIndex));
            }
        } else if(chatMessage.messageType == (int)ChatDefinition.ChatMessageType.EnhanceEquipmentItemChat) {
            if (!chatMessage.prm.ContainsKey("Item") && chatMessage.prm.ContainsKey("grade") && chatMessage.prm.ContainsKey("itemIndex")) {
                int grade = int.Parse(chatMessage.prm["grade"]);
                int itemIndex = int.Parse(chatMessage.prm["itemIndex"]);

                chatMessage.prm.Add("Item", EquipmentModel.GetItemNameByIndex(ChattingController.Instance.Context, grade, itemIndex));
            }
        } else if(chatMessage.messageType == (int)ChatDefinition.ChatMessageType.PartyQuickChat) {
            if (chatMessage.prm.ContainsKey("partyquickindex")) {
                int partyQuickIndex = int.Parse(chatMessage.prm["partyquickindex"]);
                chatMessage.prm.Add("partyquick", ChattingController.Instance.Context.Text.GetText(string.Format("Party_QuickChat_{0}", partyQuickIndex)));
            }
        } else if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.HeroLevelUpChat) {
            if (chatMessage.prm.ContainsKey("heronametext")) {
                chatMessage.prm.Add("hero", ChattingController.Instance.Context.Text.GetHeroText(chatMessage.prm["heronametext"]));
            }
        } else if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.HeroChallengeNewHighScoreChat) {
            if (chatMessage.prm.ContainsKey("heronametext")) {
                chatMessage.prm.Add("hero", ChattingController.Instance.Context.Text.GetHeroText(chatMessage.prm["heronametext"]));
            }
        }
    }

    public static void SetChatTraceNamePrmInfo(ChatMessage chatMessage)
    {
        if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.TraceClearChat) {
            TextModel textModel = ChattingController.Instance.Context.Text;
            if (chatMessage.prm.ContainsKey("tracenamekey")) {
                string traceKey = chatMessage.prm["tracenamekey"];
                chatMessage.prm.Add("tracename", textModel.GetHeroText(traceKey));
            }
        }
    }

    public static void SetChatBattleCenterNamePrmInfo(ChatMessage chatMessage)
    {
        if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.BattleCenterClearChat) {
            TextModel textModel = ChattingController.Instance.Context.Text;
            if (chatMessage.prm.ContainsKey("theme")) {
                string theme = chatMessage.prm["theme"];
                chatMessage.prm.Add("battle", textModel.GetBattleCenterTheme(int.Parse(theme)));
            }
        }
    }

    public static void SetChatHeroChallengeMaxScoreTextPrmInfo(ChatMessage chatMessage)
    {
        if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.HeroChallengeNewHighScoreChat) {
            TextModel textModel = ChattingController.Instance.Context.Text;
            chatMessage.prm.Add("maxscore", textModel.GetText(TextKey.HeroChallenge_HighScore_Chat_Link));
        }
    }

    public static void SendCompanyChatting(ChatNoticeMessageKey messageKey, string[] inputValues = null, string[] addValues = null)
    {
        if (ChattingController.Instance == null)
            return;

        if (ChattingController.Instance.CurSelectGuildInfo == null) {
            UnityEngine.Debug.Log(string.Format("SendCompanyChatting ChattingController.Instance.CurSelectPartyInfo == null "));
            return;
        }

        ChatMessage chattingMessage = new ChatMessage();
        chattingMessage.msgIdx = (int)messageKey;

        chattingMessage.partyType = ChattingController.Instance.CurSelectGuildInfo.party_type;
        chattingMessage.partyNum = ChattingController.Instance.CurSelectGuildInfo.party_num;

        ChattingController.Instance.ChatSocketManager.SendRequestPacket(ChattingPacketType.PartyChatV2Req, chattingMessage);
    }

    public static void ChatLog(string log)
    {
#if _CHATTING_LOG
        Debug.Log(log);
#endif
    }

#endregion
}
