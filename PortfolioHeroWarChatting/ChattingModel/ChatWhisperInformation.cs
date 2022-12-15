using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChatWhisperInformation
{
    #region Variables

    ChatNudgeNode _whisperRootNudge = new ChatNudgeNode();

    ChatNudgeNode _friendRootNudge = new ChatNudgeNode();
    ChatNudgeNode _guildRootNudge = new ChatNudgeNode();

    ChatWhisperTargetUserInfo _whisperTargetUserInfo = null;

    Dictionary<long /* connect id */, ChatWhisperUserData> _whisperUserMessages = new Dictionary<long, ChatWhisperUserData>();

    List<ChatWhisperGroupInfo> _whisperGroupInfos = new List<ChatWhisperGroupInfo>();

    #endregion

    #region Properties

    public ChatNudgeNode WhisperRootNudge
    {
        get { return _whisperRootNudge; }
    }

    public ChatNudgeNode FriendRootNudge
    {
        get { return _friendRootNudge; }
    }

    public ChatNudgeNode GuildRootNudge
    {
        get { return _guildRootNudge; }
    }

    public ChatWhisperTargetUserInfo WhisperTargetUserInfo
    {
        get { return _whisperTargetUserInfo; }
        set { _whisperTargetUserInfo = value; }
    }

    public Dictionary<long /* UserID */, ChatWhisperUserData> WhisperUserMessages
    {
        get { return _whisperUserMessages; }
    }

    public List<ChatWhisperGroupInfo> WhisperGroupInfos
    {
        get { return _whisperGroupInfos; }
    }

    #endregion

    #region Methods

    public void AddWhisperUserData(long connectId, ChatWhisperUserData addWhisperUser)
    {
        if(_whisperUserMessages.ContainsKey(connectId))
            return;

        _whisperRootNudge.AddChildNode(addWhisperUser.NudgeNode);

        _whisperUserMessages.Add(connectId, addWhisperUser);
    }

    public void RemoveWhiserUserData(long connectId)
    {
        if (!_whisperUserMessages.ContainsKey(connectId))
            return;

        _whisperUserMessages[connectId].NudgeNode.DeLinkAllNode();
        _whisperUserMessages.Remove(connectId);
    }

    public void AddWhisperUserMessage(ChatMakingMessage makingMessage, ChatWhisperMessage chatWhisperMessage, ChatDefinition.WhisperKind whisperKind, bool isRefresh = true)
    {
        UserModel userModel = ChattingController.Instance.Context.User;
        long connectId = -1;
        if (userModel.userData.userId == chatWhisperMessage.userID) {
            connectId = chatWhisperMessage.targetConnectID;
        } else {
            connectId = chatWhisperMessage.connectId;
        }

        List<ChatMakingMessage> makingMessages = null;
        ChatWhisperUserData whisperUserData = null;
        if (_whisperUserMessages.ContainsKey(connectId)) {
            whisperUserData = _whisperUserMessages[connectId];
            makingMessages = whisperUserData.UserWhisperMessage;
        } else {
            whisperUserData = new ChatWhisperUserData();
            whisperUserData.ConnectID = connectId;
            makingMessages = whisperUserData.UserWhisperMessage;
            AddWhisperUserData(connectId, whisperUserData);
        }

        if (whisperKind == ChatDefinition.WhisperKind.Friend) {
            if (!whisperUserData.IsFriend)
                whisperUserData.IsFriend = true;

            _friendRootNudge.AddChildNode(whisperUserData.NudgeNode);
        }

        whisperUserData.SetNoticeGuildID();

        if(whisperUserData.NoticeGuildID != -1) {
            _guildRootNudge.AddChildNode(whisperUserData.NudgeNode);
        }

        for (int i = 0; i < makingMessages.Count; i++) {
            if (makingMessages[i].chatMessageInfo.timeStamp == makingMessage.chatMessageInfo.timeStamp)
                return;
        }

        if (makingMessages.Count >= ChattingController.Instance.MaxWhisperChatLineCount) {
            makingMessages.RemoveAt(0);
        }

        bool isAddMessage = false;
        for (int i = 0; i < makingMessages.Count; i++) {
            if (makingMessages[i].chatMessageInfo.timeStamp > makingMessage.chatMessageInfo.timeStamp) {
                makingMessages.Insert(i, makingMessage);
                isAddMessage = true;
                break;
            }
        }

        if (!isAddMessage) {
            makingMessages.Add(makingMessage);
        }

        whisperUserData.LastMsgTimeStamp = makingMessage.chatMessageInfo.timeStamp;

        if (isRefresh) {
            if (makingMessage.chatMessageInfo.timeStamp > whisperUserData.ConfirmTimeStamp) {
                ChattingController.Instance.ChattingModel.AddWhisperNudgeCount(whisperUserData, makingMessage.chatMessageInfo.timeStamp);
            }
        }
    }

    public List<ChatMakingMessage> GetWhisperUserMessages(long connectId)
    {
        if (!_whisperUserMessages.ContainsKey(connectId))
            return null;

        return _whisperUserMessages[connectId].UserWhisperMessage;
    }

    public ChatWhisperUserData GetWhisperUserData(long connectId)
    {
        if (!_whisperUserMessages.ContainsKey(connectId))
            return null;

        return _whisperUserMessages[connectId];
    }

    public bool ExistWhisperMessage(long connectId)
    {
        if (_whisperUserMessages.ContainsKey(connectId)) {
            if (_whisperUserMessages[connectId].IsRequestChatServer)
                return true;
        }

        return false;
    }

    public void SetWhisperLastConfirmTimeStamp(long timeStamp)
    {
        if (_whisperTargetUserInfo == null)
            return;

        ChatWhisperUserData whisperUserData = null;
        if (_whisperUserMessages.ContainsKey(_whisperTargetUserInfo.targetConnectID)) {
            whisperUserData = _whisperUserMessages[_whisperTargetUserInfo.targetConnectID];
        } else {
            whisperUserData = new ChatWhisperUserData();
            whisperUserData.ConnectID = _whisperTargetUserInfo.targetConnectID;
            AddWhisperUserData(whisperUserData.ConnectID, whisperUserData);
        }

        List<FriendModel> friends = ChattingController.Instance.GetFriendList();
        for (int i = 0; i < friends.Count; i++) {
            long friendConnectId = friends[i].playerId <= 0 ? friends[i].userId : friends[i].playerId;
            if (friendConnectId == whisperUserData.ConnectID) {
                if (!whisperUserData.IsFriend)
                    whisperUserData.IsFriend = true;
            }
        }

        if (whisperUserData.IsFriend)
            _friendRootNudge.AddChildNode(whisperUserData.NudgeNode);

        whisperUserData.SetNoticeGuildID();

        if (whisperUserData.NoticeGuildID != -1)
            _guildRootNudge.AddChildNode(whisperUserData.NudgeNode);

        whisperUserData.ConfirmTimeStamp = timeStamp;
        whisperUserData.LastMsgTimeStamp = timeStamp;

        whisperUserData.NudgeNode.ConfirmNudge();
    }

    public void AddWhisperConfirmTimeInfo(long connectId, int messageCount, long timeStamp, long lastMsgTimeStamp)
    {
        List<FriendModel> friends = ChattingController.Instance.GetFriendList();

        ChatWhisperUserData whisperUserData = null;
        bool inputState = false;
        if (_whisperUserMessages.ContainsKey(connectId)) {
            whisperUserData = _whisperUserMessages[connectId];
        } else {
            whisperUserData = new ChatWhisperUserData();
            whisperUserData.ConnectID = connectId;
            inputState = true;
        }

        whisperUserData.NudgeNode.AddNudgeCount(messageCount);
        whisperUserData.ConfirmTimeStamp = timeStamp;
        whisperUserData.LastMsgTimeStamp = lastMsgTimeStamp;

        if(inputState) {
            AddWhisperUserData(connectId, whisperUserData);
        }

        for (int i = 0; i < friends.Count; i++) {
            long friendConnectId = friends[i].playerId <= 0 ? friends[i].userId : friends[i].playerId;
            if (friendConnectId == connectId) {
                if (!whisperUserData.IsFriend)
                    whisperUserData.IsFriend = true;
            }
        }

        if(whisperUserData.IsFriend)
            _friendRootNudge.AddChildNode(whisperUserData.NudgeNode);

        whisperUserData.SetNoticeGuildID();

        if(whisperUserData.NoticeGuildID != -1)
            _guildRootNudge.AddChildNode(whisperUserData.NudgeNode);
    }

    public void RemoveAllWhisperUserData()
    {
        List<long> connectKeys = _whisperUserMessages.Keys.ToList();
        for(int i = 0;i< connectKeys.Count;i++) {
            RemoveWhiserUserData(connectKeys[i]);
        }
    }

    public void RefreshWhiserUserIdList()
    {
        ReleaseWhisperGroupInfo();

        Dictionary<long, long> totalValidUserIDs = new Dictionary<long, long>();
        List<FriendModel> friends = ChattingController.Instance.GetFriendList();
        if (friends != null && friends.Count > 0) {
            ChatWhisperGroupInfo inputGroupInfo = new ChatWhisperGroupInfo();
            inputGroupInfo.GroupType = ChatWhisperDropDownType.WhisperFriend;
            for (int i = 0; i < friends.Count; i++) {
                long connectId = friends[i].playerId <= 0 ? friends[i].userId : friends[i].playerId;
                if (!totalValidUserIDs.ContainsKey(connectId)) {
                    totalValidUserIDs.Add(connectId, connectId);

                    ChatWhisperUserData whisperUserData = null;
                    if (!_whisperUserMessages.ContainsKey(connectId)) {
                        whisperUserData = new ChatWhisperUserData();
                        whisperUserData.ConnectID = connectId;
                        whisperUserData.IsFriend = true;
                        AddWhisperUserData(connectId, whisperUserData);
                    } else {
                        whisperUserData = _whisperUserMessages[connectId];
                        if(!whisperUserData.IsFriend)
                            whisperUserData.IsFriend = true;
                    }

                    _friendRootNudge.AddChildNode(whisperUserData.NudgeNode);

                    inputGroupInfo.NudgeNode.AddChildNode(whisperUserData.NudgeNode);
                }
            }

            _whisperGroupInfos.Add(inputGroupInfo);
        }

        Dictionary<long, ChatGuildJoinedInfo> chatGuildPartyGroup = ChattingController.Instance.ChatGuildGroupInfos;
        List<long> partyNumKeys = chatGuildPartyGroup.Keys.ToList();
        for (int i = 0; i < partyNumKeys.Count; i++) {
            long partyNum = partyNumKeys[i];

            ChatWhisperGroupInfo inputGroupInfo = new ChatWhisperGroupInfo();
            inputGroupInfo.GroupType = ChatWhisperDropDownType.WhisperGuild;
            inputGroupInfo.PartyNum = partyNum;
            ChatGuildJoinedInfo guildJoinInfo = chatGuildPartyGroup[partyNum];
            if (guildJoinInfo.playerIdList != null && guildJoinInfo.playerIdList.Count > 0) {
                for (int j = 0; j < guildJoinInfo.playerIdList.Count; j++) {
                    long connectId = guildJoinInfo.playerIdList[j];
                    if (!totalValidUserIDs.ContainsKey(connectId)) {
                        totalValidUserIDs.Add(connectId, connectId);

                        ChatWhisperUserData whisperUserData = null;
                        if (!_whisperUserMessages.ContainsKey(connectId)) {
                            whisperUserData = new ChatWhisperUserData();
                            whisperUserData.NoticeGuildID = partyNum;
                            whisperUserData.AddJoinGuildID(partyNum);
                            AddWhisperUserData(connectId, whisperUserData);
                        } else {
                            whisperUserData = _whisperUserMessages[connectId];
                            if(whisperUserData.NoticeGuildID == -1)
                                whisperUserData.NoticeGuildID = partyNum;

                            whisperUserData.AddJoinGuildID(partyNum);
                        }

                        if(whisperUserData.NoticeGuildID != -1)
                            _guildRootNudge.AddChildNode(whisperUserData.NudgeNode);

                        if (!whisperUserData.IsFriend && whisperUserData.NoticeGuildID == partyNum) {
                            inputGroupInfo.NudgeNode.AddChildNode(whisperUserData.NudgeNode);
                        }
                    }
                }
            }

            _whisperGroupInfos.Add(inputGroupInfo);
        }

        List<long> connectKeys = _whisperUserMessages.Keys.ToList();
        for (int i = 0; i < connectKeys.Count; i++) {
            if(!totalValidUserIDs.ContainsKey(connectKeys[i])) {
                RemoveWhiserUserData(connectKeys[i]);
            }
        }
    }

    public ChatWhisperGroupInfo GetWhisperGroupInfo(ChatWhisperDropDownType groupType, long partyNum = -1)
    {
        for (int i = 0; i < _whisperGroupInfos.Count; i++) {
            if(_whisperGroupInfos[i].GroupType == groupType && _whisperGroupInfos[i].PartyNum == partyNum)
                return _whisperGroupInfos[i];
        }

        return null;
    }

    public void RemoveWhisperGroupNudgeNotable(IChatNudgeNotable nudgeNotable)
    {
        for(int i = 0;i< _whisperGroupInfos.Count;i++) {
            _whisperGroupInfos[i].NudgeNode.RemoveNudgeNotable(nudgeNotable);
        }
    }

    public void ReleaseWhisperGroupInfo()
    {
        for (int i = 0; i < _whisperGroupInfos.Count; i++) {
            _whisperGroupInfos[i].NudgeNode.DeLinkCurNode();
        }

        _whisperGroupInfos.Clear();
    }

    public void ReleaseWhisperData()
    {
        RemoveAllWhisperUserData();
        _whisperTargetUserInfo = null;

        _whisperRootNudge.ClearNudgeNotable();
        _whisperRootNudge.DeLinkChildNodes();

        _friendRootNudge.ClearNudgeNotable();
        _friendRootNudge.DeLinkChildNodes();

        _guildRootNudge.ClearNudgeNotable();
        _guildRootNudge.DeLinkChildNodes();
    }

    #endregion
}
