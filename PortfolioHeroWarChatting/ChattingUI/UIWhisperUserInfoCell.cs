using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWhisperUserInfoCell : MonoBehaviour, IChatNudgeNotable
{
    #region Serialize Variables

#pragma warning disable 649

	[SerializeField] RectTransform _userIconTrn = default(RectTransform);
	[SerializeField] Text _userNameText = default(Text);
	[SerializeField] Text _connectingText = default(Text);
    [SerializeField] Text _channelConnectingText = default(Text);
    [SerializeField] Text _levelText = default(Text);
	[SerializeField] Button _userInfoButton = default(Button);
    [SerializeField] GameObject _notCheckNumObj = default(GameObject);
    [SerializeField] Text _notCheckNumText = default(Text);

#pragma warning restore 649

    #endregion

    #region Variables

    long _userId;
    long _connectId;
    ChatWhisperTargetUserInfo _whisperUserInfo = new ChatWhisperTargetUserInfo();
    UICommonUserPortraitIcon _userIcon = default(UICommonUserPortraitIcon);

    #endregion

    #region Properties

    public long UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    public long ConnectId
    {
        get { return _connectId; }
        set { _connectId = value; }
    }

    public RectTransform UserIconTrn
    {
	    get{ return _userIconTrn; }
    }
    
    public UICommonUserPortraitIcon UserIcon
	{
		get{ return _userIcon; }
		set { _userIcon = value; }
    }

    public Text UserNameText
	{
		get{ return _userNameText; }
	}

	public Text ConnectingText
	{
		get{ return _connectingText; }
	}

    public Text ChannelConnectingText
    {
        get { return _channelConnectingText; }
    }

    public Text LevelText
	{
		get{ return _levelText; }
	}

	public Button UserInfoButton
	{
		get{ return _userInfoButton; }
	}

    public GameObject NotCheckNumObj
    {
        get { return _notCheckNumObj; }
    }

    public Text NotCheckNumText
    {
        get { return _notCheckNumText; }
    }

    public ChatWhisperTargetUserInfo WhisperUserInfo
	{
		get{ return _whisperUserInfo; }
	}

    #endregion

    #region IChatNudgeNotable

    GameObject IChatNudgeNotable.GetNudgeObject()
    {
        return _notCheckNumObj;
    }

    Text IChatNudgeNotable.GetCountText()
    {
        return _notCheckNumText;
    }

    #endregion
}
