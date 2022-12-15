using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatRequestWebInfo
{
    #region Variables

    string _url;
    string _requestJson;
    Action<string, bool> _onReceiveWeb;

    #endregion

    #region Properties

    public string Url
    {
        get { return _url; }
        set { _url = value; }
    }

    public string RequestJson
    {
        get { return _requestJson; }
        set { _requestJson = value; }
    }

    public Action<string, bool> OnReceiveWeb
    {
        get { return _onReceiveWeb; }
        set { _onReceiveWeb = value; }
    }

    #endregion
}
