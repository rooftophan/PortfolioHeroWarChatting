using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChatMessageResultCode
{
	Success = 100,
}

public class ChatMessageList
{
	public string date;
	public string chat_msg;
}

public class ChatPartyMessageList
{
	public long timestamp;
	public string chat_msg;
}

public class ChatOtherUserListInfo
{
	public long other_user_id;
	public int show_type;
	public long begin_timestamp;
	public int show_count;
}

public class ChatWhisperUserMessageInfo : IChatTimeStamp
{
	public int type; // 1 : Send, 2 : Receive
	public long timestamp;
	public string chat_msg;

	public long GetTimeStamp()
	{
		return timestamp;
	}
}

public class ChatWhisperOtherUserResponseInfo
{
	public long other_user_id;
	public ChatWhisperUserMessageInfo[] message_list;
}

public class ChatWhisperLastCountResponseInfo
{
	public long other_user_id;
	public int message_count;
	public long read_timestamp;
	public long last_timestamp;
	public long del_timestamp;
}
