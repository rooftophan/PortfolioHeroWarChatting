using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChatReceiveMessageObserver
{
    void OnChatReceiveMessage(int packetType, ChatMakingMessage makingMessage, ChatMessage chatMsg);
}

public interface IChatSendMessageObserver
{
    void OnChatSendMessage(ChatDefinition.ChatMessageKind chatMessageKind, ChatMessage message);
}

public interface IScrollObjectInfo
{
	GameObject ScrollGameObject { get; }
	float ObjectMaxWidth { get; set; }
	float ObjectWidth { get; }
	float ObjectHeight { get; }
}

public interface IChatServerMessage
{
	void OnChatServerMessageData (ChatServerMessageData serverMessageData);
}

public interface IChatActionMessage
{
    void OnChatActionMessage(ChatActionMessage actionMessage);
}

public interface IChatChangeCompanyNotice
{
	void OnChangeCompanyNotice(int partyType, long partyNum, string companyNotice);
}

public interface IChatTimeStamp
{
	long GetTimeStamp();
}

public interface IChatChangePartyObserver
{
    void OnPartyChatRes(ChattingPartyChatResponse chatResponse);
}

public interface IChatButtonObserver
{
    void OnChatButtonEvent(ChatDefinition.PartMessageType partMessageType, string[] partValues);
}

public interface IChatPartyEventObserver
{
    void OnChatPartyEvent(ChatDefinition.PartyChatEventType eventType, PartyChatEventData partyChatEvent);
}

public interface IChatGuildRaidEventObserver
{
    void OnChatGuildRaidEvent(ChatDefinition.GuildRaidEventType eventType, RaidChatEventData raidChatEvent);
}