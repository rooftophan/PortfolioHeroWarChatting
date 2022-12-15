using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using LitJson;

public enum WhisperTabButtonType
{
    Friend,
    Guild,
}

public enum ChatWhisperDropDownType
{
	WhisperFriend,
	WhisperGuild,
}


public class ChatWhisperDropDownInfo
{
	#region Variables

	int _dropDownIndex;
	ChatWhisperDropDownType _dropDownType;
	object _dropDownObject;

	#endregion

	#region Properties

	public int DropDownIndex
	{
		get{ return _dropDownIndex; }
		set{ _dropDownIndex = value; }
	}

	public ChatWhisperDropDownType DropDownType
	{
		get{ return _dropDownType; }
		set{ _dropDownType = value; }
	}

	public object DropDownObject
	{
		get{ return _dropDownObject; }
		set{ _dropDownObject = value; }
	}

    #endregion
}

public class UIChatWhisperTargetPopup : MonoBehaviour, IBackKeyMethod
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] UIDropdownExpand _targetDropDown = default(UIDropdownExpand);
	[SerializeField] Button _closeButton = default(Button);
	[SerializeField] Transform _contentsTrans = default(Transform);
	[SerializeField] UIWhisperUserInfoCell _uiUserInfoCell = default(UIWhisperUserInfoCell);
	[SerializeField] GameObject _notExistFriendText = default(GameObject);
    [SerializeField] GameObject _notExistGuildText = default(GameObject);

    [SerializeField] UIChatTabButton _friendTabButton = default(UIChatTabButton);
    [SerializeField] UIChatTabButton _guildTabButton = default(UIChatTabButton);

