using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;
using System.Linq;

#region ChattingGuildChatList

public class ChattingGuildChatRequest : ChatRequestParam
{
	public int guild_num;
	public int show_count;
	public int show_type; // 1 : Designated Count, 2 : begin_date after
	public string begin_date;
	public long channel_user_id;
	public int login_key;
}

public class ChattingGuildChatResponse : ChatResponseParam
{
	public string server_date;
	public ChatMessageList[] message_list;
}

public class ChattingGuildChatList : ChattingWebProtocol<ChattingGuildChatRequest, ChattingGuildChatResponse>
{
	public ChattingGuildChatList(DataContext data, Action<ChattingGuildChatResponse> onSuccessAction, Action<ChattingGuildChatResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		
	}

	#region Variables

	ChattingGuildChatRequest _requestInfo = new ChattingGuildChatRequest();
	ChattingGuildChatResponse _responseInfo;

	#endregion

	#region Properties

	protected override string Url
	{
        get { return ChattingController.Instance.ChatWebUrl + "show_user_guild_chat.php"; }
    }

	#endregion

	#region Methods

	protected override string GetRequestJsonString()
	{
		return LitJson.JsonMapper.ToJson(_requestInfo);
	}

	protected override void SetResponseJsonData(string result)
	{
		_responseInfo = LitJson.JsonMapper.ToObject<ChattingGuildChatResponse> (result);
		_resParam = _responseInfo;
	}

	protected override void OnSuccess(ChattingGuildChatResponse resParam)
	{
		
	}

	protected override void OnFail(ChattingGuildChatResponse resParam) 
	{
		
	}

	#endregion
}

#endregion

#region ChattingPartyChatList

public class ChattingPartyChatRequest : ChatRequestParam
{
	public long party_num;
	public int party_type;
	public int show_count;
	public int show_type; // 1 : Designated Count, 2 : begin_date after
	public long begin_timestamp;
	public long channel_user_id;
	public int login_key;
}

public class ChattingPartyChatResponse : ChatResponseParam
{
	public ChatPartyBaseInfo partyInfo;
	public long server_timestamp;
	public ChatPartyMessageList[] message_list;
}

public class ChattingPartyChatList : ChattingWebProtocol<ChattingPartyChatRequest, ChattingPartyChatResponse>
{
	public ChattingPartyChatList(DataContext data, ChatPartyBaseInfo partyBaseInfo, Action<ChattingPartyChatResponse> onSuccessAction, Action<ChattingPartyChatResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		_partyInfo = partyBaseInfo;
	}

	#region Variables

	ChattingPartyChatRequest _requestInfo = new ChattingPartyChatRequest();
	ChattingPartyChatResponse _responseInfo;

	ChatPartyBaseInfo _partyInfo;

	int _requestLogCount = 100;

	#endregion

	#region Properties

	protected override string Url
	{
        get { return ChattingController.Instance.ChatWebUrl + "show_user_party_chat_v2.php"; }
    }

	#endregion

	#region Methods

	protected override void SetRequestData()
	{
        if(ChattingController.Instance == null || ChattingController.Instance.ChatSocketManager == null ||
            ChattingController.Instance.ChatSocketManager.ChattingServerInfo == null) {
            if (_onFailChatHttp != null) {
                _onFailChatHttp(_resParam);
            }

            OnFail(_resParam);
            return;
        }

        if(!ChattingController.Instance.ChatSocketManager.CheckConnectChatSocket()) {
            if (_onFailChatHttp != null) {
                _onFailChatHttp(_resParam);
            }

            OnFail(_resParam);
            return;
        }

		_requestInfo.game_server_name = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.gameServerName;
		_requestInfo.party_num = _partyInfo.party_num;
		_requestInfo.party_type = _partyInfo.party_type;

		_requestInfo.show_count = _requestLogCount;
		_requestInfo.show_type = 1;
		_requestInfo.begin_timestamp = 0;
		_requestInfo.channel_user_id = ChatHelper.GetChatChannelID(_data);
		_requestInfo.login_key = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.loginKey;
	}

	protected override string GetRequestJsonString()
	{
		return LitJson.JsonMapper.ToJson(_requestInfo);
	}

