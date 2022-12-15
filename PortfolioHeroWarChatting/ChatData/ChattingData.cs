using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ShocktroopsChatResult
{
	public long shocktroopsId;
	public long missionId;
	public int result;
	public long userId;
	public string nickname;
	public RankData[] scoreList;
}

public class MissionResultMessage
{
	public int companyType;
	public long companyId;
	public string companyName;
	public long missionId;
	public int missionContentType;
	public int missionKind;
	public int result;
	public long clearUserId;
    public long connectId;
    public string nickname;
}

public class CompetitionMissionResultMessage
{
	public long companyId;
	public long competitionMissionId;
	public string companyName;
	public int missionKind;
	public long winCompanyId;
	public long clearUserId;
	public string nickname;
    public string enemyCompanyName;
}

public class CompetitionMissionUnearnedWinMessage
{
    public long companyId;
    public long competitionMissionId;
    public int missionKind;
}

public class PartyMissionResultMessage
{
    public long missionId;
    public int missionContentType;
    public int missionKind;
    public int result;
    public long clearUserId;
    public long connectId;
    public string nickname;
    public string partyName;
}

public class MissionNewUserChatMessage
{
	public int companyType;
	public long companyId;
	public long missionId;
	public int missionContentType;
	public int missionKind;
	public long newUserId;
    public long connectId;
    public string nickname;
}

public class PartyNewUserChatMessage
{
    public long partyId;
    public long connectId;
    public long userId;
    public string nickname;
    public string partyName;
}

public class PartyUserOutChatMessage
{
    public long partyId;
    public long userId;
    public long connectId;
    public string nickname;
    public string partyName;
}

public class MissionExploreBossFoundMessage
{
	public int companyType;
	public long companyId;
	public long missionId;
	public int missionKind;
	public int missionContentType;
	public int positionX;
	public int positionY;
	public long findUserId;
	public string nickname;
}

public class MissionFieldWarBoxFoundMessage
{
	public int companyType;
	public long companyId;
	public long missionId;
	public int missionKind;
	public int nodeIndex;
	public long findUserId;
    public long connectId;
    public string nickname;
}

public class PartyFieldWarBoxFoundMessage
{
    public long missionId;
    public int missionContentType;
    public int missionKind;
    public int nodeIndex;
    public long findUserId;
    public long connectId;
    public string nickname;
}

public class CompanyNewMemberMessage
{
    public long userId;
    public long connectId;
    public long companyId;
    public string nickname;
}

public class CompanyMemberClassPromotionMessage
{
    public long userId;
    public long connectId;
    public long companyId;
    public string nickname;
    public int memberClass;
}

public class CompanyBasicShopInitMessage
{
    public long companyId;
    public string nickname;
    public string companyName;
}

public class CompanyShopGiftMessage
{
    public long userId;
    public long connectId;
    public long companyId;
    public string nickname;
    public int shopIndex;
}

public class GuildRivalryShopOpenMessage
{
    public long userId;
    public long connectId;
    public long companyId;
    public string nickname;
}

public class CompanyMissionEntranceGiftMessage
{
    public long userId;
    public long connectId;
    public long companyId;
    public string nickname;
    public int missionKind;
}

public class ChatMissionStartMessage
{
	public int companyType;
	public long companyId;
	public string companyName;
	public long missionId;
	public int missionKind;
	public int missionContentType;
}

public class ChatPartyMissionStartMessage
{
    public long partyId;
    public long missionId;
    public int missionKind;
    public int missionContentType;
    public string partyName;
}

public class ChatServerMessageData
{
	public ChatDefinition.ChatMessageType serverMesssageType;
	public int partyType = -1;
	public long partyNum = -1;
	public long userId;
    public int missionContentType;
	public object serverTypeData;
}

public class ChattingServerInfo
{
	public int result;
	public string ip;
	public string dns;
	public int port;
    public long connectId;
	public int gameServerId;
	public string gameServerName;
	public int loginKey;
	public int remainSec;
    public string chatWebUrl;
}

