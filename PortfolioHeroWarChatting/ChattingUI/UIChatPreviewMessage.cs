using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIChatPreviewMessage : MonoBehaviour
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] UIMultiChatMessage _previewMultiChatMessage = default(UIMultiChatMessage);
    [SerializeField] RectTransform _previewChatBGRectTrans = default(RectTransform);
    [SerializeField] UGUITweener[] _previewTweeners = default(UGUITweener[]);

#pragma warning restore 649

    #endregion

    #region Properties

    public UIMultiChatMessage PreviewMultiChatMessage
    {
        get { return _previewMultiChatMessage; }
    }

    public RectTransform PreviewChatBGRectTrans
    {
        get { return _previewChatBGRectTrans; }
    }

    #endregion

    #region Methods

    public void SetPreviewMultiChatText(List<MultiChatTextInfo> multiChatInfos, Color chatTextColor, int messageType)
    {
        if (ChattingController.Instance.IsChattingPopup)
            return;

        if (multiChatInfos == null || multiChatInfos.Count == 0)
            return;

        for(int i = 0;i< _previewTweeners.Length; i++) {
            _previewTweeners[i].ResetToBeginning();
            _previewTweeners[i].PlayForward();
            _previewTweeners[i].enabled = true;
        }

        _previewMultiChatMessage.ResetPreviewChatMessage();
        List<MultiChatTextInfo> chatTextInfos = _previewMultiChatMessage.SetPreviewChatMessageList(multiChatInfos, chatTextColor, messageType);
        if (chatTextInfos != null && chatTextInfos.Count > 0) {
            float previewTextWidth = 0f;
            for (int i = 0; i < chatTextInfos.Count; i++) {
                previewTextWidth += (chatTextInfos[i].PartTextWidth * 0.8f);
            }

            SetMessageBGWidth(previewTextWidth);

            this.gameObject.SetActive(true);
        }
    }

    public void SetMessageBGWidth(float width)
    {
        width += 30f;
        if (width > 800f)
            width = 800f;

        _previewChatBGRectTrans.sizeDelta = new Vector2(width, _previewChatBGRectTrans.sizeDelta.y);
    }

    #endregion
}
