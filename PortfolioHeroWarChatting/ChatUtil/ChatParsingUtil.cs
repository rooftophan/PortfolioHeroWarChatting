using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Text;
using System.Linq;
using System;

public class ChatParsingUtil
{
	public static JsonData GetChatMessageJsonData(string chatMsg)
	{
		return JsonMapper.ToObject (chatMsg);
	}

	public static ChatMessage GetChatMessageParsingByJson(JsonData chatJson)
	{
		ChatMessage retValue = new ChatMessage();

        if (((IDictionary)chatJson).Contains("chatKind")) {
            retValue.chatKind = (int)chatJson["chatKind"];
        }
        retValue.messageType = (int)chatJson["messageType"];

        retValue.isSelfNotify = (bool)chatJson ["isSelfNotify"];

		retValue.msgIdx = (int)chatJson ["msgIdx"];
		retValue.partyType = (int)chatJson ["partyType"];

		if (chatJson ["partyNum"].IsInt) {
			retValue.partyNum = (long)(int)chatJson ["partyNum"];
		} else if(chatJson ["partyNum"].IsLong) {
			retValue.partyNum = (long)chatJson ["partyNum"];
		}

		if (chatJson ["timeStamp"].IsInt) {
			retValue.timeStamp = (long)(int)chatJson ["timeStamp"];
		} else if(chatJson ["timeStamp"].IsLong) {
			retValue.timeStamp = (long)chatJson ["timeStamp"];
		}

		if (chatJson ["userID"].IsInt) {
			retValue.userID = (long)(int)chatJson ["userID"];
		} else if(chatJson ["userID"].IsLong) {
			retValue.userID = (long)chatJson ["userID"];
		}

        if (((IDictionary)chatJson).Contains("connectId")) {
            if (chatJson["connectId"].IsInt) {
                retValue.connectId = (long)(int)chatJson["connectId"];
            } else if (chatJson["connectId"].IsLong) {
                retValue.connectId = (long)chatJson["connectId"];
            }
        }

        if (chatJson ["prm"] != null) {
			JsonData prmJson = chatJson ["prm"];
			foreach (DictionaryEntry entry in prmJson as IDictionary) {
				string keyValue = (string)entry.Key;
				string prmValue = (string)(JsonData)entry.Value;
				retValue.prm.Add (keyValue, prmValue);
			}
		}

		if (chatJson ["partMessageInfos"] != null) {
			JsonData partMessageInfoJson = chatJson ["partMessageInfos"];
			foreach (DictionaryEntry entry in partMessageInfoJson as IDictionary) {
				string keyValue = (string)entry.Key;
				JsonData jPartMsgInfoValue = (JsonData)entry.Value;

				ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo ();
				inputPartMessageInfo.partMessageType = (int)jPartMsgInfoValue ["partMessageType"];

				if (jPartMsgInfoValue ["partValues"] != null) {
					inputPartMessageInfo.partValues = new string[jPartMsgInfoValue ["partValues"].Count];
					for (int i = 0; i < jPartMsgInfoValue ["partValues"].Count; i++) {
						inputPartMessageInfo.partValues [i] = (string)jPartMsgInfoValue ["partValues"] [i];
					}
				}

				retValue.partMessageInfos.Add (keyValue, inputPartMessageInfo);
			}
		}

		return retValue;
	}

	public static ChatMessage GetChatMessageParsingByString(string chatMsg)
	{
		return GetChatMessageParsingByJson(GetChatMessageJsonData(chatMsg));
	}

