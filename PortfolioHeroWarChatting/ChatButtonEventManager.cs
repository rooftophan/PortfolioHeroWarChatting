using Framework.Controller;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Controller;

public class ChatButtonEventManager
{
    #region Variables

    DataContext _context;

    UserDetailPopupHandler _userDetailPopup = null;
    EquipmentPopupHandler _equipmentInfoPopup = null;
    CardPopupHandler _uiCardInfoPopup = null;

    List<IChatButtonObserver> _chatButtonObservers = new List<IChatButtonObserver>();

    #endregion

    #region Properties

    public DataContext Context
    {
        get { return _context; }
        set { _context = value; }
    }

    #endregion

    #region Methods

    public void AttachChatButtonOb(IChatButtonObserver buttonOb)
    {
        if(_chatButtonObservers.Contains(buttonOb))
            return;

        _chatButtonObservers.Add(buttonOb);
    }

    public void DetachChatButtonOb(IChatButtonObserver buttonOb)
    {
        if(!_chatButtonObservers.Contains(buttonOb))
            return;

        _chatButtonObservers.Remove(buttonOb);
    }

    public void NotifyChatButtonEvent(ChatDefinition.PartMessageType partMessageType, string[] partValues)
    {
        for(int i = 0;i< _chatButtonObservers.Count;i++) {
            _chatButtonObservers[i].OnChatButtonEvent(partMessageType, partValues);
        }
    }

    void ViewEquipmentItem(string equipmentJsonString)
    {
        if (string.IsNullOrEmpty(equipmentJsonString) || equipmentJsonString.Length < 10)
            return;

        EquipmentData equipment = ChatParsingUtil.GetJsonStringToEquipmentData(equipmentJsonString);

        if (_equipmentInfoPopup != null) {
            _equipmentInfoPopup.Dispose();
            _equipmentInfoPopup = null;
        }

        if (ChattingController.Instance != null)
            ChattingController.Instance.SetEnableScrollTouch(false);

        _equipmentInfoPopup = new EquipmentPopupHandler();
        _equipmentInfoPopup.Init(_context, EquipmentPopupHandler.PopupType.Normal);
        _equipmentInfoPopup.Show(equipment, () => {
            if (ChattingController.Instance != null)
                ChattingController.Instance.SetEnableScrollTouch(true);
            _equipmentInfoPopup.Dispose();
        });
        _equipmentInfoPopup.SetCamDepth(90);
    }

    void ViewCardItem(string itemIndex)
    {
        if (_uiCardInfoPopup != null) {
            _uiCardInfoPopup.Dispose();
            _uiCardInfoPopup = null;
        }

        if (ChattingController.Instance != null)
            ChattingController.Instance.SetEnableScrollTouch(false);

        _uiCardInfoPopup = new CardPopupHandler();
        _uiCardInfoPopup.Init(_context, CardPopupHandler.PopupType.Normal);
        _uiCardInfoPopup.Show(int.Parse(itemIndex), () => {
            if (ChattingController.Instance != null)
                ChattingController.Instance.SetEnableScrollTouch(true);

            _uiCardInfoPopup.Dispose();
        });
        _uiCardInfoPopup.SetCamDepth(90);
    }

    public void ReleasePopup()
    {
        if (_equipmentInfoPopup != null) {
            _equipmentInfoPopup.Dispose();
            _equipmentInfoPopup = null;
        }

        if (_uiCardInfoPopup != null) {
            _uiCardInfoPopup.Dispose();
            _uiCardInfoPopup = null;
        }

        if (_userDetailPopup != null) {
            if (_userDetailPopup._uiView != null)
                _userDetailPopup.Dispose();
            _userDetailPopup = null;
        }
    }

    void ShortCutHeroChallegeMaxScore(int heroIndex)
    {
        ChatShortcutData shortcutData = new ChatShortcutData();
        shortcutData.shortcutType = ChatDefinition.ChatShortcutType.HeroChallenge;
        shortcutData.shortcutValue = heroIndex;

        ChattingController.Instance.ShortcutData = shortcutData;

        var currentController = GameSystem.Instance.Current as BaseController<GameSystem>;
        if (currentController != null) {
            currentController.TransitShortcut(new Type[] { typeof(MainController), typeof(VirtualTrainingController), typeof(HeroChallengeController) }, null);
        }

        if (ChattingController.Instance.IsChattingPopup) {
            ChattingController.Instance.UIMultiChat.CloseChattingPopup();
        }
    }

    #endregion

    #region CallBack Methods

