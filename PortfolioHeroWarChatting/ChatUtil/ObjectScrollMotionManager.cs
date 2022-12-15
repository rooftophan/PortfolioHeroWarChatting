using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectScrollMotionManager
{
	#region Definitions

	public enum MotionType
	{
		TimeMoveMotion,
		OutLineMoveMotion,
		Acceleration,
	}

	#endregion

	#region Variables

	float _maxSpeedValue = 3000f;

	List<ObjectScrollMotionData> _objectMotionAnis = new List<ObjectScrollMotionData>();

	#endregion

	#region Properties

	public List<ObjectScrollMotionData> ObjectMotionAnis
	{
		get{ return _objectMotionAnis; }
	}

	#endregion

	#region Methods

	public void Update()
	{
		if (_objectMotionAnis.Count == 0)
			return;

		List<ObjectScrollMotionData> delObjectMotionAnis = new List<ObjectScrollMotionData> ();
		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			switch (_objectMotionAnis [i].ObjectMotionType) {
			case MotionType.TimeMoveMotion:
				if (UpdateTimeMoveMotion (_objectMotionAnis [i]) == 1) {
					delObjectMotionAnis.Add (_objectMotionAnis [i]);
				}
				break;
			case MotionType.OutLineMoveMotion:
				if (UpdateOutLineMoveMotion (_objectMotionAnis [i]) == 1) {
					delObjectMotionAnis.Add (_objectMotionAnis [i]);
				}
				break;
			case MotionType.Acceleration:
				if (UpdateAccelerationMotion (_objectMotionAnis [i]) == 1) {
					delObjectMotionAnis.Add (_objectMotionAnis [i]);
				}
				break;
			}
		}

		for (int i = 0; i < delObjectMotionAnis.Count; i++) {
			_objectMotionAnis.Remove (delObjectMotionAnis [i]);
			if (delObjectMotionAnis [i].OnCompletedObjectMotion != null) {
				if(_objectMotionAnis.Count == 0)
					delObjectMotionAnis [i].OnCompletedObjectMotion ();
			}
		}
	}

	int UpdateTimeMoveMotion(ObjectScrollMotionData objectMotionAni)
	{
		int retValue = 0;
		objectMotionAni.CurTime += Time.deltaTime;
		float moveValue = objectMotionAni.SpeedValue * Time.deltaTime;
		if (objectMotionAni.Distance - moveValue <= 0f) {
			moveValue = objectMotionAni.Distance;
			retValue = 1;
		}

		objectMotionAni.Distance -= moveValue;

		float calcX = objectMotionAni.CosA * moveValue;
		float calcY = objectMotionAni.SinA * moveValue;

		if (objectMotionAni.OnChangeMoveMotionValue != null) {
			objectMotionAni.OnChangeMoveMotionValue (objectMotionAni.ObjectMotionType, new Vector2 (calcX, calcY), objectMotionAni.MotionID);
		}

		return retValue;
	}

	int UpdateOutLineMoveMotion(ObjectScrollMotionData objectMotionAni)
	{
		int retValue = 0;
		objectMotionAni.CurTime += Time.deltaTime;
		float moveValue = objectMotionAni.SpeedValue * Time.deltaTime;
		if (objectMotionAni.CurMoveValue + moveValue >= objectMotionAni.Distance) {
			moveValue = objectMotionAni.Distance - objectMotionAni.CurMoveValue;
			retValue = 1;
		}

		objectMotionAni.CurMoveValue += moveValue;

		float calcX = objectMotionAni.CosA * moveValue;
		float calcY = objectMotionAni.SinA * moveValue;

		if (objectMotionAni.OnChangeMoveMotionValue != null) {
			objectMotionAni.OnChangeMoveMotionValue (objectMotionAni.ObjectMotionType, new Vector2 (calcX, calcY), objectMotionAni.MotionID);
		}

		return retValue;
	}

	int UpdateAccelerationMotion(ObjectScrollMotionData objectMotionAni)
	{
		int retValue = 0;
		float moveValue = objectMotionAni.SpeedValue * Time.deltaTime;

		float decreaseValue = 30f;
		if (objectMotionAni.SpeedValue > 2000f) {
			decreaseValue = 90f;
		} else if (objectMotionAni.SpeedValue > 1000f) {
			decreaseValue = 60f;
		}

		if (objectMotionAni.SpeedValue - decreaseValue < 0f) {
			retValue = 1;
		} else {
			objectMotionAni.SpeedValue -= decreaseValue;
		}

		if (objectMotionAni.MoveDirection == 0) { // Left
			moveValue = -moveValue;
		}

		if (objectMotionAni.OnChangeMoveMotionValue != null) {
			float calcX = objectMotionAni.CosA * moveValue;
			float calcY = objectMotionAni.SinA * moveValue;
			objectMotionAni.OnChangeMoveMotionValue (objectMotionAni.ObjectMotionType, new Vector2 (calcX, calcY), objectMotionAni.MotionID);
		}

		return retValue;
	}

	public void ChangeDistanceValue(int motionID, float changeScale)
	{
		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			if(_objectMotionAnis[i].MotionID == motionID){
				_objectMotionAnis [i].Distance = _objectMotionAnis [i].StartDistance + ((_objectMotionAnis [i].StartDistance * changeScale - _objectMotionAnis [i].StartDistance) * 0.1f);
			}
		}
	}

	public void ChangeSpeedValue(int motionID, float changeValue)
	{
		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			if(_objectMotionAnis[i].MotionID == motionID){
				_objectMotionAnis [i].SpeedValue = changeValue;
				break;
			}
		}
	}

	public void AddObjectTimeMovement(float motionTime, Vector3 startPos, Vector3 destPos, Action<MotionType, Vector2, int> OnChangeMoveMotionValue, int motionID = -1, Action onCompletedMotionAni = null)
	{
		if (startPos == destPos) {
			if (_objectMotionAnis.Count == 0 && onCompletedMotionAni != null)
				onCompletedMotionAni ();
			return;
		}

		float curTime = 0f;
		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			if (_objectMotionAnis [i].MotionID == motionID) {
				curTime = _objectMotionAnis [i].CurTime;
				_objectMotionAnis.RemoveAt (i);
				break;
			}
		}

		motionTime -= curTime;

		if (motionTime <= 0f) {
			if (_objectMotionAnis.Count == 0 && onCompletedMotionAni != null)
				onCompletedMotionAni ();
			return;
		}

		float gapX = destPos.x - startPos.x;
		float gapY = destPos.y - startPos.y;
		float distance = Mathf.Sqrt((float)(Mathf.Pow(gapX, 2f) + Mathf.Pow(gapY, 2f)));

		ObjectScrollMotionData inputMotionAni = new ObjectScrollMotionData ();
		inputMotionAni.MotionID = motionID;
		inputMotionAni.SinA = gapY/distance;
		inputMotionAni.CosA = gapX/distance;
		inputMotionAni.Distance = distance;
		inputMotionAni.SpeedValue = distance / motionTime;
		inputMotionAni.ObjectMotionType = MotionType.TimeMoveMotion;
		inputMotionAni.OnChangeMoveMotionValue = OnChangeMoveMotionValue;
		inputMotionAni.OnCompletedObjectMotion = onCompletedMotionAni;

		_objectMotionAnis.Add (inputMotionAni);
	}

	public void AddOutLineMovement(Vector3 startPos, Vector3 destPos, Action<MotionType, Vector2, int> OnChangeMoveMotionValue, int motionID = -1, Action onCompletedMotionAni = null)
	{
		if (startPos == destPos) {
			if (_objectMotionAnis.Count == 0 && onCompletedMotionAni != null)
				onCompletedMotionAni ();
			return;
		}

		float gapX = destPos.x - startPos.x;
		float gapY = destPos.y - startPos.y;
		float distance = Mathf.Sqrt((float)(Mathf.Pow(gapX, 2f) + Mathf.Pow(gapY, 2f)));

		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			if (_objectMotionAnis [i].MotionID == motionID) {
				_objectMotionAnis [i].SinA = gapY/distance;
				_objectMotionAnis [i].CosA = gapX/distance;
				_objectMotionAnis [i].Distance = distance;
				_objectMotionAnis [i].StartDistance = distance;
				_objectMotionAnis [i].CurMoveValue = 0f;
				return;
			}
		}

		ObjectScrollMotionData inputMotionAni = new ObjectScrollMotionData ();
		inputMotionAni.MotionID = motionID;
		inputMotionAni.SinA = gapY/distance;
		inputMotionAni.CosA = gapX/distance;
		inputMotionAni.Distance = distance;
		inputMotionAni.StartDistance = distance;
		float calcSpeedValue = distance / 0.2f;

		if (calcSpeedValue < 500f)
			calcSpeedValue = 500f;
		else if (calcSpeedValue > _maxSpeedValue) {
			calcSpeedValue = _maxSpeedValue;
		}

		inputMotionAni.SpeedValue = calcSpeedValue;
		inputMotionAni.ObjectMotionType = MotionType.OutLineMoveMotion;
		inputMotionAni.OnChangeMoveMotionValue = OnChangeMoveMotionValue;
		inputMotionAni.OnCompletedObjectMotion = onCompletedMotionAni;

		_objectMotionAnis.Add (inputMotionAni);
	}

	public bool AddAccelerationListMove(ObjectScrollManager.ScrollMoveType moveType, float gapTime, float moveDis, Action<MotionType, Vector2, int> OnChangeMoveMotionValue, int motionID = -1, Action onCompletedMotionAni = null)
	{
		if (gapTime == 0f || moveDis == 0f)
			return false;

		float velocity = moveDis / gapTime;

		ObjectScrollMotionData inputMotionAni = new ObjectScrollMotionData ();

		if (velocity > 0f) { // Right : 1
			inputMotionAni.MoveDirection = 1;
		} else { // Left : 0
			inputMotionAni.MoveDirection = 0;
		}

		inputMotionAni.MotionID = motionID;
		inputMotionAni.SpeedValue = Mathf.Abs (velocity);
		if (inputMotionAni.SpeedValue > _maxSpeedValue) {
			inputMotionAni.SpeedValue = _maxSpeedValue;
		} 

		switch (moveType) {
		case ObjectScrollManager.ScrollMoveType.HorizontalScroll:
			inputMotionAni.SinA = 0f;
			inputMotionAni.CosA = 1f;
			break;
		case ObjectScrollManager.ScrollMoveType.VerticalScroll:
			inputMotionAni.SinA = 1f;
			inputMotionAni.CosA = 0f;
			break;
		}

		inputMotionAni.ObjectMotionType = MotionType.Acceleration;

		inputMotionAni.OnChangeMoveMotionValue = OnChangeMoveMotionValue;
		inputMotionAni.OnCompletedObjectMotion = onCompletedMotionAni;

		_objectMotionAnis.Add (inputMotionAni);

		return true;
	}

	public void ReleaseObjectMovement(int motionID, bool isCallComplete = true)
	{
		if (_objectMotionAnis.Count == 0)
			return;

		for (int i = 0; i < _objectMotionAnis.Count; i++) {
			if (_objectMotionAnis [i].MotionID == motionID) {
				if (isCallComplete) {
					if (_objectMotionAnis [i].OnCompletedObjectMotion != null) {
						_objectMotionAnis [i].OnCompletedObjectMotion ();
					}
				}

				_objectMotionAnis.RemoveAt (i);
				break;
			}
		}
	}

	public void ReleaseAllMotion()
	{
		if (_objectMotionAnis.Count == 0)
			return;

		_objectMotionAnis.Clear ();
	}

	public bool CheckObjectMotionState()
	{
		if (_objectMotionAnis.Count > 0)
			return true;

		return false;
	}

	#endregion
}
