using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBaseObject
{
	#region Variables

	IScrollObjectInfo _scrollObjInfo = null;
	float _objPosX;
	float _objPosY;

	#endregion

	#region Properties

	public IScrollObjectInfo ScrollObjInfo
	{
		get{ return _scrollObjInfo; }
		set{ _scrollObjInfo = value; }
	}

	public float ObjPosX
	{
		get{ return _objPosX; }
		set{ _objPosX = value; }
	}

	public float ObjPosY
	{
		get{ return _objPosY; }
		set{ _objPosY = value; }
	}

	#endregion

	#region Methods

	public void SetScrollObjectPos(float posX, float posY)
	{
		_objPosX = posX;
		_objPosY = posY;

		_scrollObjInfo.ScrollGameObject.transform.localPosition = new Vector3 (posX, posY, 0f);
	}

	public float GetTopPosY(float moveObjY)
	{
		return _objPosY + moveObjY + (_scrollObjInfo.ObjectHeight * 0.5f);
	}

	public float GetBottomPosY(float moveObjY)
	{
		return _objPosY + moveObjY - (_scrollObjInfo.ObjectHeight * 0.5f);
	}

	#endregion
}
