using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatNudgeNode
{
    #region Variables

    int _nudgeCount = 0;
    int _changeValue = 0;

    List<ChatNudgeNode> _parentNodes = null;
    List<ChatNudgeNode> _childNodes = null;

    List<IChatNudgeNotable> _nudgeNotables = new List<IChatNudgeNotable>();

    #endregion

    #region Properties

    public List<ChatNudgeNode> ParentNodes
    {
        get { return _parentNodes; }
    }

    public List<ChatNudgeNode> ChildNodes
    {
        get { return _childNodes; }
    }

    #endregion

    #region Methods

    public void AddChildNode(ChatNudgeNode addNode)
    {
        if(_childNodes == null)
            _childNodes = new List<ChatNudgeNode>();

        if (_childNodes.Contains(addNode))
            return;

        _childNodes.Add(addNode);
        if(addNode._parentNodes == null)
            addNode._parentNodes = new List<ChatNudgeNode>();
        addNode._parentNodes.Add(this);

        AddNudgeCount(addNode.GetNodeNudgeCount());
    }

    public void AddNudgeNotable(IChatNudgeNotable nudgeNotable)
    {
        if(_nudgeNotables.Contains(nudgeNotable))
            return;

        _nudgeNotables.Add(nudgeNotable);

        RefreshNodeNudgeCount();
    }

    public void RemoveNudgeNotable(IChatNudgeNotable nudgeNotable)
    {
        if(!_nudgeNotables.Contains(nudgeNotable))
            return;

        _nudgeNotables.Remove(nudgeNotable);
    }

    public void ClearNudgeNotable()
    {
        _nudgeNotables.Clear();
    }

    public void DeLinkCurNode()
    {
        if (_parentNodes != null && _parentNodes.Count > 0) {
            for (int i = 0; i < _parentNodes.Count; i++) {
                if (_parentNodes[i].ChildNodes != null && _parentNodes[i].ChildNodes.Count > 0) {
                    if(_parentNodes[i].ChildNodes.Contains(this)) {
                        _parentNodes[i].AddNudgeCount(-GetNodeNudgeCount());
                        _parentNodes[i].ChildNodes.Remove(this);
                    }
                }
            }
        }

        if (_childNodes != null && _childNodes.Count > 0) {
            for (int i = 0; i < _childNodes.Count; i++) {
                if (_childNodes[i].ParentNodes != null && _childNodes[i].ParentNodes.Count > 0) {
                    if (_childNodes[i].ParentNodes.Contains(this)) {
                        _childNodes[i].ParentNodes.Remove(this);
                    }
                }
            }
        }
    }

    public void DeLinkAllNode()
    {
        if (_parentNodes != null && _parentNodes.Count > 0) {
            for (int i = 0; i < _parentNodes.Count; i++) {
                if (_parentNodes[i].ChildNodes != null && _parentNodes[i].ChildNodes.Count > 0) {
                    if (_parentNodes[i].ChildNodes.Contains(this)) {
                        _parentNodes[i].AddNudgeCount(-GetNodeNudgeCount());
                        _parentNodes[i].ChildNodes.Remove(this);
                    }
                }
            }
        }

        if (_childNodes != null && _childNodes.Count > 0) {
            for (int i = 0; i < _childNodes.Count; i++) {
                _childNodes[i].DeLinkAllNode();
            }
        }

        if(_childNodes != null)
            _childNodes.Clear();

        _nudgeCount = 0;
        _changeValue = 0;
    }

    public void DeLinkChildNodes()
    {
        if (_childNodes != null && _childNodes.Count > 0) {
            for (int i = 0; i < _childNodes.Count; i++) {
                if (_childNodes[i] == null) continue;

                if (_childNodes[i].ParentNodes != null && _childNodes[i].ParentNodes.Count > 0) {
                    if (_childNodes[i].ParentNodes.Contains(this)) {
                        _childNodes[i].ParentNodes.Remove(this);
                    }
                }

                _childNodes[i].DeLinkChildNodes();
            }
        }

        if (_childNodes != null)
            _childNodes.Clear();
    }

    public int GetNodeNudgeCount()
    {
        return _nudgeCount + _changeValue;
    }

    public int GetChildNodeNudgeCount()
    {
        int retValue = 0;

        if (_childNodes != null && _childNodes.Count > 0) {
            for (int i = 0; i < _childNodes.Count; i++) {
                retValue += _childNodes[i].GetNodeNudgeCount();
            }
        }

        return retValue;
    }

    public void RefreshNodeNudgeCount()
    {
        if (_nudgeNotables.Count > 0) {
            for(int i = 0;i< _nudgeNotables.Count;i++) {
                IChatNudgeNotable nudgeNotable = _nudgeNotables[i];
                if (_nudgeCount > 0) {
                    if (nudgeNotable.GetNudgeObject() != null)
                        nudgeNotable.GetNudgeObject().SetActive(true);
                    if (_nudgeCount > 99) {
                        if (nudgeNotable.GetCountText() != null)
                            nudgeNotable.GetCountText().text = string.Format("{0}", 99);
                    } else {
                        if (nudgeNotable.GetCountText() != null)
                            nudgeNotable.GetCountText().text = string.Format("{0}", _nudgeCount);
                    }
                } else {
                    if (nudgeNotable.GetNudgeObject() != null)
                        nudgeNotable.GetNudgeObject().SetActive(false);
                }
            }
            
        }
    }

    public void ConfirmNudge()
    {
        AddNudgeCount(-_nudgeCount);
        _changeValue = 0;
    }

    public void AddNudgeCount(int addValue)
    {
        if(addValue == 0)
            return;

        int saveNudgeCount = _nudgeCount;
        if (_nudgeCount + addValue > ChattingController.Instance.MaxChatLineCount) {
            _nudgeCount = ChattingController.Instance.MaxChatLineCount;
        } else {
            _nudgeCount += addValue;
        }

        int validChangeCount = _nudgeCount - saveNudgeCount;
        if (_parentNodes != null && _parentNodes.Count > 0) {
            for (int i = 0; i < _parentNodes.Count; i++) {
                _parentNodes[i].AddNudgeCount(validChangeCount);
            }
        }

        RefreshNodeNudgeCount();
    }

    public void AddChangeValue(int addValue)
    {
        if (_changeValue + addValue > ChattingController.Instance.MaxChatLineCount) {
            _changeValue = ChattingController.Instance.MaxChatLineCount;
        } else {
            _changeValue += addValue;
        }
    }

    public void RefreshChangeValue()
    {
        if (_changeValue == 0)
            return;

        if (_parentNodes != null && _parentNodes.Count > 0) {
            for (int i = 0; i < _parentNodes.Count; i++) {
                _parentNodes[i].AddNudgeCount(_changeValue);
            }
        }

        _nudgeCount += _changeValue;
        _changeValue = 0;
        RefreshNodeNudgeCount();
    }

    #endregion
}