public class ChatBaseMessage
{
    public int chatKind = 0;
    public int messageType; // ChatDefinition.ChatMessageType Value
    public bool isSelfNotify = false;
	public int msgIdx = -1;

	public int partyType = -1;
	public long partyNum;
	public long timeStamp = 0;
	public long userID = 0;
    public long connectId = 0;
    public long missionID;
    public int missionContentType;

    public ChatDefinition.ChatMessageKind sendType = ChatDefinition.ChatMessageKind.None;

    public Dictionary<string /*fieldName */, string> prm = new Dictionary<string, string>();

    public void CopyChatBaseMessage(ChatBaseMessage message)
	{
        this.chatKind = message.chatKind;
        this.messageType = message.messageType;
        this.isSelfNotify = message.isSelfNotify;
		this.msgIdx = message.msgIdx;
		this.partyType = message.partyType;
		this.partyNum = message.partyNum;
		this.timeStamp = message.timeStamp;
		this.userID = message.userID;
        this.connectId = message.connectId;
        this.missionID = message.missionID;
        this.missionContentType = message.missionContentType;
        this.sendType = message.sendType;

        List<string> prmKeys = message.prm.Keys.ToList();
        for(int i = 0;i< prmKeys.Count;i++) {
            this.prm.Add(prmKeys[i], message.prm[prmKeys[i]]);
        }
    }
}

public class ChatPartMessageInfo
{
	public int partMessageType;
	public string[] partValues;
}

public class ChatMessage : ChatBaseMessage
{
	public int saveState = 1; // 1 : save, 2: not save
    public Dictionary<string /* fieldName */, ChatPartMessageInfo> partMessageInfos = new Dictionary<string, ChatPartMessageInfo>();

    public ChatPartMessageInfo GetPartMessageInfo(string fieldName)
	{
		if (partMessageInfos == null)
			return null;

		if (partMessageInfos.ContainsKey (fieldName))
			return partMessageInfos [fieldName];

		return null;
	}
}

public class ChatWhisperMessage : ChatMessage
{
    public int whisperKind;
    public long companyID = -1;
	public long targetUserID = -1;
    public long targetConnectID = -1;
	public string targetUserName = "";
	public string sendUserName = "";

    public void CopyWhisperMessage(ChatWhisperMessage whisperMessage)
    {
        this.whisperKind = whisperMessage.whisperKind;
        this.companyID = whisperMessage.companyID;
        this.targetUserID = whisperMessage.targetUserID;
        this.targetConnectID = whisperMessage.targetConnectID;
        this.targetUserName = whisperMessage.targetUserName;
        this.sendUserName = whisperMessage.sendUserName;
    }
}

public class ChatGMSMessage : ChatMessage
{
    public Color chatColor;
    public int market;
    public string messageText;
    public float fixedTime;
    public float gapTime;
}

public class ChatActionMessage : ChatBaseMessage
{

}

public class ChatUserListInfo
{
	public long guild_num;
	public long[] user_list;
}

public class ChatUserListRequestData
{
	public long[] userlist;
	public string message;
}

public class ChatPartyBaseInfo
{
	public long party_num = -1;
	public int party_type; // 1 : Shocktroops, 2 : Daily Mission, 3 : User Company
}

public class ChatGuildJoinedInfo : ChatPartyBaseInfo
{
	public string guildName = "";
	public string notice = "";
    public List<long> userIdList = new List<long>();
    public List<long> playerIdList = new List<long>();
    public List<List<MultiChatTextInfo>> guildNoticeMessages = new List<List<MultiChatTextInfo>>();
}

public class ChatPartyJoinedInfo : ChatPartyBaseInfo
{
    public string partyName = "";
    public List<long> userIdList = new List<long>();
}

public class ChatDailyMissionPartyInfo : ChatPartyBaseInfo
{
	public int missionKind;
}

public class ChatMakingMessage
{
    #region Variables

    MultiChatTextInfo _timeStampTextInfo = null;

    public Color messageColor;
	public int[] chatMessageKinds;
	public List<MultiChatTextInfo> multiChatTextInfoList;
    public List<MultiChatTextInfo> multiChatTextOtherViewList = null;

