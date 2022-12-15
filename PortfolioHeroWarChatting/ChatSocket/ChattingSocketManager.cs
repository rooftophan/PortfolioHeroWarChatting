using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Linq;

public class ChattingSocketManager : IChatSendMessageObserver
{
	#region Definition

	public enum ChatCurrentState
	{
		Disconnected = 0,
		Connecting,
		LoginConnect,
		DelayConnect,
		Connected,
		Reconnect,
		ReconnectGameServer,
        ReconnectingGameServer,
		ServerMaintenance,
        LockChatting,
	}

	#endregion

	#region Variables

	ChatCurrentState _chatCurState = ChatCurrentState.Disconnected;

	ActionEventTimer _eventTimer = new ActionEventTimer();

	bool _isLogin = false;

	float _connectTimeoutSec = 10f;
	int _retryConnectCount = 3;

	float _loginTimeoutSec = 10f;
	int _retryLoginCount = 3;

	bool _isPingRequestPacket = false;
	float _pingStartTime;

	ChattingPacket _recvPacket = new ChattingPacket();

	TcpClient _clientSocket = null;
	//NetworkStream _serverStream = null;

	ChattingServerInfo _chattingServerInfo = null;
	DataContext _data;

	Action _onRequestNetChatInfo = null;

	MultiChattingModel _chattingModel;

	ChattingController _chatController = null;
	Action _onSuccessPartyUpdate = null;
    Action<ChatUserInfoListPacket[]> _onSuccessUserInfoList = null;

    Action<bool> _onFinishCurPartyConnect;

    List<IChatChangePartyObserver> _chatChangePartyGroupObservers = new List<IChatChangePartyObserver>();

    #endregion

    #region EventTime ID Variables

    int _connectTimeID 				                    = 1;
	int _loginTimeID 				                    = 2;
	int _pingTimeID 				                    = 3;
	int _chatInfoTimeID				                    = 4;
	int _previewUITimeID			                    = 5;

    int _reconnectTimeID                                = 6;
    public const int requestUserInfoTimeID              = 7;
    public const int sendObtainCardDelayTimeID          = 8;
    public const int previewNextCheckTimeID             = 9;
    public const int sendAchieveHeroLevelDelayTimeID    = 10;
    public const int reserveChatInfoTimeID              = 11;

    #endregion

    #region Properties

    public ChatCurrentState ChatCurState
	{
		get{ return _chatCurState; }
	}

	//public NetworkStream ServerStream
	//{
	//	get{ return _serverStream; }
	//}

	public ChattingServerInfo ChattingServerInfo
	{
		get{ return _chattingServerInfo; }
		set{ _chattingServerInfo = value; }
	}

	public Action OnRequestNetChatInfo
	{
		get{ return _onRequestNetChatInfo; }
		set{ _onRequestNetChatInfo = value; }
	}

	public int ChatInfoTimeID
	{
		get{ return _chatInfoTimeID; }
	}

	public ActionEventTimer EventTimer
	{
		get{ return _eventTimer; }
	}

	public int PreviewUITimeID
	{
		get{ return _previewUITimeID; }
	}

	public float PingStartTime
	{
		get{ return _pingStartTime; }
		set{ _pingStartTime = value; }
	}

	public bool IsPingRequestPacket
	{
		get{ return _isPingRequestPacket; }
		set{ _isPingRequestPacket = value; }
	}

	#endregion

	#region Methods

	public void InitChatSocket(DataContext context, MultiChattingModel chatModel, ChattingController chatCtrl)
	{
		_data = context;
		_chattingModel = chatModel;
		_chatController = chatCtrl;
	}

	public void UpdateChatSocket()
	{
        if (_clientSocket != null && _clientSocket.Connected) {
            OnReceivedChatPacket();

            switch (_chatCurState) {
                case ChatCurrentState.Connected:
                    if (_requestQueuePackets.Count > 0) {
                        WriteServerStream(_requestQueuePackets.Dequeue());
                    }

                    if (_isPingRequestPacket) {
                        float curPingCheckTime = Time.realtimeSinceStartup;
                        if ((curPingCheckTime - _pingStartTime) > 15f) {
                            if (IsValidDisconnect()) {
                                Disconnect();
                                Reconnect();
                            }
                            return;
                        }
                    }

                    if (!_isLogin) {
                        ReChatLogin();
                    }
                    break;
            }
        } else {
            switch (_chatCurState) {
                case ChatCurrentState.Connected:
                    Disconnect();
                    Reconnect();
                    break;
                case ChatCurrentState.Reconnect:
                    Connect();
                    break;
                case ChatCurrentState.ReconnectGameServer:
                    SetCurChatState(ChatCurrentState.ReconnectingGameServer);
                    _onRequestNetChatInfo();
                    break;
                default:
                    break;
            }
        }

        _eventTimer.UpdateGameTimer ();
	}

    public bool CheckConnectChatSocket()
    {
        if (_clientSocket != null && _clientSocket.Connected)
            return true;

        return false;
    }

    void OnReceivedChatPacket () {
		try {			
			if(_clientSocket.Available <= 0)
				return;

			int nLength = Mathf.Min (_recvPacket.curPacketLength - _recvPacket.bytesRead, _clientSocket.Available);
#if _CHATTING_LOG
            Debug.Log(string.Format("_recvPacket.bytesRead : {0}, _clientSocket.Available : {1}", _recvPacket.bytesRead, _clientSocket.Available));
#endif
            if (_clientSocket.GetStream ().Read (_recvPacket.packetBuffer, _recvPacket.bytesRead, nLength) != nLength)
				return;

			_recvPacket.bytesRead += nLength;
			while (_recvPacket.bytesRead > 0) {
				short packetLen	= System.BitConverter.ToInt16 (_recvPacket.packetBuffer, 0); 
				short packetNum	= System.BitConverter.ToInt16 (_recvPacket.packetBuffer, 2); 

				// Network to Host byte-order. (BE2LE)
				if (System.BitConverter.IsLittleEndian) {
					packetLen = IPAddress.HostToNetworkOrder(packetLen);
					packetNum = IPAddress.HostToNetworkOrder(packetNum);
				}

#if _CHATTING_LOG
                Debug.Log(string.Format("OnReceivedChatPacket packet.bytesRead : {0}, packetLen : {1}, packetNum : {2}", _recvPacket.bytesRead, packetLen, packetNum));
#endif

                if (packetLen > _recvPacket.bytesRead) {
                    if(packetLen > _recvPacket.curPacketLength) {
                        _recvPacket.curPacketLength += (packetLen - _recvPacket.curPacketLength + 1);
                        byte[] temp = _recvPacket.packetBuffer;
                        _recvPacket.ResetPacketBuffer(_recvPacket.curPacketLength);
                        System.Array.Copy(temp, 0, _recvPacket.packetBuffer, 0, temp.Length);

                        byte[] tempBack = _recvPacket.backBuffer;
                        _recvPacket.ResetBackBuffer(_recvPacket.curPacketLength);
                        System.Array.Copy(tempBack, 0, _recvPacket.backBuffer, 0, tempBack.Length);
                    }
                    return;
                }

				_recvPacket.bufPos = sizeof(short) + sizeof(short);

                ReceiveResponsePacket(packetNum);

                // swap buffer
                _recvPacket.bytesRead = Mathf.Max (0, _recvPacket.bytesRead - packetLen);
				if (_recvPacket.bytesRead > 0) {
					System.Array.Copy (_recvPacket.packetBuffer, packetLen, _recvPacket.backBuffer, 0, _recvPacket.bytesRead);
					byte[] temp = _recvPacket.backBuffer;
					_recvPacket.backBuffer = _recvPacket.packetBuffer;
					_recvPacket.packetBuffer = temp;
				} else {
                    _recvPacket.ClearPacket();
				}
			}
		}
		catch(Exception e)
		{
			Debug.Log (string.Format("OnReceivedChatPacket Receive Fail : {0}", e.ToString()));
			_recvPacket.ClearPacket ();
		}
	}

