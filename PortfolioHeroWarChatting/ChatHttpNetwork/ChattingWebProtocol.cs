using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using UnityEngine.Networking;

public class ChattingWebProtocol<TReq, TRes> where TReq : ChatRequestParam where TRes : ChatResponseParam
{
	public ChattingWebProtocol(DataContext data, Action<TRes> onSuccessAction, Action<TRes> onFailAction)
	{
		_data = data;
		_onSuccessChatHttp = onSuccessAction;
		_onFailChatHttp = onFailAction;
	}

	#region Variables

	protected DataContext _data;

	protected Action<TRes> _onSuccessChatHttp;
	protected Action<TRes> _onFailChatHttp;

	protected TRes _resParam = null;

	#endregion

	#region Properties

	protected virtual string Url { get{ return ""; } }

	#endregion

	#region Methods

	protected virtual string GetRequestJsonString()
	{
		return "";
	}

	protected virtual void SetResponseJsonData(string result)
	{
		
	}

    public virtual void RequestHttpWeb()
    {
        SetRequestData();

        try {
            string json = GetRequestJsonString();

            ChattingController.Instance.RequestChatUnityWeb(Url, json, OnReceiveUnityWeb);
        } catch (Exception e) {
            Debug.Log(string.Format("RequestHttpWeb {0}", e.ToString()));
            if (_onFailChatHttp != null)
                _onFailChatHttp(_resParam);
        }
    }

    void OnReceiveUnityWeb(string receiveText, bool isSuccess)
    {
        if(!isSuccess) {
            Debug.Log(string.Format("OnReceiveUnityWeb Error : {0}", receiveText));
            if (_onFailChatHttp != null)
                _onFailChatHttp(_resParam);
        }

        try {
            SetResponseJsonData(receiveText);

            if (_resParam.result_code == (int)ChatMessageResultCode.Success) {
                if (_onSuccessChatHttp != null) {
                    _onSuccessChatHttp(_resParam);
                }

                OnSuccess(_resParam);
            } else {
                if (_onFailChatHttp != null) {
                    _onFailChatHttp(_resParam);
                }

                OnFail(_resParam);
            }
        } catch (Exception e) {
            Debug.Log(string.Format("RequestHttpWeb {0}", e.ToString()));
            if (_onFailChatHttp != null)
                _onFailChatHttp(_resParam);
        }
    }

    protected virtual void SetRequestData()
	{
	}

	protected virtual void OnSuccess(TRes resParam)
	{

	}

	protected virtual void OnFail(TRes resParam)
	{

	}

	#endregion
}