    public static ChatGMSMessage GetChatMessageParsingByServerChat(string chatMsg)
    {
        JsonData chatJson = GetChatMessageJsonData(chatMsg);

        int market = (int)chatJson["market"];
        if(!ChatEventManager.CheckValidChatMarket(market))
            return null;

        ChatGMSMessage retValue = new ChatGMSMessage();

        if (((IDictionary)chatJson).Contains("chatKind")) {
            retValue.chatKind = (int)chatJson["chatKind"];
        }

        retValue.messageType = (int)chatJson["messageType"];
        retValue.msgIdx = (int)chatJson["msgIdx"];

        if (chatJson["timestamp"].IsInt) {
            retValue.timeStamp = (long)(int)chatJson["timestamp"];
        } else if (chatJson["timestamp"].IsLong) {
            retValue.timeStamp = (long)chatJson["timestamp"];
        }

        if((chatJson as IDictionary).Contains("fixedTime")) {
            if (chatJson["fixedTime"].IsInt) {
                retValue.fixedTime = (float)(int)chatJson["fixedTime"];
            } else if (chatJson["fixedTime"].IsDouble) {
                retValue.fixedTime = (float)(double)chatJson["fixedTime"];
            }
            
            //if(retValue.fixedTime < 5) {
            //    retValue.fixedTime = 5;
            //}
        } else {
            Debug.Log(string.Format("!!!!!--GMS GetChatMessageParsingByServerChat NotExist fixedTime"));
            retValue.fixedTime = 5f;
        }

        if ((chatJson as IDictionary).Contains("gapTime")) {
            if (chatJson["gapTime"].IsInt) {
                retValue.gapTime = (float)(int)chatJson["gapTime"];
            } else if (chatJson["gapTime"].IsDouble) {
                retValue.gapTime = (float)(double)chatJson["gapTime"];
            }
        } else {
            Debug.Log(string.Format("!!!!!--GMS GetChatMessageParsingByServerChat NotExist gapTime"));
            retValue.gapTime = 5f;
        }

#if _CHATTING_LOG
        Debug.Log(string.Format("GMS GetChatMessageParsingByServerChat fixedTime : {0}, gapTime : {1}", retValue.fixedTime, retValue.gapTime));
#endif

        retValue.chatColor = ChatEventManager.GetChatColor((string)chatJson["color"]);
        retValue.market = market;

        JsonData langJson = chatJson["langs"];
        for(int i = 0;i< langJson.Count;i++) {
            JsonData langSubJson = langJson[i];
            int lang_id = (int)langSubJson["lang_id"];
            if(lang_id == (int)GameSystem.Instance.SystemData.SystemOption.Language) {
                retValue.messageText = (string)langSubJson["content"];
                break;
            }
        }

        return retValue;
    }

    public static string[] GetChatSystemParsingMessages(string message)
    {
        string[] retValue = message.Split('#');

        return retValue;
    }

