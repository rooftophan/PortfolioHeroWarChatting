using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ObjectScrollManager : MonoBehaviour 
{
	#region Definitions

	public enum ScrollMoveType
	{
		HorizontalScroll 	= 1,
		VerticalScroll		= 2,
	}

	public enum MoveDirection
	{
		None 				= 0,
		PlusMove			= 1,
		MinusMove			= 2,
	}

    #endregion

    #region Serialze Variables

#pragma warning disable 649

    [SerializeField] Vector2 _referenceResolution = default(Vector2);
	[SerializeField] Transform _objListTrans = default(Transform);
	[SerializeField] ScrollMoveType _moveType = default(ScrollMoveType);
	[SerializeField] Camera _scrollCamera = default(Camera);
	[SerializeField] BoxCollider _scrollTouchArea = default(BoxCollider);
	[SerializeField] float _topGapY = default(float);
	[SerializeField] float _bottomGapY = default(float);
	[SerializeField] float _objGapValue = default(float);
	[SerializeField] RectTransform _viewObjMaskRectTrans = default(RectTransform);
    [SerializeField] bool _isResizeViewRect = default(bool);

#pragma warning restore 649

    #endregion

    #region Variables

    bool _isUpdate = true;

	protected ScreenTouchValue[] _screenTouchValues = new ScreenTouchValue[2];
	private float _firstStartX;
	private float _firstStartY;
	private int _touchCount;

	Action<float, float> _onScrollObject = null;
	Rect _scrollAreaRect;
	float _startRectY = -10000f;
	float _curStartRectY = 0f;
	float _endRectY;

	List<ScrollBaseObject> _scrollObjectList = new List<ScrollBaseObject>();

	float _startViewRectTop;
	float _startBoxCollideHeight;
	bool _isTouch = true;

	ObjectScrollMotionManager _objectMotionManager = new ObjectScrollMotionManager();

	int _outLineMotionID = 1;
	int _scrollMotionID = 2;

	float _widthRatio = 1f;
	float _heightRatio = 1f;

	float _startTouchTime;
	Vector2 _startTouchValue;

	bool _isScrollMoveMotion = false;
	bool _isCollideOutLine = false;

	int _curFocusScrollIndex = -1;
	MoveDirection _moveDir = MoveDirection.None;

    Func<float, float, bool> _onValidTouchArea = null;

    bool _isBottomScrollState = true;
    Action<bool> _onBottomScrollState;

	#endregion

	#region Properties

	public Transform ObjListTrans
	{
		get{ return _objListTrans; }
	}

	public Action<float, float> OnScrollObject
	{
		get{ return _onScrollObject; }
		set{ _onScrollObject = value; }
	}

	public List<ScrollBaseObject> ScrollObjectList
	{
		get{ return _scrollObjectList; }
	}

	public bool IsTouch
	{
		get{ return _isTouch; }
		set{ _isTouch = value; }
	}

    public Func<float, float, bool> OnValidTouchArea
    {
        get { return _onValidTouchArea; }
        set { _onValidTouchArea = value; }
    }

    public Action<bool> OnBottomScrollState
    {
        get { return _onBottomScrollState; }
        set { _onBottomScrollState = value; }
    }

    #endregion

    #region MonoBehaviour Methods

	void Start()
	{
		InitScrollData ();
	}

	void OnEnable()
	{
		InitTouchValue ();

		_isUpdate = true;
		StartCoroutine (UpdateObjScroll ());
	}

	void OnDisable()
	{
		StopCoroutine (UpdateObjScroll ());
		_isUpdate = false;
	}

	#endregion

	#region Methods

	public void InitScrollData()
	{
		if (_startRectY != -10000f)
			return;

        bool isSafeArea = false;
        if(UISafeAreaPanel.GetSafeArea().x > 0f) {
            isSafeArea = true;
        }

		_widthRatio = _referenceResolution.x / _scrollCamera.pixelWidth;
        if(isSafeArea) {
            _widthRatio *= 1.1f;
        }
		float calcHeight = _scrollCamera.pixelHeight * _widthRatio;
		_heightRatio = calcHeight / _scrollCamera.pixelHeight;
		float gapHeight = calcHeight - _referenceResolution.y;
        if(_isResizeViewRect) {
            _scrollTouchArea.size = new Vector3(_scrollTouchArea.size.x, _scrollTouchArea.size.y + gapHeight, _scrollTouchArea.size.z);
        }

		_startRectY = -(_scrollTouchArea.size.y * 0.5f);
		_endRectY = _scrollTouchArea.size.y * 0.5f;

		_curStartRectY = 0f;

		Vector2 touchScrPos = _scrollCamera.WorldToScreenPoint (_scrollTouchArea.transform.position);
		float realWidth = _scrollTouchArea.size.x / _widthRatio;
		float realHeight = _scrollTouchArea.size.y / _heightRatio;
		float halfWidth = realWidth * 0.5f;
		float halfHeight = realHeight * 0.5f;
		_scrollAreaRect = new Rect (touchScrPos.x - halfWidth, touchScrPos.y - halfHeight, 
			realWidth, realHeight);

		_startViewRectTop = _viewObjMaskRectTrans.offsetMax.y;
		_startBoxCollideHeight = _scrollTouchArea.size.y;
	}

    public void InitPartyScrollData()
    {
        if (_startRectY != -10000f)
            return;

        _widthRatio = _referenceResolution.x / _scrollCamera.pixelWidth;

        float calcHeight = _scrollCamera.pixelHeight * _widthRatio;
        _heightRatio = calcHeight / _scrollCamera.pixelHeight;
        float gapHeight = calcHeight - _referenceResolution.y;
        if (_isResizeViewRect) {
            _scrollTouchArea.size = new Vector3(_scrollTouchArea.size.x, _scrollTouchArea.size.y + gapHeight, _scrollTouchArea.size.z);
        }

        _startRectY = -(_scrollTouchArea.size.y * 0.5f);
        _endRectY = _scrollTouchArea.size.y * 0.5f;

        _curStartRectY = 0f;

        Vector2 touchScrPos = _scrollCamera.WorldToScreenPoint(_scrollTouchArea.transform.position);
        float realWidth = _scrollTouchArea.size.x / _widthRatio;
        float realHeight = _scrollTouchArea.size.y / _heightRatio;
        float halfWidth = realWidth * 0.5f;
        float halfHeight = realHeight * 0.5f;
        _scrollAreaRect = new Rect(touchScrPos.x - halfWidth, touchScrPos.y - halfHeight,
            realWidth, realHeight);

        _startViewRectTop = _viewObjMaskRectTrans.offsetMax.y;
        _startBoxCollideHeight = _scrollTouchArea.size.y;
    }

    public void SetRectGapHeightValue(float gapValue)
	{
		if (gapValue == 0f)
			return;
		
		_viewObjMaskRectTrans.offsetMax = new Vector2 (_viewObjMaskRectTrans.offsetMax.x, _viewObjMaskRectTrans.offsetMax.y - gapValue);
		_scrollTouchArea.size = new Vector3 (_scrollTouchArea.size.x, _scrollTouchArea.size.y - gapValue, _scrollTouchArea.size.z);

		_startRectY = -(_scrollTouchArea.size.y * 0.5f);
		_endRectY = _scrollTouchArea.size.y * 0.5f;
    }

	public void ReleaseScrollData(bool isResize)
	{
		if (isResize) {
			if (_viewObjMaskRectTrans != null && _viewObjMaskRectTrans.offsetMax.y != _startViewRectTop) {
				_viewObjMaskRectTrans.offsetMax = new Vector2 (_viewObjMaskRectTrans.offsetMax.x, _startViewRectTop);
				_scrollTouchArea.size = new Vector3 (_scrollTouchArea.size.x, _startBoxCollideHeight, _scrollTouchArea.size.z);

				_startRectY = -(_scrollTouchArea.size.y * 0.5f);
				_endRectY = _scrollTouchArea.size.y * 0.5f;
			}
		}

		_scrollObjectList.Clear ();

        if(_objListTrans != null)
		    _objListTrans.localPosition = Vector3.zero;

		InitTouchValue ();

		ReleaseAllScrollMotion ();

        if (!_isBottomScrollState) {
            _isBottomScrollState = true;
            if (_onBottomScrollState != null) {
                _onBottomScrollState(true);
            }
        }
    }

    public void SetStartListPos()
    {
        _objListTrans.localPosition = Vector3.zero;
    }

	void UpdateEditor()
	{
        if (Input.GetMouseButtonDown (0) && _screenTouchValues [0].touchID == -1) {
            _firstStartX = Input.mousePosition.x;
			_firstStartY = Input.mousePosition.y;

			if (!CheckValidTouchArea(_firstStartX, _firstStartY)) {
				return;
			}

			_screenTouchValues [0].touchID = 0;
			_screenTouchValues [0].curPosition = Input.mousePosition;
			_screenTouchValues [0].prePosition = Input.mousePosition;
			_screenTouchValues [0].firstDragState = false;

			if (_isScrollMoveMotion) {
				ReleaseScrollMoveMotion ();
			}

		} else if (Input.GetMouseButton (0) && _screenTouchValues [0].touchID == 0) {
			_screenTouchValues [0].prePosition = _screenTouchValues [0].curPosition;
			_screenTouchValues [0].curPosition = Input.mousePosition;

			if (_screenTouchValues [0].firstDragState) {

				MoveScroll (Input.mousePosition, _screenTouchValues [0].curPosition.x - _screenTouchValues [0].prePosition.x, _screenTouchValues [0].curPosition.y - _screenTouchValues [0].prePosition.y);
			} else {
				if (Mathf.Abs (_firstStartX - Input.mousePosition.x) > (6f) || Mathf.Abs (_firstStartY - Input.mousePosition.y) > (6f)) {
					SetFirstDragData (Input.mousePosition);
				}
			}
		} else if (Input.GetMouseButtonUp (0) && _screenTouchValues [0].touchID == 0) {
			_screenTouchValues [0].touchID = -1;

			int releaseIndex = -1;
			if (_curFocusScrollIndex != -1) {
				releaseIndex = _curFocusScrollIndex;
			}

			bool isOutLineMotion = false;

            switch (_moveType) {
		    case ScrollMoveType.HorizontalScroll:
	            isOutLineMotion = CheckHorizontalScrollMotion ();
				break;
		    case ScrollMoveType.VerticalScroll:
		        isOutLineMotion = CheckVerticalScrollMotion ();
				break;
			}

			if (!isOutLineMotion && _screenTouchValues [0].firstDragState) {
				if (releaseIndex != -1) {
					CheckMoveScrollMotion (releaseIndex, Input.mousePosition);
				}
			}
		}
	}

	void UpdateDevice()
	{
		if (Input.touchCount > 0) {
			for (int i = 0; i < Input.touchCount; i++) {
				Touch curTouch = Input.GetTouch(i);

				if (curTouch.phase == TouchPhase.Began) {
					if (!CheckValidTouchArea(curTouch.position.x, curTouch.position.y)) {
						return;
					}

					if (_touchCount == 0) {
						_firstStartX = curTouch.position.x;
						_firstStartY = curTouch.position.y;
					}

					if (_screenTouchValues [0].touchID == -1) {
						_touchCount++;

						_screenTouchValues [0].touchID = curTouch.fingerId;
						_screenTouchValues [0].curPosition = curTouch.position;
						_screenTouchValues [0].prePosition = curTouch.position;
						_screenTouchValues [0].firstDragState = false;

						if (_isScrollMoveMotion) {
							ReleaseScrollMoveMotion ();
						}
					} else if (_screenTouchValues [1].touchID == -1) {
						_touchCount++;

						_screenTouchValues [1].touchID = curTouch.fingerId;
						_screenTouchValues [1].curPosition = curTouch.position;
						_screenTouchValues [1].prePosition = curTouch.position;
						_screenTouchValues [1].firstDragState = false;

						if (_isScrollMoveMotion) {
							ReleaseScrollMoveMotion ();
						}
					}
				} else if (curTouch.phase == TouchPhase.Moved) {
					if (_screenTouchValues [0].touchID == curTouch.fingerId) {
						_screenTouchValues [0].prePosition = _screenTouchValues [0].curPosition;
						_screenTouchValues [0].curPosition = curTouch.position;

						if (_screenTouchValues [0].firstDragState) {

							MoveScroll (curTouch.position, _screenTouchValues [0].curPosition.x - _screenTouchValues [0].prePosition.x, _screenTouchValues [0].curPosition.y - _screenTouchValues [0].prePosition.y, 0);
						} else {
							if (Mathf.Abs (_firstStartX - curTouch.position.x) > 6 || Mathf.Abs (_firstStartY - curTouch.position.y) > 6) {
								SetFirstDragData (curTouch.position, 0);
							}
						}
					} else if (_screenTouchValues [1].touchID == curTouch.fingerId) {
						_screenTouchValues [1].prePosition = _screenTouchValues [1].curPosition;
						_screenTouchValues [1].curPosition = curTouch.position;

						if (_screenTouchValues [1].firstDragState) {

							MoveScroll (curTouch.position, _screenTouchValues [1].curPosition.x - _screenTouchValues [1].prePosition.x, _screenTouchValues [1].curPosition.y - _screenTouchValues [1].prePosition.y, 1);
						} else {
							if (Mathf.Abs (_firstStartX - curTouch.position.x) > 6 || Mathf.Abs (_firstStartY - curTouch.position.y) > 6) {
								SetFirstDragData (curTouch.position, 1);
							}
						}
					}
				} else if (curTouch.phase == TouchPhase.Ended) {
					int releaseIndex = -1;

                    bool isScrollMotion = false;
					if (_screenTouchValues [0].touchID == curTouch.fingerId) {
						_screenTouchValues [0].touchID = -1;
                        if(_screenTouchValues [0].firstDragState)
                            isScrollMotion = true;
						_touchCount--;
						if (_curFocusScrollIndex == 0) {
							releaseIndex = 0;
							_curFocusScrollIndex = -1;
						}
					} else if (_screenTouchValues [1].touchID == curTouch.fingerId) {
						_screenTouchValues [1].touchID = -1;
                        if(_screenTouchValues [1].firstDragState)
                            isScrollMotion = true;
						_touchCount--;
						if (_curFocusScrollIndex == 1) {
							releaseIndex = 1;
							_curFocusScrollIndex = -1;
						}
					}

					bool isOutLineMotion = false;

					if (_touchCount == 0) {
						switch (_moveType) {
						case ScrollMoveType.HorizontalScroll:
							isOutLineMotion = CheckHorizontalScrollMotion ();
							break;
						case ScrollMoveType.VerticalScroll:
							isOutLineMotion = CheckVerticalScrollMotion ();
							break;
						}
					}

					if (!isOutLineMotion && isScrollMotion) {
						if (releaseIndex != -1) {
							CheckMoveScrollMotion (releaseIndex, curTouch.position);
						}
					}

				} else if (curTouch.phase == TouchPhase.Canceled) {
					int releaseIndex = -1;
                    bool isScrollMotion = false;
					if (_screenTouchValues [0].touchID == curTouch.fingerId) {
						_screenTouchValues [0].touchID = -1;
                        if(_screenTouchValues [0].firstDragState)
                            isScrollMotion = true;
						_touchCount--;
						if (_curFocusScrollIndex == 0) {
							releaseIndex = 0;
							_curFocusScrollIndex = -1;
						}
					} else if (_screenTouchValues [1].touchID == curTouch.fingerId) {
						_screenTouchValues [1].touchID = -1;
                        if(_screenTouchValues [1].firstDragState)
                            isScrollMotion = true;
						_touchCount--;
						if (_curFocusScrollIndex == 1) {
							releaseIndex = 1;
							_curFocusScrollIndex = -1;
						}
					}

					bool isOutLineMotion = false;

					if (_touchCount == 0) {
						switch (_moveType) {
						case ScrollMoveType.HorizontalScroll:
							isOutLineMotion = CheckHorizontalScrollMotion ();
							break;
						case ScrollMoveType.VerticalScroll:
							isOutLineMotion = CheckVerticalScrollMotion ();
							break;
						}
					}

                    if (!isOutLineMotion && isScrollMotion) {
						if (releaseIndex != -1) {
							CheckMoveScrollMotion (releaseIndex, curTouch.position);
						}
					}
				}
			}
		}
	}

	public void InitTouchValue()
	{
		for (int i = 0; i < _screenTouchValues.Length; i++) {
			_screenTouchValues [i] = new ScreenTouchValue ();
			_screenTouchValues [i].touchID = -1;
			_screenTouchValues [i].curPosition = Vector3.zero;
			_screenTouchValues [i].prePosition = Vector3.zero;
			_screenTouchValues [i].firstDragState = false;
		}

		_firstStartX = 0f;
		_firstStartY = 0f;
		_touchCount = 0;
		_curFocusScrollIndex = -1;
	}

	public void ResetTouchValue()
	{
		if (_screenTouchValues == null || _screenTouchValues.Length == 0)
			return;

		for (int i = 0; i < _screenTouchValues.Length; i++) {
			_screenTouchValues [i].touchID = -1;
			_screenTouchValues [i].curPosition = Vector3.zero;
			_screenTouchValues [i].prePosition = Vector3.zero;
			_screenTouchValues [i].firstDragState = false;
		}

		_firstStartX = 0f;
		_firstStartY = 0f;
		_touchCount = 0;
		_curFocusScrollIndex = -1;
	}

	bool CheckValidTouchArea(float posX, float posY)
	{
		if ((_scrollAreaRect.xMin <= posX && _scrollAreaRect.xMax >= posX) &&
		   (_scrollAreaRect.yMin <= posY && _scrollAreaRect.yMax >= posY)) {

            if (_onValidTouchArea != null) {
                if(_onValidTouchArea(posX, posY)) {
                    return true;
                } else {
                    return false;
                }
            }

            return true;
		}

		return false;
	}

	protected void MoveScroll(Vector2 touchPos, float moveX, float moveY, int focusIndex = 0)
	{
		int isPlusState = -1; // 0 : Minus, 1 : Plus
		switch (_moveType) {
		case ScrollMoveType.HorizontalScroll:
			moveY = 0f;

			if (moveX != 0f) {
				moveX *= _widthRatio;
				if (moveX < 0f) {
					isPlusState = 0;
				} else {
					isPlusState = 1;
				}
			} else {
				SetFirstDragData (touchPos);
				_moveDir = MoveDirection.None;
			}

			break;
		case ScrollMoveType.VerticalScroll:
			moveX = 0f;

			if (moveY != 0f) {
				moveY *= _heightRatio;
				if (moveY < 0f) {
					isPlusState = 0;
				} else {
					isPlusState = 1;
				}
			} else {
				SetFirstDragData (touchPos);
				_moveDir = MoveDirection.None;
			}
			break;
		}

		if (isPlusState != -1) {
			switch (_moveDir) {
			case MoveDirection.None:
				if (isPlusState == 0) {
					_moveDir = MoveDirection.MinusMove;
				} else if (isPlusState == 1){
					_moveDir = MoveDirection.PlusMove;
				}
				SetFirstDragData (touchPos);
				break;
			case MoveDirection.PlusMove:
				if (isPlusState == 0) {
					_moveDir = MoveDirection.MinusMove;
					SetFirstDragData (touchPos);
				}
				break;
			case MoveDirection.MinusMove:
				if (isPlusState == 1) {
					_moveDir = MoveDirection.PlusMove;
					SetFirstDragData (touchPos);
				}
				break;
			}
		}

        SetObjListTransPosition(new Vector3(GetObjCalcMovePosX(moveX), GetObjCalcMovePosY(moveY), _objListTrans.localPosition.z));

        if (_onScrollObject != null) {
			_onScrollObject (moveX, moveY);
		}
	}

    void SetObjListTransPosition(Vector3 listTransPos)
    {
        _objListTrans.localPosition = listTransPos;

        float bottomRectY = _startRectY + _bottomGapY;

        if (_curStartRectY <= bottomRectY + 1f) {
            float lastObjY = _scrollObjectList[_scrollObjectList.Count - 1].GetBottomPosY(_objListTrans.localPosition.y);
            float gapY = 80f;
            if (lastObjY < bottomRectY - gapY) {
                if (_isBottomScrollState) {
                    if (_onBottomScrollState != null) {
                        _onBottomScrollState(false);
                    }
                    _isBottomScrollState = false;
                }
            } else {
                if (!_isBottomScrollState) {
                    if (_onBottomScrollState != null) {
                        _onBottomScrollState(true);
                    }
                    _isBottomScrollState = true;
                }
            }
        }
    }

	float GetObjCalcMovePosX(float moveX)
	{
		return _objListTrans.localPosition.x + moveX;
	}

	float GetObjCalcMovePosY(float moveY)
	{
		if (moveY > 0f) {
			float compareObjY = _scrollObjectList [_scrollObjectList.Count - 1].GetBottomPosY (_objListTrans.localPosition.y);
			if (compareObjY + moveY > _curStartRectY) {
				moveY = moveY * 0.5f;
			}
		} else if(moveY < 0f){
			float compareObjY = _scrollObjectList [0].GetTopPosY (_objListTrans.localPosition.y);
			if (compareObjY + moveY < _endRectY - _topGapY) {
				moveY = moveY * 0.5f;
			}
		}

		return _objListTrans.localPosition.y + moveY;
	}

	bool CheckHorizontalScrollMotion()
	{
		return false;
	}

	bool CheckVerticalScrollMotion()
	{
		bool retValue = false;
		float compareLastObjY = _scrollObjectList [_scrollObjectList.Count - 1].GetBottomPosY (_objListTrans.localPosition.y);
		if (compareLastObjY > _curStartRectY) {
			float gapValue = _curStartRectY - compareLastObjY;
			Vector3 startPos = new Vector3 (0f, _objListTrans.localPosition.y, 0f);
			Vector3 destPos = new Vector3 (0f, _objListTrans.localPosition.y + gapValue, 0f);

			_objectMotionManager.AddOutLineMovement (startPos, destPos, OnChangeMoveMotionValue, _outLineMotionID, OnCompletedOutLineMotionAni);

			retValue = true;
		} else {
			float compareFirstObjY = _scrollObjectList [0].GetTopPosY (_objListTrans.localPosition.y);
			if (compareFirstObjY < _endRectY - _topGapY) {
				float gapValue = (_endRectY - _topGapY) - compareFirstObjY;
				Vector3 startPos = new Vector3 (0f, _objListTrans.localPosition.y, 0f);
				Vector3 destPos = new Vector3 (0f, _objListTrans.localPosition.y + gapValue, 0f);

				_objectMotionManager.AddOutLineMovement (startPos, destPos, OnChangeMoveMotionValue, _outLineMotionID, OnCompletedOutLineMotionAni);

				retValue = true;
			}
		}

		return retValue;
	}

	bool CheckVerticalOutLinePos()
	{
		bool retValue = false;
		float compareLastObjY = _scrollObjectList [_scrollObjectList.Count - 1].GetBottomPosY (_objListTrans.localPosition.y);
		if (compareLastObjY > _curStartRectY) {
			retValue = true;
		} else {
			float compareFirstObjY = _scrollObjectList [0].GetTopPosY (_objListTrans.localPosition.y);
			if (compareFirstObjY < _endRectY - _topGapY) {
				retValue = true;
			}
		}

		return retValue;
	}

	void CheckMoveScrollMotion(int focusIndex, Vector2 posValue)
	{
		float gapTime = Time.time - _startTouchTime;
		float gapMove = 0f;

		switch (_moveType) {
		case ScrollMoveType.HorizontalScroll:
			gapMove = posValue.x - _startTouchValue.x;
			break;
		case ScrollMoveType.VerticalScroll:
			gapMove = posValue.y - _startTouchValue.y;
			break;
		}

		if (_objectMotionManager.AddAccelerationListMove (_moveType, gapTime, gapMove, OnChangeMoveMotionValue, _scrollMotionID, OnCompletedMoveMotionScroll)) {
			_isScrollMoveMotion = true;
		}
	}

	public void AddScrollObject(IScrollObjectInfo scrollObjInfo, bool isBottomScrollObj = false)
	{
		ScrollBaseObject scrollObject = new ScrollBaseObject ();
		scrollObject.ScrollObjInfo = scrollObjInfo;

		scrollObject.ScrollObjInfo.ScrollGameObject.transform.SetParent (_objListTrans);
		scrollObject.ScrollObjInfo.ScrollGameObject.transform.localScale = Vector3.one;

		float calcPosX = 0f;
		float calcPosY = 0f;
        if (_scrollObjectList.Count == 0) {
            switch (_moveType) {
                case ScrollMoveType.HorizontalScroll:
                    break;
                case ScrollMoveType.VerticalScroll: {
                        calcPosY = _endRectY - _topGapY - (scrollObject.ScrollObjInfo.ObjectHeight * 0.5f);
                        _curStartRectY = calcPosY - (scrollObject.ScrollObjInfo.ObjectHeight * 0.5f);
                    }
                    break;
            }
        } else {
            ScrollBaseObject lastScrollObject = _scrollObjectList[_scrollObjectList.Count - 1];

            switch (_moveType) {
                case ScrollMoveType.HorizontalScroll:
                    break;
                case ScrollMoveType.VerticalScroll: {
                        calcPosY = lastScrollObject.ObjPosY - (lastScrollObject.ScrollObjInfo.ObjectHeight * 0.5f) - _objGapValue - (scrollObject.ScrollObjInfo.ObjectHeight * 0.5f);
                        _curStartRectY = calcPosY - (scrollObject.ScrollObjInfo.ObjectHeight * 0.5f);
                        float bottomRectY = _startRectY + _bottomGapY;
                        if (_curStartRectY < bottomRectY) {
                            _curStartRectY = bottomRectY;
                        }

                        if(_isBottomScrollState || isBottomScrollObj) {
                            float curBottomObjPosY = calcPosY - scrollObject.ScrollObjInfo.ObjectHeight * 0.5f + _objListTrans.transform.localPosition.y;

                            if (curBottomObjPosY < bottomRectY) {
                                float gapRectYValue = bottomRectY - curBottomObjPosY;
                                _objListTrans.transform.localPosition = new Vector3(_objListTrans.transform.localPosition.x,
                                    _objListTrans.transform.localPosition.y + gapRectYValue,
                                    _objListTrans.transform.localPosition.z);
                            }

                            if (!_isBottomScrollState) {
                                if (_onBottomScrollState != null) {
                                    _onBottomScrollState(true);
                                }
                                _isBottomScrollState = true;
                            }
                        }
                    }
                    break;
            }
        }

        scrollObject.SetScrollObjectPos (calcPosX, calcPosY);

		_scrollObjectList.Add (scrollObject);
	}

    public void MoveBottomScrollObj()
    {
        if (_scrollObjectList == null || _scrollObjectList.Count == 0)
            return;

        ScrollBaseObject lastScrollObject = _scrollObjectList[_scrollObjectList.Count - 1];
        float curBottomObjPosY = lastScrollObject.GetBottomPosY(_objListTrans.localPosition.y);

        float bottomRectY = _startRectY + _bottomGapY;
        if (curBottomObjPosY < bottomRectY) {
            float gapRectYValue = bottomRectY - curBottomObjPosY;
            _objListTrans.transform.localPosition = new Vector3(_objListTrans.transform.localPosition.x,
                _objListTrans.transform.localPosition.y + gapRectYValue,
                _objListTrans.transform.localPosition.z);
        }

        if (_onBottomScrollState != null) {
            _onBottomScrollState(true);
        }
        _isBottomScrollState = true;

        if (_isScrollMoveMotion) {
            ReleaseScrollMoveMotion();
        }
    }

	public ScrollBaseObject GetScrollObj(int objIndex)
	{
		if (_scrollObjectList.Count <= objIndex)
			return null;

		return _scrollObjectList [objIndex];
	}

	public void RemoveScrollObj(int objIndex)
	{
		if (_scrollObjectList.Count > objIndex) {
			_scrollObjectList.RemoveAt (objIndex);
		}
	}

    public int GetScrollObjCount()
    {
        return _scrollObjectList.Count;
    }

	void SetFirstDragData(Vector2 touchValue, int focusIndex = 0)
	{
		if (focusIndex == 0) {
			if (_screenTouchValues [1].firstDragState) {
				_screenTouchValues [1].firstDragState = false;
			}
		} else {
			if (_screenTouchValues [0].firstDragState) {
				_screenTouchValues [0].firstDragState = false;
			}
		}

		_screenTouchValues[focusIndex].firstDragState = true;

		SetStartDragTouchValues (touchValue);

		_curFocusScrollIndex = focusIndex;
	}

	void SetStartDragTouchValues(Vector2 touchValue)
	{
		_startTouchTime = Time.time;
		_startTouchValue = touchValue;
	}

	void ReleaseScrollMoveMotion()
	{
		_objectMotionManager.ReleaseObjectMovement (_scrollMotionID, false);
		_isScrollMoveMotion = false;
		_isCollideOutLine = false;
	}

	void ReleaseAllScrollMotion()
	{
		_objectMotionManager.ReleaseAllMotion ();
		_isScrollMoveMotion = false;
		_isCollideOutLine = false;
	}

	#endregion

	#region Coroutine Methods

	IEnumerator UpdateObjScroll()
	{
		while (_isUpdate) {
			if (_scrollObjectList.Count > 0 && _isTouch){
				#if UNITY_EDITOR
				UpdateEditor ();
				#else
				UpdateDevice ();
				#endif

				_objectMotionManager.Update ();
			}

			yield return null;
		}
	}

	#endregion

	#region CallBack Methods

	void OnChangeMoveMotionValue(ObjectScrollMotionManager.MotionType motionType, Vector2 moveValue, int motionID)
	{
        switch (_moveType) {
            case ScrollMoveType.HorizontalScroll:
                SetObjListTransPosition(new Vector3(_objListTrans.localPosition.x + moveValue.x, _objListTrans.localPosition.y, _objListTrans.localPosition.z));
                break;
            case ScrollMoveType.VerticalScroll:
                SetObjListTransPosition(new Vector3(_objListTrans.localPosition.x, _objListTrans.localPosition.y + moveValue.y, _objListTrans.localPosition.z));

                if (motionID == _scrollMotionID && !_isCollideOutLine) {
                    if (CheckVerticalOutLinePos()) {
                        _objectMotionManager.ChangeSpeedValue(_scrollMotionID, 500f);
                        _isCollideOutLine = true;
                    }
                }
                break;
        }
    }

	void OnCompletedOutLineMotionAni()
	{
		
	}

	void OnCompletedMoveMotionScroll()
	{
		switch (_moveType) {
		case ScrollMoveType.HorizontalScroll:
			CheckHorizontalScrollMotion ();
			break;
		case ScrollMoveType.VerticalScroll:
			CheckVerticalScrollMotion ();
			break;
		}

		_isScrollMoveMotion = false;
		_isCollideOutLine = false;
	}

	#endregion
}
