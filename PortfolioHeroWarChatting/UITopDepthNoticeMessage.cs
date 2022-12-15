using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITopDepthNoticeMessage : MonoBehaviour
{
    private static UITopDepthNoticeMessage _instance = null;
    public static UITopDepthNoticeMessage Instance
    {
        get { return _instance; }
    }

    #region Serialize Variables

#pragma warning disable 649

    [Header("TopFlowNotice")]
    [SerializeField] TopFlowNoticeManager _topFlowNoticeManager = default(TopFlowNoticeManager);

    [Header("Chat SystemMessage Objs")]
    [SerializeField] GameObject _chatSystemMessageObj = default(GameObject);
    [SerializeField] Text _chatSystemMessageText = default(Text);

    [Header("Party Objs")]
    [SerializeField] UIPartyNotifyPopupManager _partyNotifyPopupManager = default(UIPartyNotifyPopupManager);
    [SerializeField] UIPartyTopNoticeInfo _partyTopNoticeInfo = default(UIPartyTopNoticeInfo);
    [SerializeField] UIPartyUserPenaltyPopup _partyUserPenaltyPopup = default(UIPartyUserPenaltyPopup);
    [SerializeField] UIPartyUserPenaltyPopup _partyUserScoreAddPopup = default(UIPartyUserPenaltyPopup);

#pragma warning restore 649

    #endregion

    #region Variables

    ActionEventTimer _eventTimer = new ActionEventTimer();
    List<UGUITweener> _messageTweeners = new List<UGUITweener>();
    List<UGUITweener> _flowMessageTweeners = new List<UGUITweener>();
    bool _isEnable = false;
    int _curParseIndex = 0;
    float _messageTotalTime = 10f;
    float _curTotalTime = 0f;
    float _curDurationTime = 10f;
    bool _isAppearTween = false;

    bool _isEffectState = false;

    #endregion

    #region Properties

    public Text ChatSystemMessageText
    {
        get { return _chatSystemMessageText; }
    }

    public UIPartyNotifyPopupManager PartyNotifyPopupManager
    {
        get { return _partyNotifyPopupManager; }
    }

    public UIPartyTopNoticeInfo PartyTopNoticeInfo
    {
        get { return _partyTopNoticeInfo; }
    }

    public UIPartyUserPenaltyPopup PartyUserPenaltyPopup
    {
        get { return _partyUserPenaltyPopup; }
    }

    public UIPartyUserPenaltyPopup PartyUserScoreAddPopup
    {
        get { return _partyUserScoreAddPopup; }
    }

    public bool IsEffectState
    {
        get { return _isEffectState; }
        set { _isEffectState = value; }
    }

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        _instance = this;

        _messageTweeners.Clear();
        UGUITweener[] tweeners = _chatSystemMessageObj.transform.GetComponentsInChildren<UGUITweener>(true);
        for(int i = 0;i< tweeners.Length;i++) {
            tweeners[i].enabled = false;
            _messageTweeners.Add(tweeners[i]);
        }

        _flowMessageTweeners.Clear();
        UGUITweener[] flowTweeners = _topFlowNoticeManager.transform.GetComponentsInChildren<UGUITweener>(true);
        for (int i = 0; i < flowTweeners.Length; i++) {
            flowTweeners[i].enabled = false;
            _flowMessageTweeners.Add(flowTweeners[i]);
        }

        if(_flowMessageTweeners.Count > 0) {
            _flowMessageTweeners[0].AddOnFinished(() => OnFinishTween());
        }

        _chatSystemMessageObj.SetActive(false);
        _topFlowNoticeManager.gameObject.SetActive(false);

        _topFlowNoticeManager.NoticeButton.onClick.RemoveAllListeners();
        _topFlowNoticeManager.NoticeButton.onClick.AddListener(() => OnFlowMessageClick());
    }

    #endregion

    #region Methods

    public void SetEnableFlowSystemParsingMessage(string[] parsingMessages, Color textColor, float totalTime = 10f, float gapTime = 10f)
    {
        if (parsingMessages == null || parsingMessages.Length == 0)
            return;

        _messageTotalTime = totalTime;
        if(_messageTotalTime <= 0f) {
            Debug.Log(string.Format("SetEnableFlowSystemParsingMessage _messageTotalTime <= 0f"));
            _messageTotalTime = 10f;
        }
        _curTotalTime = 0f;
        _curDurationTime = gapTime;
        if(_curDurationTime <= 0f) {
            Debug.Log(string.Format("SetEnableFlowSystemParsingMessage _curDurationTime <= 0f"));
            _curDurationTime = 10f;
        }

        _curParseIndex = 0;

        if (!_topFlowNoticeManager.gameObject.activeSelf) {
            _topFlowNoticeManager.InitFlowNotice();
        }

        _topFlowNoticeManager.SetMessageText(parsingMessages[_curParseIndex]);
        _curParseIndex++;
        _topFlowNoticeManager.SetMessageColor(textColor);

        if (!_topFlowNoticeManager.gameObject.activeSelf) {
            _isAppearTween = true;
            _topFlowNoticeManager.gameObject.SetActive(true);
            for (int i = 0; i < _flowMessageTweeners.Count; i++) {
                _flowMessageTweeners[i].PlayForward();
                _flowMessageTweeners[i].enabled = true;
            }
        }

        _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnFinishFlowParseDurationTime, _curDurationTime, parsingMessages, 1);

        SetEnableNotice(true);
    }

    void SetEnableNotice(bool enable)
    {
        _isEnable = enable;
        if(_isEnable) {
            StopCoroutine(UpdateSystemNotice());
            StartCoroutine(UpdateSystemNotice());
        } else {
            StopCoroutine(UpdateSystemNotice());
        }
    }

    public static void ReleaseChatSystemMessage()
    {
        if (_instance == null)
            return;

#if UNITY_EDITOR
        Destroy(_instance.gameObject, 0f);
#else
		DestroyImmediate (_instance.gameObject);
#endif
        _instance = null;
    }

    void SetDisableNotice()
    {
        _chatSystemMessageObj.SetActive(false);
        SetEnableNotice(false);
    }

    void SetDisableFlowNotice()
    {
        _topFlowNoticeManager.gameObject.SetActive(false);
        SetEnableNotice(false);
    }

    void SetHideTween()
    {
        if (_isEnable) {
            _isAppearTween = false;
            for (int i = 0; i < _messageTweeners.Count; i++) {
                _messageTweeners[i].PlayReverse();
                _messageTweeners[i].enabled = true;
            }
        }
    }

    void SetFlowHideTween()
    {
        if (_isEnable) {
            _isAppearTween = false;
            for (int i = 0; i < _flowMessageTweeners.Count; i++) {
                _flowMessageTweeners[i].PlayReverse();
                _flowMessageTweeners[i].enabled = true;
            }
        }
    }

    void CloseFlowMessage()
    {
        _eventTimer.ReleaseGameTimerList();

        SetDisableFlowNotice();

        if (!_isAppearTween) {
            for (int i = 0; i < _flowMessageTweeners.Count; i++) {
                _flowMessageTweeners[i].PlayForward();
                _flowMessageTweeners[i].ResetToBeginning();
                _flowMessageTweeners[i].enabled = false;
            }
        } else {
            for (int i = 0; i < _flowMessageTweeners.Count; i++) {
                _flowMessageTweeners[i].ResetToBeginning();
                _flowMessageTweeners[i].enabled = false;
            }
        }
    }

    public void SetEffectState(bool state)
    {
        _isEffectState = state;

        //if (_isEffectState) {
        //    _partyTopNoticeInfo.CloseOnly();
        //} else {
        //    _partyTopNoticeInfo.RefreshTopNotice();
        //}
    }

    #endregion

    #region CallBack Methods

    void OnFinishDurationTime(object objData)
    {
        SetHideTween();
    }

    void OnFinishFlowParseDurationTime(object objData)
    {
        _curTotalTime += _curDurationTime;
        string[] parsingMessages = (string[])objData;
        if (parsingMessages.Length <= _curParseIndex) {
            if (_curTotalTime >= _messageTotalTime) {
                SetFlowHideTween();
                return;
            } else {
                _curParseIndex = 0;
            }
        }

        _topFlowNoticeManager.SetMessageText(parsingMessages[_curParseIndex]);
        _curParseIndex++;
        _eventTimer.SetGameTimerData(ActionEventTimer.TimerType.RealTime, OnFinishFlowParseDurationTime, _curDurationTime, parsingMessages, 1);
    }

    public void OnFinishTween()
    {
        if(!_isAppearTween) {
            SetDisableFlowNotice();
        }
    }

    void OnFlowMessageClick()
    {
        CloseFlowMessage();
        ChattingController.Instance.UIMultiChat.ShowChattingPopup(ChatDefinition.ChatMessageKind.ChannelChat);
    }

    #endregion

    #region Coroutine Methods

    IEnumerator UpdateSystemNotice()
    {
        while (_isEnable) {
            _eventTimer.UpdateGameTimer();
            yield return null;
        }
    }

    #endregion
}