    public ChatMessage chatMessageInfo;

    public ChatDefinition.ChatOtherView chatOtherView = ChatDefinition.ChatOtherView.None;

    #endregion

    #region Properties

    public MultiChatTextInfo TimeStampTextInfo
    {
        get { return _timeStampTextInfo; }
        set { _timeStampTextInfo = value; }
    }

#endregion

    #region Methods

    public void CopyChatMakingMessage(ChatMakingMessage makingMessage)
	{
        this.chatMessageInfo = makingMessage.chatMessageInfo;

		this.messageColor = makingMessage.messageColor;
		this.chatMessageKinds = makingMessage.chatMessageKinds;

		if (makingMessage.multiChatTextInfoList != null) {
			this.multiChatTextInfoList = new List<MultiChatTextInfo> ();
			for (int i = 0; i < makingMessage.multiChatTextInfoList.Count; i++) {
				MultiChatTextInfo inputChatTextInfo = new MultiChatTextInfo ();
				inputChatTextInfo.CopyChatTextInfo (makingMessage.multiChatTextInfoList [i]);
				this.multiChatTextInfoList.Add (inputChatTextInfo);
			}
		}

        if (makingMessage.multiChatTextOtherViewList != null) {
            this.multiChatTextOtherViewList = new List<MultiChatTextInfo>();
            for (int i = 0; i < makingMessage.multiChatTextOtherViewList.Count; i++) {
                MultiChatTextInfo inputChatTextInfo = new MultiChatTextInfo();
                inputChatTextInfo.CopyChatTextInfo(makingMessage.multiChatTextOtherViewList[i]);
                this.multiChatTextOtherViewList.Add(inputChatTextInfo);
            }
        }
    }

    public void RefreshTimeStampText()
    {
        if(_timeStampTextInfo == null)
            return;

        float preWidth = _timeStampTextInfo.PartTextWidth;
        _timeStampTextInfo.ChatPartMessage = string.Format(" ({0})", UIMultiChatting.GetConfirmMessageTimeText(chatMessageInfo.timeStamp));
        _timeStampTextInfo.PartTextWidth = ChattingController.Instance.TimeSupportText.GetTextWidth(_timeStampTextInfo.ChatPartMessage);

        float gapWidth = _timeStampTextInfo.PartTextWidth - preWidth;
        if(gapWidth > 0.1f || gapWidth < -0.1f) {
            if(_timeStampTextInfo.UIPartMessage != null) {
                Vector3 messagePos = _timeStampTextInfo.UIPartMessage.transform.localPosition;
                _timeStampTextInfo.UIPartMessage.transform.localPosition = new Vector3(messagePos.x + (gapWidth * 0.5f), messagePos.y, 0f);
            }
        }
    }

    #endregion
}

public class GuildJoinedInfo
{
	public long guildId;
	public int tribe;
    public int level;
	public string guildName;
	public string notice;
    public long createTime;
    public long lastChatViewTime;
    public long[] userIdList;
    public long[] playerIdList;
}

public class PartyJoinedInfo
{
    public long partyId;
    public string partyName;
    public long createTime;
    public long lastChatViewTime;
    public long[] userIdList;
}

public class ChatButtonObjInfo
{
#region Variables

	UIChattingButton _uiChatButtonInfo;

#endregion

#region Properties

	public UIChattingButton UIChatButtonInfo
	{
		get{ return _uiChatButtonInfo; }
		set{ _uiChatButtonInfo = value; }
	}

#endregion
}

public class ChatPartyLastConfirmTime
{
#region Variables

	int _partyType;
	long _partyNum;
    long _partyJoinTimeStamp;
    long _lastConfirmTimeStamp;

#endregion

#region Properties

	public int PartyType
	{
		get{ return _partyType; }
		set{ _partyType = value; }
	}

	public long PartyNum
	{
		get{ return _partyNum; }
		set{ _partyNum = value; }
	}

