using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopFlowNoticeManager : MonoBehaviour
{
    #region Definitions

    public enum FlowNoticeStep
    {
        StartDelay,
        Flow,
    }

    #endregion

    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] Text _centerMessageText = default(Text);
    [SerializeField] Text _flowMessageText_1 = default(Text);
    [SerializeField] Text _flowMessageText_2 = default(Text);

    [SerializeField] Button _noticeButton = default(Button);

#pragma warning restore 649

    #endregion

    #region Variables

    float _viewRectWidth = 1200f;
    bool _isFlowState;
    float _startDelayTime = 1f;
    float _curStartDelayTime = 0f;
    float _flowSpeed = 150f;
    float _messageWidth;

    float _gapMessagePos = 60f;

    FlowNoticeStep _flowStep = FlowNoticeStep.StartDelay;

    RectTransform _flowMessageRectTrans_1 = null;
    RectTransform _flowMessageRectTrans_2 = null;

    #endregion

    #region Properties

    public Button NoticeButton
    {
        get { return _noticeButton; }
    }

    #endregion

    #region MonoBehaviour Methods

    private void Update()
    {
        if(_isFlowState) {
            switch (_flowStep) {
                case FlowNoticeStep.StartDelay:
                    _curStartDelayTime += Time.deltaTime;
                    if (_curStartDelayTime >= _startDelayTime) {
                        _flowStep = FlowNoticeStep.Flow;
                    }
                    break;
                case FlowNoticeStep.Flow:
                    float gapValue = 20f;
                    float moveValue = _flowSpeed * Time.deltaTime;

                    _flowMessageRectTrans_1.anchoredPosition3D = new Vector3(_flowMessageRectTrans_1.anchoredPosition3D.x - moveValue, 0f, 0f);
                    _flowMessageRectTrans_2.anchoredPosition3D = new Vector3(_flowMessageRectTrans_2.anchoredPosition3D.x - moveValue, 0f, 0f);
                    if (_flowMessageRectTrans_1.anchoredPosition3D.x < -(_messageWidth + gapValue)) {
                        _flowMessageRectTrans_1.anchoredPosition3D = new Vector3(_messageWidth + _gapMessagePos, 0f, 0f);
                    } else if (_flowMessageRectTrans_2.anchoredPosition3D.x < -(_messageWidth + gapValue)) {
                        _flowMessageRectTrans_2.anchoredPosition3D = new Vector3(_messageWidth + _gapMessagePos, 0f, 0f);
                    }
                    break;
            }
        }
    }

    #endregion

    #region Methods

    public void InitFlowNotice()
    {
        if(_flowMessageRectTrans_1 == null) {
            _flowMessageRectTrans_1 = _flowMessageText_1.gameObject.GetComponent<RectTransform>();
            _flowMessageRectTrans_2 = _flowMessageText_2.gameObject.GetComponent<RectTransform>();
        }
    }

    public void SetMessageText(string message)
    {
        _centerMessageText.text = message;
        _messageWidth = _centerMessageText.preferredWidth;
        _flowStep = FlowNoticeStep.StartDelay;
        _curStartDelayTime = 0f;

        if (_messageWidth > _viewRectWidth) {

            _centerMessageText.gameObject.SetActive(false);
            _isFlowState = true;
            _flowMessageText_1.text = message;
            _flowMessageText_1.gameObject.SetActive(true);

            _flowMessageText_2.text = message;
            _flowMessageText_2.gameObject.SetActive(true);

            _flowMessageRectTrans_1.anchoredPosition3D = Vector3.zero;
            _flowMessageRectTrans_2.anchoredPosition3D = new Vector3(_messageWidth + _gapMessagePos, 0f, 0f);
        } else {
            _centerMessageText.gameObject.SetActive(true);
            _flowMessageText_1.gameObject.SetActive(false);
            _flowMessageText_2.gameObject.SetActive(false);
            _isFlowState = false;
        }
    }

    public void SetMessageColor(Color color)
    {
        _flowMessageText_1.color = color;
        _flowMessageText_2.color = color;
        _centerMessageText.color = color;
    }

    #endregion

}
