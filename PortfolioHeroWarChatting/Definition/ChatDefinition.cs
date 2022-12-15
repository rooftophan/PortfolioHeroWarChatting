using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatDefinition
{
	public enum ChatMessageType
	{
        None                                    = -1,
		ClientSendMessage					    = 0, // Use Client Only
		SheetChatNoticeMessage				    = 1,

		MissionStartMessage					    = 10,
		CompetitionMissionStart				    = 11,
        PartyMissionStart                       = 12,

        ShocktroopsChatResult				    = 101,
		MissionResultChat					    = 102,
		CompetitionMissionResultChat		    = 103,
        CompetitionMissionUnearnedWinChat       = 104,
        PartyMissionResultChat                  = 105,

        MissionNewUserChat					    = 201,
		CompetitionMissionNewUserChat		    = 202,
        PartyMissionParticipationChat           = 203,

		MissionExploreBossFound				    = 301,
        PartyMissionExploreBossFound            = 302,

        MissionFieldWarBoxChat				    = 401,
        PartyMissionFieldWarBoxChat             = 402,

        GuildNewMemberMessage                   = 501,
        GuildMemberClassPromotionMessage        = 502,
        GuildBasicShopInitMessage               = 503,
        GuildShopGiftMessage                    = 504,
        GuildShopRivalryOpenMessage             = 505,
        GuildShopMissionEntranceOpen            = 506,

        PartyInvitationChat                     = 601,
        PartyNewUserChat                        = 602,
        PartyUserOutChat                        = 603,
        PartyQuickChat                          = 604,

        ChangeGuildNotice					    = 1000,

        ObtainEquipmentItemChat                 = 1100,
        ObtainCardItemChat                      = 1101,
        EnhanceEquipmentItemChat                = 1102,
        UserLevelUpChat                         = 1103,
        TraceClearChat                          = 1104,
        BattleCenterClearChat                   = 1105,
        HeroLevelUpChat                         = 1106,
        SuccessRefineryFloorChat                = 1107,
        HeroChallengeNewHighScoreChat           = 1108,

        WhisperFriendChat                       = 1200,
        WhisperGuildChat                        = 1201,
        WhisperPartyInviteChat                  = 1202,
        WhisperPartyMissionStartAlarm           = 1203,

        GMChannelChat                           = 2000,
        GMSystemChatMessage                     = 2001,
    }

	public enum ChatMessageKind
	{
		None					= -1,
		ChannelChat				= 0,
		GuildChat 		        = 1,
		WhisperChat				= 2,
		AnnouncementChat		= 3,
        NoticeboardMessage      = 4,
        GuildSystemChat         = 5,
        PartyChat               = 6,
        MyPartyChat             = 7,
    }

	public enum ChatViewTextType
	{
		None				= -1,
		NormalText			= 0,
		NormalButtonText	= 1, // Use ShortCut
		ButtonBGText		= 2,
		ShortcutButton		= 3,
        TimeNormalText      = 4,
        CurrencyImgText     = 5,
        PartyAcceptButton   = 6,
        PartyDenyButton     = 7,
        PartyRoleImg        = 8,
	}

	public enum ChatButtonTextType
	{
		UserInfoButton		= 0,
		ItemInfoButton		= 1,
	}

	public enum PartMessageType
	{
		None				    = -1,
		NormalMessageType	    = 0,
		UserInfoType		    = 1,
		ItemInfoType		    = 2,
		MissionInfoType		    = 4,
		CompanyInfoType		    = 5,
		EnemyUserInfoType	    = 6,
        ShortcutType            = 7,
        TimeStampType           = 8,
        ChatColorType           = 9,
        CurrencyImgType         = 10,
        PartyAcceptType         = 11,
        PartyDenyType           = 12,
        PartyRoleImgType        = 13,
        PartyExploreHelpSpot    = 14,
        PartyQuickChat          = 15,
        HeroChallengeMaxScore   = 16,
	}

    public enum ChatShortcutType
    {
        CompanyCurrentMission       = 1,
        CompanyPastMission          = 2,
        CompanyDuel                 = 3,
        CompanyBulletinBoard        = 4,
        TrophyShop                  = 5,
        PartyMission                = 6,
        HeroChallenge               = 7,
    }

    public enum ChatRewardItemPlace
    {
        None                        = 0,
        CompanySupplyBox            = 1,
        CompanyTrophyStore          = 2,
        CompanySpecialStore         = 3,
    }

    public enum WhisperKind
    {
        Friend          = 0,
        Guild           = 1,
        Party           = 2,
    }

    public enum ChatColorType
    {
        None = 0,
        EnemyName,
    }

    public enum GuildUIType
    {
        Normal      = 0,
        ChatOnly    = 1,
        SystemOnly  = 2,
    }

    public enum PartyChatEventType
    {
        PartyMemberEntered          = 1,
        PartyMemberMoved            = 2,
        TeamResponse                = 3,
        ReadySelectHero             = 4,
        ReadyComplete               = 5,
        InviteAcceptResponse        = 6,
        ExploreObatinTreasure       = 7,
        UserFlagSet                 = 8,
        BattleStartEnd              = 9,
        BossDamage                  = 10,
        PartyMissionEnd             = 11,
        PartyGiveUp                 = 12,
    }

    public enum ChatParsingType
    {
        NormalText          = 0,
        GroupInfo           = 1,
        FieldInfo           = 2,
        NewLineNormalText   = 3,
    }

    public enum ChatSendType
    {
        None        = 0,
        Channel     = 1,
        Guild       = 2,
        Whisper     = 3,
        Party       = 4,
    }

    public enum PartyRequestType
    {
        ListSelect      = 1,
        Invitation      = 2,
    }

    public enum PartyQuickChatKind
    {
        PartyReady,
        PartyMission,
    }

    public enum ChatOtherView
    {
        None,
        PartyQuickReady,
        PartyQuickMission,
    }

    public enum GuildRaidEventType
    {
        SpotOtherOccupy     = 0,
    }
}