	protected override void SetResponseJsonData(string result)
	{
		_responseInfo = new ChattingPartyChatResponse ();

		_responseInfo.partyInfo = _partyInfo;

		JsonData jsonRoot = JsonMapper.ToObject (result);
		_responseInfo.result_code = (int)jsonRoot ["result_code"];
		_responseInfo.result_message = (string)jsonRoot ["result_message"];
		if (jsonRoot ["server_timestamp"].IsInt) {
			_responseInfo.server_timestamp = (long)(int)jsonRoot ["server_timestamp"];
		} else if(jsonRoot ["server_timestamp"].IsLong) {
			_responseInfo.server_timestamp = (long)jsonRoot ["server_timestamp"];
		}

		JsonData messageJsonDataList = jsonRoot ["message_list"];

		_responseInfo.message_list = new ChatPartyMessageList[messageJsonDataList.Count];
		for (int i = 0; i < messageJsonDataList.Count; i++) {
			JsonData messageJson = messageJsonDataList [i];

			ChatPartyMessageList inputPartyMessage = new ChatPartyMessageList ();
			if (messageJson ["timestamp"].IsInt) {
				inputPartyMessage.timestamp = (long)(int)messageJson ["timestamp"];
			} else if (messageJson ["timestamp"].IsLong) {
				inputPartyMessage.timestamp = (long)messageJson ["timestamp"];
			}

			inputPartyMessage.chat_msg = (string)messageJson ["chat_msg"];
			_responseInfo.message_list [i] = inputPartyMessage;
		}

		_resParam = _responseInfo;
	}

	protected override void OnSuccess(ChattingPartyChatResponse resParam)
	{

	}

	protected override void OnFail(ChattingPartyChatResponse resParam) 
	{

	}

	#endregion
}

#endregion

#region ChattingPartyConnectUserList

public class ChattingPartyConnectUserListRequest : ChatRequestParam
{
	public ChatHttpPartyInfo[] party_num_list;
	public int get_extra_data;
}

public class ChattingPartyConnectUserListResponse : ChatResponseParam
{
	public ChatHttpPartyUserList[] party_user_list;
}

public class ChattingPartyConnectUserList : ChattingWebProtocol<ChattingPartyConnectUserListRequest, ChattingPartyConnectUserListResponse>
{
	public ChattingPartyConnectUserList(DataContext data, ChatHttpPartyInfo[] partyInfos, Action<ChattingPartyConnectUserListResponse> onSuccessAction, Action<ChattingPartyConnectUserListResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		_requestPartyInfos = partyInfos;
	}

	#region Variables

	ChattingPartyConnectUserListRequest _requestInfo = new ChattingPartyConnectUserListRequest();
	ChattingPartyConnectUserListResponse _responseInfo;

	ChatHttpPartyInfo[] _requestPartyInfos;

	#endregion

	#region Properties

	protected override string Url
	{
        get { return ChattingController.Instance.ChatWebUrl + "show_party_user_list_v2.php"; }
    }

	#endregion

	#region Methods

	protected override void SetRequestData()
	{
		_requestInfo.game_server_name = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.gameServerName;
		_requestInfo.party_num_list = _requestPartyInfos;
		_requestInfo.get_extra_data = 0;
	}

	protected override string GetRequestJsonString()
	{
		return LitJson.JsonMapper.ToJson(_requestInfo);
	}

	protected override void SetResponseJsonData(string result)
	{
		_responseInfo = new ChattingPartyConnectUserListResponse ();

		JsonData jsonRoot = JsonMapper.ToObject (result);
		_responseInfo.result_code = (int)jsonRoot ["result_code"];
		_responseInfo.result_message = (string)jsonRoot ["result_message"];

		if (jsonRoot ["party_user_list"] != null) {
			_responseInfo.party_user_list = new ChatHttpPartyUserList[jsonRoot["party_user_list"].Count];
			for (int i = 0; i < jsonRoot ["party_user_list"].Count; i++) {
				JsonData partyUserInfo = jsonRoot ["party_user_list"] [i];

				ChatHttpPartyUserList inputPartyUserList = new ChatHttpPartyUserList ();

				if (partyUserInfo ["party_num"].IsInt) {
					inputPartyUserList.party_num = (long)(int)partyUserInfo ["party_num"];
				} else if (partyUserInfo ["party_num"].IsLong) {
					inputPartyUserList.party_num = (long)partyUserInfo ["party_num"];
				}

				inputPartyUserList.party_type = (int)partyUserInfo ["party_type"];

				if (partyUserInfo ["user_list"] != null) {
					inputPartyUserList.user_list = new ChatHttpUserListInfo[partyUserInfo ["user_list"].Count];
					for (int j = 0; j < partyUserInfo ["user_list"].Count; j++) {
						JsonData userListJson = partyUserInfo ["user_list"] [j];
						ChatHttpUserListInfo inputUserListInfo = new ChatHttpUserListInfo ();

						if (userListJson ["user_id"].IsInt) {
							inputUserListInfo.user_id = (long)(int)userListJson ["user_id"];
						} else if (userListJson ["user_id"].IsLong) {
							inputUserListInfo.user_id = (long)userListJson ["user_id"];
						}

						inputUserListInfo.extra_data = (string)userListJson ["extra_data"];

						inputPartyUserList.user_list [j] = inputUserListInfo;
					}
				}

				_responseInfo.party_user_list [i] = inputPartyUserList;
			}
		}

		_resParam = _responseInfo;
	}