    public static ChatWhisperMessage GetChatWhisperMessageParsingByJson(JsonData chatJson)
	{
		ChatWhisperMessage retValue = new ChatWhisperMessage();

        if (((IDictionary)chatJson).Contains("chatKind")){
            retValue.chatKind = (int)chatJson["chatKind"];
        }

        retValue.messageType = (int)chatJson["messageType"];
        retValue.isSelfNotify = (bool)chatJson ["isSelfNotify"];

		retValue.msgIdx = (int)chatJson ["msgIdx"];
		retValue.partyType = (int)chatJson ["partyType"];

		if (chatJson ["partyNum"].IsInt) {
			retValue.partyNum = (long)(int)chatJson ["partyNum"];
		} else if(chatJson ["partyNum"].IsLong) {
			retValue.partyNum = (long)chatJson ["partyNum"];
		}

		if (chatJson ["timeStamp"].IsInt) {
			retValue.timeStamp = (long)(int)chatJson ["timeStamp"];
		} else if(chatJson ["timeStamp"].IsLong) {
			retValue.timeStamp = (long)chatJson ["timeStamp"];
		}

		if (chatJson ["userID"].IsInt) {
			retValue.userID = (long)(int)chatJson ["userID"];
		} else if(chatJson ["userID"].IsLong) {
			retValue.userID = (long)chatJson ["userID"];
		}

        if (((IDictionary)chatJson).Contains("connectId")) {
            if (chatJson["connectId"].IsInt) {
                retValue.connectId = (long)(int)chatJson["connectId"];
            } else if (chatJson["connectId"].IsLong) {
                retValue.connectId = (long)chatJson["connectId"];
            }
        }

        retValue.sendUserName = (string)chatJson ["sendUserName"];

        if(chatJson["whisperKind"] != null)
            retValue.whisperKind = (int)chatJson["whisperKind"];

        if (chatJson ["targetUserID"].IsInt) {
			retValue.targetUserID = (long)(int)chatJson ["targetUserID"];
		} else if(chatJson ["targetUserID"].IsLong) {
			retValue.targetUserID = (long)chatJson ["targetUserID"];
		}

        if (((IDictionary)chatJson).Contains("targetConnectID")){
            if (chatJson["targetConnectID"].IsInt) {
                retValue.targetConnectID = (long)(int)chatJson["targetConnectID"];
            } else if (chatJson["targetConnectID"].IsLong) {
                retValue.targetConnectID = (long)chatJson["targetConnectID"];
            }
        }

        if (chatJson["companyID"] != null) {
            if (chatJson["companyID"].IsInt) {
                retValue.companyID = (long)(int)chatJson["companyID"];
            } else if (chatJson["companyID"].IsLong) {
                retValue.companyID = (long)chatJson["companyID"];
            }
        }

        retValue.targetUserName = (string)chatJson ["targetUserName"];

		if (chatJson ["prm"] != null) {
			JsonData prmJson = chatJson ["prm"];
			foreach (DictionaryEntry entry in prmJson as IDictionary) {
				string keyValue = (string)entry.Key;
				string prmValue = (string)(JsonData)entry.Value;
				retValue.prm.Add (keyValue, prmValue);
			}
		}

		if (chatJson ["partMessageInfos"] != null) {
			JsonData partMessageInfoJson = chatJson ["partMessageInfos"];
			foreach (DictionaryEntry entry in partMessageInfoJson as IDictionary) {
				string keyValue = (string)entry.Key;
				JsonData jPartMsgInfoValue = (JsonData)entry.Value;

				ChatPartMessageInfo inputPartMessageInfo = new ChatPartMessageInfo ();
				inputPartMessageInfo.partMessageType = (int)jPartMsgInfoValue ["partMessageType"];

				if (jPartMsgInfoValue ["partValues"] != null) {
					inputPartMessageInfo.partValues = new string[jPartMsgInfoValue ["partValues"].Count];
					for (int i = 0; i < jPartMsgInfoValue ["partValues"].Count; i++) {
						inputPartMessageInfo.partValues [i] = (string)jPartMsgInfoValue ["partValues"] [i];
					}
				}

				retValue.partMessageInfos.Add (keyValue, inputPartMessageInfo);
			}
		}

		return retValue;
	}

	public static ChatWhisperMessage GetChatWhisperMessageParsingByString(string chatMsg)
	{
		return GetChatWhisperMessageParsingByJson(GetChatMessageJsonData(chatMsg));
	}

	public static string MakeChatMessageToString(ChatMessage chatMessage)
	{
		StringBuilder sb = new StringBuilder();
		JsonWriter writer = new JsonWriter(sb);
		writer.WriteObjectStart();
		{
            writer.WritePropertyName("chatKind");
            writer.Write(chatMessage.chatKind);

            writer.WritePropertyName("messageType");
			writer.Write(chatMessage.messageType);

            writer.WritePropertyName("isSelfNotify");
			writer.Write(chatMessage.isSelfNotify);

			writer.WritePropertyName("msgIdx");
			writer.Write(chatMessage.msgIdx);

			writer.WritePropertyName("partyType");
			writer.Write(chatMessage.partyType);

			writer.WritePropertyName("partyNum");
			writer.Write(chatMessage.partyNum);

			writer.WritePropertyName("timeStamp");
			writer.Write(chatMessage.timeStamp);

			writer.WritePropertyName("userID");
			writer.Write(chatMessage.userID);

            writer.WritePropertyName("connectId");
            writer.Write(chatMessage.connectId);

            writer.WritePropertyName("prm");
			if (chatMessage.prm != null) {
				writer.WriteObjectStart ();
				{
					List<string> prmKeys = chatMessage.prm.Keys.ToList ();
					for (int i = 0; i < prmKeys.Count; i++) {
						writer.WritePropertyName(prmKeys[i]);
						writer.Write(chatMessage.prm[prmKeys[i]]);
					}
				}
				writer.WriteObjectEnd ();
			} else {
				writer.Write (null);
			}

			writer.WritePropertyName("partMessageInfos");
			if (chatMessage.partMessageInfos != null) {
				writer.WriteObjectStart ();
				{
					List<string> partMsgKeys = chatMessage.partMessageInfos.Keys.ToList ();
					for (int i = 0; i < partMsgKeys.Count; i++) {
						ChatPartMessageInfo partMessageInfo = chatMessage.partMessageInfos [partMsgKeys [i]];
						writer.WritePropertyName(partMsgKeys[i]);
						writer.WriteObjectStart ();
						{
							writer.WritePropertyName("partMessageType");
							writer.Write(partMessageInfo.partMessageType);

							writer.WritePropertyName("partValues");
							if (partMessageInfo.partValues != null) {
								writer.WriteArrayStart ();
								{
									for (int j = 0; j < partMessageInfo.partValues.Length; j++) {
										writer.Write (partMessageInfo.partValues [j]);
									}
								}
								writer.WriteArrayEnd ();
							} else {
								writer.Write (null);
							}

						}
						writer.WriteObjectEnd ();
					}
				}
				writer.WriteObjectEnd ();
			} else {
				writer.Write (null);
			}
		}
		writer.WriteObjectEnd ();

		return sb.ToString ();
	}

