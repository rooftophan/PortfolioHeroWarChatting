using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChattingButton : MonoBehaviour, IChatNudgeNotable
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Text _chatTitleText = default(Text);
	[SerializeField] Text _lastMessageText = default(Text);

	[SerializeField] GameObject _notCheckNumObjs = default(GameObject);
	[SerializeField] Text _notCheckChatNumText = default(Text);
	[SerializeField] Button _chatButton = default(Button);

	[SerializeField] GameObject _lastConfirmNoticeObj = default(GameObject);
	[SerializeField] Text _lastConfirmNoticeText = default(Text);

    [SerializeField] GameObject _selectObj = default(GameObject);
    [SerializeField] Text _selectTitleText = default(Text);

    [SerializeField] Button _channelChangeButton = default(Button);
    [SerializeField] Text _channelChangeText = default(Text);

#pragma warning restore 649

    #endregion

    #region Variables

    ChatDefinition.ChatMessageKind _chatMessageKind;
	int _partyType = -1;
	long _partyNum = -1;

	#endregion

	#region Properties

	public Text ChatTitleText
	{
		get{ return _chatTitleText; }
	}

	public Text LastMessageText
	{
		get{ return _lastMessageText; }
	}

	public GameObject NotCheckNumObjs
	{
		get{ return _notCheckNumObjs; }
	}

	public GameObject LastConfirmNoticeObj
	{
		get{ return _lastConfirmNoticeObj; }
	}

	public Text LastConfirmNoticeText
	{
		get{ return _lastConfirmNoticeText; }
	}

	public Text NotCheckChatNumText
	{
		get{ return _notCheckChatNumText; }
	}

	public Button ChatButton
	{
		get{ return _chatButton; }
	}

    public GameObject SelectObj
	{
		get{ return _selectObj; }
	}

    public Text SelectTitleText
	{
		get{ return _selectTitleText; }
	}

    public Button ChannelChangeButton
	{
		get{ return _channelChangeButton; }
	}

    public Text ChannelChangeText
	{
		get{ return _channelChangeText; }
	}

	public ChatDefinition.ChatMessageKind ChatMessageKind
    {
		get{ return _chatMessageKind; }
		set{ _chatMessageKind = value; }
	}

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

	#endregion

	#region Methods

    public void SetTitleText(string titleName)
    {
        _chatTitleText.text = titleName;
        _selectTitleText.text = titleName;
    }

    #endregion

    #region IChatNudgeNotable
    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        return _notCheckNumObjs;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        return _notCheckChatNumText;
    }

    #endregion
}