	void SetCurChatState(ChatCurrentState curState, float timeoutSec = 0f)
	{
		_chatCurState = curState;

		Debug.Log(string.Format("!!!!! SetCurChatState curState : {0}", _chatCurState));
		switch (_chatCurState) {
            case ChatCurrentState.Connecting:
                _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnConnectTimeout, _connectTimeoutSec, null, _connectTimeID);
                break;
            case ChatCurrentState.LoginConnect:
                _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnLoginTimeout, _loginTimeoutSec, null, _loginTimeID);
                break;
            case ChatCurrentState.DelayConnect:
                _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.Normal, OnDelayConnect, 0.5f);
                break;
            case ChatCurrentState.Connected:
                if (!_eventTimer.ExistTimerDataByID(_pingTimeID))
                    _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnPingPacketRes, 10f, null, _pingTimeID);

                _pingStartTime = Time.realtimeSinceStartup;

                ChattingController.Instance.SetWhisperServerChatLog();
                CheckCurChatPartyGroup();

                _chattingModel.ChannelChatInfo.NudgeNode.RefreshNodeNudgeCount();
                break;
            case ChatCurrentState.ServerMaintenance:
                _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.ServerTime, OnServerMaintenanceTimeout, timeoutSec, null, _pingTimeID);
                break;
        }
    }

    public bool isConnected()
    {
        return (_chatCurState == ChatCurrentState.Connected);
    }

	public void Connect()
	{
		if(_chattingServerInfo == null)
			return;

		if (_chattingServerInfo.result != (int)ServerResultCode.SUCCESS) {
			switch ((ServerResultCode)_chattingServerInfo.result) {
			case ServerResultCode.SERVER_MAINTENANCE:
				#if _CHATTING_LOG
				Debug.Log (string.Format ("ChatServer ChatCurrentState.ServerMaintenance!!! chattingModel.ChattingServerInfo.remainSec : {0}", _chattingServerInfo.remainSec));
				#endif
				SetCurChatState (ChatCurrentState.ServerMaintenance, _chattingServerInfo.remainSec);
				break;
			}

			return;
		}

		try 
		{
            IPAddress[] remoteHost = Dns.GetHostAddresses(_chattingServerInfo.dns);

            if (_clientSocket == null) {
                _clientSocket = new TcpClient(remoteHost[0].AddressFamily);
            }

#if _CHATTING_LOG
            Debug.Log(string.Format("Chat!!! Connect dns : {0}, port : {1}, AddressFamily : {2}", _chattingServerInfo.dns, _chattingServerInfo.port, remoteHost[0].AddressFamily));
#endif

            _clientSocket.BeginConnect(remoteHost[0], _chattingServerInfo.port, new AsyncCallback(OnConnectSocket), _clientSocket);

            SetCurChatState (ChatCurrentState.Connecting);
			_retryConnectCount--;
		} 
        catch (Exception e) {
            Disconnect();
            _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnReconnectTime, 10f, null, _reconnectTimeID);
            Debug.Log(e.ToString());
        }
    }

	public void Disconnect()
	{
#if _CHATTING_LOG
		Debug.Log (string.Format ("Chatting!!! Disconnect()"));
#endif

        if (_chatCurState == ChatCurrentState.Disconnected)
            return;

		if (_chatCurState == ChatCurrentState.Connected) {
			if (_chatController != null) {
                _chatController.NotifyChatReceiveMessage(-1, GetDisconnectChatMessage());
            }
        }

        _isPingRequestPacket = false;

		SetCurChatState (ChatCurrentState.Disconnected);
		_recvPacket.ClearPacket();

		if (_eventTimer.ExistTimerDataByID (_pingTimeID))
			_eventTimer.RemoveTimeEventByID (_pingTimeID);

		_eventTimer.CompleteTimeEventByTimeID (_previewUITimeID);
		_eventTimer.ReleaseGameTimerList ();

		if (ChattingController.Instance != null) {
			ChattingController.Instance.ReleaseAllChatInfo ();
		}

		try
		{
			_isLogin = false;
			if(_clientSocket != null)
			{
				if (_clientSocket.Connected) {
                    _clientSocket.Client.Disconnect(false);
				}
				
				_clientSocket.Close();
                _clientSocket.Dispose();
                _clientSocket = null;
			}
		}
        catch (Exception e) {
            Debug.Log(e.ToString());
        }
    }

    public void LockDisconnectChatting()
    {
        if (_clientSocket != null && _clientSocket.Connected && _chatCurState == ChatCurrentState.Connected) {
            if (_chatController != null) {
                _chatController.NotifyChatReceiveMessage(-1, GetDisconnectChatMessage());
                _chatController.NotifyChatReceiveMessage(-1, GetLockDisconnectChatMessage());
            }
        }

        _isPingRequestPacket = false;

        _recvPacket.ClearPacket();

        if (_eventTimer.ExistTimerDataByID(_pingTimeID))
            _eventTimer.RemoveTimeEventByID(_pingTimeID);

        _eventTimer.CompleteTimeEventByTimeID(_previewUITimeID);
        _eventTimer.ReleaseGameTimerList();

        try {
            _isLogin = false;
            if (_clientSocket != null) {
                _clientSocket.Client.Disconnect(false);
                _clientSocket.GetStream().Close();
                _clientSocket.Close();
                _clientSocket = null;
            }
        } catch (Exception e) {
            ChatEventManager.ChatLog(e.ToString());
        }

        SetCurChatState(ChatCurrentState.LockChatting);
    }

    ChatMessage GetDisconnectChatMessage()
    {
        ChatMessage retMessage = new ChatMessage();
        retMessage.isSelfNotify = true;
        retMessage.msgIdx = (int)ChatNoticeMessageKey.DisConnect;

        return retMessage;
    }

    ChatMessage GetLockDisconnectChatMessage()
    {
        ChatMessage retMessage = new ChatMessage();
        retMessage.isSelfNotify = true;
        retMessage.msgIdx = (int)ChatNoticeMessageKey.FastWriteWarningMessage;

        return retMessage;
    }

    public void Reconnect()
	{
		_retryConnectCount = 3;
		_retryLoginCount = 3;
        SetCurChatState(ChatCurrentState.ReconnectGameServer);
    }

	public void ReconnectGameServer()
	{
		_retryConnectCount = 3;
		_retryLoginCount = 3;
		SetCurChatState (ChatCurrentState.ReconnectGameServer);
	}

	public void CheckCurChatPartyGroup(Action onPartyUpdated = null)
	{
        if(ChattingController.Instance.PartyChatModel.CurPartyId != -1) {
            SendConnectCurPartyPacket(ChattingController.Instance.PartyChatModel.CurPartyId, null);
            return;
        }

		List<ChatPartyBaseInfo> chatPartyInfos = new List<ChatPartyBaseInfo>();

        // My Party Chat
        chatPartyInfos.Add(ChattingController.Instance.MyChatPartyInfo);

        // Guild Group
        Dictionary<long /* Party Num */, ChatGuildJoinedInfo> guildInfos = ChattingController.Instance.ChatGuildGroupInfos;
        if (guildInfos != null) {
            List<long> guildIdKeys = guildInfos.Keys.ToList();
            for (int i = 0; i < guildIdKeys.Count; i++) {
                ChatPartyBaseInfo chatGuildInfo = guildInfos[guildIdKeys[i]];
                chatPartyInfos.Add(chatGuildInfo);
            }
        }

        _onSuccessPartyUpdate = onPartyUpdated;

		SendConnectPartyGroupPacket (chatPartyInfos);
	}

	void SetChatLoginData()
	{
		SetCurChatState(ChatCurrentState.LoginConnect);
		SendRequestLoginPacket(ChattingPacketType.Login2Req);
		_retryLoginCount--;
	}

	void ReChatLogin()
	{
        if(_clientSocket == null)
            return;

		if (_chatCurState != ChatCurrentState.Connected)
			return;

		if (!_clientSocket.Connected) {
			return;
		}

		_retryLoginCount = 3;
		SetChatLoginData ();
	}

    Queue<ChattingPacket> _requestQueuePackets = new Queue<ChattingPacket>();

    void AddQueueRequestPacket(ChattingPacket addPacket)
    {
        _requestQueuePackets.Enqueue(addPacket);
    }

    void WriteServerStream(ChattingPacket requestChatPacket)
	{
        if(requestChatPacket == null)
            return;

        if (_clientSocket.GetStream().CanWrite) {
            try {
				_clientSocket.GetStream().Write(requestChatPacket.packetBuffer, 0, (int)(requestChatPacket.packetLength));
            } catch (Exception e) {
                Debug.Log(string.Format("WriteServerStream Fail : {0}", e.ToString()));
                if (IsValidDisconnect()) {
                    Disconnect();
                    Reconnect();
                }
            }
        } else {
            Debug.Log(string.Format("WriteServerStream _serverStream.CanWrite : {0}", _clientSocket.GetStream().CanWrite));
        }
    }

	public void SendRequestPacket(ChattingPacketType packetType, ChatMessage message = null)
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		if (_chatCurState != ChatCurrentState.Connected)
			return;

		ChattingPacket requestChatPacket = null;

		string messageText = "";
		if (message != null) {
			messageText = ChatParsingUtil.MakeChatMessageToString (message);
		}

		switch (packetType) {
		case ChattingPacketType.PingReq:
			requestChatPacket = GetRequestPingReqPacket ();
			break;
		case ChattingPacketType.UserChatReq:
			{
				requestChatPacket = GetRequestUserChatReqPacket (messageText);
			}
			break;
		case ChattingPacketType.PartyChatV2Req:
			{
				requestChatPacket = GetRequestPartyChatV2ReqPacket (messageText, message.saveState, message.partyNum, message.partyType);
			}
			break;
		}

        AddQueueRequestPacket(requestChatPacket);
	}

    public void SendRequestPartyEventPacket(long partyNum, int partyType, string sendMessage)
    {
        if (_clientSocket == null || !_clientSocket.Connected)
            return;

        if (_chatCurState != ChatCurrentState.Connected)
            return;

        ChattingPacket requestChatPacket = GetRequestPartyChatV2ReqPacket(sendMessage, 2, partyNum, partyType);

        AddQueueRequestPacket(requestChatPacket);
    }

	public void SendWhisperRequestPacket(ChatWhisperMessage message = null)
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		if (_chatCurState != ChatCurrentState.Connected)
			return;

		ChattingPacket requestChatPacket = null;

		string messageText = "";
		if (message != null) {
			messageText = ChatParsingUtil.MakeChatWhisperMessageToString (message);
		}

		requestChatPacket = GetRequestWhisperChatReqPacket (messageText, message.targetConnectID);

        AddQueueRequestPacket(requestChatPacket);
	}

	public void SendConnectPartyGroupPacket(List<ChatPartyBaseInfo> chatPartyInfos)
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		if (_chatCurState != ChatCurrentState.Connected)
			return;

		if (!ChattingController.Instance.IsChatGuildConnected) {
			if (chatPartyInfos == null || chatPartyInfos.Count == 0)
				return;
		}

		ChattingPacket requestChatPacket = GetRequestChangePartyV2ReqPacket (false, chatPartyInfos);

        AddQueueRequestPacket(requestChatPacket);
	}

    public void SendConnectCurPartyPacket(long partyId, Action<bool> onFinishConnect, bool isEventOnly = false, bool isSinglePlay = false)
    {
        if(partyId == -1) {
            _onFinishCurPartyConnect = null;
        }

        if (_clientSocket == null || !_clientSocket.Connected) {
            if(onFinishConnect != null)
                onFinishConnect(false);
            return;
        }

        if (_chatCurState != ChatCurrentState.Connected) {
            if (onFinishConnect != null)
                onFinishConnect(false);
            return;
        }

        if(onFinishConnect != null)
            _onFinishCurPartyConnect = onFinishConnect;

        List<ChatPartyBaseInfo> chatPartyInfos = new List<ChatPartyBaseInfo>();

        // My Party Chat
        chatPartyInfos.Add(ChattingController.Instance.MyChatPartyInfo);

        // Guild
        Dictionary<long /* Party Num */, ChatGuildJoinedInfo> guildInfos = ChattingController.Instance.ChatGuildGroupInfos;
        if (guildInfos != null) {
            List<long> guildIdKeys = guildInfos.Keys.ToList();
            for (int i = 0; i < guildIdKeys.Count; i++) {
                ChatPartyBaseInfo chatGuildInfo = guildInfos[guildIdKeys[i]];
                chatPartyInfos.Add(chatGuildInfo);
            }
        }

        // Cur Party
        if (partyId != -1 && !isEventOnly) {
            ChatPartyBaseInfo inputPartyInfo = new ChatPartyBaseInfo();
            if (isSinglePlay) {
                inputPartyInfo.party_type = (int)ChatPartyType.ChatSingleParty;
            } else {
                inputPartyInfo.party_type = (int)ChatPartyType.ChatParty;
            }
            
            inputPartyInfo.party_num = partyId;
            chatPartyInfos.Add(inputPartyInfo); 
        }

        // Cur Event  Party
        if (partyId != -1) {
            ChatPartyBaseInfo inputPartyInfo = new ChatPartyBaseInfo();
            if (isSinglePlay) {
                inputPartyInfo.party_type = (int)ChatPartyType.ChatSingleEventParty;
            } else {
                inputPartyInfo.party_type = (int)ChatPartyType.ChatEventParty;
            }
            
            inputPartyInfo.party_num = partyId;
            chatPartyInfos.Add(inputPartyInfo);
        }

        ChattingPacket requestChatPacket = null;
        if(chatPartyInfos.Count > 0) {
            requestChatPacket = GetRequestChangePartyV2ReqPacket(false, chatPartyInfos);
        } else {
            requestChatPacket = GetRequestChangePartyV2ReqPacket(true);
        }

        AddQueueRequestPacket(requestChatPacket);
    }

    public void SendDisconnectPartyGroupPacket()
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		if (_chatCurState != ChatCurrentState.Connected)
			return;

		ChattingPacket requestChatPacket = GetRequestChangePartyV2ReqPacket (true);

        AddQueueRequestPacket(requestChatPacket);
	}

	public void SendRequestLoginPacket(ChattingPacketType packetType)
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		ChattingPacket requestChatPacket = null;

		switch (packetType) {
		case ChattingPacketType.LoginReq:
			requestChatPacket = GetRequestLoginReqPacket ();
			break;
		case ChattingPacketType.Login2Req:
			requestChatPacket = GetRequestLogin2ReqPacket ();
			break;
		}

        WriteServerStream(requestChatPacket);
	}

    public void SendRequestUserInfoListPacket(long[] connectIDs, Action<ChatUserInfoListPacket[]> onSuccessPacket)
    {
        if (_clientSocket == null || !_clientSocket.Connected) {
            onSuccessPacket(null);
            return;
        }

        if(connectIDs == null || connectIDs.Length == 0) {
            onSuccessPacket(null);
            return;
        }

        _onSuccessUserInfoList = onSuccessPacket;
        ChattingPacket requestChatPacket = GetRequestUserInfoListPacket(connectIDs);

        AddQueueRequestPacket(requestChatPacket);
    }

    public void SendRequestChangeGroupPacket(int channelNum)
	{
		if (_clientSocket == null || !_clientSocket.Connected)
			return;

		ChattingPacket requestChatPacket = GetRequestChangeGroupPacket(channelNum);

        AddQueueRequestPacket(requestChatPacket);
	}

    public bool IsValidDisconnect()
    {
        if (_chatCurState != ChatCurrentState.Connecting && _chatCurState != ChatCurrentState.DelayConnect &&
                    _chatCurState != ChatCurrentState.LoginConnect && _chatCurState != ChatCurrentState.Reconnect && _chatCurState != ChatCurrentState.ReconnectGameServer &&
                    _chatCurState != ChatCurrentState.ServerMaintenance) {
            return true;
        }

        return false;
    }

	public void ReceiveResponsePacket(int packetType)
	{
        ChatMessage recMessage = null;

        switch ((ChattingPacketType)packetType) {
            case ChattingPacketType.Error:
                Debug.Log("Server Error " + _recvPacket.ReadInt());
                if (IsValidDisconnect()) {
                    Disconnect();
                    ReconnectGameServer();
                }               
                return;
            case ChattingPacketType.LoginRes:
                recMessage = OnResponseLoginResPacket();
                break;
            case ChattingPacketType.Login2Res:
                recMessage = OnResponseLogin2ResPacket();
                break;
            case ChattingPacketType.PingRes:
                OnResponsePingResPacket();
                break;
            case ChattingPacketType.UserChatRes:
                OnResponseUserChatResPacket();
                break;
            case ChattingPacketType.UserChatNotify:
                recMessage = OnResponseUserChatNotifyResPacket();
                break;
            case ChattingPacketType.GroupChangeRes:
                recMessage = OnResponseChangeGroupResPacket();
                break;
            case ChattingPacketType.GuildChangeRes:
                break;
            case ChattingPacketType.GuildChatRes:
                break;
            case ChattingPacketType.GuildChatV2Res:
                break;
            case ChattingPacketType.GuildChatNotify:
                break;
            case ChattingPacketType.UserInfoListRes:
                OnResponseUserInfoListPacket();
                break;
            case ChattingPacketType.ServerChatNotify:
                recMessage = OnResponseServerChatNotifyResPacket();
                break;
            case ChattingPacketType.WideChatRes:
                break;
            case ChattingPacketType.UserListChatRes:
                break;
            case ChattingPacketType.WhisperRes:
                OnResponseWhisperResPacket();
                break;
            case ChattingPacketType.WhisperNotify:
                recMessage = OnResponseWhisperNotifyResPacket() as ChatMessage;
                break;
            case ChattingPacketType.PartyChangeRes:
                break;
            case ChattingPacketType.PartyChatRes:
                break;
            case ChattingPacketType.PartyChatNotify:
                recMessage = OnResponsePartyChatNotifyResPacket();
                break;
            case ChattingPacketType.PartyChangeV2Res:
                recMessage = OnResponseChangePartyV2ResPacket();
                break;
            case ChattingPacketType.PartyChatV2Res:
                OnResponsePartyChatV2ResPacket();
                break;
            case ChattingPacketType.PartyChatV2Notify:
                recMessage = OnResponsePartyChatV2NotifyResPacket();
                break;
            case ChattingPacketType.ServerPartyChatV2Notify:
                recMessage = OnResponseServerPartyChatV2NotifyResPacket();
                break;
        }
        
        if (_chatController != null && recMessage != null) {
            _chatController.NotifyChatReceiveMessage(packetType, recMessage);
        }
	}

	#endregion

	#region Request Packet Methods

	ChattingPacket GetRequestLoginReqPacket()
	{
		ChattingPacket retPacket = new ChattingPacket ();

        retPacket.WriteHeader(ChattingPacketType.LoginReq);
		retPacket.WriteInt(1); //social channel(1:hive 2:kakao)
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data)); //connect id
		retPacket.WriteInt(_chattingServerInfo.gameServerId); //game server id
		retPacket.WriteInt(_chattingServerInfo.loginKey); //login key
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestLogin2ReqPacket()
	{
		ChattingPacket retPacket = new ChattingPacket ();

        retPacket.WriteHeader(ChattingPacketType.Login2Req);
		retPacket.WriteInt(1); //social channel(1:hive 2:kakao)
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data)); //connect id
		retPacket.WriteInt(_chattingServerInfo.gameServerId); //game server id
		retPacket.WriteInt(_chattingServerInfo.loginKey); //login key
		retPacket.WriteInt(_chattingModel.ChatGroupNum); // chat group num
		retPacket.WriteInt(0); // lang code 0 : 101 Num Channel Start, ex) 1 => 1 * 1000, 1000 Num Channel Start
		retPacket.WriteInt(0); // guild num 0 : Unjoin Guild
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestPingReqPacket()
	{
		ChattingPacket retPacket = new ChattingPacket ();

        retPacket.WriteHeader(ChattingPacketType.PingReq);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data));
		retPacket.WriteLong(_chattingModel.ChatUserId);
		retPacket.WriteInt(10); 	//Ping ID
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestUserChatReqPacket(string message)
	{
		ChattingPacket retPacket = new ChattingPacket ();

        byte[] encbuf = System.Text.Encoding.UTF8.GetBytes(message);
        string base64Str = System.Convert.ToBase64String(encbuf);
        encbuf = System.Text.ASCIIEncoding.ASCII.GetBytes(base64Str);

        if (encbuf.Length + 24 > retPacket.curPacketLength) {
            retPacket.curPacketLength += (encbuf.Length + 24 - retPacket.curPacketLength);
            retPacket.ResetPacketBuffer(retPacket.curPacketLength);
        }

        retPacket.WriteHeader(ChattingPacketType.UserChatReq);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data));
		retPacket.WriteLong(_chattingModel.ChatUserId);
		retPacket.WriteEncBuf(encbuf);
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestWhisperChatReqPacket(string message, long targetUserID)
	{
		ChattingPacket retPacket = new ChattingPacket ();

        byte[] encbuf = System.Text.Encoding.UTF8.GetBytes(message);
        string base64Str = System.Convert.ToBase64String(encbuf);
        encbuf = System.Text.ASCIIEncoding.ASCII.GetBytes(base64Str);

        if (encbuf.Length + 34 > retPacket.curPacketLength) {
            retPacket.curPacketLength += (encbuf.Length + 34 - retPacket.curPacketLength);
            retPacket.ResetPacketBuffer(retPacket.curPacketLength);
        }

        retPacket.WriteHeader(ChattingPacketType.WhisperReq);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data));
		retPacket.WriteLong(_chattingModel.ChatUserId);
		retPacket.WriteLong(targetUserID);
		retPacket.WriteEncBuf(encbuf);
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestPartyChatV2ReqPacket(string message, int messageSaveState, long partyNum, int partyType)
	{
		ChattingPacket retPacket = new ChattingPacket ();

        byte[] encbuf = System.Text.Encoding.UTF8.GetBytes(message);
        string base64Str = System.Convert.ToBase64String(encbuf);
        encbuf = System.Text.ASCIIEncoding.ASCII.GetBytes(base64Str);

        if(encbuf.Length + 54 > retPacket.curPacketLength) {
            retPacket.curPacketLength += (encbuf.Length + 54 - retPacket.curPacketLength);
            retPacket.ResetPacketBuffer(retPacket.curPacketLength);
        }

        retPacket.WriteHeader(ChattingPacketType.PartyChatV2Req);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data));
		retPacket.WriteLong(_chattingModel.ChatUserId);
		retPacket.WriteInt (messageSaveState); // 1 : Save Message, 2 : Not Save Message
		retPacket.WriteLong(partyNum);
		retPacket.WriteInt (partyType);
		retPacket.WriteEncBuf(encbuf);
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestChangePartyV2ReqPacket(bool isDisconnect, List<ChatPartyBaseInfo> chatPartyInfos = null)
	{
		ChattingPacket retPacket = new ChattingPacket ();

        retPacket.WriteHeader(ChattingPacketType.PartyChangeV2Req);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data)); //connect id
		retPacket.WriteLong(_chattingModel.ChatUserId);
		if(isDisconnect || chatPartyInfos == null || chatPartyInfos.Count == 0) // Party Group Discconect
		{
			retPacket.WriteInt(0);
		}
		else
		{
			retPacket.WriteInt(chatPartyInfos.Count);
			for (int i = 0; i < chatPartyInfos.Count; i++) {
				retPacket.WriteLong(chatPartyInfos[i].party_num);
				retPacket.WriteInt(chatPartyInfos[i].party_type);
			}
		}
		retPacket.MakePacket();

		return retPacket;
	}

	ChattingPacket GetRequestChangeGroupPacket(int channelNum)
	{
		ChattingPacket retPacket = new ChattingPacket ();

        retPacket.WriteHeader(ChattingPacketType.GroupChangeReq);
		retPacket.WriteLong(ChatHelper.GetChatChannelID(_data)); //connect id
		retPacket.WriteLong(_chattingModel.ChatUserId);
		retPacket.WriteInt(channelNum);
		retPacket.MakePacket();

		return retPacket;
	}

    ChattingPacket GetRequestUserInfoListPacket(long[] connectIDs)
    {
        ChattingPacket retPacket = new ChattingPacket();

        retPacket.WriteHeader(ChattingPacketType.UserInfoListReq);
        retPacket.WriteLong(ChatHelper.GetChatChannelID(_data)); //chat connect id
        retPacket.WriteLong(_chattingModel.ChatUserId);
        retPacket.WriteInt(connectIDs.Length); //request User Count
        for(int i = 0;i< connectIDs.Length;i++) {
            retPacket.WriteLong(connectIDs[i]); //Connect ID
        }
        
        retPacket.MakePacket();

        return retPacket;
    }

    #endregion

    #region Response Packet Methods

    ChatMessage OnResponseLoginResPacket()
	{
		ChatMessage retMessage = null;
		_isLogin = true;
		_chattingModel.ChatUserId = _recvPacket.ReadLong();
		_chattingModel.ChatGroupNum = _recvPacket.ReadInt();

		retMessage = new ChatMessage();
		retMessage.isSelfNotify = true;
		retMessage.msgIdx = (int)ChatNoticeMessageKey.ChannelEnter;
		retMessage.prm.Add("channel", _chattingModel.ChatGroupNum.ToString());

		_eventTimer.RemoveTimeEventByID(_loginTimeID);
		SetCurChatState (ChatCurrentState.DelayConnect);

		return retMessage;
	}

	ChatMessage OnResponseLogin2ResPacket()
	{
		ChatMessage retMessage = null;

		_isLogin = true;
		_chattingModel.ChatUserId = _recvPacket.ReadLong();
		_chattingModel.ChatGroupNum = _recvPacket.ReadInt();
        _chattingModel.LangCode = _recvPacket.ReadInt(); // lang Code
		_recvPacket.ReadInt(); // Guild Num

		retMessage = new ChatMessage();
		retMessage.isSelfNotify = true;
		retMessage.msgIdx = (int)ChatNoticeMessageKey.ChannelEnter;
		retMessage.prm.Add("channel", _chattingModel.ChatGroupNum.ToString());

		_eventTimer.RemoveTimeEventByID(_loginTimeID);
		SetCurChatState (ChatCurrentState.DelayConnect);

		return retMessage;
	}

    ChatMessage GetEnterGuildChatMessage()
    {
        ChatMessage retMessage = null;

        retMessage = new ChatMessage();
        retMessage.isSelfNotify = true;
        retMessage.msgIdx = (int)ChatNoticeMessageKey.GuildChannelEnter;
        retMessage.timeStamp = TimeUtil.GetTimeStamp();

        return retMessage;
    }

	ChatMessage OnResponseChangeGroupResPacket()
	{
		ChatMessage retMessage = null;

		int result = _recvPacket.ReadInt();
		int groupNum = _recvPacket.ReadInt();

		if(result == 1)
		{
			_chattingModel.ChatGroupNum = groupNum;

			ChattingController.Instance.ResetChannelChatMessage ();

			retMessage = new ChatMessage();
			retMessage.isSelfNotify = true;
			retMessage.msgIdx = (int)ChatNoticeMessageKey.ChannelEnter;
			retMessage.prm.Add("channel", _chattingModel.ChatGroupNum.ToString());
		}

        #if _CHATTING_LOG
		Debug.Log("OnResponseChangeGroupResPacket result : " + result + " groupNum : " + groupNum);
        #endif

		return retMessage;
	}

	void OnResponsePingResPacket()
	{
#if _CHATTING_LOG
//		Debug.Log (string.Format ("SetResponsePingResPacket"));
#endif
		_recvPacket.ReadInt (); // Ping_Num

		if(!_eventTimer.ExistTimerDataByID(_pingTimeID))
			_eventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnPingPacketRes, 10f, null, _pingTimeID);
		_isPingRequestPacket = false;
	}

	void OnResponseUserChatResPacket()
	{
		int result = _recvPacket.ReadInt();

#if _CHATTING_LOG
		Debug.Log("OnResponseUserChatResPacket " + (result == 1 ? "Success" : "Fail"));
#endif

		if (result != 1) {
			OnFailedPacketRes ();
		}
	}

    void OnResponseUserInfoListPacket()
    {
        int result = _recvPacket.ReadInt();

#if _CHATTING_LOG
        //Debug.Log("OnResponseUserInfoListPacket " + (result == 1 ? "Success" : "Fail"));
#endif

        if(result != 0) { // Result Always 0 Return
            Debug.Log(string.Format("OnResponseUserInfoListPacket result != 0"));
        }

        int res_user_info_count = _recvPacket.ReadInt();

        ChatUserInfoListPacket[] chatUserInfos = new ChatUserInfoListPacket[res_user_info_count];
        for (int i = 0; i < res_user_info_count; i++) {
            ChatUserInfoListPacket inputChatUserInfo = new ChatUserInfoListPacket();
            inputChatUserInfo.channel_user_id = _recvPacket.ReadLong();
            inputChatUserInfo.chat_group_num = _recvPacket.ReadInt();
            chatUserInfos[i] = inputChatUserInfo;
        }

        if (_onSuccessUserInfoList != null) {
            _onSuccessUserInfoList(chatUserInfos);
            _onSuccessUserInfoList = null;
        }
    }

    void OnResponseWhisperResPacket()
	{
		int result = _recvPacket.ReadInt();

#if _CHATTING_LOG
		Debug.Log("OnResponseWhisperResPacket " + (result == 1 ? "Success" : "Fail"));
#endif

		if (result != 1) {
			OnFailedPacketRes ();
		}
	}

	ChatMessage OnResponseUserChatNotifyResPacket()
	{
		short msg_size = 0;
		string msg = string.Empty;

		msg_size = _recvPacket.ReadShort();
		msg = _recvPacket.ReadString(msg_size);
#if _CHATTING_LOG
        Debug.Log(string.Format("OnResponseUserChatNotifyResPacket msg : {0}", msg));
#endif

        ChatMessage chatMessage = ChatParsingUtil.GetChatMessageParsingByString(msg);

        ChatEventManager.SetChatItemPrmInfo(chatMessage);
        ChatEventManager.SetChatTraceNamePrmInfo(chatMessage);
        ChatEventManager.SetChatBattleCenterNamePrmInfo(chatMessage);
        ChatEventManager.SetChatHeroChallengeMaxScoreTextPrmInfo(chatMessage);

        return chatMessage;
	}

	ChatMessage OnResponseServerChatNotifyResPacket()
	{
		short msg_size = 0;
		string msg = string.Empty;

		msg_size = _recvPacket.ReadShort();
		msg = _recvPacket.ReadString(msg_size);

#if _CHATTING_LOG
		Debug.Log (string.Format ("!!!!! OnResponseServerChatNotifyResPacket msg_size : {0}, msg : {1}", msg_size, msg));
#endif

        ChatGMSMessage chatMessage = ChatParsingUtil.GetChatMessageParsingByServerChat(msg);
        if(chatMessage.messageType == (int)ChatDefinition.ChatMessageType.GMChannelChat) {
            chatMessage.prm.Add("msg", string.Format("[GM]{0}",chatMessage.messageText));
        } else if(chatMessage.messageType == (int)ChatDefinition.ChatMessageType.GMSystemChatMessage) {
            SetGMSystemChatMessage(chatMessage);
            return null;
        }

        ChatEventManager.SetChatItemPrmInfo(chatMessage);
        ChatEventManager.SetChatTraceNamePrmInfo(chatMessage);
        ChatEventManager.SetChatBattleCenterNamePrmInfo(chatMessage);

        return (ChatMessage)chatMessage;
	}

    void SetGMSystemChatMessage(ChatGMSMessage chatSystemMessage)
    {
        string[] parsingMessages = ChatParsingUtil.GetChatSystemParsingMessages(chatSystemMessage.messageText);
        UITopDepthNoticeMessage.Instance.SetEnableFlowSystemParsingMessage(parsingMessages, chatSystemMessage.chatColor, chatSystemMessage.fixedTime, chatSystemMessage.gapTime);

        if(parsingMessages != null && parsingMessages.Length > 0) {
            for(int i = 0;i< parsingMessages.Length;i++) {
                string message = parsingMessages[i];
                ChatGMSMessage chatMessage = ChatHelper.GetChatGMSMessage((int)ChatNoticeMessageKey.NoticeMessage, ChatDefinition.ChatMessageType.GMSystemChatMessage, message, chatSystemMessage.chatColor);
                _chatController.NotifyChatReceiveMessage((int)ChattingPacketType.ServerChatNotify, chatMessage);
            }
        }
    }

    void OnResponsePartyChatV2ResPacket()
	{
		int result = _recvPacket.ReadInt();

#if _CHATTING_LOG
		Debug.Log("OnResponsePartyChatV2ResPacket " + (result == 1 ? "Success" : "Fail"));
#endif

		if (result != 1) {
			OnFailedPacketRes ();
		}
	}

	ChatMessage OnResponsePartyChatNotifyResPacket()
	{
		short msg_size = 0;
		string msg = string.Empty;

		long partyNum = _recvPacket.ReadLong (); // party_num
		long timeStamp = _recvPacket.ReadLong (); //timestamp

		msg_size = _recvPacket.ReadShort();
		msg = _recvPacket.ReadString(msg_size);

		ChatMessage chatMessage = ChatParsingUtil.GetChatMessageParsingByString(msg);
		chatMessage.timeStamp = timeStamp;
		chatMessage.partyNum = partyNum;

        ChatEventManager.SetChatItemPrmInfo(chatMessage);
        ChatEventManager.SetChatTraceNamePrmInfo(chatMessage);
        ChatEventManager.SetChatBattleCenterNamePrmInfo(chatMessage);

        return chatMessage;
	}

	ChatMessage OnResponsePartyChatV2NotifyResPacket()
	{
		long party_num = _recvPacket.ReadLong();
		int party_type = _recvPacket.ReadInt ();

		long timeStamp = _recvPacket.ReadLong(); // timestamp
		short msg_size = _recvPacket.ReadShort();
		string msg = _recvPacket.ReadString(msg_size);

#if _CHATTING_LOG
		Debug.Log (string.Format ("PACKET_ID_USER_PARTY_CHAT_V2_RES msg_size : {0}, msg : {1}, timeStamp : {2}", msg_size, msg, timeStamp));
//		TimeSpan time = TimeSpan.FromMilliseconds((double)timeStamp);

//		string datePatt = @"M/d/yyyy hh:mm:ss tt";
//		DateTime dispDt = new System.DateTime(1970, 1, 1, 9, 0, 0, System.DateTimeKind.Utc).AddMilliseconds((double)timeStamp);
//
//		string dtString = dispDt.ToString(datePatt);
//
//		Debug.Log (string.Format ("OnResponsePartyChatV2NotifyResPacket dtString : {0}", dtString));
#endif

        if(party_type == (int)ChatPartyType.ChatEventParty) {
            PartyChatEventData partyEventData = PartyChatEventHelper.GetPartyChatEventParsingByString(msg);
            ChattingController.Instance.NotifyChatPartyEvent((ChatDefinition.PartyChatEventType)partyEventData.partyEventType, partyEventData);
            return null;
        } else {
            ChatMessage chatMessage = ChatParsingUtil.GetChatMessageParsingByString(msg);
            chatMessage.timeStamp = timeStamp;
            chatMessage.partyType = party_type;
            chatMessage.partyNum = party_num;

            ChatEventManager.SetChatMissionCommonPrmInfo(_data, chatMessage);

            ChatEventManager.SetChatItemPrmInfo(chatMessage);

            ChatEventManager.SetChatTraceNamePrmInfo(chatMessage);
            ChatEventManager.SetChatBattleCenterNamePrmInfo(chatMessage);

            ChatEventManager.SetChatCompanyShopPrmInfo(_data, chatMessage);

            return chatMessage;
        }
	}

	ChatMessage OnResponseServerPartyChatV2NotifyResPacket()
	{
		ChatMessage retMessage = null;

		long party_num = _recvPacket.ReadLong();
		int party_type = _recvPacket.ReadInt ();
        long timestamp = _recvPacket.ReadLong();

        short msg_size = _recvPacket.ReadShort();
		string msg = _recvPacket.ReadString(msg_size);

#if _CHATTING_LOG
        Debug.Log (string.Format ("PACKET_ID_SERVER_PARTY_CHAT_V2_NOTIFY party_num : {0}, party_type : {1} timestamp : {2}, msg_size : {3}, msg : {4}", party_num, party_type, timestamp, msg_size, msg));
#endif

        retMessage = _chatController.ChatEventManager.GetChatServerMessage (msg, party_type, party_num, timestamp, true);

        if (retMessage != null) {
            ChatEventManager.SetChatMissionCommonPrmInfo(_data, retMessage);
            ChatEventManager.SetChatItemPrmInfo(retMessage);
            ChatEventManager.SetChatTraceNamePrmInfo(retMessage);
            ChatEventManager.SetChatBattleCenterNamePrmInfo(retMessage);
            ChatEventManager.SetChatCompanyShopPrmInfo(_data, retMessage);
        }

        return retMessage;
	}

	ChatMessage OnResponseChangePartyV2ResPacket()
	{
        ChatMessage retValue = null;
		int result = _recvPacket.ReadInt();
		int partyCount = _recvPacket.ReadInt();

        ChattingController.Instance.IsPartyGroupConnected = false;
        ChattingController.Instance.IsChatGuildConnected = false;
        ChattingController.Instance.IsMyPartyConnected = false;
        ChattingController.Instance.IsPartyEventConnected = false;

        if (result == 1) {
			if (partyCount > 0) {
				for (int i = 0; i < partyCount; i++) {
					long partyNum = _recvPacket.ReadLong ();
					int partyType = _recvPacket.ReadInt ();
                    if(partyType == (int)ChatPartyType.ChatParty) {
                        ChattingController.Instance.IsPartyGroupConnected = true;
                    } else if(partyType == (int)ChatPartyType.ChatGuild) {
                        ChattingController.Instance.IsChatGuildConnected = true;
                    } else if(partyType == (int)ChatPartyType.ChatUserParty) {
                        ChattingController.Instance.IsMyPartyConnected = true;
                    } else if(partyType == (int)ChatPartyType.ChatEventParty) {
                        ChattingController.Instance.IsPartyEventConnected = true;
                    }
#if _CHATTING_LOG
                    Debug.Log(string.Format("OnResponseChangePartyV2ResPacket partyType : {0}, partyNum : {1}", partyType, partyNum));
#endif

                    if (!ChattingController.Instance.CheckRequestPartyMessage(partyType, partyNum)) {
                        ChatPartyBaseInfo requestParyInfo = new ChatPartyBaseInfo();
                        requestParyInfo.party_num = partyNum;
                        requestParyInfo.party_type = partyType;

                        ChattingPartyChatList partyChatList = new ChattingPartyChatList(_data, requestParyInfo, OnSuccessChattingPartyChatList, OnFailChattingPartyChatList);
                        partyChatList.RequestHttpWeb();
                    }
                }

                if (_onFinishCurPartyConnect != null) {
                    if(ChattingController.Instance.IsPartyGroupConnected || ChattingController.Instance.IsPartyEventConnected) {
                        _onFinishCurPartyConnect(true);
                    } else {
                        _onFinishCurPartyConnect(false);
                    }
                }
			} else {
                if (_onFinishCurPartyConnect != null) {
                    _onFinishCurPartyConnect(false);
                }
            }
		} else {
            OnFailedPacketRes ();

            if (_onFinishCurPartyConnect != null) {
                _onFinishCurPartyConnect(false);
            }
        }

        return retValue;
    }

	ChatWhisperMessage OnResponseWhisperNotifyResPacket()
	{
		long send_channel_user_id = _recvPacket.ReadLong();
        _recvPacket.ReadLong(); // recv_channel_user_id
        long timeStamp = _recvPacket.ReadLong();
		short msg_size = _recvPacket.ReadShort();
		string msg = _recvPacket.ReadString(msg_size);

#if _CHATTING_LOG
		Debug.Log (string.Format ("OnResponseWhisperNotifyResPacket msg_size : {0}, msg : {1}, timeStamp : {2}", msg_size, msg, timeStamp));
#endif

		ChatWhisperMessage chatMessage = ChatParsingUtil.GetChatWhisperMessageParsingByString(msg);

		long whisperUserID = 0;
        long connectId = 0;
		string whisperUserName = "";
        if (ChatHelper.GetChatChannelID(ChattingController.Instance.Context) == send_channel_user_id) {
            chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperToChatting;
            whisperUserID = chatMessage.targetUserID;
            connectId = chatMessage.targetConnectID;
            whisperUserName = chatMessage.targetUserName;
        } else {
            chatMessage.msgIdx = (int)ChatNoticeMessageKey.WhisperFromChatting;
            whisperUserID = chatMessage.userID;
            connectId = chatMessage.connectId;
            whisperUserName = chatMessage.sendUserName;
        }

        if (chatMessage.partMessageInfos.ContainsKey ("user")) {
			chatMessage.partMessageInfos.Remove ("user");
		}

		chatMessage.partMessageInfos.Add ("user", ChattingController.Instance.GetUserInfoChatPartInfo (whisperUserID, connectId));

		chatMessage.prm.Add ("user", whisperUserName);

		chatMessage.timeStamp = timeStamp;

		return chatMessage;
	}

