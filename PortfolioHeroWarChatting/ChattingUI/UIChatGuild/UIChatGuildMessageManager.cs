using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChatGuildMessageManager : MonoBehaviour
{
    #region Serialize Variables

#pragma warning disable 649

    [SerializeField] ObjectScrollManager _noticeScrollManager = default(ObjectScrollManager);
    [SerializeField] ObjectScrollManager _guildSystemScrollManager = default(ObjectScrollManager);
    [SerializeField] ObjectScrollManager _guildNormalScrollManager = default(ObjectScrollManager);
    [SerializeField] ObjectScrollManager _guildLargeScrollManager = default(ObjectScrollManager);

    [SerializeField] Transform _guildSystemNoticeRoot = default(Transform);
    [SerializeField] Transform _guildChatNoticeRoot = default(Transform);
    [SerializeField] Transform _guildLargeNoticeRoot = default(Transform);

    [SerializeField] Button _systemScrollButton = default(Button);
    [SerializeField] Button _guildChatScrollButton = default(Button);
    [SerializeField] Button _largeScrollButton = default(Button);

    [SerializeField] Text _largeTitleText = default(Text);

#pragma warning restore 649

    #endregion

    #region Variables

    ChatDefinition.GuildUIType _guildUIType = ChatDefinition.GuildUIType.Normal;

    ChatGuildCacheInfo _noticeGuildCache = new ChatGuildCacheInfo();
    ChatGuildCacheInfo _guildSystemCache = new ChatGuildCacheInfo();
    ChatGuildCacheInfo _guildNormalChatCache = new ChatGuildCacheInfo();

    #endregion

    #region Properties

    public ChatDefinition.GuildUIType GuildUIType
    {
        get { return _guildUIType; }
        set { _guildUIType = value; }
    }

    public ObjectScrollManager NoticeScrollManager
    {
        get { return _noticeScrollManager; }
    }

    public ObjectScrollManager GuildSystemScrollManager
    {
        get { return _guildSystemScrollManager; }
    }

    public ObjectScrollManager GuildNormalScrollManager
    {
        get { return _guildNormalScrollManager; }
    }

    public ObjectScrollManager GuildLargeScrollManager
    {
        get { return _guildLargeScrollManager; }
    }

    public Transform GuildSystemNoticeRoot
    {
        get { return _guildSystemNoticeRoot; }
    }

    public Transform GuildLargeNoticeRoot
    {
        get { return _guildLargeNoticeRoot; }
    }

    public Button SystemScrollButton
    {
        get { return _systemScrollButton; }
    }

    public Button GuildChatScrollButton
    {
        get { return _guildChatScrollButton; }
    }

    public Button LargeScrollButton
    {
        get { return _largeScrollButton; }
    }

    public Text LargeTitleText
    {
        get { return _largeTitleText; }
    }

    public ChatGuildCacheInfo NoticeGuildCache
    {
        get { return _noticeGuildCache; }
    }

    public ChatGuildCacheInfo GuildSystemCache
    {
        get { return _guildSystemCache; }
    }

    public ChatGuildCacheInfo GuildNormalChatCache
    {
        get { return _guildNormalChatCache; }
    }

    #endregion

    #region Methods

    public void InitChatGuildUI()
    {
        SetGuildUIType((ChatDefinition.GuildUIType)ChattingController.Instance.ChattingModel.ChatSaveInfo.guildUIType);

        _noticeScrollManager.InitScrollData();
        _guildSystemScrollManager.InitScrollData();
        _guildNormalScrollManager.InitScrollData();
        _guildLargeScrollManager.InitScrollData();
    }

    public void SetTouchEnable(bool isEnable)
    {
        _noticeScrollManager.IsTouch = isEnable;
        _guildSystemScrollManager.IsTouch = isEnable;
        _guildNormalScrollManager.IsTouch = isEnable;
        _guildLargeScrollManager.IsTouch = isEnable;
    }

    public void SetGuildUIType(ChatDefinition.GuildUIType guildType)
    {
        TextModel textModel = null;
        if (ChattingController.Instance != null) {
            textModel = ChattingController.Instance.Context.Text;
        }

        _guildUIType = guildType;
        switch (_guildUIType) {
            case ChatDefinition.GuildUIType.Normal:
                _guildSystemScrollManager.gameObject.SetActive(true);
                _guildNormalScrollManager.gameObject.SetActive(true);
                _guildLargeScrollManager.gameObject.SetActive(false);
                break;
            case ChatDefinition.GuildUIType.ChatOnly:
                _guildSystemScrollManager.gameObject.SetActive(false);
                _guildNormalScrollManager.gameObject.SetActive(false);
                _guildLargeScrollManager.gameObject.SetActive(true);

                if(textModel != null) {
                    _largeTitleText.text = textModel.GetText(TextKey.CT_Chatting);
                }
                break;
            case ChatDefinition.GuildUIType.SystemOnly:
                _guildSystemScrollManager.gameObject.SetActive(false);
                _guildNormalScrollManager.gameObject.SetActive(false);
                _guildLargeScrollManager.gameObject.SetActive(true);
                if (textModel != null) {
                    _largeTitleText.text = textModel.GetText(TextKey.CT_Text01);
                }
                break;
        }

       
    }

    public void MakeMultiCacheList(UIMultiChatMessage uiMultiChatMessage, Transform cacheChatParentTrans)
    {
        _noticeGuildCache.MakeMultiCacheList(uiMultiChatMessage, cacheChatParentTrans, 10);
        _guildSystemCache.MakeMultiCacheList(uiMultiChatMessage, cacheChatParentTrans, ChattingController.Instance.MaxChatLineCount);
        _guildNormalChatCache.MakeMultiCacheList(uiMultiChatMessage, cacheChatParentTrans, ChattingController.Instance.MaxChatLineCount);
    }

    public void ClearNoticeChatMessage(Transform cacheChatParentTrans, bool isResize)
    {
        if (_noticeGuildCache.MultiChatCaches != null) {
            for (int i = 0; i < _noticeGuildCache.MultiChatCaches.Length; i++) {
                if (_noticeGuildCache.MultiChatCaches[i].transform.parent == _noticeScrollManager.ObjListTrans) {
                    _noticeGuildCache.MultiChatCaches[i].transform.SetParent(cacheChatParentTrans.transform, false);
                }

                _noticeGuildCache.MultiChatCaches[i].ResetChatMessage();
                _noticeGuildCache.MultiChatCaches[i].gameObject.SetActive(false);
            }
        }

        _noticeGuildCache.CacheIndex = 0;

        _noticeScrollManager.ReleaseScrollData(isResize);
    }

    public void ClearSystemChatMessage(Transform cacheChatParentTrans, bool isResize)
    {
        ObjectScrollManager curObjectScroll = null;
        if(_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            curObjectScroll = _guildLargeScrollManager;
        } else {
            curObjectScroll = _guildSystemScrollManager;
        }

        if (_guildSystemCache.MultiChatCaches != null) {
            for (int i = 0; i < _guildSystemCache.MultiChatCaches.Length; i++) {
                if (_guildSystemCache.MultiChatCaches[i].transform.parent == curObjectScroll.ObjListTrans) {
                    _guildSystemCache.MultiChatCaches[i].transform.SetParent(cacheChatParentTrans.transform, false);
                }

                _guildSystemCache.MultiChatCaches[i].ResetChatMessage();
                _guildSystemCache.MultiChatCaches[i].gameObject.SetActive(false);
            }
        }

        _guildSystemCache.CacheIndex = 0;

        curObjectScroll.ReleaseScrollData(isResize);
    }

    public void ClearNormalChatMessage(Transform cacheChatParentTrans, bool isResize)
    {
        ObjectScrollManager curObjectScroll = null;
        if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            curObjectScroll = _guildLargeScrollManager;
        } else {
            curObjectScroll = _guildNormalScrollManager;
        }

        if (_guildNormalChatCache.MultiChatCaches != null) {
            for (int i = 0; i < _guildNormalChatCache.MultiChatCaches.Length; i++) {
                if (_guildNormalChatCache.MultiChatCaches[i].transform.parent == curObjectScroll.ObjListTrans) {
                    _guildNormalChatCache.MultiChatCaches[i].transform.SetParent(cacheChatParentTrans.transform, false);
                }

                _guildNormalChatCache.MultiChatCaches[i].ResetChatMessage();
                _guildNormalChatCache.MultiChatCaches[i].gameObject.SetActive(false);
            }
        }

        _guildNormalChatCache.CacheIndex = 0;

        curObjectScroll.ReleaseScrollData(isResize);
    }

    public void ClearGuildTopNoticeChatMessage(Transform cacheChatParentTrans)
    {
        _guildSystemCache.ClearNoticeMessage(cacheChatParentTrans);
        _guildNormalChatCache.ClearNoticeMessage(cacheChatParentTrans);
    }

    public UIMultiChatMessage AddNoticeChatMessage(Transform cacheChatParentTrans)
    {
        UIMultiChatMessage message = _noticeGuildCache.MultiChatCaches[_noticeGuildCache.CacheIndex];
        if (message.transform.parent == _noticeScrollManager.ObjListTrans) {
            message.transform.SetParent(cacheChatParentTrans.transform, false);
        }

        if (_noticeGuildCache.CacheIndex == _noticeGuildCache.MaxCacheCount - 1) {
            _noticeGuildCache.CacheIndex = 0;
        } else {
            _noticeGuildCache.CacheIndex++;
        }

        message.gameObject.SetActive(true);

        return message;
    }

    public UIMultiChatMessage AddSystemChatMessage(Transform cacheChatParentTrans)
    {
        UIMultiChatMessage message = _guildSystemCache.MultiChatCaches[_guildSystemCache.CacheIndex];
        if (message.transform.parent == _noticeScrollManager.ObjListTrans) {
            message.transform.SetParent(cacheChatParentTrans.transform, false);
        }

        if (_guildSystemCache.CacheIndex == _guildSystemCache.MaxCacheCount - 1) {
            _guildSystemCache.CacheIndex = 0;
        } else {
            _guildSystemCache.CacheIndex++;
        }

        message.gameObject.SetActive(true);

        return message;
    }

    public UIMultiChatMessage AddNormalChatMessage(Transform cacheChatParentTrans)
    {
        UIMultiChatMessage message = _guildNormalChatCache.MultiChatCaches[_guildNormalChatCache.CacheIndex];
        if (message.transform.parent == _noticeScrollManager.ObjListTrans) {
            message.transform.SetParent(cacheChatParentTrans.transform, false);
        }

        if (_guildNormalChatCache.CacheIndex == _guildNormalChatCache.MaxCacheCount - 1) {
            _guildNormalChatCache.CacheIndex = 0;
        } else {
            _guildNormalChatCache.CacheIndex++;
        }

        message.gameObject.SetActive(true);

        return message;
    }

    public UIMultiChatMessage AddSystemNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor)
    {
        UIMultiChatMessage retValue = null;

        if(_guildUIType == ChatDefinition.GuildUIType.Normal) {
            _guildSystemCache.AddSystemNoticeChatMessage(chatTextInfos, chatColor, _guildSystemNoticeRoot);
        } else if(_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            _guildSystemCache.AddSystemNoticeChatMessage(chatTextInfos, chatColor, _guildLargeNoticeRoot);
        }

        return retValue;
    }

    public UIMultiChatMessage AddNormalChatNoticeChatMessage(List<MultiChatTextInfo> chatTextInfos, Color chatColor)
    {
        UIMultiChatMessage retValue = null;

        if (_guildUIType == ChatDefinition.GuildUIType.Normal) {
            _guildNormalChatCache.AddNormalChatNoticeChatMessage(chatTextInfos, chatColor, _guildChatNoticeRoot);
        } else if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            _guildNormalChatCache.AddNormalChatNoticeChatMessage(chatTextInfos, chatColor, _guildLargeNoticeRoot);
        }

        return retValue;
    }

    public void AddGuildSystemScrollObject(IScrollObjectInfo scrollObjInfo)
    {
        if(_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            _guildLargeScrollManager.AddScrollObject(scrollObjInfo);
        } else {
            _guildSystemScrollManager.AddScrollObject(scrollObjInfo);
        }
    }

    public int GetGuildSystemScrollCount()
    {
        if (_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            return _guildLargeScrollManager.GetScrollObjCount();
        } else {
            return _guildSystemScrollManager.GetScrollObjCount();
        }
    }

    public void RemoveGuildSystemScroll(int posIndex, Transform cacheChatParentTrans)
    {
        ObjectScrollManager curScrollManager = null;

        if (_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            curScrollManager = _guildLargeScrollManager;
        } else {
            curScrollManager = _guildSystemScrollManager;
        }

        UIMultiChatMessage multiChatMessage = curScrollManager.GetScrollObj(posIndex).ScrollObjInfo as UIMultiChatMessage;

        if (multiChatMessage.transform.parent == curScrollManager.ObjListTrans) {
            multiChatMessage.transform.SetParent(cacheChatParentTrans.transform, false);
        }

        multiChatMessage.ResetChatMessage();
        multiChatMessage.gameObject.SetActive(false);

        curScrollManager.RemoveScrollObj(posIndex);
    }

    public void AddGuildNormalScrollObject(IScrollObjectInfo scrollObjInfo)
    {
        if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            _guildLargeScrollManager.AddScrollObject(scrollObjInfo);
        } else {
            _guildNormalScrollManager.AddScrollObject(scrollObjInfo);
        }
    }

    public int GetGuildNormalScrollCount()
    {
        if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            return _guildLargeScrollManager.GetScrollObjCount();
        } else {
            return _guildNormalScrollManager.GetScrollObjCount();
        }
    }

    public void RemoveGuildNormalScroll(int posIndex, Transform cacheChatParentTrans)
    {
        ObjectScrollManager curScrollManager = null;

        if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            curScrollManager = _guildLargeScrollManager;
        } else {
            curScrollManager = _guildNormalScrollManager;
        }

        UIMultiChatMessage multiChatMessage = curScrollManager.GetScrollObj(posIndex).ScrollObjInfo as UIMultiChatMessage;

        if (multiChatMessage.transform.parent == curScrollManager.ObjListTrans) {
            multiChatMessage.transform.SetParent(cacheChatParentTrans.transform, false);
        }

        multiChatMessage.ResetChatMessage();
        multiChatMessage.gameObject.SetActive(false);

        curScrollManager.RemoveScrollObj(posIndex);
    }

    public void SetSystemRectGapHeightValue(float gapValue)
    {
        if (_guildUIType == ChatDefinition.GuildUIType.SystemOnly) {
            _guildLargeScrollManager.SetRectGapHeightValue(gapValue);
        } else {
            _guildSystemScrollManager.SetRectGapHeightValue(gapValue);
        }
    }

    public void SetNormalChatRectGapHeightValue(float gapValue)
    {
        if (_guildUIType == ChatDefinition.GuildUIType.ChatOnly) {
            _guildLargeScrollManager.SetRectGapHeightValue(gapValue);
        } else {
            _guildNormalScrollManager.SetRectGapHeightValue(gapValue);
        }
    }

    #endregion
}
