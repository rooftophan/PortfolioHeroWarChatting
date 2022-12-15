using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatWhisperGroupInfo
{
    #region Variables

    ChatWhisperDropDownType _groupType;
    long _partyNum = -1;
    ChatNudgeNode _nudgeNode = new ChatNudgeNode();

    #endregion

    #region Properties

    public ChatWhisperDropDownType GroupType
    {
        get { return _groupType; }
        set { _groupType = value; }
    }

    public long PartyNum
    {
        get { return _partyNum; }
        set { _partyNum = value; }
    }

    public ChatNudgeNode NudgeNode
    {
        get { return _nudgeNode; }
    }

    #endregion
}
