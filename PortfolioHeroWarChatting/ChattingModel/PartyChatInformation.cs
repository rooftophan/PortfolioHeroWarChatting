using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyChatInformation : PartyBaseChatInformation
{
    #region Variables

    long _partyJoinTimeStamp;
    long _lastConfirmTimeStamp;

    #endregion

    #region Properties

    public long PartyJoinTimeStamp
    {
        get { return _partyJoinTimeStamp; }
        set { _partyJoinTimeStamp = value; }
    }

    public long LastConfirmTimeStamp
    {
        get { return _lastConfirmTimeStamp; }
        set { _lastConfirmTimeStamp = value; }
    }

    #endregion

    #region Methods

    public override void ReleaseChatMessages()
    {
        base.ReleaseChatMessages();
    }

    #endregion
}
