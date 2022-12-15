using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChatHelper
{
    const int _limitLineCount = 3;
    private const int _logLineCount = 30;
    public const float partyScrollMaxWidth = 870f;//1136f;//1130f;
    public const float quickChatValidWidth = 720f;
    public const float quickPartyMissionChatWidth = 510f;

    public static ChatDefinition.PartyQuickChatKind partyQuickChatKind;

    private static int LimitLineCount
    {
        get
        {
            if (NewBattleCore.FormulaManager.Instance != null && NewBattleCore.FormulaManager.Instance.FormulaLog)
                return _logLineCount;

            return _limitLineCount;
        }
    }
    
    public static long GetChatChannelID(DataContext dataContext)
    {
        return ChattingController.Instance.ChatSocketManager.ChattingServerInfo.connectId;
    }

    public static ChatMessage GetPartyChatMessage(DataContext data, ChatEventManager chatEventManager, ChatPartyMessageList partyMessage, string msg)
    {
        ChatMessage chatMessage = null;
        JsonData msgJson = ChatParsingUtil.GetChatMessageJsonData(msg);
        if (((IDictionary)msgJson).Contains("data")) {
            chatMessage = chatEventManager.GetChatServerMessageJson(msgJson);

            if (chatMessage == null)
                return null;
        } else {
            chatMessage = ChatParsingUtil.GetChatMessageParsingByJson(msgJson);

            if (chatMessage == null)
                return null;
        }

        ChatEventManager.SetChatItemPrmInfo(chatMessage);
        ChatEventManager.SetChatTraceNamePrmInfo(chatMessage);
        ChatEventManager.SetChatBattleCenterNamePrmInfo(chatMessage);

        ChatEventManager.SetChatMissionCommonPrmInfo(data, chatMessage);
        ChatEventManager.SetChatCompanyShopPrmInfo(data, chatMessage);

        chatMessage.timeStamp = partyMessage.timestamp;

        if (chatMessage.prm.ContainsKey("user") && chatMessage.userID != 0) {
            if (chatMessage.partMessageInfos == null) {
                chatMessage.partMessageInfos = new Dictionary<string, ChatPartMessageInfo>();
            }

            if (!chatMessage.partMessageInfos.ContainsKey("user")) {
                ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
                inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.UserInfoType;
                inputPartMessageInfo.partValues = new string[2];
                inputPartMessageInfo.partValues[0] = chatMessage.userID.ToString();
                inputPartMessageInfo.partValues[1] = chatMessage.connectId.ToString();

                chatMessage.partMessageInfos.Add("user", inputPartMessageInfo);
            }
        }

        return chatMessage;
    }

    public static bool IsKorean(char ch)
    {
        //( 한글자 || 자음 , 모음 )
        if ((0xAC00 <= ch && ch <= 0xD7A3) || (0x3131 <= ch && ch <= 0x318E))
            return true;
        else
            return false;
    }

    public static bool IsEnglish(char ch)
    {
        if ((0x61 <= ch && ch <= 0x7A) || (0x41 <= ch && ch <= 0x5A))
            return true;
        else
            return false;
    }

    public static bool IsNumeric(char ch)
    {
        if (0x30 <= ch && ch <= 0x39)
            return true;
        else
            return false;
    }

    public static bool IsAllowedCharacter(char ch, string allowedCharacters)
    {
        return allowedCharacters.Contains<char>(ch);
    }

    public static string GetCardGrade(CardGrade grade)
    {
        string retValue = "";

        switch (grade) {
            case CardGrade.R:
                retValue = "R";
                break;
            case CardGrade.SR:
                retValue = "SR";
                break;
            case CardGrade.SSR:
                retValue = "SSR";
                break;
        }

        return retValue;
    }

    #region Message Color

    public static ChatMessageColorType GetChattingMessageColorType(DataContext context, int chattingMessageIndex)
    {
        var sheetChatNoticeMessage = context.Sheet.SheetChatNoticeMessage;
        if (sheetChatNoticeMessage.Sheet.Info.Key.ContainsKey(chattingMessageIndex))
            return (ChatMessageColorType)Enum.Parse(typeof(ChatMessageColorType), sheetChatNoticeMessage.MessageColorType[chattingMessageIndex]);

        return ChatMessageColorType.Normal;
    }

    public static Color GetChatMessageColor(DataContext context, ChatBaseMessage chatMessage)
    {
        return GetChatMessageColor(GetChattingMessageColorType(context, chatMessage.msgIdx));
    }

    public static Color GetChatMessageColor(DataContext context, int messageIndex)
    {
        return GetChatMessageColor(GetChattingMessageColorType(context, messageIndex));
    }

    public static Color GetChatMessageColor(ChatMessageColorType colorType)
    {
        switch (colorType) {
            case ChatMessageColorType.Normal:
                return ColorPreset.CHAT_NORMAL;
            case ChatMessageColorType.NormalSystemMessage:
                return ColorPreset.CHAT_NormalSystemMessage;
            case ChatMessageColorType.CompanyNormal:
                return ColorPreset.CHAT_CompanyNormal;
            case ChatMessageColorType.CompanyDefaultSystemMessage:
                return ColorPreset.CHAT_CompanyDefaultSystemMessage;
            case ChatMessageColorType.CompanyMissonSystemMessage:
                return ColorPreset.CHAT_CompanyMissionSystemMessage;
            case ChatMessageColorType.CompanyMissonBattleFail:
                return ColorPreset.CHAT_CompanyMissionBattleFail;
            case ChatMessageColorType.CompanyMissonBattleSuccess:
                return ColorPreset.CHAT_CompanyMissionBattleSuccess;
            case ChatMessageColorType.CompanyMissionSuccess:
                return ColorPreset.CHAT_CompanyMissionSuccess;
            case ChatMessageColorType.CompanyMissionFail:
                return ColorPreset.CHAT_CompanyMissionFail;
            case ChatMessageColorType.Whisper:
                return ColorPreset.CHAT_Whisper;
            case ChatMessageColorType.NoticeMessage:
                return ColorPreset.CHAT_NoticeMessage;
            default:
                return ColorPreset.CHAT_NORMAL;
        }
    }

    public static bool IsUserColorChatMessage(ChatBaseMessage chatMessage)
    {
        if (chatMessage.msgIdx == (int)ChatNoticeMessageKey.NormalChatting ||
            chatMessage.msgIdx == (int)ChatNoticeMessageKey.UserCompanyChatting ||
            chatMessage.msgIdx == (int)ChatNoticeMessageKey.MissionChatting) {
            return true;
        } else {
            return false;
        }
    }

    #endregion

    #region Make MultiChatTextInfo List

    public static List<MultiChatTextInfo> GetMultiChatTextInfoList(string chattingMessageText, ChatMessage chatMessage = null, List<MultiChatTextInfo> addMultiTextInfo = null)
    {
        if (ChattingController.Instance == null)
            return null;

        if (string.IsNullOrEmpty(chattingMessageText))
            return null;

        List<ChatMessageParsingInfo> chatParsingInfos = new List<ChatMessageParsingInfo>();

        // Check Struct
        char[] chatMessageChars = chattingMessageText.ToCharArray();

        SetChatMessageParsing(chatMessage, chatMessageChars, chatParsingInfos);

        SetChatParsingColor(chatMessage, chatParsingInfos);

        List<MultiChatTextInfo> retChatTextInfos = new List<MultiChatTextInfo>();

        float curMessageWidth = 0f;
        int lineCount = 0;

        float messageMaxWidth = 0f;
        UIChatPartMessage supportTextMessage = null;
        if (chatMessage != null && chatMessage.partyType == (int)ChatPartyType.ChatParty) {
            messageMaxWidth = partyScrollMaxWidth;

            supportTextMessage = ChattingController.Instance.SupportTextMessage;
        } else {
            supportTextMessage = ChattingController.Instance.SupportTextMessage;
            messageMaxWidth = ChattingController.Instance.UIMultiChat.UIMultiChatPopup.ChatScrollMaxWidth;
        }

        SetChatParsingInfo(chatParsingInfos, retChatTextInfos, supportTextMessage, messageMaxWidth, ref curMessageWidth, ref lineCount);

        SetAddMultiTextInfo(addMultiTextInfo, lineCount, curMessageWidth, messageMaxWidth, retChatTextInfos);

        return retChatTextInfos;
    }

    public static List<MultiChatTextInfo> GetQuickMultiChatTextInfoList(ChatMessage chatMessage, UIChatPartMessage supportTextMessage, float messageMaxWidth)
    {
        if (ChattingController.Instance == null)
            return null;

        string chattingMessageText = ChattingController.Instance.Context.Text.GetChatNoticeSheetText(chatMessage.msgIdx);

        return GetQuickMultiChatTextInfoList(chattingMessageText, chatMessage, supportTextMessage, messageMaxWidth);
    }

    public static List<MultiChatTextInfo> GetQuickMultiChatTextInfoList(string chattingMessageText, ChatMessage chatMessage = null, List<MultiChatTextInfo> addMultiTextInfo = null)
    {
        if (ChattingController.Instance == null)
            return null;

        if (string.IsNullOrEmpty(chattingMessageText))
            return null;

        List<ChatMessageParsingInfo> chatParsingInfos = new List<ChatMessageParsingInfo>();

        // Check Struct
        char[] chatMessageChars = chattingMessageText.ToCharArray();

        SetChatMessageParsing(chatMessage, chatMessageChars, chatParsingInfos);

        SetChatParsingColor(chatMessage, chatParsingInfos);

        List<MultiChatTextInfo> retChatTextInfos = new List<MultiChatTextInfo>();

        float curMessageWidth = 0f;
        int lineCount = 0;

        float messageMaxWidth = 0f;
        UIChatPartMessage supportTextMessage = null;
        if (chatMessage != null && chatMessage.partyType == (int)ChatPartyType.ChatParty) {
            if(partyQuickChatKind == ChatDefinition.PartyQuickChatKind.PartyReady) {
                messageMaxWidth = quickChatValidWidth;
            } else if(partyQuickChatKind == ChatDefinition.PartyQuickChatKind.PartyMission) {
                messageMaxWidth = quickPartyMissionChatWidth;
            } else {
                messageMaxWidth = quickChatValidWidth;
            }
            
            supportTextMessage = ChattingController.Instance.PartyQuickViewText;

            AddChatPartyRoleIconImg(chatMessage, retChatTextInfos, out curMessageWidth);
        } else {
            messageMaxWidth = ChattingController.Instance.UIMultiChat.UIMultiChatPopup.ChatScrollMaxWidth;
            supportTextMessage = ChattingController.Instance.SupportTextMessage;
        }

        SetChatParsingInfo(chatParsingInfos, retChatTextInfos, supportTextMessage, messageMaxWidth, ref curMessageWidth, ref lineCount);

        SetAddMultiTextInfo(addMultiTextInfo, lineCount, curMessageWidth, messageMaxWidth, retChatTextInfos);

        return retChatTextInfos;
    }

    public static List<MultiChatTextInfo> GetQuickMultiChatTextInfoList(string chattingMessageText,  ChatMessage chatMessage, UIChatPartMessage supportTextMessage, float messageMaxWidth,  List<MultiChatTextInfo> addMultiTextInfo = null)
    {
        if (ChattingController.Instance == null)
            return null;

        if (string.IsNullOrEmpty(chattingMessageText))
            return null;

        List<ChatMessageParsingInfo> chatParsingInfos = new List<ChatMessageParsingInfo>();

        // Check Struct
        char[] chatMessageChars = chattingMessageText.ToCharArray();

        SetChatMessageParsing(chatMessage, chatMessageChars, chatParsingInfos);

        SetChatParsingColor(chatMessage, chatParsingInfos);

        List<MultiChatTextInfo> retChatTextInfos = new List<MultiChatTextInfo>();

        float curMessageWidth = 0f;
        int lineCount = 0;

        SetChatParsingInfo(chatParsingInfos, retChatTextInfos, supportTextMessage, messageMaxWidth, ref curMessageWidth, ref lineCount);

        SetAddMultiTextInfo(addMultiTextInfo, lineCount, curMessageWidth, messageMaxWidth, retChatTextInfos);

        return retChatTextInfos;
    }

    static void SetChatParsingColor(ChatMessage chatMessage, List<ChatMessageParsingInfo> chatParsingInfos)
    {
        if(chatMessage == null)
            return;

        if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.GMSystemChatMessage) {
            ChatGMSMessage gmsMessage = chatMessage as ChatGMSMessage;
            for (int i = 0; i < chatParsingInfos.Count; i++) {
                if (chatParsingInfos[i].fieldKeyText == "msg") {
                    chatParsingInfos[i].isChatColor = true;
                    chatParsingInfos[i].chatColor = gmsMessage.chatColor;
                    break;
                }
            }
        } else if(chatMessage.msgIdx == (int)ChatNoticeMessageKey.SkillCardAcquisition ||
            chatMessage.msgIdx == (int)ChatNoticeMessageKey.FuryCardAcquisition) {
            for (int i = 0; i < chatParsingInfos.Count; i++) {
                if (chatParsingInfos[i].fieldKeyText == "tier") {
                    chatParsingInfos[i].isChatColor = true;
                    chatParsingInfos[i].chatColor = ColorPreset.CHAT_CardTier;
                    break;
                }
            }
        }
    }

    static void SetChatParsingInfo(List<ChatMessageParsingInfo> chatParsingInfos, List<MultiChatTextInfo> retChatTextInfos, UIChatPartMessage supportTextMessage, float messageMaxWidth, ref float curMessageWidth, ref int lineCount)
    {
        MultiChatTextInfo inputChatTextInfo = null;
        for (int i = 0; i < chatParsingInfos.Count; i++) {
            if (chatParsingInfos[i].parsingType == (int)ChatDefinition.ChatParsingType.NormalText || chatParsingInfos[i].partMessageType == ChatDefinition.PartMessageType.NormalMessageType) { // Normal
                float calcWidth = 0f;
                List<MultiChatTextInfo> partChatTextInfo = GetPartChatTextInfos(ChatDefinition.ChatViewTextType.NormalText, supportTextMessage, messageMaxWidth, "", curMessageWidth, chatParsingInfos[i].messageText, lineCount, out calcWidth);

                int startIndex = 0;

                if (chatParsingInfos[i].isChatColor) {
                    for (int j = 0; j < partChatTextInfo.Count; j++) {
                        partChatTextInfo[j].IsChatColor = true;
                        partChatTextInfo[j].ChatColor = chatParsingInfos[i].chatColor;
                    }
                } else {
                    if (retChatTextInfos.Count > 0) {
                        MultiChatTextInfo chatTextInfo = retChatTextInfos[retChatTextInfos.Count - 1];
                        if (!chatTextInfo.IsChatColor && chatTextInfo.ChatViewType == ChatDefinition.ChatViewTextType.NormalText) {
                            for (int j = 0; j < partChatTextInfo.Count; j++) {
                                if (chatTextInfo.LineCount == partChatTextInfo[j].LineCount) {
                                    chatTextInfo.ChatPartMessage += partChatTextInfo[j].ChatPartMessage;
                                    chatTextInfo.PartTextWidth += partChatTextInfo[j].PartTextWidth;
                                    startIndex = j + 1;
                                } else {
                                    break;
                                }
                            }
                        }
                    }
                }

                if(startIndex < partChatTextInfo.Count) {
                    for (int j = startIndex; j < partChatTextInfo.Count; j++) {
                        retChatTextInfos.Add(partChatTextInfo[j]);
                    }
                }

                lineCount = partChatTextInfo[partChatTextInfo.Count - 1].LineCount;

                curMessageWidth = calcWidth;
            } else if (chatParsingInfos[i].parsingType == (int)ChatDefinition.ChatParsingType.GroupInfo || chatParsingInfos[i].parsingType == (int)ChatDefinition.ChatParsingType.FieldInfo) { // Group : 1, Field : 2
                float calcPartWidth = 0f;
                if (chatParsingInfos[i].partMessageType == ChatDefinition.PartMessageType.TimeStampType) {
                    calcPartWidth = ChattingController.Instance.TimeSupportText.GetTextWidth(chatParsingInfos[i].messageText);
                } else {
                    calcPartWidth = supportTextMessage.GetTextWidth(chatParsingInfos[i].messageText);
                }

                inputChatTextInfo = null;

                bool isInputState = true;
                if (chatParsingInfos[i].partMessageType == ChatDefinition.PartMessageType.None) {
                    if (calcPartWidth + curMessageWidth > messageMaxWidth) {
                        lineCount++;
                        curMessageWidth = 0;
                    }

                    if (!chatParsingInfos[i].isChatColor) {
                        if (retChatTextInfos.Count > 0) {
                            MultiChatTextInfo chatTextInfo = retChatTextInfos[retChatTextInfos.Count - 1];
                            if (!chatTextInfo.IsChatColor && chatTextInfo.ChatViewType == ChatDefinition.ChatViewTextType.NormalText && chatTextInfo.LineCount == lineCount) {
                                chatTextInfo.ChatPartMessage += chatParsingInfos[i].messageText;
                                chatTextInfo.PartTextWidth += calcPartWidth;
                                isInputState = false;
                            }
                        }
                    }
                }

                if(isInputState) {
                    inputChatTextInfo = new MultiChatTextInfo();
                    inputChatTextInfo.IsChatColor = chatParsingInfos[i].isChatColor;
                    inputChatTextInfo.ChatColor = chatParsingInfos[i].chatColor;
                    inputChatTextInfo.ChatPartKeyValue = chatParsingInfos[i].fieldKeyText;
                    inputChatTextInfo.PartMessageInfo = chatParsingInfos[i].partMessageInfo;
                }

                if (chatParsingInfos[i].partMessageType == ChatDefinition.PartMessageType.None) {
                    if(isInputState)
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.NormalText;
                } else {
                    ChatDefinition.PartMessageType partMessageType = GetPartMessageType(inputChatTextInfo.ChatPartKeyValue);
                    if (partMessageType == ChatDefinition.PartMessageType.ItemInfoType) {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.ButtonBGText;
                    } else if (partMessageType == ChatDefinition.PartMessageType.CurrencyImgType) {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.CurrencyImgText;
                        calcPartWidth += 32;
                    } else if (partMessageType == ChatDefinition.PartMessageType.PartyAcceptType) {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.PartyAcceptButton;
                    } else if (partMessageType == ChatDefinition.PartMessageType.PartyDenyType) {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.PartyDenyButton;
                    } else if (partMessageType == ChatDefinition.PartMessageType.PartyRoleImgType) {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.PartyRoleImg;
                        calcPartWidth += 32;
                    } else {
                        inputChatTextInfo.ChatViewType = ChatDefinition.ChatViewTextType.NormalButtonText;
                    }

                    inputChatTextInfo.OnClickPartMessage = ChattingController.Instance.ButtonEventManager.OnClickChatPartMessage;

                    if (calcPartWidth + curMessageWidth > messageMaxWidth) {
                        lineCount++;
                        curMessageWidth = 0;
                    }
                }

                if (isInputState) {
                    inputChatTextInfo.ChatPartMessage = chatParsingInfos[i].messageText;
                    inputChatTextInfo.LineCount = lineCount;
                    inputChatTextInfo.PartTextWidth = calcPartWidth;
                    retChatTextInfos.Add(inputChatTextInfo);
                }

                curMessageWidth += calcPartWidth;
            }

            if (lineCount > LimitLineCount)
                break;
        }
    }

    static void AddChatPartyRoleIconImg(ChatMessage chatMessage, List<MultiChatTextInfo> addMultiTextInfo, out float curMessageWidth)
    {
        curMessageWidth = 0f;

        if(chatMessage.prm.ContainsKey("roleIndex")) {
            ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
            inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.PartyRoleImgType;
            inputPartMessageInfo.partValues = new string[1];
            inputPartMessageInfo.partValues[0] = chatMessage.prm["roleIndex"];

            if (!chatMessage.partMessageInfos.ContainsKey("roleIndex")) {
                chatMessage.partMessageInfos.Add("roleIndex", inputPartMessageInfo);
            }

            MultiChatTextInfo chatTextInfo = GetChatPartyRoleIconTextInfo(inputPartMessageInfo);
            curMessageWidth += chatTextInfo.PartTextWidth;
            addMultiTextInfo.Add(chatTextInfo);
        }
    }

    static void SetChatMessageParsing(ChatMessage chatMessage, char[] chatMessageChars, List<ChatMessageParsingInfo> chatParsingInfos)
    {
        bool isGroupState = false;

        bool isFieldState = false;

        ChatMessageParsingInfo inputNormalParsingInfo = new ChatMessageParsingInfo();
        inputNormalParsingInfo.parsingType = (int)ChatDefinition.ChatParsingType.NormalText;
        inputNormalParsingInfo.startIndex = 0;

        ChatMessageParsingInfo inputGroupParsingInfo = null;
        ChatMessageFieldInfo inputGroupFieldInfo = null;

        ChatMessageParsingInfo inputFieldParsingInfo = null;

        for (int i = 0; i < chatMessageChars.Length; i++) {
            if (chatMessageChars[i] == '[') {
                if (inputNormalParsingInfo.startIndex < i) {
                    inputNormalParsingInfo.endIndex = i - 1;
                    int normalLength = inputNormalParsingInfo.endIndex - inputNormalParsingInfo.startIndex + 1;
                    char[] normalChars = new char[normalLength];
                    for (int j = 0; j < normalLength; j++) {
                        normalChars[j] = chatMessageChars[inputNormalParsingInfo.startIndex + j];
                    }

                    inputNormalParsingInfo.messageText = new string(normalChars);
                    chatParsingInfos.Add(inputNormalParsingInfo);
                }
                inputGroupParsingInfo = new ChatMessageParsingInfo();
                inputGroupParsingInfo.parsingType = (int)ChatDefinition.ChatParsingType.GroupInfo;
                inputGroupParsingInfo.startIndex = i;
                isGroupState = true;

            } else if (isGroupState && chatMessageChars[i] == ']') {
                if (chatMessageChars.Length > i + 1 && chatMessageChars[i + 1] == ' ') {
                    i++;
                }

                inputGroupParsingInfo.endIndex = i;

                int groupLength = inputGroupParsingInfo.endIndex - inputGroupParsingInfo.startIndex + 1;
                char[] groupChars = new char[groupLength];
                for (int j = 0; j < groupLength; j++) {
                    groupChars[j] = chatMessageChars[inputGroupParsingInfo.startIndex + j];
                }
                inputGroupParsingInfo.messageText = new string(groupChars);
                inputGroupParsingInfo.MakeGroupFieldMessage(chatMessage);
                chatParsingInfos.Add(inputGroupParsingInfo);

                isGroupState = false;

                inputNormalParsingInfo = new ChatMessageParsingInfo();
                inputNormalParsingInfo.parsingType = (int)ChatDefinition.ChatParsingType.NormalText;
                inputNormalParsingInfo.startIndex = i + 1;
            } else if (chatMessageChars[i] == '{') {
                if (isGroupState) {
                    inputGroupFieldInfo = new ChatMessageFieldInfo();
                    inputGroupFieldInfo.startIndex = i;
                } else {
                    inputFieldParsingInfo = new ChatMessageParsingInfo();
                    inputFieldParsingInfo.parsingType = (int)ChatDefinition.ChatParsingType.FieldInfo;
                    inputFieldParsingInfo.startIndex = i;

                    if (inputNormalParsingInfo.startIndex < i) {
                        inputNormalParsingInfo.endIndex = i - 1;
                        int normalLength = inputNormalParsingInfo.endIndex - inputNormalParsingInfo.startIndex + 1;
                        char[] normalChars = new char[normalLength];
                        for (int j = 0; j < normalLength; j++) {
                            normalChars[j] = chatMessageChars[inputNormalParsingInfo.startIndex + j];
                        }

                        inputNormalParsingInfo.messageText = new string(normalChars);
                        chatParsingInfos.Add(inputNormalParsingInfo);
                    }
                }

                isFieldState = true;
            } else if (isFieldState && chatMessageChars[i] == '}') {
                if (isGroupState) {
                    inputGroupFieldInfo.endIndex = i;

                    int fieldLength = inputGroupFieldInfo.endIndex - inputGroupFieldInfo.startIndex + 1;
                    char[] fieldChars = new char[fieldLength];
                    for (int j = 0; j < fieldLength; j++) {
                        fieldChars[j] = chatMessageChars[inputGroupFieldInfo.startIndex + j];
                    }
                    inputGroupFieldInfo.fieldText = new string(fieldChars);

                    char[] fieldKeyChars = new char[fieldLength - 2];
                    for (int j = 0; j < fieldLength - 2; j++) {
                        fieldKeyChars[j] = chatMessageChars[inputGroupFieldInfo.startIndex + j + 1];
                    }

                    inputGroupFieldInfo.fieldKeyText = new string(fieldKeyChars);
                    if (chatMessage != null)
                        inputGroupFieldInfo.fieldPartMessageInfo = chatMessage.GetPartMessageInfo(inputGroupFieldInfo.fieldKeyText);
                    inputGroupFieldInfo.partMessageType = GetPartMessageType(inputGroupFieldInfo.fieldKeyText);

                    inputGroupParsingInfo.includeGroupFieldInfos.Add(inputGroupFieldInfo);
                } else {
                    inputFieldParsingInfo.endIndex = i;
                    bool emptyChar = false;
                    if (chatMessageChars.Length > i + 1 && chatMessageChars[i + 1] == ' ') {
                        emptyChar = true;
                        i++;
                    }

                    int fieldLength = inputFieldParsingInfo.endIndex - inputFieldParsingInfo.startIndex + 1;
                    char[] fieldChars = new char[fieldLength];
                    for (int j = 0; j < fieldLength; j++) {
                        fieldChars[j] = chatMessageChars[inputFieldParsingInfo.startIndex + j];
                    }
                    inputFieldParsingInfo.fieldText = new string(fieldChars);

                    char[] fieldKeyChars = new char[fieldLength - 2];
                    for (int j = 0; j < fieldLength - 2; j++) {
                        fieldKeyChars[j] = chatMessageChars[inputFieldParsingInfo.startIndex + j + 1];
                    }
                    inputFieldParsingInfo.fieldKeyText = new string(fieldKeyChars);
                    if (chatMessage != null)
                        inputFieldParsingInfo.partMessageInfo = chatMessage.GetPartMessageInfo(inputFieldParsingInfo.fieldKeyText);
                    inputFieldParsingInfo.partMessageType = GetPartMessageType(inputFieldParsingInfo.fieldKeyText);

                    if (chatMessage != null && chatMessage.prm.ContainsKey(inputFieldParsingInfo.fieldKeyText)) {
                        if (emptyChar) {
                            inputFieldParsingInfo.messageText = string.Format("{0} ", chatMessage.prm[inputFieldParsingInfo.fieldKeyText]);
                        } else {
                            inputFieldParsingInfo.messageText = chatMessage.prm[inputFieldParsingInfo.fieldKeyText];
                        }
                    } else {
                        if (emptyChar) {
                            inputFieldParsingInfo.messageText = string.Format("{0} ", inputFieldParsingInfo.fieldText);
                        } else {
                            inputFieldParsingInfo.messageText = inputFieldParsingInfo.fieldText;
                        }
                    }

                    chatParsingInfos.Add(inputFieldParsingInfo);

                    inputNormalParsingInfo = new ChatMessageParsingInfo();
                    inputNormalParsingInfo.parsingType = (int)ChatDefinition.ChatParsingType.NormalText;
                    inputNormalParsingInfo.startIndex = i + 1;
                }

                isFieldState = false;
            } else if (i == chatMessageChars.Length - 1) {
                if (inputNormalParsingInfo.startIndex <= i) {
                    inputNormalParsingInfo.endIndex = i;
                    int normalLength = inputNormalParsingInfo.endIndex - inputNormalParsingInfo.startIndex + 1;
                    char[] normalChars = new char[normalLength];
                    for (int j = 0; j < normalLength; j++) {
                        normalChars[j] = chatMessageChars[inputNormalParsingInfo.startIndex + j];
                    }

                    inputNormalParsingInfo.messageText = new string(normalChars);
                    chatParsingInfos.Add(inputNormalParsingInfo);
                }
            }
        }
    }

    static List<MultiChatTextInfo> GetPartChatTextInfos(ChatDefinition.ChatViewTextType chatTextType, UIChatPartMessage supportTextMessage, float messageMaxWidth, string partKeyValue, float curMessageWidth, string partMessage, int lineCount, out float calcWidth, bool isQuickMessage = false)
    {
        calcWidth = 0f;

        if (string.IsNullOrEmpty(partMessage)) {
            Debug.Log(string.Format("GetPartChatTextInfos string.IsNullOrEmpty(partMessage)"));
            return null;
        }

        List<MultiChatTextInfo> retValue = new List<MultiChatTextInfo>();

        char[] normalCharTexts = partMessage.ToCharArray();

        MultiChatTextInfo inputChatTextInfo = null;

        float curNormalWidth = curMessageWidth;
        int startIndex = 0;
        int exceptCount = 0;

        int curEmptyIndex = -1;

        for (int i = 0; i < normalCharTexts.Length; i++) {
            if (lineCount > LimitLineCount) {
                break;
            }

            if (normalCharTexts[i] == ' ') {
                curEmptyIndex = i;
            }

            if (normalCharTexts[i] == 0x0D || normalCharTexts[i] == '\n') {
                if (startIndex <= i - 1) {
                    int normalLength = i - startIndex;
                    char[] inputNormalChars = new char[normalLength];
                    for (int j = 0; j < normalLength; j++) {
                        inputNormalChars[j] = normalCharTexts[startIndex + j];
                    }
                    string normalStringText = new string(inputNormalChars);

                    inputChatTextInfo = new MultiChatTextInfo();
                    inputChatTextInfo.ChatViewType = chatTextType;
                    inputChatTextInfo.ChatPartKeyValue = partKeyValue;
                    inputChatTextInfo.ChatPartMessage = normalStringText;
                    inputChatTextInfo.PartTextWidth = supportTextMessage.GetTextWidth(normalStringText);
                    inputChatTextInfo.LineCount = lineCount;
                    retValue.Add(inputChatTextInfo);

                    lineCount++;
                    curNormalWidth = 0f;
                    startIndex = i + 1;
                    if (i + 1 < normalCharTexts.Length) {
                        for (int j = i + 1; j < normalCharTexts.Length; j++) {
                            if (normalCharTexts[j] != 0x0D && normalCharTexts[j] != '\n') {
                                startIndex = j;
                                break;
                            }
                        }
                    }
                    
                    curEmptyIndex = -1;
                }
                continue;
            }

            float curCharWidth = supportTextMessage.GetTextWidthByChar(normalCharTexts[i]);

            if ((curNormalWidth + curCharWidth > messageMaxWidth)) {
                i--;
                if (i < startIndex) {
                    curNormalWidth = 0f;
                    lineCount++;
                    exceptCount++;
                    if (exceptCount > 10000) {
                        Debug.Log(string.Format("GetPartChatTextInfos exceptCount > 10000!!!!!!!"));
                    }
                    continue;
                }

                bool isAlphabet = false;
                if (normalCharTexts[i] != ' ') {
                    Encoding enc = Encoding.UTF8;
                    if (enc.GetByteCount(normalCharTexts, i + 1, 1) == 1) {
                        isAlphabet = true;
                    }
                }

                if (isAlphabet && curEmptyIndex != -1) {
                    Encoding enc = Encoding.UTF8;
                    if (enc.GetByteCount(normalCharTexts, i, 1) != 3) {
                        i = curEmptyIndex;
                    }
                }

                int normalLength = i - startIndex + 1;
                char[] inputNormalChars = new char[normalLength];
                for (int j = 0; j < normalLength; j++) {
                    inputNormalChars[j] = normalCharTexts[startIndex + j];
                }
                string normalStringText = new string(inputNormalChars);

                inputChatTextInfo = new MultiChatTextInfo();
                inputChatTextInfo.ChatViewType = chatTextType;
                inputChatTextInfo.ChatPartKeyValue = partKeyValue;
                inputChatTextInfo.ChatPartMessage = normalStringText;
                inputChatTextInfo.PartTextWidth = supportTextMessage.GetTextWidth(normalStringText);
                inputChatTextInfo.LineCount = lineCount;
                retValue.Add(inputChatTextInfo);

                lineCount++;
                curNormalWidth = 0f;
                startIndex = i + 1;
                curEmptyIndex = -1;
            } else {
                curNormalWidth += curCharWidth;
            }

            if (i == normalCharTexts.Length - 1) {
                if (startIndex <= i) {
                    int normalLength = i - startIndex + 1;
                    char[] inputNormalChars = new char[normalLength];
                    for (int j = 0; j < normalLength; j++) {
                        inputNormalChars[j] = normalCharTexts[startIndex + j];
                    }
                    string normalStringText = new string(inputNormalChars);

                    inputChatTextInfo = new MultiChatTextInfo();
                    inputChatTextInfo.ChatViewType = chatTextType;
                    inputChatTextInfo.ChatPartKeyValue = partKeyValue;
                    inputChatTextInfo.ChatPartMessage = normalStringText;
                    inputChatTextInfo.PartTextWidth = supportTextMessage.GetTextWidth(normalStringText);
                    inputChatTextInfo.LineCount = lineCount;
                    retValue.Add(inputChatTextInfo);
                }
            }
        }

        calcWidth = curNormalWidth;

        return retValue;
    }

    static void SetAddMultiTextInfo(List<MultiChatTextInfo> addMultiTextInfo, int lineCount, float curMessageWidth, float messageMaxWidth, List<MultiChatTextInfo> retChatTextInfos)
    {
        UIChatPartMessage supportTextMessage = ChattingController.Instance.SupportTextMessage;

        if (addMultiTextInfo != null && addMultiTextInfo.Count > 0 && lineCount < LimitLineCount) {
            for (int i = 0; i < addMultiTextInfo.Count; i++) {
                MultiChatTextInfo addChatInfo = addMultiTextInfo[i];
                float calcPartWidth = 0f;
                if (addChatInfo.ChatViewType == ChatDefinition.ChatViewTextType.TimeNormalText) {
                    calcPartWidth = ChattingController.Instance.TimeSupportText.GetTextWidth(addChatInfo.ChatPartMessage);
                } else {
                    calcPartWidth = supportTextMessage.GetTextWidth(addChatInfo.ChatPartMessage);
                }

                if (calcPartWidth + curMessageWidth > messageMaxWidth) {
                    lineCount++;
                    curMessageWidth = 0;
                }

                addChatInfo.LineCount = lineCount;
                addChatInfo.PartTextWidth = calcPartWidth;

                curMessageWidth += calcPartWidth;

                retChatTextInfos.Add(addChatInfo);

                if (lineCount > LimitLineCount)
                    break;
            }
        }
    }

    public static ChatDefinition.PartMessageType GetPartMessageType(string fieldValue)
    {
        if (fieldValue == "msg") {
            return ChatDefinition.PartMessageType.NormalMessageType;
        } else if (fieldValue == "user" || fieldValue == "user1" || fieldValue == "user2") {
            return ChatDefinition.PartMessageType.UserInfoType;
        } else if (fieldValue == "Item") {
            return ChatDefinition.PartMessageType.ItemInfoType;
        } else if (fieldValue == "mission_kind") {
            return ChatDefinition.PartMessageType.MissionInfoType;
        } else if (fieldValue == "company") {
            return ChatDefinition.PartMessageType.CompanyInfoType;
        } else if (fieldValue == "enemyuser") {
            return ChatDefinition.PartMessageType.EnemyUserInfoType;
        } else if (fieldValue == "shortcut") {
            return ChatDefinition.PartMessageType.ShortcutType;
        } else if (fieldValue == "Currency") {
            return ChatDefinition.PartMessageType.CurrencyImgType;
        } else if (fieldValue == "role") {
            return ChatDefinition.PartMessageType.PartyRoleImgType;
        } else if (fieldValue == "explorespotbattle") {
            return ChatDefinition.PartMessageType.PartyExploreHelpSpot;
        } else if (fieldValue == "partyquickindex") {
            return ChatDefinition.PartMessageType.PartyQuickChat;
        } else if (fieldValue == "maxscore") {
            return ChatDefinition.PartMessageType.HeroChallengeMaxScore;
        }

        return ChatDefinition.PartMessageType.None;
    }

    public static Color GetChatMakingMessageColor(DataContext context, ChatMakingMessage chatMessage)
    {
        if (chatMessage.chatMessageInfo.messageType == (int)ChatDefinition.ChatMessageType.GMChannelChat) {
            return chatMessage.messageColor;
        }

        if (chatMessage.chatMessageInfo.userID == context.User.userData.userId) {
            if (ChatHelper.IsUserColorChatMessage(chatMessage.chatMessageInfo)) {
                return ColorPreset.CHAT_MY_MESSAGE;
            } else {
                return ChatHelper.GetChatMessageColor(context, chatMessage.chatMessageInfo);
            }
        } else {
            return ChatHelper.GetChatMessageColor(context, chatMessage.chatMessageInfo);
        }
    }

    public static List<MultiChatTextInfo> AddChatMessageInfo(TextModel textModel, ChatMessage chatMessage, out MultiChatTextInfo timeStampTextInfo)
    {
        timeStampTextInfo = null;
        string chattingMessageText = textModel.GetChatNoticeSheetText(chatMessage.msgIdx);
        SheetChatNoticeMessageRow chatNoticeRow = ChattingController.Instance.Context.Sheet.SheetChatNoticeMessage[chatMessage.msgIdx];

        List<MultiChatTextInfo> addMultiTextInfo = new List<MultiChatTextInfo>();

        for (int i = 0; i < chatNoticeRow.IndicateLocation.Length; i++) {
            if (chatNoticeRow.IndicateLocation[i] == (int)ChatDefinition.ChatMessageKind.AnnouncementChat) {
                AddChatShortcutMessage(chatMessage, chatNoticeRow, addMultiTextInfo);

                timeStampTextInfo = GetChatTimeStampTextInfo(chatMessage.timeStamp);
                addMultiTextInfo.Add(timeStampTextInfo);
                break;
            } else if (chatNoticeRow.IndicateLocation[i] == (int)ChatDefinition.ChatMessageKind.WhisperChat) {

                timeStampTextInfo = GetChatTimeStampTextInfo(chatMessage.timeStamp);
                addMultiTextInfo.Add(timeStampTextInfo);
                break;
            } else if (chatNoticeRow.IndicateLocation[i] == (int)ChatDefinition.ChatMessageKind.MyPartyChat) {
                if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.PartyInvitationChat) {
                    AddChatPartyAcceptMessage(chatMessage, chatNoticeRow, addMultiTextInfo);
                    AddChatPartyDenyMessage(chatMessage, chatNoticeRow, addMultiTextInfo);
                } else if (chatMessage.messageType == (int)ChatDefinition.ChatMessageType.PartyMissionStart) {
                    AddChatPartyStartMissionAlarmShortcut(chatMessage, chatNoticeRow, addMultiTextInfo);
                }

                timeStampTextInfo = GetChatTimeStampTextInfo(chatMessage.timeStamp);
                addMultiTextInfo.Add(timeStampTextInfo);
            }
        }
        
        return addMultiTextInfo;
    }

    public static void AddChatShortcutMessage(ChatMessage chatMessage, SheetChatNoticeMessageRow chatNoticeRow, List<MultiChatTextInfo> addMultiTextInfo)
    {
        TextModel textModel = ChattingController.Instance.Context.Text;

        chatMessage.prm.Add("shortcut", textModel.GetText(TextKey.CT_ShortCut));

        ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
        inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.ShortcutType;
        inputPartMessageInfo.partValues = new string[4];
        inputPartMessageInfo.partValues[0] = chatNoticeRow.ShortCutIndex.ToString();
        inputPartMessageInfo.partValues[1] = chatMessage.partyNum.ToString();
        inputPartMessageInfo.partValues[2] = chatMessage.missionID.ToString();
        inputPartMessageInfo.partValues[3] = chatMessage.missionContentType.ToString();

        if (!chatMessage.partMessageInfos.ContainsKey("shortcut")) {
            chatMessage.partMessageInfos.Add("shortcut", inputPartMessageInfo);
        }

        addMultiTextInfo.Add(GetChatShortcutTextInfo(inputPartMessageInfo));
    }

    public static MultiChatTextInfo GetChatTimeStampTextInfo(long timeStamp)
    {
        MultiChatTextInfo retValue = new MultiChatTextInfo();
        retValue.ChatViewType = ChatDefinition.ChatViewTextType.TimeNormalText;
        retValue.ChatPartKeyValue = "timeStamp";
        ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
        inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.TimeStampType;
        inputPartMessageInfo.partValues = new string[1];
        inputPartMessageInfo.partValues[0] = timeStamp.ToString();
        retValue.PartMessageInfo = inputPartMessageInfo;
        retValue.ChatPartMessage = string.Format(" ({0})", UIMultiChatting.GetConfirmMessageTimeText(timeStamp));

        UIChatPartMessage supportTextMessage = ChattingController.Instance.TimeSupportText;

        retValue.PartTextWidth = supportTextMessage.GetTextWidth(retValue.ChatPartMessage);

        return retValue;
    }

    public static MultiChatTextInfo GetChatShortcutTextInfo(ChatPartMessageInfo partMessageInfo)
    {
        MultiChatTextInfo retValue = new MultiChatTextInfo();
        retValue.ChatViewType = ChatDefinition.ChatViewTextType.NormalButtonText;
        retValue.ChatPartKeyValue = "shortcut";
        retValue.PartMessageInfo = partMessageInfo;
        retValue.ChatPartMessage = string.Format(" {0}", ChattingController.Instance.Context.Text.GetText(TextKey.CT_ShortCut));

        UIChatPartMessage supportTextMessage = ChattingController.Instance.SupportTextMessage;

        retValue.PartTextWidth = supportTextMessage.GetTextWidth(retValue.ChatPartMessage);
        retValue.OnClickPartMessage = ChattingController.Instance.ButtonEventManager.OnClickChatPartMessage;

        return retValue;
    }

    public static void AddChatPartyAcceptMessage(ChatMessage chatMessage, SheetChatNoticeMessageRow chatNoticeRow, List<MultiChatTextInfo> addMultiTextInfo)
    {
        TextModel textModel = ChattingController.Instance.Context.Text;

        chatMessage.prm.Add("partyaccept", textModel.GetText(TextKey.Party_Popup_Invitation_Accept));

        string partyname = "";
        if (chatMessage.prm.ContainsKey("partyname"))
            partyname = chatMessage.prm["partyname"];

        string partyId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("partyId"))
            partyId = chatMessage.prm["partyId"];

        string missionId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("missionId"))
            missionId = chatMessage.prm["missionId"];

        string missionKind = (-1).ToString();
        if (chatMessage.prm.ContainsKey("missionkind"))
            missionKind = chatMessage.prm["missionkind"];

        ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
        inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.PartyAcceptType;
        inputPartMessageInfo.partValues = new string[5];
        inputPartMessageInfo.partValues[0] = partyname;
        inputPartMessageInfo.partValues[1] = partyId;
        inputPartMessageInfo.partValues[2] = missionId;
        inputPartMessageInfo.partValues[3] = chatMessage.userID.ToString();
        inputPartMessageInfo.partValues[4] = missionKind;

        if (!chatMessage.partMessageInfos.ContainsKey("partyaccept")) {
            chatMessage.partMessageInfos.Add("partyaccept", inputPartMessageInfo);
        }

        addMultiTextInfo.Add(GetChatPartyInviteAcceptTextInfo(inputPartMessageInfo));
    }

    public static void AddChatPartyDenyMessage(ChatMessage chatMessage, SheetChatNoticeMessageRow chatNoticeRow, List<MultiChatTextInfo> addMultiTextInfo)
    {
        TextModel textModel = ChattingController.Instance.Context.Text;

        chatMessage.prm.Add("partydeny", textModel.GetText(TextKey.Party_Popup_Invitation_Deny));

        string partyname = "";
        if (chatMessage.prm.ContainsKey("partyname"))
            partyname = chatMessage.prm["partyname"];

        string partyId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("partyId"))
            partyId = chatMessage.prm["partyId"];

        string missionId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("missionId"))
            missionId = chatMessage.prm["missionId"];

        ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
        inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.PartyDenyType;
        inputPartMessageInfo.partValues = new string[4];
        inputPartMessageInfo.partValues[0] = partyname;
        inputPartMessageInfo.partValues[1] = partyId;
        inputPartMessageInfo.partValues[2] = missionId;
        inputPartMessageInfo.partValues[3] = chatMessage.userID.ToString();

        if (!chatMessage.partMessageInfos.ContainsKey("partydeny")) {
            chatMessage.partMessageInfos.Add("partydeny", inputPartMessageInfo);
        }

        addMultiTextInfo.Add(GetChatPartyInviteDenyTextInfo(inputPartMessageInfo));
    }

    public static MultiChatTextInfo GetChatPartyInviteAcceptTextInfo(ChatPartMessageInfo partMessageInfo)
    {
        MultiChatTextInfo retValue = new MultiChatTextInfo();
        retValue.ChatViewType = ChatDefinition.ChatViewTextType.PartyAcceptButton;
        retValue.ChatPartKeyValue = "partyaccept";
        retValue.PartMessageInfo = partMessageInfo;
        retValue.ChatPartMessage = string.Format(" {0}", ChattingController.Instance.Context.Text.GetText(TextKey.Party_Popup_Invitation_Accept));

        UIChatPartMessage supportTextMessage = ChattingController.Instance.SupportTextMessage;

        retValue.PartTextWidth = supportTextMessage.GetTextWidth(retValue.ChatPartMessage);
        retValue.OnClickPartMessage = ChattingController.Instance.ButtonEventManager.OnClickChatPartMessage;

        return retValue;
    }

    public static MultiChatTextInfo GetChatPartyInviteDenyTextInfo(ChatPartMessageInfo partMessageInfo)
    {
        MultiChatTextInfo retValue = new MultiChatTextInfo();
        retValue.ChatViewType = ChatDefinition.ChatViewTextType.PartyDenyButton;
        retValue.ChatPartKeyValue = "partydeny";
        retValue.PartMessageInfo = partMessageInfo;
        retValue.ChatPartMessage = string.Format(" {0}", ChattingController.Instance.Context.Text.GetText(TextKey.Party_Popup_Invitation_Deny));

        UIChatPartMessage supportTextMessage = ChattingController.Instance.SupportTextMessage;

        retValue.PartTextWidth = supportTextMessage.GetTextWidth(retValue.ChatPartMessage);
        retValue.OnClickPartMessage = ChattingController.Instance.ButtonEventManager.OnClickChatPartMessage;

        return retValue;
    }

    public static MultiChatTextInfo GetChatPartyRoleIconTextInfo(ChatPartMessageInfo partMessageInfo)
    {
        MultiChatTextInfo retValue = new MultiChatTextInfo();
        retValue.ChatViewType = ChatDefinition.ChatViewTextType.PartyRoleImg;
        retValue.ChatPartKeyValue = "roleIndex";
        retValue.PartMessageInfo = partMessageInfo;

        retValue.PartTextWidth = 32f;

        return retValue;
    }

    public static void AddChatPartyStartMissionAlarmShortcut(ChatMessage chatMessage, SheetChatNoticeMessageRow chatNoticeRow, List<MultiChatTextInfo> addMultiTextInfo)
    {
        TextModel textModel = ChattingController.Instance.Context.Text;

        chatMessage.prm.Add("shortcut", textModel.GetText(TextKey.CT_ShortCut));

        string partyId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("partyId"))
            partyId = chatMessage.prm["partyId"];

        string missionId = (-1).ToString();
        if (chatMessage.prm.ContainsKey("missionId"))
            missionId = chatMessage.prm["missionId"];

        ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo();
        inputPartMessageInfo.partMessageType = (int)ChatDefinition.PartMessageType.ShortcutType;
        inputPartMessageInfo.partValues = new string[4];
        inputPartMessageInfo.partValues[0] = chatNoticeRow.ShortCutIndex.ToString();
        inputPartMessageInfo.partValues[1] = partyId;
        inputPartMessageInfo.partValues[2] = missionId;
        inputPartMessageInfo.partValues[3] = chatMessage.missionContentType.ToString();

        if (!chatMessage.partMessageInfos.ContainsKey("shortcut")) {
            chatMessage.partMessageInfos.Add("shortcut", inputPartMessageInfo);
        }

        addMultiTextInfo.Add(GetChatShortcutTextInfo(inputPartMessageInfo));
    }

    public static ChatGMSMessage GetChatGMSMessage(int msgIdx, ChatDefinition.ChatMessageType messageType, string message, Color chatColor)
    {
        ChatGMSMessage retValue = new ChatGMSMessage();
        retValue.msgIdx = msgIdx;
        retValue.messageType = (int)messageType;
        retValue.prm.Add("msg", string.Format("{0}", message));
        retValue.chatColor = chatColor;

        return retValue;
    }

    public static long PlayerId
    {
        get {
#if USE_HIVE
            var playerInfo = HiveHelper.instance.PlayerInfo;
            if (playerInfo != null)
            {
                return playerInfo.playerId;
            }
#else
            var user = GameSystem.Instance.Data.User;
            if (user != null && user.userData != null) {
                return user.ID;
            }
#endif
            return 0;
        }
    }

    #endregion
}