	public static string MakeChatWhisperMessageToString(ChatWhisperMessage chatMessage)
	{
		StringBuilder sb = new StringBuilder();
		JsonWriter writer = new JsonWriter(sb);
		writer.WriteObjectStart();
		{
            writer.WritePropertyName("chatKind");
            writer.Write(chatMessage.chatKind);

            writer.WritePropertyName("messageType");
			writer.Write(chatMessage.messageType);

			writer.WritePropertyName("isSelfNotify");
			writer.Write(chatMessage.isSelfNotify);

			writer.WritePropertyName("msgIdx");
			writer.Write(chatMessage.msgIdx);

			writer.WritePropertyName("partyType");
			writer.Write(chatMessage.partyType);

			writer.WritePropertyName("partyNum");
			writer.Write(chatMessage.partyNum);

			writer.WritePropertyName("timeStamp");
			writer.Write(chatMessage.timeStamp);

			writer.WritePropertyName("userID");
			writer.Write(chatMessage.userID);

            writer.WritePropertyName("connectId");
            writer.Write(chatMessage.connectId);

            writer.WritePropertyName("sendUserName");
			writer.Write(chatMessage.sendUserName);

			writer.WritePropertyName("targetUserID");
			writer.Write(chatMessage.targetUserID);

            writer.WritePropertyName("targetConnectID");
            writer.Write(chatMessage.targetConnectID);

            writer.WritePropertyName("whisperKind");
            writer.Write(chatMessage.whisperKind);

            writer.WritePropertyName("companyID");
            writer.Write(chatMessage.companyID);

            writer.WritePropertyName("targetUserName");
			writer.Write(chatMessage.targetUserName);

			writer.WritePropertyName("prm");
			if (chatMessage.prm != null) {
				writer.WriteObjectStart ();
				{
					List<string> prmKeys = chatMessage.prm.Keys.ToList ();
					for (int i = 0; i < prmKeys.Count; i++) {
						writer.WritePropertyName(prmKeys[i]);
						writer.Write(chatMessage.prm[prmKeys[i]]);
					}
				}
				writer.WriteObjectEnd ();
			} else {
				writer.Write (null);
			}

			writer.WritePropertyName("partMessageInfos");
			if (chatMessage.partMessageInfos != null) {
				writer.WriteObjectStart ();
				{
					List<string> partMsgKeys = chatMessage.partMessageInfos.Keys.ToList ();
					for (int i = 0; i < partMsgKeys.Count; i++) {
						ChatPartMessageInfo partMessageInfo = chatMessage.partMessageInfos [partMsgKeys [i]];
						writer.WritePropertyName(partMsgKeys[i]);
						writer.WriteObjectStart ();
						{
							writer.WritePropertyName("partMessageType");
							writer.Write(partMessageInfo.partMessageType);

							writer.WritePropertyName("partValues");
							if (partMessageInfo.partValues != null) {
								writer.WriteArrayStart ();
								{
									for (int j = 0; j < partMessageInfo.partValues.Length; j++) {
										writer.Write (partMessageInfo.partValues [j]);
									}
								}
								writer.WriteArrayEnd ();
							} else {
								writer.Write (null);
							}

						}
						writer.WriteObjectEnd ();
					}
				}
				writer.WriteObjectEnd ();
			} else {
				writer.Write (null);
			}
		}
		writer.WriteObjectEnd ();

		return sb.ToString ();
	}