#pragma warning restore 649

    #endregion

    #region Variables

    int _curSelectIndex = 0;

	ChatHttpUserListInfo[] _curUserListInfo;

	Action<UIWhisperUserInfoCell> _onSelectUserInfoCell = null;

	List<ChatWhisperDropDownInfo> _dropDownInfos = new List<ChatWhisperDropDownInfo>();
	List<UIWhisperUserInfoCell> _curUserInfoCells = new List<UIWhisperUserInfoCell>();

    long _curCompanyID = -1;
    bool _isRequestState = false;
    UIWhisperUserInfoCell _curSelectUserInfoCell = null;
    bool _isRequestUserInfo = false;

    WhisperTabButtonType _tabButtonType = WhisperTabButtonType.Friend;
    ChatGuildJoinedInfo _guildJoinedInfo = null;

    #endregion

    #region Properties

    public UIChatTabButton FriendTabButton
    {
        get { return _friendTabButton; }
    }

    public UIChatTabButton GuildTabButton
    {
        get { return _guildTabButton; }
    }

    public Action<UIWhisperUserInfoCell> OnSelectUserInfoCell
	{
		get{ return _onSelectUserInfoCell; }
		set{ _onSelectUserInfoCell = value; }
	}

    public ChatGuildJoinedInfo GuildJoinedInfo
    {
        get { return _guildJoinedInfo; }
        set { _guildJoinedInfo = value; }
    }

    #endregion

    #region MonoBehaviour Methods

    void Awake()
	{
		_closeButton.onClick.AddListener (() => OnClosePopup ());
        _targetDropDown.OnCreateComplete = OnCreateComplete;
        _targetDropDown.OnDropdownAddItem = OnDropdownAddItem;
        _targetDropDown.OnDropdownRemoveItem = OnDropdownRemoveItem;
    }

	void OnEnable()
	{
        BackKeyService.Register(this);
        ChattingController.Instance.SetEnableScrollTouch(false);
	}

    void OnDisable()
    {
        BackKeyService.UnRegister(this);
        ChattingController.Instance.SetEnableScrollTouch(true);
        ChattingController.Instance.ChattingModel.WhisperInformation.ReleaseWhisperGroupInfo();
    }

    #endregion

    #region Methods

    public void InitWhisperTargetPopup()
	{
        ChattingController.Instance.ChattingModel.WhisperInformation.RefreshWhiserUserIdList();

        List<ChatGuildJoinedInfo> chatGuildInfos = ChattingController.Instance.GetChatGuildInfoList();
        if(chatGuildInfos.Count > 0) {
            _guildJoinedInfo = chatGuildInfos[0];
        }

        _friendTabButton.TabButtonType = WhisperTabButtonType.Friend;
        _friendTabButton.TabButton.onClick.RemoveAllListeners();
        _friendTabButton.TabButton.onClick.AddListener(() => OnClickTabButton(_friendTabButton));

        _guildTabButton.TabButtonType = WhisperTabButtonType.Guild;
        _guildTabButton.TabButton.onClick.RemoveAllListeners();
        _guildTabButton.TabButton.onClick.AddListener(() => OnClickTabButton(_guildTabButton));

        SetTabButton(_tabButtonType);
    }

    public void CheckRemoveGuildInfo(long guildId)
    {
        if(_guildJoinedInfo != null) {
            if(_guildJoinedInfo.party_num == guildId) {
                _guildJoinedInfo = null;
            }
        }
    }

    public void InitWhisperTargetText(TextModel textModel)
    {
        _friendTabButton.TitleText.text = textModel.GetText(TextKey.UI_Text_13);
        _guildTabButton.TitleText.text = textModel.GetText(TextKey.UI_Text_57);
    }

    public void SetTabButton(WhisperTabButtonType buttonType)
    {
        switch (buttonType) {
            case WhisperTabButtonType.Friend: {
                    ReleaseCurUserInfoCells();
                    _tabButtonType = buttonType;
                    _friendTabButton.SelectObj.SetActive(true);
                    _guildTabButton.SelectObj.SetActive(false);
                    RequestWhisperFriendList();
                }
                break;
            case WhisperTabButtonType.Guild: {
                    if (_guildJoinedInfo != null) {
                        ReleaseCurUserInfoCells();
                        _tabButtonType = buttonType;
                        _friendTabButton.SelectObj.SetActive(false);
                        _guildTabButton.SelectObj.SetActive(true);
                        DataContext context = ChattingController.Instance.Context;
                        ChatHttpPartyInfo[] partyInfos = new ChatHttpPartyInfo[1];
                        partyInfos[0] = new ChatHttpPartyInfo();
                        partyInfos[0].party_num = _guildJoinedInfo.party_num;
                        partyInfos[0].party_type = _guildJoinedInfo.party_type;
                        _curUserListInfo = null;
                        ChattingPartyConnectUserList connectUserList = new ChattingPartyConnectUserList(context, partyInfos, OnSuccessPartyUserConnect, OnFailPartyUserConnect);
                        connectUserList.RequestHttpWeb();
                    } else {
                        UIFloatingMessagePopup.Show(GameSystem.Instance.Data.Text.GetText(TextKey.CT_Text03));
                    }
                }
                break;
        }
    }

    void CheckNotConfirmMessageCount(ChatWhisperDropDownInfo dropDownInfo)
    {
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
        ChatWhisperGroupInfo whisperGroup = null;

        chatModel.WhisperInformation.RemoveWhisperGroupNudgeNotable((IChatNudgeNotable)this);

        switch (dropDownInfo.DropDownType) {
            case ChatWhisperDropDownType.WhisperFriend:
                whisperGroup = chatModel.WhisperInformation.GetWhisperGroupInfo(dropDownInfo.DropDownType);
                break;
            case ChatWhisperDropDownType.WhisperGuild:
                ChatGuildJoinedInfo companyPartyInfo = dropDownInfo.DropDownObject as ChatGuildJoinedInfo;
                whisperGroup = chatModel.WhisperInformation.GetWhisperGroupInfo(dropDownInfo.DropDownType, companyPartyInfo.party_num);
                break;
        }

        if(whisperGroup != null) {
            whisperGroup.NudgeNode.AddNudgeNotable((IChatNudgeNotable)this);
        }
    }

    void SetDropDownContentsNotConfirmCount()
    {
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

        for (int i = 0;i< _dropDownInfos.Count;i++) {
            UIDropdownExpandItem dropdownItem = _targetDropDown.DropdownItems[i];
            ChatWhisperGroupInfo whisperGroup = chatModel.WhisperInformation.WhisperGroupInfos[i];
            whisperGroup.NudgeNode.AddNudgeNotable(dropdownItem.groupDropdownContent);
        }
    }

	public void ReleaseWhisperTargetPopup()
	{
		_dropDownInfos.Clear ();

        ChattingController.Instance.ChattingModel.WhisperInformation.ReleaseWhisperGroupInfo();

        _targetDropDown.ClearOptions();
		ReleaseCurUserInfoCells ();
		_targetDropDown.value = 0;
		_curSelectIndex = 0;
	}

    void RequestWhisperGuildTargets(ChatGuildJoinedInfo guildInfo)
    {
        DataContext context = ChattingController.Instance.Context;

        _curCompanyID = guildInfo.party_num;
        new GuildMemberList(context, _curCompanyID, OnSuccessCompanyMemberList).Execute();
    }

    void RequestWhisperFriendList()
    {
        DataContext context = ChattingController.Instance.Context;

        new FriendListTask(context, OnSuccessFriendListTask, OnFailFriendListTask).Execute();
    }

    long[] GetFriendConnectIDs()
    {
        long[] retValue = null;

        List<FriendModel> friends = ChattingController.Instance.GetFriendList();
        if (friends != null && friends.Count > 0) {
            retValue = new long[friends.Count];
            for (int i = 0; i < friends.Count; i++) {
                FriendModel friendInfo = friends[i];
                retValue[i] = friendInfo.playerId <= 0 ? friendInfo.userId : friendInfo.playerId;
            }
        }

        return retValue;
    }

    void SetFriendUserListCell(ChatUserInfoListPacket[] userInfoList)
    {
        _notExistGuildText.SetActive(false);

        if (!_isRequestUserInfo) {
            List<FriendModel> friendList = ChattingController.Instance.GetFriendList();
            if(friendList != null) {
                _notExistFriendText.SetActive(friendList.Count == 0);
            }
            return;
        }

        TextModel textModel = ChattingController.Instance.Context.Text;
        UserModel user = ChattingController.Instance.Context.User;
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

        List<FriendModel> friends = ChattingController.Instance.GetFriendList();
		if (friends != null) {
			_notExistFriendText.SetActive (friends.Count == 0);

			for (int i = 0; i < friends.Count; i++) {
				FriendModel friendInfo = friends [i];
				if (friendInfo.userId == user.userData.userId)
					continue;

                long connectId = friendInfo.playerId <= 0 ? friendInfo.userId : friendInfo.playerId;
                UIWhisperUserInfoCell userInfoCell = Instantiate (_uiUserInfoCell) as UIWhisperUserInfoCell;
                userInfoCell.UserId = friendInfo.userId;
                userInfoCell.ConnectId = connectId;

                ChatWhisperUserData whisperUserData = chatModel.WhisperInformation.GetWhisperUserData(connectId);
               
                if (whisperUserData != null) {
                    whisperUserData.NudgeNode.AddNudgeNotable((IChatNudgeNotable)userInfoCell);
                } else {
					userInfoCell.NotCheckNumObj.SetActive (false);
				}

				userInfoCell.transform.SetParent (_contentsTrans);
				userInfoCell.transform.localScale = Vector3.one;
				userInfoCell.gameObject.SetActive (true);

                userInfoCell.UserIcon = Icon.UserPortrait.Create(userInfoCell.UserIconTrn);
                Icon.UserPortrait.Show(userInfoCell.UserIcon, friendInfo.userPortraitIndex, friendInfo.userPortraitBorderIndex);
                userInfoCell.UserNameText.text = friendInfo.userNickname;
				userInfoCell.LevelText.text = string.Format ("Lv.{0}", friendInfo.userLevel);
				userInfoCell.UserInfoButton.onClick.AddListener (() => OnUserInfoButton (userInfoCell));
            

				bool isConnecting = false;
				if (userInfoList != null && userInfoList.Length > 0) {
					for (int j = 0; j < userInfoList.Length; j++) {
						if (userInfoList [j].channel_user_id == connectId) {
							if (userInfoList [j].chat_group_num >= 0) {
								isConnecting = true;
								userInfoCell.ConnectingText.gameObject.SetActive (true);
                                userInfoCell.ConnectingText.text = textModel.GetText(TextKey.CT_Friend_Connect);
                                userInfoCell.ConnectingText.color = ColorPreset.HUDGauge_GoodEffect;
                                if (userInfoCell.ChannelConnectingText != null)
								    userInfoCell.ChannelConnectingText.gameObject.SetActive (false);
							}
							break;
						}
					}
				}

				if (!isConnecting) {
					userInfoCell.ConnectingText.gameObject.SetActive (true);
                    if(userInfoCell.ChannelConnectingText != null)
					    userInfoCell.ChannelConnectingText.gameObject.SetActive (false);
					userInfoCell.ConnectingText.text = friendInfo.LastLoginAtText;
				}

				userInfoCell.WhisperUserInfo.whisperKind = (int)ChatDefinition.WhisperKind.Friend;
				userInfoCell.WhisperUserInfo.targetUserID = friendInfo.userId;
                userInfoCell.WhisperUserInfo.targetConnectID = connectId;
                userInfoCell.WhisperUserInfo.targetNickname = friendInfo.userNickname;

				_curUserInfoCells.Add (userInfoCell);
			}
		}

        _isRequestUserInfo = false;
    }

	void ReleaseCurUserInfoCells()
	{
		for (int i = 0; i < _curUserInfoCells.Count; i++) {
            MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;
            ChatWhisperUserData whisperUserData = chatModel.WhisperInformation.GetWhisperUserData(_curUserInfoCells[i].ConnectId);
            if(whisperUserData!= null) {
                whisperUserData.NudgeNode.RemoveNudgeNotable((IChatNudgeNotable)_curUserInfoCells[i]);
            }
            Destroy (_curUserInfoCells [i].gameObject);
		}

		_curUserInfoCells.Clear ();
	}

	bool CheckCompanyConnectingUser(long connectId)
	{
		if (_curUserListInfo == null || _curUserListInfo.Length == 0)
			return false;

		for (int i = 0; i < _curUserListInfo.Length; i++) {
			if (_curUserListInfo [i].user_id == connectId)
				return true;
		}

		return false;
	}

    void RequestWhisperTargetUserMsgList(long otherUserID)
    {
        ChatOtherUserListInfo[] otherUserListInfo = new ChatOtherUserListInfo[1];
        otherUserListInfo[0] = new ChatOtherUserListInfo();
        otherUserListInfo[0].other_user_id = otherUserID;
        otherUserListInfo[0].show_type = 1; // 1 : Show Count , 2 : Large begin_timestamp
        otherUserListInfo[0].begin_timestamp = 0;
        otherUserListInfo[0].show_count = 50;

        ChattingWhisperChatList whisperChatList = new ChattingWhisperChatList(ChattingController.Instance.Context, otherUserListInfo, OnSuccessChattingWhisperChatList, OnFailChattingWhisperChatList);
        whisperChatList.RequestHttpWeb();
    }

    void SetSelectUserInfoCell()
    {
        if (_onSelectUserInfoCell != null) {
            _onSelectUserInfoCell(_curSelectUserInfoCell);
        }
        this.gameObject.SetActive(false);
    }

    #endregion

    #region Callback Methods

	public void OnClosePopup()
	{
		this.gameObject.SetActive (false);
	}

	private void OnSuccessCompanyMemberList(GuildMemberListResponse resParam)
	{
		TextModel textModel = ChattingController.Instance.Context.Text;
		UserModel user = ChattingController.Instance.Context.User;
        MultiChattingModel chatModel = ChattingController.Instance.ChattingModel;

		if (resParam.guildMemberList != null && resParam.guildMemberList.Length > 0) {
			_notExistFriendText.SetActive (false);

			for (int i = 0; i < resParam.guildMemberList.Length; i++) {
				if (resParam.guildMemberList[i].userId == user.userData.userId)
					continue;

                long connectId = resParam.guildMemberList[i].playerId <= 0 ? resParam.guildMemberList[i].userId : resParam.guildMemberList[i].playerId;

                UIWhisperUserInfoCell userInfoCell = Instantiate (_uiUserInfoCell) as UIWhisperUserInfoCell;

                ChatWhisperUserData whisperUserData = chatModel.WhisperInformation.GetWhisperUserData(connectId);
               
                if (whisperUserData != null) {
                    if (whisperUserData.NoticeGuildID == _curCompanyID)
                        whisperUserData.NudgeNode.AddNudgeNotable(userInfoCell as IChatNudgeNotable);
                } else {
                    userInfoCell.NotCheckNumObj.SetActive(false);
                }

                userInfoCell.transform.SetParent (_contentsTrans);
				userInfoCell.transform.localScale = Vector3.one;
				userInfoCell.gameObject.SetActive (true);

                userInfoCell.UserIcon = Icon.UserPortrait.Create(userInfoCell.UserIconTrn);
                Icon.UserPortrait.Show(userInfoCell.UserIcon, resParam.guildMemberList[i].portraitIndex, resParam.guildMemberList[i].portraitBoarderIndex);
                userInfoCell.UserNameText.text = resParam.guildMemberList[i].nickname;
				userInfoCell.LevelText.text = string.Format ("Lv.{0}", resParam.guildMemberList[i].userLevel);
				userInfoCell.UserInfoButton.onClick.AddListener (() => OnUserInfoButton (userInfoCell));
                userInfoCell.ConnectingText.gameObject.SetActive(true);

                if (CheckCompanyConnectingUser (connectId)) {
                    userInfoCell.ConnectingText.text = textModel.GetText(TextKey.CT_Friend_Connect);
                    userInfoCell.ConnectingText.color = ColorPreset.HUDGauge_GoodEffect;
                } else
                {
                    var lastAccessAtTime = TimeUtil.KstTime.GetKstToKstDateTime(resParam.guildMemberList[i].lastAccessAt);
                    var span = TimeUtil.KstTime.CurrentServerTime - lastAccessAtTime;
                    userInfoCell.ConnectingText.text = TimeUtil.GetTime(ChattingController.Instance.Context, span);
                }

                userInfoCell.WhisperUserInfo.whisperKind = (int)ChatDefinition.WhisperKind.Guild;
                userInfoCell.WhisperUserInfo.targetUserID = resParam.guildMemberList[i].userId;
                userInfoCell.WhisperUserInfo.targetConnectID = connectId;
                userInfoCell.WhisperUserInfo.targetNickname = resParam.guildMemberList[i].nickname;
                userInfoCell.WhisperUserInfo.companyID = _curCompanyID;

                _curUserInfoCells.Add (userInfoCell);
			}

            _notExistGuildText.SetActive(_curUserInfoCells.Count > 0 ? false : true);
        }
	}

    void OnSuccessFriendListTask(FriendListResponce res)
    {
        long[] friendConnectIDs = GetFriendConnectIDs();
        if (friendConnectIDs != null && friendConnectIDs.Length > 0) {
            if (!_isRequestUserInfo) {
                _isRequestUserInfo = true;
                if (ChattingController.Instance.ChatSocketManager.ChatCurState == ChattingSocketManager.ChatCurrentState.Connected) {
                    ChattingController.Instance.ChatSocketManager.SendRequestUserInfoListPacket(friendConnectIDs, OnSuccessUserInfoPacket);
                    ChattingController.Instance.ChatSocketManager.EventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnTimeoutUserInfo, 3f, null, ChattingSocketManager.requestUserInfoTimeID);
                } else {
                    OnTimeoutUserInfo(null);
                }
            }
        } else {
            SetFriendUserListCell(null);
        }
    }

    void OnFailFriendListTask(FriendListResponce res)
    {
        SetFriendUserListCell(null);
    }

    void OnUserInfoButton(UIWhisperUserInfoCell userInfoCell)
	{
        if(_isRequestState)
            return;

        _curSelectUserInfoCell = userInfoCell;
        ChatWhisperUserData whisperUser = ChattingController.Instance.ChattingModel.WhisperInformation.GetWhisperUserData(_curSelectUserInfoCell.WhisperUserInfo.targetConnectID);
        if (whisperUser != null) {
            if(whisperUser.IsRequestChatServer) {
                SetSelectUserInfoCell();
            } else {
                _isRequestState = true;

                RequestWhisperTargetUserMsgList(userInfoCell.WhisperUserInfo.targetConnectID);
            }
        } else {
            _isRequestState = true;

            RequestWhisperTargetUserMsgList(userInfoCell.WhisperUserInfo.targetConnectID);
        }
    }

    void OnSuccessChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
    {
        _isRequestState = false;

        if (chatResponse != null && chatResponse.info != null && chatResponse.info.Length > 0) {
            List<IChatTimeStamp> chatMessageInfos = new List<IChatTimeStamp>();
            for (int i = 0; i < chatResponse.info.Length; i++) {
                ChatWhisperOtherUserResponseInfo whisperChatInfo = chatResponse.info[i];

                if (whisperChatInfo.message_list != null && whisperChatInfo.message_list.Length > 0) {
                    for (int j = 0; j < whisperChatInfo.message_list.Length; j++) {
                        chatMessageInfos.Add(whisperChatInfo.message_list[j]);
                    }
                }
            }

            chatMessageInfos.Sort(new ChatTimeStampComparer());

            if (chatMessageInfos.Count > ChattingController.Instance.MaxChatLineCount) {
                int gapValue = chatMessageInfos.Count - ChattingController.Instance.MaxChatLineCount;
                for (int i = 0; i < gapValue; i++) {
                    chatMessageInfos.RemoveAt(0);
                }
            }

            for (int i = 0; i < chatMessageInfos.Count; i++) {
                ChatWhisperUserMessageInfo whisperMessageInfo = chatMessageInfos[i] as ChatWhisperUserMessageInfo;
                byte[] decbuf = System.Convert.FromBase64String(whisperMessageInfo.chat_msg);
                string msg = System.Text.Encoding.UTF8.GetString(decbuf);

#if _CHATTING_LOG
                Debug.Log(string.Format("OnSuccessChattingWhisperChatList timestamp : {0}, msg : {1}", whisperMessageInfo.timestamp, msg));
#endif

                JsonData msgJson = ChatParsingUtil.GetChatMessageJsonData(msg);
                ChatWhisperMessage chatMessage = ChatParsingUtil.GetChatWhisperMessageParsingByJson(msgJson);
                chatMessage.timeStamp = whisperMessageInfo.timestamp;

                long whisperUserID = 0;
                long connectid = 0;
                string whisperUserName = "";

                if (whisperMessageInfo.type == 1) { // Send
                    chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperToChatting;
                    whisperUserID = chatMessage.targetUserID;
                    connectid = chatMessage.targetConnectID;
                    whisperUserName = chatMessage.targetUserName;
                } else if (whisperMessageInfo.type == 2) { // Receive
                    chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperFromChatting;
                    whisperUserID = chatMessage.userID;
                    connectid = chatMessage.connectId;
                    whisperUserName = chatMessage.sendUserName;
                }

                if (chatMessage.partMessageInfos.ContainsKey("user")) {
                    chatMessage.partMessageInfos.Remove("user");
                }

                chatMessage.partMessageInfos.Add("user", ChattingController.Instance.GetUserInfoChatPartInfo(whisperUserID, connectid));

                if (!chatMessage.prm.ContainsKey("user"))
                    chatMessage.prm.Add("user", whisperUserName);

                ChattingController.Instance.ChattingModel.AddServerWhisperChatMessage(chatMessage, false);
            }
        }

        ChatWhisperUserData whisperUser = ChattingController.Instance.ChattingModel.WhisperInformation.GetWhisperUserData(_curSelectUserInfoCell.WhisperUserInfo.targetConnectID);
        if(whisperUser != null) {
            if(!whisperUser.IsRequestChatServer)
                whisperUser.IsRequestChatServer = true;
        }

        SetSelectUserInfoCell();
    }

    void OnFailChattingWhisperChatList(ChattingWhisperChatListResponse chatResponse)
    {
        _isRequestState = false;

        SetSelectUserInfoCell();
    }

    void OnSuccessPartyUserConnect(ChattingPartyConnectUserListResponse resParam)
	{
		if (resParam.party_user_list.Length > 0) {
			_curUserListInfo = resParam.party_user_list [0].user_list;
		}

        RequestWhisperGuildTargets(_guildJoinedInfo);

    }

	void OnFailPartyUserConnect(ChattingPartyConnectUserListResponse resParam)
	{
        RequestWhisperGuildTargets(_guildJoinedInfo);
    }

    void OnSuccessUserInfoPacket(ChatUserInfoListPacket[] userInfoList)
    {
        SetFriendUserListCell(userInfoList);
        ChattingController.Instance.ChatSocketManager.EventTimer.RemoveTimeEventByID(ChattingSocketManager.requestUserInfoTimeID);
    }

    void OnTimeoutUserInfo(object objData)
    {
        SetFriendUserListCell(null);
    }

    void OnCreateComplete()
    {
        SetDropDownContentsNotConfirmCount();
    }

    void OnDropdownAddItem(UIDropdownExpandItem item)
    {

    }

    void OnDropdownRemoveItem(UIDropdownExpandItem item)
    {

    }

    void OnClickTabButton(UIChatTabButton tabButtonInfo)
    {
        SetTabButton(tabButtonInfo.TabButtonType);
    }

    #endregion

    #region IBackKeyMethod

    public void ExecuteBackKey()
    {
        if(_closeButton != null)
            _closeButton.onClick.Invoke();
    }

    #endregion
}
