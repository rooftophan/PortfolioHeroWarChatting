using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

#region ##ChattingInfo

public class ChatInfoRequest : RequestParam
{
	public long userId;
}

public class ChatInfoResponse : ResponseParam
{
	public long userId;
	public ChattingServerInfo chat;
}

public class ChatInfoTask : WebDelegate<ChatInfoRequest, ChatInfoResponse>
{
	public ChatInfoTask(DataContext data, Action<ChatInfoResponse> onSuccess, Action<ChatInfoResponse> onFail = null) : base(data, onSuccess, onFail) { }

	protected override void OnExecute()
	{
		_request.userId = _data.User.userData.userId;

		base.OnExecute();
	}

	protected override void OnSuccess(ChatInfoResponse resParam)
	{
	}

	protected override void OnFail(ChatInfoResponse resParam) 
	{
	}
}

#endregion

#region ##GuildChatViewUpdate

public class GuildChatViewUpdateRequest : RequestParam
{
    public long userId;
    public long guildId;
    public long chatViewTime;
}

public class GuildChatViewUpdateResponse : ResponseParam
{
    public CompanyMember companyMember;
}

public class ProtocolGuildChatViewUpdate : WebDelegate<GuildChatViewUpdateRequest, GuildChatViewUpdateResponse>
{
    long _guildId;
    long _chatViewTime;

    public ProtocolGuildChatViewUpdate(DataContext data, long guildId, long chatViewTime, Action<GuildChatViewUpdateResponse> onSuccess, Action<GuildChatViewUpdateResponse> onFail = null) : base(data, onSuccess, onFail)
    {
        _guildId = guildId;
        _chatViewTime = chatViewTime;
    }

    protected override void OnExecute()
    {
        _request.userId = _data.User.userData.userId;
        _request.guildId = _guildId;
        _request.chatViewTime = _chatViewTime;

        base.OnExecute();
    }

    protected override void OnSuccess(GuildChatViewUpdateResponse resParam)
    {
      
    }

    protected override void OnFail(GuildChatViewUpdateResponse resParam)
    {
    }
}

#endregion

#region ##PartyMyChatViewUpdate

public class PartyMyChatViewUpdateRequest : RequestParam
{
    public long userId;
    public long chatViewTime;
}

public class PartyMyChatViewUpdateResponse : ResponseParam
{
    
}

public class ProtocolPartyMyChatViewUpdate : WebDelegate<PartyMyChatViewUpdateRequest, PartyMyChatViewUpdateResponse>
{
    long _chatViewTime;

    public ProtocolPartyMyChatViewUpdate(DataContext data, long chatViewTime, Action<PartyMyChatViewUpdateResponse> onSuccess, Action<PartyMyChatViewUpdateResponse> onFail = null) : base(data, onSuccess, onFail)
    {
        _chatViewTime = chatViewTime;
    }

    protected override void OnExecute()
    {
        _request.userId = _data.User.userData.userId;
        _request.chatViewTime = _chatViewTime;

        base.OnExecute();
    }

    protected override void OnSuccess(PartyMyChatViewUpdateResponse resParam)
    {

    }

    protected override void OnFail(PartyMyChatViewUpdateResponse resParam)
    {
    }
}

#endregion