public enum MultiChatMessageTab
{
	None 				= -1,
	Chatting			= 0,
	Max,
}

public enum ChatMessageColorType
{
	Normal 								= 0,
	NormalSystemMessage 				= 1,
	CompanyNormal 						= 2,
	CompanyDefaultSystemMessage 		= 3,
	CompanyMissonSystemMessage 			= 4,
	CompanyMissonBattleFail 			= 5,
	CompanyMissonBattleSuccess	 		= 6,
	CompanyMissionSuccess		 		= 7,
	CompanyMissionFail					= 8,
	Whisper								= 9,
    NoticeMessage                       = 10,
}

public enum ChatMessageSendType
{
	Normal = 0,
	Self = 1,
	Company = 2
}

public enum ChattingPacketType
{
	Error 						= 200,	// PACKET_ID_ERROR

	LoginReq 					= 1001,	// PACKET_ID_LOGIN_REQ
	LoginRes 					= 1002,	// PACKET_ID_LOGIN_RES

	PingReq 					= 1003,	// PACKET_ID_PING_REQ
	PingRes 					= 1004,	// PACKET_ID_PING_RES

	GroupChangeReq 				= 1005, // PACKET_ID_GROUP_CHANGE_REQ
	GroupChangeRes 				= 1006, // PACKET_ID_GROUP_CHANGE_RES

	UserChatReq 				= 1007, // PACKET_ID_USER_CHAT_REQ
	UserChatRes 				= 1008, // PACKET_ID_USER_CHAT_RES
	UserChatNotify 				= 1010, // PACKET_ID_USER_CHAT_NOTIFY

	ServerChatNotify 			= 1012, // PACKET_ID_SERVER_CHAT_NOTIFY

	WideChatReq 				= 1013,
	WideChatRes 				= 1014,

	UserListChatReq 			= 1015, // PACKET_ID_USER_CHAT_USER_LIST_REQ
	UserListChatRes 			= 1016, // PACKET_ID_USER_CHAT_USER_LIST_RES

	Login2Req 					= 1017, // PACKET_ID_LOGIN_V2_REQ
	Login2Res 					= 1018, // PACKET_ID_LOGIN_V2_RES

	LangCodeChangeReq			= 1019, // PACKET_ID_LANG_CODE_CHANGE_REQ
	LangCodeChangeRes			= 1020, // PACKET_ID_LANG_CODE_CHANGE_RES

	GuildChangeReq 				= 1021,	// PACKET_ID_GUILD_NUM_CHANGE_REQ
	GuildChangeRes 				= 1022,	// PACKET_ID_GUILD_NUM_CHANGE_RES

	GuildChatReq 				= 1023, // PACKET_ID_USER_GUILD_CHAT_REQ
	GuildChatRes 				= 1024,	// PACKET_ID_USER_GUILD_CHAT_RES
	GuildChatNotify 			= 1026,	// PACKET_ID_USER_GUILD_CHAT_NOTIFY

	UserInfoListReq				= 1027,	// PACKET_ID_USER_INFO_LIST_REQ
	UserInfoListRes				= 1028,	// PACKET_ID_USER_INFO_LIST_RES

	WhisperReq 					= 1033, // PACKET_ID_USER_WHISPER_REQ
	WhisperRes 					= 1034, // PACKET_ID_USER_WHISPER_RES
	WhisperNotify 				= 1036, // PACKET_ID_USER_WHISPER_NOTIFY

	GuildChatV2Req				= 1039, // PACKET_ID_USER_GUILD_CHAT_V2_REQ
	GuildChatV2Res				= 1040, // PACKET_ID_USER_GUILD_CHAT_V2_RES

	PartyChangeReq 				= 1041, // PACKET_ID_USER_PARTY_UPDATE_REQ
	PartyChangeRes 				= 1042, // PACKET_ID_USER_PARTY_UPDATE_RES

	PartyChatReq 				= 1043, // PACKET_ID_USER_PARTY_CHAT_REQ
	PartyChatRes 				= 1044, // PACKET_ID_USER_PARTY_CHAT_RES
	PartyChatNotify 			= 1046, // PACKET_ID_USER_PARTY_CHAT_NOTIFY

	PartyChangeV2Req 			= 1053, // PACKET_ID_USER_PARTY_UPDATE_V2_REQ
	PartyChangeV2Res 			= 1054, // PACKET_ID_USER_PARTY_UPDATE_V2_RES

	PartyChatV2Req 				= 1055, // PACKET_ID_USER_PARTY_CHAT_V2_REQ
	PartyChatV2Res 				= 1056, // PACKET_ID_USER_PARTY_CHAT_V2_RES

	PartyChatV2Notify 			= 1058, // PACKET_ID_USER_PARTY_CHAT_V2_RES
	ServerPartyChatV2Notify 	= 1060, // PACKET_ID_SERVER_PARTY_CHAT_V2_NOTIFY
}

public enum ChattingErrorValue
{
	NotValidGameServerIDLoginKey = 203,
}

public enum ChatPartyType
{
	None = 0,
    ChatParty               = 2,
	ChatGuild				= 3,
    ChatGuildEvent          = 4,
    ChatUserParty           = 10,
    ChatEventParty          = 20,
    ChatSingleParty         = 30,
    ChatSingleEventParty    = 31,
}