    public long PartyJoinTimeStamp
    {
        get { return _partyJoinTimeStamp; }
        set { _partyJoinTimeStamp = value; }
    }

    public long LastConfirmTimeStamp
	{
		get{ return _lastConfirmTimeStamp; }
		set{ _lastConfirmTimeStamp = value; }
	}

#endregion
}

public class PartyChatMessageInfo
{
#region Variables

	int _partyType;
	long _partyNum;
	int _notConfirmCount = 0;
	int _announceNotConfirmCount = 0;

#endregion

#region Properties

	public int PartyType
	{
		get{ return _partyType ; }
		set{ _partyType = value; }
	}

	public long PartyNum
	{
		get{ return _partyNum ; }
		set{ _partyNum = value; }
	}

	public int NotConfirmCount
	{
		get{ return _notConfirmCount ; }
		set{ _notConfirmCount = value; }
	}

	public int AnnounceNotConfirmCount
	{
		get{ return _announceNotConfirmCount ; }
		set{ _announceNotConfirmCount = value; }
	}

#endregion
}

public class NoticeTextInfo
{
    public int id; // 1: ko, 2: en, 3: ja, 4: zh_hans, 5: zh_hants, 6: de, 7: fr
    public string text;
}

public class NoticeListInfo
{
	public long noticeId;
    public string beginAt;
    public string finishAt;
    public string color;
    public int market; // All : -1, Apple : 0, Google : 1
    public NoticeTextInfo[] textList;
}

public class ChatNoticeCurInfo
{
    public DateTime beginTime;
    public DateTime endTime;
    public Color color;
    public string noticeText;
}

public class ChatWhisperTargetUserInfo
{
    public int whisperKind;
    public long companyID = -1;
	public long targetUserID;
    public long targetConnectID;
	public string targetNickname;
}

public class ChatHttpPartyInfo
{
	public long party_num;
	public int party_type;
}

public class ChatHttpUserListInfo
{
	public long user_id;
	public string extra_data;
}

public class ChatHttpPartyUserList
{
	public long party_num;
	public int party_type;
	public ChatHttpUserListInfo[] user_list;
}

public class ChatUserInfoList
{
    public long channel_user_id;
    public int chat_group_num;
    public int guild_num;
    public long[] party_num_list;
    public string extra_data;
}

public class ChatUserInfoListPacket
{
    public long channel_user_id;
    public int chat_group_num;
}

public class ChatTimeStampComparer : IComparer<IChatTimeStamp>
{
	public int Compare(IChatTimeStamp firstValue, IChatTimeStamp secondValue)
	{
		if (firstValue.GetTimeStamp () > secondValue.GetTimeStamp ()) {
			return 1;
		} else if(firstValue.GetTimeStamp () < secondValue.GetTimeStamp ()) {
			return -1;
		}

		return 0;
	}
}

public class ChatShortcutData
{
    public ChatDefinition.ChatShortcutType shortcutType;
    public long partyNum;
    public MissionContentType missionContentType;
    public long missionID;
    public object shortcutValue;
}

public class ChatWhisperConfirmTimeStamp
{
#region Variables

    long _userID = -1;
    long _confirmTimeStamp = -1;
    int _notConfirmCount = 0;
    long _lastMsgTimeStamp = 0;

#endregion

#region Properties

    public long UserID
    {
        get { return _userID; }
        set { _userID = value; }
    }

    public long ConfirmTimeStamp
    {
        get { return _confirmTimeStamp; }
        set { _confirmTimeStamp = value; }
    }

    public int NotConfirmCount
    {
        get { return _notConfirmCount; }
        set { _notConfirmCount = value; }
    }

    public long LastMsgTimeStamp
    {
        get { return _lastMsgTimeStamp; }
        set { _lastMsgTimeStamp = value; }
    }

#endregion
}

public class ChatWhisperUserData
{
#region Variables

    long _connectID = -1;
    long _confirmTimeStamp = -1;
    long _lastMsgTimeStamp = -1;
    bool _isRequestChatServer = false;
    bool _isFriend = false;
    long _noticeGuildID = -1;
    ChatNudgeNode _nudgeNode = new ChatNudgeNode();