    public static ChatPartMessageInfo GetEquipmentDataChatPartInfo(EquipmentData equipment)
    {
        ChatPartMessageInfo retValue = new ChatPartMessageInfo();
        retValue.partMessageType = (int)ChatDefinition.PartMessageType.ItemInfoType;
        retValue.partValues = new string[2];
        retValue.partValues[0] = "Equipment";
        retValue.partValues[1] = GetEquipmentDataJsonString(equipment);

        return retValue;
    }

    public static string GetEquipmentDataJsonString(EquipmentData equipment)
    {
        string retValue = "";

        try
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);
            writer.WriteObjectStart();
            {
                writer.WritePropertyName("id");
                writer.Write(equipment.id);

                writer.WritePropertyName("index");
                writer.Write(equipment.index);

                writer.WritePropertyName("enhanceLevel");
                writer.Write(equipment.enhanceLevel);

                writer.WritePropertyName("heroId");
                writer.Write(equipment.heroId);

                writer.WritePropertyName("mainStatIndex");
                writer.Write(equipment.mainStatIndex);

                writer.WritePropertyName("prefixStatIndex");
                writer.Write(equipment.prefixStatIndex);

                writer.WritePropertyName("prefixStatValue");
                writer.Write(equipment.prefixStatValue);

                if (equipment.subStat != null && equipment.subStat.Length > 0)
                {
                    writer.WritePropertyName("subStat");
                    writer.WriteArrayStart();
                    {
                        for (int i = 0; i < equipment.subStat.Length; i++)
                        {
                            writer.WriteArrayStart();
                            {
                                for (int j = 0; j < equipment.subStat[i].Length; j++)
                                {
                                    writer.Write(equipment.subStat[i][j]);
                                }
                            }
                            writer.WriteArrayEnd();
                        }
                    }
                    writer.WriteArrayEnd();
                }

                writer.WritePropertyName("isLock");
                writer.Write(equipment.isLock);

            }
            writer.WriteObjectEnd();