	protected override void OnSuccess(ChattingPartyConnectUserListResponse resParam)
	{

	}

	protected override void OnFail(ChattingPartyConnectUserListResponse resParam) 
	{

	}

	#endregion
}

#endregion

#region ChattingWhisperChatLastCount

public class ChattingWhisperChatLastCountRequest : ChatRequestParam
{
	public long channel_user_id;
	public int login_key;
}

public class ChattingWhisperChatLastCountResponse : ChatResponseParam
{
	public long server_timestamp;
	public ChatWhisperLastCountResponseInfo[] info;
}

public class ChattingWhisperChatLastCount : ChattingWebProtocol<ChattingWhisperChatLastCountRequest, ChattingWhisperChatLastCountResponse>
{
	public ChattingWhisperChatLastCount(DataContext data, Action<ChattingWhisperChatLastCountResponse> onSuccessAction, Action<ChattingWhisperChatLastCountResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		
	}

	#region Variables

	ChattingWhisperChatLastCountRequest _requestInfo = new ChattingWhisperChatLastCountRequest();
	ChattingWhisperChatLastCountResponse _responseInfo;

	#endregion

	#region Properties

	protected override string Url
	{
        get { return ChattingController.Instance.ChatWebUrl + "show_user_whisper_last_count.php"; }
    }

	#endregion

	#region Methods

	protected override void SetRequestData()
	{
		_requestInfo.game_server_name = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.gameServerName;
		_requestInfo.channel_user_id = ChatHelper.GetChatChannelID(_data);
		_requestInfo.login_key = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.loginKey;
	}

	protected override string GetRequestJsonString()
	{
		return LitJson.JsonMapper.ToJson(_requestInfo);
	}

	protected override void SetResponseJsonData(string result)
	{
		try{
			_responseInfo = new ChattingWhisperChatLastCountResponse ();

			JsonData jsonRoot = JsonMapper.ToObject (result);
			_responseInfo.result_code = (int)jsonRoot ["result_code"];
			_responseInfo.result_message = (string)jsonRoot ["result_message"];

			if (jsonRoot ["info"] != null) {
				_responseInfo.info = new ChatWhisperLastCountResponseInfo[jsonRoot ["info"].Count];
				for (int i = 0; i < jsonRoot ["info"].Count; i++) {
					JsonData messageCountInfo = jsonRoot ["info"] [i];

					ChatWhisperLastCountResponseInfo inputInfo = new ChatWhisperLastCountResponseInfo ();

					if (messageCountInfo ["other_user_id"].IsInt) {
						inputInfo.other_user_id = (long)(int)messageCountInfo ["other_user_id"];
					} else if (messageCountInfo ["other_user_id"].IsLong) {
						inputInfo.other_user_id = (long)messageCountInfo ["other_user_id"];
					}

					inputInfo.message_count = (int)messageCountInfo ["message_count"];

					if (messageCountInfo ["read_timestamp"].IsInt) {
						inputInfo.read_timestamp = (long)(int)messageCountInfo ["read_timestamp"];
					} else if (messageCountInfo ["read_timestamp"].IsLong) {
						inputInfo.read_timestamp = (long)messageCountInfo ["read_timestamp"];
					}

					if (messageCountInfo ["last_timestamp"].IsInt) {
						inputInfo.last_timestamp = (long)(int)messageCountInfo ["last_timestamp"];
					} else if (messageCountInfo ["last_timestamp"].IsLong) {
						inputInfo.last_timestamp = (long)messageCountInfo ["last_timestamp"];
					}

					if (messageCountInfo ["del_timestamp"].IsInt) {
						inputInfo.del_timestamp = (long)(int)messageCountInfo ["del_timestamp"];
					} else if (messageCountInfo ["del_timestamp"].IsLong) {
						inputInfo.del_timestamp = (long)messageCountInfo ["del_timestamp"];
					}

					_responseInfo.info [i] = inputInfo;
				}
			}
			_resParam = _responseInfo;
		}catch(Exception e) {
			Debug.Log (string.Format("SetResponseJsonData Fail : {0}", e.ToString()));
		}
	}

	protected override void OnSuccess(ChattingWhisperChatLastCountResponse resParam)
	{

	}

	protected override void OnFail(ChattingWhisperChatLastCountResponse resParam) 
	{

	}

	#endregion
}

#endregion

#region ChattingWhisperChatList

public class ChattingWhisperChatListRequest : ChatRequestParam
{
	public long channel_user_id;
	public int login_key;
	public ChatOtherUserListInfo[] other_user_id_list;
}

public class ChattingWhisperChatListResponse : ChatResponseParam
{
	public long server_timestamp;
	public ChatWhisperOtherUserResponseInfo[] info;
}