#endregion

    #region CallBack Methods

	void OnDelayConnect(object objData)
	{
		SetCurChatState (ChatCurrentState.Connected);
	}

	void OnConnectTimeout(object objData)
	{
		Disconnect ();

		if (_retryConnectCount <= 0) {
			Debug.Log (string.Format ("Chat Socket Connect Failed!!!"));
			_retryConnectCount = 3;
			_eventTimer.SetGameTimerData (ActionEventTimer.TimerType.RealTime, OnReconnectTime, 60f);
			return;
		}

		Connect ();
	}

	void OnLoginTimeout(object objData)
	{
		Debug.Log (string.Format ("OnLoginTimeout SetChatLoginData"));
		if (_retryLoginCount <= 0) {
			Debug.Log (string.Format ("Chat Server Login Failed!!!"));
			_retryLoginCount = 3;
			Disconnect ();
			_eventTimer.SetGameTimerData (ActionEventTimer.TimerType.RealTime, OnReconnectTime, 60f);
			return;
		}

		SetChatLoginData ();
	}

	public void OnPingPacketRes(object objData)
	{
		if (_clientSocket == null || !_clientSocket.Connected || !_isLogin) {
			if(_clientSocket == null)
				Debug.Log (string.Format ("OnPingPacketRes _clientSocket == null"));
			else if(!_clientSocket.Connected)
				Debug.Log (string.Format ("OnPingPacketRes _clientSocket.Connected == FALSE"));
			else if(!_isLogin)
				Debug.Log (string.Format ("OnPingPacketRes chattingModel.IsLogin == FALSE"));

            if (IsValidDisconnect()) {
                Disconnect();
                Reconnect();
            }
			return;
		}

		_pingStartTime = Time.realtimeSinceStartup;
		_isPingRequestPacket = true;

		SendRequestPacket (ChattingPacketType.PingReq);
	}

	void OnServerMaintenanceTimeout(object objData)
	{
		Debug.Log (string.Format ("OnServerMaintenanceTimeout"));

        if (_onRequestNetChatInfo != null)
		    _onRequestNetChatInfo ();
	}

	void OnFailedPacketRes()
	{
        _recvPacket.ClearPacket();
    }

	void OnReChatLoginTime(object objData)
	{
		ReChatLogin ();
	}

	void OnReconnectTime(object objData)
	{
		Reconnect ();
	}

    void OnSuccessChattingPartyChatList(ChattingPartyChatResponse chatResponse)
    {
        NotifyPartyChatRes(chatResponse);

        if(chatResponse.partyInfo.party_type == (int)ChatPartyType.ChatGuild) {
            ChatMessage sendMessage = GetEnterGuildChatMessage();
            sendMessage.partyType = chatResponse.partyInfo.party_type;
            sendMessage.partyNum = chatResponse.partyInfo.party_num;
            ChattingController.Instance.CurSelectGuildInfo = chatResponse.partyInfo;
            _chatController.NotifyChatReceiveMessage((int)ChattingPacketType.PartyChangeV2Res, sendMessage);
        }
    }


    void OnFailChattingPartyChatList(ChattingPartyChatResponse chatResponse)
    {

    }

    #endregion

    #region AsyncCallBack Methdos

    void OnConnectSocket(IAsyncResult ar) 
	{
		try
		{
			if(_clientSocket.Connected)
			{
				_eventTimer.RemoveTimeEventByID(_connectTimeID);
#if _CHATTING_LOG
                Debug.Log (string.Format ("OnConnectSocket SetChatLoginData"));
#endif
                SetChatLoginData();
			}
			else
			{
				_clientSocket.EndConnect(ar);
			}
		} catch (Exception e) {
            Debug.Log(e.ToString());
            Disconnect();
        }
    }