            retValue = sb.ToString();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        return retValue;
    }

    public static EquipmentData GetJsonStringToEquipmentData(string equipmentString)
    {
        EquipmentData retValue = new EquipmentData();

        JsonData equipmentJson = JsonMapper.ToObject(equipmentString);

        if (equipmentJson["id"].IsInt)
        {
            retValue.id = (long)(int)equipmentJson["id"];
        }
        else if (equipmentJson["id"].IsLong)
        {
            retValue.id = (long)equipmentJson["id"];
        }

        retValue.index = (int)equipmentJson["index"];
        retValue.enhanceLevel = (int)equipmentJson["enhanceLevel"];

        if (equipmentJson["heroId"].IsInt)
        {
            retValue.heroId = (long)(int)equipmentJson["heroId"];
        }
        else if (equipmentJson["id"].IsLong)
        {
            retValue.heroId = (long)equipmentJson["heroId"];
        }

        retValue.mainStatIndex = (int)equipmentJson["mainStatIndex"];
        retValue.prefixStatIndex = (int)equipmentJson["prefixStatIndex"];
        retValue.prefixStatValue = (float)equipmentJson["prefixStatValue"];

        if (equipmentJson["subStat"] != null)
        {
            retValue.subStat = new float[equipmentJson["subStat"].Count][];
            for (int i = 0; i < equipmentJson["subStat"].Count; i++)
            {
                JsonData subStatJson = equipmentJson["subStat"][i];
                retValue.subStat[i] = new float[subStatJson.Count];
                for (int j = 0; j < subStatJson.Count; j++)
                {
                    retValue.subStat[i][j] = (float)subStatJson[j];
                }
            }
        }
        retValue.isLock = (int)equipmentJson["isLock"];

        return retValue;
    }

    public static ChatPartMessageInfo GetCardDataChatPartInfo(CardData cardItem)
    {
        ChatPartMessageInfo retValue = new ChatPartMessageInfo();
        retValue.partMessageType = (int)ChatDefinition.PartMessageType.ItemInfoType;
        retValue.partValues = new string[2];
        retValue.partValues[0] = "Card";
        retValue.partValues[1] = GetCardDataJsonString(cardItem);

        return retValue;
    }

    public static ChatPartMessageInfo GetCardIndexChatPartInfo(int itemIndex)
    {
        ChatPartMessageInfo retValue = new ChatPartMessageInfo();
        retValue.partMessageType = (int)ChatDefinition.PartMessageType.ItemInfoType;
        retValue.partValues = new string[2];
        retValue.partValues[0] = "Card";
        retValue.partValues[1] = itemIndex.ToString();

        return retValue;
    }

    public static ChatPartMessageInfo GetHeroChallengeChatPartInfo(int heroIndex)
    {
        ChatPartMessageInfo retValue = new ChatPartMessageInfo();
        retValue.partMessageType = (int)ChatDefinition.PartMessageType.HeroChallengeMaxScore;
        retValue.partValues = new string[1];
        retValue.partValues[0] = heroIndex.ToString();

        return retValue;
    }

    public static string GetCardDataJsonString(CardData cardItem)
    {
        string retValue = "";

        try {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);
            writer.WriteObjectStart();
            {
                writer.WritePropertyName("userId");
                writer.Write(cardItem.userId);

                writer.WritePropertyName("heroId");
                writer.Write(cardItem.heroId);

                writer.WritePropertyName("heroIndex");
                writer.Write(cardItem.heroIndex);

                writer.WritePropertyName("id");
                writer.Write(cardItem.id);

                writer.WritePropertyName("index");
                writer.Write(cardItem.index);

                writer.WritePropertyName("level");
                writer.Write(cardItem.level);

                writer.WritePropertyName("exp");
                writer.Write(cardItem.exp);

                writer.WritePropertyName("skillEnhance");
                if (cardItem.skillEnhance != null)
                {
                    writer.WriteObjectStart();
                    {
                        List<string> keys = cardItem.skillEnhance.Keys.ToList();
                        for (int i = 0; i < keys.Count; i++)
                        {
                            writer.WritePropertyName(keys[i]);
                            writer.Write(cardItem.skillEnhance[keys[i]]);
                        }
                    }
                    writer.WriteObjectEnd();
                }
                else
                    writer.Write(null);

                writer.WritePropertyName("isLock");
                writer.Write(cardItem.isLock);

            }
            writer.WriteObjectEnd();

            retValue = sb.ToString();
        } catch (Exception e) {
            Debug.Log(e.ToString());
        }

        return retValue;
    }

    public static CardData GetJsonStringToCardData(string CardItemString)
    {
        CardData retValue = new CardData();

        JsonData CardItemJson = JsonMapper.ToObject(CardItemString);

        if (CardItemJson["heroId"].IsInt)
            retValue.heroId = (long)(int)CardItemJson["heroId"];
        else if (CardItemJson["heroId"].IsLong)
            retValue.heroId = (long)CardItemJson["heroId"];

        retValue.heroIndex = (int)CardItemJson["heroIndex"];

        if (CardItemJson["id"].IsInt)
            retValue.id = (long)(int)CardItemJson["id"];
        else if (CardItemJson["id"].IsLong)
            retValue.id = (long)CardItemJson["id"];
        
        retValue.index = (int)CardItemJson["index"];
        retValue.level = (int)CardItemJson["level"];
        retValue.exp = (int)CardItemJson["exp"];
        
        if(CardItemJson["skillEnhance"] != null)
        {
            var skillLevelJson = CardItemJson["skillEnhance"];
            foreach(DictionaryEntry each in skillLevelJson as IDictionary)
            {
                var key = (string)each.Key;
                var value = (int)each.Value;
                retValue.skillEnhance.Add(key, value);
            }
        }
        
        return retValue;
    }
}