public class ChattingWhisperChatList : ChattingWebProtocol<ChattingWhisperChatListRequest, ChattingWhisperChatListResponse>
{
	public ChattingWhisperChatList(DataContext data, Dictionary<long /* userID */, ChatWhisperTargetUserInfo> whisperTargetUserInfos, Action<ChattingWhisperChatListResponse> onSuccessAction, Action<ChattingWhisperChatListResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		List<long> targetUserKeys = whisperTargetUserInfos.Keys.ToList ();
		_otherUserList = new ChatOtherUserListInfo[targetUserKeys.Count];

		for (int i = 0; i < targetUserKeys.Count; i++) {
			ChatWhisperTargetUserInfo targetUserInfo = whisperTargetUserInfos [targetUserKeys [i]];

			_otherUserList [i] = new ChatOtherUserListInfo ();
			_otherUserList [i].other_user_id = targetUserInfo.targetConnectID;
			_otherUserList [i].show_type = 1; // 1 : Show Count , 2 : Large begin_timestamp
			_otherUserList [i].begin_timestamp = 0;
			_otherUserList [i].show_count = _requestShowCount;
		}
	}

	public ChattingWhisperChatList(DataContext data, ChatOtherUserListInfo[] otherUserListInfo, Action<ChattingWhisperChatListResponse> onSuccessAction, Action<ChattingWhisperChatListResponse> onFailAction) : base(data, onSuccessAction, onFailAction)
	{
		_otherUserList = otherUserListInfo;
	}

	#region Variables

	ChattingWhisperChatListRequest _requestInfo = new ChattingWhisperChatListRequest();
	ChattingWhisperChatListResponse _responseInfo;

	ChatOtherUserListInfo[] _otherUserList;

	int _requestShowCount = 32;

	#endregion

	#region Properties

	protected override string Url
	{
        get { return ChattingController.Instance.ChatWebUrl + "show_user_whisper.php"; }
    }

	#endregion

	#region Methods

	protected override void SetRequestData()
	{
		_requestInfo.game_server_name = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.gameServerName;
		_requestInfo.channel_user_id = ChatHelper.GetChatChannelID(_data);
		_requestInfo.login_key = ChattingController.Instance.ChatSocketManager.ChattingServerInfo.loginKey;
		_requestInfo.other_user_id_list = _otherUserList;
	}

	protected override string GetRequestJsonString()
	{
		return LitJson.JsonMapper.ToJson(_requestInfo);
	}

	protected override void SetResponseJsonData(string result)
	{
		try{
			_responseInfo = new ChattingWhisperChatListResponse ();

			JsonData jsonRoot = JsonMapper.ToObject (result);
			_responseInfo.result_code = (int)jsonRoot ["result_code"];
			_responseInfo.result_message = (string)jsonRoot ["result_message"];

			if (jsonRoot ["info"] != null) {
				_responseInfo.info = new ChatWhisperOtherUserResponseInfo[jsonRoot ["info"].Count];
				for (int i = 0; i < jsonRoot ["info"].Count; i++) {
					JsonData otherUserInfoJson = jsonRoot ["info"] [i];

					ChatWhisperOtherUserResponseInfo inputInfo = new ChatWhisperOtherUserResponseInfo ();

					if (otherUserInfoJson ["other_user_id"].IsInt) {
						inputInfo.other_user_id = (long)(int)otherUserInfoJson ["other_user_id"];
					} else if (otherUserInfoJson ["other_user_id"].IsLong) {
						inputInfo.other_user_id = (long)otherUserInfoJson ["other_user_id"];
					}

					inputInfo.message_list = new ChatWhisperUserMessageInfo[otherUserInfoJson ["message_list"].Count];
					for (int j = 0; j < otherUserInfoJson ["message_list"].Count; j++) {
						JsonData messageListJson = otherUserInfoJson ["message_list"] [j];

						ChatWhisperUserMessageInfo inputMessageInfo = new ChatWhisperUserMessageInfo ();
						inputMessageInfo.type = (int)messageListJson ["type"];

						if (messageListJson ["timestamp"].IsInt) {
							inputMessageInfo.timestamp = (long)(int)messageListJson ["timestamp"];
						} else if (messageListJson ["timestamp"].IsLong) {
							inputMessageInfo.timestamp = (long)messageListJson ["timestamp"];
						}

						inputMessageInfo.chat_msg = (string)messageListJson ["chat_msg"];

						inputInfo.message_list [j] = inputMessageInfo;
					}

					_responseInfo.info [i] = inputInfo;
				}
			}
			_resParam = _responseInfo;
		}catch(Exception e) {
			Debug.Log (string.Format("SetResponseJsonData Fail : {0}", e.ToString()));
		}
	}

	protected override void OnSuccess(ChattingWhisperChatListResponse resParam)
	{

	}

	protected override void OnFail(ChattingWhisperChatListResponse resParam) 
	{

	}

	#endregion
}

#endregion