#endregion

    #region IChatSendMessageObserver

    void IChatSendMessageObserver.OnChatSendMessage (ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage message)
	{
        switch (chatMessageKind) {
            case ChatDefinition.ChatMessageKind.ChannelChat:
                SendRequestPacket(ChattingPacketType.UserChatReq, message);
                break;
            case ChatDefinition.ChatMessageKind.GuildChat:
            case ChatDefinition.ChatMessageKind.GuildSystemChat:
                if (ChattingController.Instance.IsChatGuildConnected) {
#if _CHATTING_LOG
                    Debug.Log(string.Format("OnChatSendMessage message partyType {0}, partyNum : {1}", message.partyType, message.partyNum));
#endif
                    SendRequestPacket(ChattingPacketType.PartyChatV2Req, message);
                }
                break;
            case ChatDefinition.ChatMessageKind.PartyChat:
#if _CHATTING_LOG
                Debug.Log(string.Format("OnChatSendMessage PartyChat message partyType {0}, partyNum : {1}", message.partyType, message.partyNum));
#endif
                SendRequestPacket(ChattingPacketType.PartyChatV2Req, message);
                break;
            case ChatDefinition.ChatMessageKind.WhisperChat:
                SendWhisperRequestPacket(message as ChatWhisperMessage);
                break;
        }
    }

    #endregion

    #region IChatChangePartyObserver

    public void AttachChatChangePartyOb(IChatChangePartyObserver partyObserver)
    {
        if(_chatChangePartyGroupObservers.Contains(partyObserver))
            return;

        _chatChangePartyGroupObservers.Add(partyObserver);
    }

    public void DetachChatChangePartyOb(IChatChangePartyObserver partyObserver)
    {
        if (!_chatChangePartyGroupObservers.Contains(partyObserver))
            return;

        _chatChangePartyGroupObservers.Remove(partyObserver);
    }

    void NotifyPartyChatRes(ChattingPartyChatResponse chatResponse)
    {
        for(int i = 0;i< _chatChangePartyGroupObservers.Count;i++) {
            _chatChangePartyGroupObservers[i].OnPartyChatRes(chatResponse);
        }
    }

    public void ReleaseAllChangePartyOb()
    {
        _chatChangePartyGroupObservers.Clear();
    }

    #endregion
}
