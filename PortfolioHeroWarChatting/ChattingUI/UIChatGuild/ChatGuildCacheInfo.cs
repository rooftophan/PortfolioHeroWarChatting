using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatGuildCacheInfo
{
    #region Variables

    UIMultiChatMessage[] _multiChatCaches;
    int _cacheIndex = 0;
    int _maxCacheCount;

    UIMultiChatMessage[] _noticeCacheMessages;
    int _noticeCacheIndex = 0;
    int _noticeMaxCount = 1;
    List<UIMultiChatMessage> _noticeMsgInfos = new List<UIMultiChatMessage>();

    float _noticeGapValue = 0f;
    float _curNoticeHeight = 0f;

    #endregion

    #region Properties

    public UIMultiChatMessage[] MultiChatCaches
    {
        get { return _multiChatCaches; }
    }

    public int CacheIndex
    {
        get { return _cacheIndex; }
        set { _cacheIndex = value; }
    }

    public int MaxCacheCount
    {
        get { return _maxCacheCount; }
        set { _maxCacheCount = value; }
    }

    public int NoticeMaxCount
    {
        get { return _noticeMaxCount; }
        set { _noticeMaxCount = value; }
    }

    public float CurNoticeHeight
    {
        get { return _curNoticeHeight; }
    }

    #endregion

    #region Methods

    public void MakeMultiCacheList(UIMultiChatMessage uiMultiChatMessage, Transform cacheChatParentTrans, int maxCount)
    {
        _maxCacheCount = maxCount;
        _multiChatCaches = new UIMultiChatMessage[maxCount];

        for (int i = 0; i < maxCount; i++) {
            _multiChatCaches[i] = GameObject.Instantiate<UIMultiChatMessage>(uiMultiChatMessage);

            var rectTransform = _multiChatCaches[i].GetComponent<RectTransform>();

            rectTransform.SetParent(cacheChatParentTrans.transform, false);
            _multiChatCaches[i].gameObject.SetActive(false);
        }

        MakeGuildNoticeCacheMessageList(uiMultiChatMessage, cacheChatParentTrans);
    }

    public void ClearMultiChatMessage(ObjectScrollManager objScrollManager, Transform cacheChatParentTrans)
    {
        if (_multiChatCaches != null) {
            for (int i = 0; i < _multiChatCaches.Length; i++) {
                if (_multiChatCaches[i].transform.parent == objScrollManager.ObjListTrans) {
                    _multiChatCaches[i].transform.SetParent(cacheChatParentTrans.transform, false);
                }

                _multiChatCaches[i].ResetChatMessage();
                _multiChatCaches[i].gameObject.SetActive(false);
            }
        }

        _cacheIndex = 0;
    }

    public void MakeGuildNoticeCacheMessageList(UIMultiChatMessage uiMultiChatMessage, Transform cacheChatParentTrans)
    {
        if (ChattingController.Instance == null)
            return;

        _noticeCacheMessages = new UIMultiChatMessage[_noticeMaxCount];

        for (int i = 0; i < _noticeMaxCount; i++) {
            _noticeCacheMessages[i] = GameObject.Instantiate<UIMultiChatMessage>(uiMultiChatMessage);

            var rectTransform = _noticeCacheMessages[i].GetComponent<RectTransform>();

            rectTransform.SetParent(cacheChatParentTrans.transform, false);
            _noticeCacheMessages[i].gameObject.SetActive(false);
        }
    }

    public UIMultiChatMessage AddSystemNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor, Transform chatNoticeRootTrans)
    {
        UIMultiChatMessage message = _noticeCacheMessages[_noticeCacheIndex];
        message.transform.SetParent(chatNoticeRootTrans, false);
        if (message.MultiChatTextInfos != null)
            message.ResetChatMessage();

        if (_noticeCacheIndex == _maxCacheCount - 1) {
            _noticeCacheIndex = 0;
        } else {
            _noticeCacheIndex++;
        }

        message.gameObject.SetActive(true);
        message.SetChatMessageList(chatTextInfos, chatColor);

        // Calc StartPosY
        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        float textHeight = (float)message.TextHeight;

        int quotientHeight = maxHeightCount / 2;
        int restHeight = maxHeightCount % 2;

        float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (textHeight * quotientHeight) - (textHeight * 0.5f);
        } else { // Odd
            startY = (textHeight * quotientHeight);
        }
        /////

        float startPosY = -startY;
        if (_noticeMsgInfos.Count == 0) {
            _curNoticeHeight += message.ObjectHeight;
        } else {
            _curNoticeHeight += (message.ObjectHeight + _noticeGapValue);

            UIMultiChatMessage lastNoticeMsg = _noticeMsgInfos[_noticeMsgInfos.Count - 1];

            // Calc StartPosY
            int lastHeightCount = 0;
            lastHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

            float lastTextHeight = (float)message.TextHeight;

            int lastQuotientHeight = lastHeightCount / 2;
            int lastRestHeight = lastHeightCount % 2;

            float lastStartY = 0f;
            if (lastRestHeight == 0) { // Even
                lastStartY = (lastTextHeight * lastQuotientHeight) - (lastTextHeight * 0.5f);
            } else { // Odd
                lastStartY = (lastTextHeight * lastQuotientHeight);
            }
            /////

            float lastObjPosY = lastNoticeMsg.transform.localPosition.y + lastStartY;
            startPosY += lastObjPosY - (lastNoticeMsg.ObjectHeight * 0.5f) - _noticeGapValue - (message.ObjectHeight * 0.5f);
        }

        message.transform.localPosition = new Vector3(0f, startPosY, 0f);

        _noticeMsgInfos.Add(message);

        return message;
    }

    public UIMultiChatMessage AddNormalChatNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor, Transform chatNoticeRootTrans)
    {
        UIMultiChatMessage message = _noticeCacheMessages[_noticeCacheIndex];
        message.transform.SetParent(chatNoticeRootTrans, false);
        if (message.MultiChatTextInfos != null)
            message.ResetChatMessage();

        if (_noticeCacheIndex == _maxCacheCount - 1) {
            _noticeCacheIndex = 0;
        } else {
            _noticeCacheIndex++;
        }

        message.gameObject.SetActive(true);
        message.SetChatMessageList(chatTextInfos, chatColor);

        // Calc StartPosY
        int maxHeightCount = 0;
        maxHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

        float textHeight = (float)message.TextHeight;

        int quotientHeight = maxHeightCount / 2;
        int restHeight = maxHeightCount % 2;

        float startY = 0f;
        if (restHeight == 0) { // Even
            startY = (textHeight * quotientHeight) - (textHeight * 0.5f);
        } else { // Odd
            startY = (textHeight * quotientHeight);
        }
        /////

        float startPosY = -startY;
        if (_noticeMsgInfos.Count == 0) {
            _curNoticeHeight += message.ObjectHeight;
        } else {
            _curNoticeHeight += (message.ObjectHeight + _noticeGapValue);

            UIMultiChatMessage lastNoticeMsg = _noticeMsgInfos[_noticeMsgInfos.Count - 1];

            // Calc StartPosY
            int lastHeightCount = 0;
            lastHeightCount = chatTextInfos[chatTextInfos.Count - 1].LineCount + 1;

            float lastTextHeight = (float)message.TextHeight;

            int lastQuotientHeight = lastHeightCount / 2;
            int lastRestHeight = lastHeightCount % 2;

            float lastStartY = 0f;
            if (lastRestHeight == 0) { // Even
                lastStartY = (lastTextHeight * lastQuotientHeight) - (lastTextHeight * 0.5f);
            } else { // Odd
                lastStartY = (lastTextHeight * lastQuotientHeight);
            }
            /////

            float lastObjPosY = lastNoticeMsg.transform.localPosition.y + lastStartY;
            startPosY += lastObjPosY - (lastNoticeMsg.ObjectHeight * 0.5f) - _noticeGapValue - (message.ObjectHeight * 0.5f);
        }

        message.transform.localPosition = new Vector3(0f, startPosY, 0f);

        _noticeMsgInfos.Add(message);

        return message;
    }

    public void ClearNoticeMessage(Transform cacheChatParentTrans)
    {
        if (_noticeCacheMessages != null) {
            for (int i = 0; i < _noticeCacheMessages.Length; i++) {
                if (_noticeCacheMessages[i].transform.parent != cacheChatParentTrans.transform) {
                    _noticeCacheMessages[i].transform.SetParent(cacheChatParentTrans.transform, false);
                }

                _noticeCacheMessages[i].ResetChatMessage();
                _noticeCacheMessages[i].gameObject.SetActive(false);
            }
        }

        _noticeCacheIndex = 0;
        _curNoticeHeight = 0f;
        _noticeMsgInfos.Clear();
    }

    #endregion
}