    public void OnClickChatPartMessage(ChatPartMessageInfo partMessageInfo)
    {
        if (ChattingController.Instance == null || partMessageInfo == null)
            return;

        UserModel userModel = ChattingController.Instance.Context.User;

        switch ((ChatDefinition.PartMessageType)partMessageInfo.partMessageType) {
            case ChatDefinition.PartMessageType.UserInfoType: {
                    long userID = long.Parse(partMessageInfo.partValues[0]);

                    long connectID = -1;
                    if(partMessageInfo.partValues.Length > 1) {
                        connectID = long.Parse(partMessageInfo.partValues[1]);
                    }

                    if (ChattingController.Instance.CurChatMessageKind == ChatDefinition.ChatMessageKind.GuildChat &&
                        (ChattingController.Instance.UIMultiChat != null && ChattingController.Instance.UIMultiChat.CurSelectChattingInput != null)) {
                        ChattingController.Instance.CheckGuildUserID(ChattingController.Instance.UIMultiChat.CurSelectChattingInput.PartyNum, userID, connectID);
                    }

                    _userDetailPopup = new UserDetailPopupHandler(_context, UserDetailPopupShowType.OtherUser, 0, userID);
                    _userDetailPopup.Show();
                }
                break;
            case ChatDefinition.PartMessageType.EnemyUserInfoType: {
                    long enemyUserId = long.Parse(partMessageInfo.partValues[0]);

                    _userDetailPopup = new UserDetailPopupHandler(_context, UserDetailPopupShowType.OtherUser, 0, enemyUserId);
                    _userDetailPopup.Show();
                }
                break;
            case ChatDefinition.PartMessageType.ItemInfoType: {
                    if (partMessageInfo.partValues[0] == "Equipment") {
                        ViewEquipmentItem(partMessageInfo.partValues[1]);
                    } else if (partMessageInfo.partValues[0] == "Card") {
                        ViewCardItem(partMessageInfo.partValues[1]);
                    }
                }
                break;
            case ChatDefinition.PartMessageType.MissionInfoType:
                break;
            case ChatDefinition.PartMessageType.CompanyInfoType:
                break;
            case ChatDefinition.PartMessageType.ShortcutType: {
                    if (!ChattingController.Instance.IsBattleState) {
                        int shortcutIndex = int.Parse(partMessageInfo.partValues[0]);
                        long partyNum = long.Parse(partMessageInfo.partValues[1]);
                        long missionID = 0;
                        int missionContentType = 0;
                        if (partMessageInfo.partValues.Length > 2) {
                            missionID = long.Parse(partMessageInfo.partValues[2]);
                            missionContentType = int.Parse(partMessageInfo.partValues[3]);
                        }

                        switch ((ChatDefinition.ChatShortcutType)shortcutIndex) {
                            case ChatDefinition.ChatShortcutType.CompanyCurrentMission:
                            case ChatDefinition.ChatShortcutType.CompanyPastMission:
                            case ChatDefinition.ChatShortcutType.CompanyDuel:
                            case ChatDefinition.ChatShortcutType.CompanyBulletinBoard:
                            case ChatDefinition.ChatShortcutType.TrophyShop: {
                                    ChatShortcutData shortcutData = new ChatShortcutData();
                                    shortcutData.shortcutType = (ChatDefinition.ChatShortcutType)shortcutIndex;
                                    shortcutData.partyNum = partyNum;
                                    shortcutData.missionID = missionID;
                                    shortcutData.missionContentType = (MissionContentType)missionContentType;

                                    ChattingController.Instance.ShortcutData = shortcutData;

                                    Popup.SelectYesNo.Show(_context.Text.GetText(TextKey.CT_CompanyEnter), OnConfirmShortcut, OnCancelShortcut);
                                }
                                break;
                        }
                    } else {
                        Popup.Normal.Show(_context.Text.GetText(TextKey.CT_CompanyDonotMove));
                    }
                }
                break;
            case ChatDefinition.PartMessageType.PartyExploreHelpSpot: {
                    NotifyChatButtonEvent((ChatDefinition.PartMessageType)partMessageInfo.partMessageType, partMessageInfo.partValues);
                }
                break;
            case ChatDefinition.PartMessageType.HeroChallengeMaxScore: {
                    int heroIndex = int.Parse(partMessageInfo.partValues[0]);
                    ShortCutHeroChallegeMaxScore(heroIndex);
                }
                break;
        }
    }

    void OnConfirmShortcut()
    {
        var currentController = GameSystem.Instance.Current as BaseController<GameSystem>;
        if (currentController != null) {
            var shortcutData = ChattingController.Instance.ShortcutData;

            if (shortcutData.shortcutType == ChatDefinition.ChatShortcutType.CompanyCurrentMission)
                GuildMissionHandler.ShortCut(currentController, shortcutData.partyNum, GuildMissionHandler.tabType.Proceed, shortcutData.missionID, shortcutData.missionContentType);
            else if (shortcutData.shortcutType == ChatDefinition.ChatShortcutType.CompanyPastMission)
                GuildMissionHandler.ShortCut(currentController, shortcutData.partyNum, GuildMissionHandler.tabType.Result, shortcutData.missionID, shortcutData.missionContentType);
        }

        if (ChattingController.Instance.IsChattingPopup) {
            ChattingController.Instance.UIMultiChat.CloseChattingPopup();
        }
    }

    void OnCancelShortcut()
    {
        ChattingController.Instance.ShortcutData = null;
    }

    void OnCompletePartyInviteDeny(PartyInvitationResponseResponse res)
    {
        ChattingController.Instance.UIMultiChat.RefreshChatMessage();
    }

#endregion
}
