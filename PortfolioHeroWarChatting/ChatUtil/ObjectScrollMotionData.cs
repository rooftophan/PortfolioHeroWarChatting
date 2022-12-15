using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectScrollMotionData
{
	#region Variables

	ObjectScrollMotionManager.MotionType _objectMotionType;

	int _motionID;
	float _startScale;
	float _destScale;
	float _gapScale;
	float _scaleSpeedValue;
	float _speedValue;
	float _curTime = 0f;
	bool _isPlusState = false;

	float _sinA;
	float _cosA;
	float _distance;
	float _curMoveValue = 0f;

	float _startDistance;

	Action<ObjectScrollMotionManager.MotionType, float> _onChangeObjectMotionValue;
	Action<ObjectScrollMotionManager.MotionType, Vector2, int> _onChangeMoveMotionValue;
	Action _onCompletedObjectMotion;

	int _moveDirection;

	#endregion

	#region Properties

	public ObjectScrollMotionManager.MotionType ObjectMotionType
	{
		get{ return _objectMotionType; }
		set{ _objectMotionType = value; }
	}

	public int MotionID
	{
		get{ return _motionID; }
		set{ _motionID = value; }
	}

	public float StartScale
	{
		get{ return _startScale; }
		set{ _startScale = value; }
	}

	public float DestScale
	{
		get{ return _destScale; }
		set{ _destScale = value; }
	}

	public float GapScale
	{
		get{ return _gapScale; }
		set{ _gapScale = value; }
	}

	public float SpeedValue
	{
		get{ return _speedValue; }
		set{
			_speedValue = value;
		}
	}

	public float ScaleSpeedValue
	{
		get{ return _scaleSpeedValue; }
		set{ _scaleSpeedValue = value; }
	}

	public bool IsPlusState
	{
		get{ return _isPlusState; }
		set{ _isPlusState = value; }
	}

	public float CurTime
	{
		get{ return _curTime; }
		set{ _curTime = value; }
	}

	public float SinA
	{
		get{ return _sinA; }
		set{ _sinA = value; }
	}

	public float CosA
	{
		get{ return _cosA; }
		set{ _cosA = value; }
	}

	public float Distance
	{
		get{ return _distance; }
		set{ _distance = value; }
	}

	public float StartDistance
	{
		get{ return _startDistance; }
		set{ _startDistance = value; }
	}

	public float CurMoveValue
	{
		get{ return _curMoveValue; }
		set{ _curMoveValue = value; }
	}

	public int MoveDirection
	{
		get{ return _moveDirection; }
		set{ _moveDirection = value; }
	}

	public Action<ObjectScrollMotionManager.MotionType, float> OnChangeObjectMotionValue
	{
		get{ return _onChangeObjectMotionValue; }
		set{ _onChangeObjectMotionValue = value; }
	}

	public Action<ObjectScrollMotionManager.MotionType, Vector2, int> OnChangeMoveMotionValue
	{
		get{ return _onChangeMoveMotionValue; }
		set{ _onChangeMoveMotionValue = value; }
	}

	public Action OnCompletedObjectMotion
	{
		get{ return _onCompletedObjectMotion; }
		set{ _onCompletedObjectMotion = value; }
	}

	#endregion
}