    List<long> _userJoinCompanyIDs = new List<long>();
    List<ChatMakingMessage> _userWhisperMessage = new List<ChatMakingMessage>();

#endregion

#region Properties

    public long ConnectID
    {
        get { return _connectID; }
        set { _connectID = value; }
    }

    public long ConfirmTimeStamp
    {
        get { return _confirmTimeStamp; }
        set { _confirmTimeStamp = value; }
    }

    public long LastMsgTimeStamp
    {
        get { return _lastMsgTimeStamp; }
        set { _lastMsgTimeStamp = value; }
    }

    public bool IsRequestChatServer
    {
        get { return _isRequestChatServer; }
        set { _isRequestChatServer = value; }
    }

    public bool IsFriend
    {
        get { return _isFriend; }
        set { _isFriend = value; }
    }

    public long NoticeGuildID
    {
        get { return _noticeGuildID; }
        set { _noticeGuildID = value;}
    }

    public ChatNudgeNode NudgeNode
    {
        get { return _nudgeNode; }
    }

    public List<long> UserJoinCompanyIDs
    {
        get { return _userJoinCompanyIDs; }
    }

    public List<ChatMakingMessage> UserWhisperMessage
    {
        get { return _userWhisperMessage; }
    }

#endregion

#region Methods

    public void SetNoticeGuildID()
    {
        if(_noticeGuildID != -1)
            return;

        List<ChatGuildJoinedInfo> companyPartyInfos = ChattingController.Instance.GetChatGuildJoinInfos();
        for (int i = 0; i < companyPartyInfos.Count; i++) {
            if (companyPartyInfos[i].playerIdList != null && companyPartyInfos[i].playerIdList.Count > 0) {
                for (int j = 0; j < companyPartyInfos[i].playerIdList.Count; j++) {
                    if (companyPartyInfos[i].playerIdList[j] == _connectID) {
                        if (_noticeGuildID == -1) {
                            _noticeGuildID = companyPartyInfos[i].party_num;
                        }
                        AddJoinGuildID(companyPartyInfos[i].party_num);
                        break;
                    }
                }
            }

        }
    }

    public void AddJoinGuildID(long companyID)
    {
        if(_userJoinCompanyIDs.Contains(companyID))
            return;

        _userJoinCompanyIDs.Add(companyID);
    }

#endregion
}

public class MultiChatListInfo
{
#region Variables

    Color _chatColor;
    List<MultiChatTextInfo> _chatTextInfos = null;

#endregion

#region Properties

    public Color ChatColor
    {
        get { return _chatColor; }
        set { _chatColor = value; }
    }

    public List<MultiChatTextInfo> ChatTextInfos
    {
        get { return _chatTextInfos; }
        set { _chatTextInfos = value; }
    }

#endregion
}

public class ChatSaveInfo
{
    public int guildUIType;
}

public class PartyRequestChatInfo
{
    public int requestType; // 1 : List Select, 2 : Invitation
    public int difficulty;
    public long senderUserId;
    public long connectId;
    public string senderNickname;
    public int senderLevel;
    public int senderUsingKey;
    public int senderRepresentHeroIndex;
    public int senderReputationScore;
    public long invitationTime;
}

public class ChatRepeatSendInfo
{
    public ChatDefinition.ChatMessageKind messageKind = ChatDefinition.ChatMessageKind.None;
    public string message = "";
    public long sendTime = -1;
    public int repeatCount = 0;

    public void ResetData()
    {
        messageKind = ChatDefinition.ChatMessageKind.None;
        message = "";
        sendTime = -1;
        repeatCount = 0;
    }
}

public class ChatSendCardObtainInfo
{
    public int itemIndex;
    public int rewardGrade;
    public CardType cardType;
}

public class ChatPreviewMessageInfo
{
    public int packetType;
    public int messageType;
    public Color chatTextColor;
    public List<MultiChatTextInfo> multiChatInfos;
}

public class ChatSendAchieveHeroLevelInfo
{
    public HeroModel hero;
}

