using System;
using System.Collections;
using System.Collections.Generic;
using GameCore;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.ClientComponent;
using GameService.ServerProxy;
using GameService.Util;
using Futilef;
using ImgAttr = Futilef.GpController.ImgAttr;
using CamAttr = Futilef.GpController.CamAttr;
using UnityEngine;
using UnityEngine.UI;
using FateUnity.Script.UI;
using FateUnity.Components.UI;
using FateUnity.Components.UI.Chat;
using FateUnity.Components.UI.Dice;
using FateUnity.Components.UI.ObjectList;
using FateUnity.Components.UI.StorySelection;

namespace FateUnity.Script.ClientComponentImpl {
    public class StorySceneController : MonoBehaviour, IStorySceneController {
        private static int ConvertEsType(EaseType easeType) {
            return (int)easeType;
        }

        private sealed class ImgData {
            public readonly int id;
            public bool repeating = false;
            public AssetReference[] spriteGrp = null;
            public GameCore.Vec2 pos = new GameCore.Vec2();
            public int zIdx = 0;

            public ImgData(int id) {
                this.id = id;
            }

            /////////////////////////////
            // Next animation settings
            public LinkedList<ObjectAnimCommand> cmds = null;
            public ObjectAnimCommand[] repeatCmds = null;
            public float repeatStartOffsetTime = -1.0f;
            public float repeatEndTime = -1.0f;
        }

        private sealed class CamData {
            public bool repeating = false;
            
            // Next animation settings
            public LinkedList<CameraAnimCommand> cmds = null;
            public CameraAnimCommand[] repeatCmds = null;
            public float repeatStartOffsetTime = -1.0f;
            public float repeatEndTime = -1.0f;
        }

        private struct DelayedFuncCall {
            public const int ClearObjects = 0, AddObject = 1, RemoveObject = 2;

            public int FuncType;
            public object[] parameters;
        }

        private readonly Queue<DelayedFuncCall> _funcQueue = new Queue<DelayedFuncCall>();
        private readonly Dictionary<int, ImgData> _imgDict = new Dictionary<int, ImgData>();
        private readonly HashSet<int> _interactableIDs = new HashSet<int>();
        private CamData _cameraData = new CamData();
        private GpController _gpc = null;
        private bool _cameraFreedom = false;

        public StorySceneDialog dialog;
        public StorySelectionList selectionList;

        private void Awake() {
		    GameServerProxy.Instance.StoryScene.BindController(this);
        }

        private void OnEnable() {
            _gpc = new GpController(Camera.main);
            _funcQueue.Clear();
            _imgDict.Clear();
            _interactableIDs.Clear();
            _cameraData = new CamData();
            _cameraFreedom = false;
            dialog.Show();
            selectionList.Hide();
        }

        private void OnDisable() {
            if (_gpc != null) _gpc.Dispose();
            _funcQueue.Clear();
            _imgDict.Clear();
            _interactableIDs.Clear();
            _cameraData = new CamData();
            _cameraFreedom = false;
            dialog.Hide();
            selectionList.Hide();
        }

        private void Update() {
            float deltaTime = Time.deltaTime;
            if (_gpc != null) {
                if (!_gpc.IsWorking()) {
                    while (_funcQueue.Count > 0) {
                        var delayedCall = _funcQueue.Dequeue();
                        switch (delayedCall.FuncType) {
                            case DelayedFuncCall.ClearObjects:  ExecuteClearObjects(); break;
                            case DelayedFuncCall.AddObject:     ExecuteAddObject((int)delayedCall.parameters[0], (AssetReference[])delayedCall.parameters[1]); break;
                            case DelayedFuncCall.RemoveObject:  ExecuteRemoveObject((int)delayedCall.parameters[0]); break;
                        }
                    }
                }
                _gpc.Update(deltaTime);
            }
        }

        private void ExecuteClearObjects() {
            _gpc.RmImg(-1);
            _imgDict.Clear();
            _interactableIDs.Clear();
        }

        private void ExecuteAddObject(int id, AssetReference[] spriteGroup) {
            _gpc.AddImg(id, spriteGroup[0].uid);
            _imgDict.Add(id, new ImgData(id) { spriteGrp = spriteGroup });
        }

        private void ExecuteRemoveObject(int id) {
            _gpc.RmImg(id);
            _imgDict.Remove(id);
            _interactableIDs.Remove(id);
        }

        public void ClearObjects() {
            if (_gpc == null) return;
            _funcQueue.Enqueue(new DelayedFuncCall { FuncType = DelayedFuncCall.ClearObjects });
        }

        public void AddObject(int id, AssetReference[] spriteGroup) {
            if (_gpc == null) return;
            _funcQueue.Enqueue(new DelayedFuncCall { FuncType = DelayedFuncCall.AddObject, parameters = new object[] { id, spriteGroup } });
        }

        public void RemoveObject(int id) {
            if (_gpc == null) return;
            _funcQueue.Enqueue(new DelayedFuncCall { FuncType = DelayedFuncCall.RemoveObject, parameters = new object[] { id } });
        }
        
        public void SetCameraAnimation(CameraAnimCommand[] cameraAnimation) {
            if (_gpc == null) return;
            if (_cameraData.repeating && _cameraData.repeatEndTime < 0) return;
            _cameraData.cmds = new LinkedList<CameraAnimCommand>(cameraAnimation);
        }

        public void SetObjectAnimation(int id, ObjectAnimCommand[] objectAnimation) {
            if (_gpc == null) return;
            var imgData = _imgDict[id];
            if (imgData.repeating && imgData.repeatEndTime < 0) return;
            imgData.cmds = new LinkedList<ObjectAnimCommand>(objectAnimation);
        }

        public void SetCameraNextRepeatStartAfter(CameraAnimCommand[] repeatAnimation, float offsetTime) {
            if (_gpc == null) return;
            if (_cameraData.repeating && _cameraData.repeatEndTime < 0) return;
            _cameraData.repeatCmds = repeatAnimation;
            _cameraData.repeatStartOffsetTime = offsetTime;
        }

        public void SetCameraCurrentRepeatStopAt(float endTime) {
            if (_gpc == null) return;
            if (!_cameraData.repeating) return;
            _cameraData.repeatEndTime = endTime;
        }

        public void SetObjectNextRepeatStartAfter(int id, ObjectAnimCommand[] repeatAnimation, float offsetTime) {
            if (_gpc == null) return;
            var imgData = _imgDict[id];
            if (imgData.repeating && imgData.repeatEndTime < 0) return;
            imgData.repeatCmds = repeatAnimation;
            imgData.repeatStartOffsetTime = offsetTime;
        }

        public void SetObjectCurrentRepeatStopAt(int id, float endTime) {
            if (_gpc == null) return;
            var imgData = _imgDict[id];
            if (!imgData.repeating) return;
            imgData.repeatEndTime = endTime;
        }
        
        public void PlayAnimations() {
            if (_gpc == null) return;
            if (_cameraFreedom) _cameraData = new CamData();
            _gpc.Wait(-1);
            float leastOffset = 0.0f, offsetCrop;
            while (leastOffset != Mathf.Infinity) {
                _gpc.Wait(leastOffset);
                offsetCrop = leastOffset;
                leastOffset = Mathf.Infinity;
                if (!_cameraData.repeating ^ _cameraData.repeatEndTime >= 0) {
                    if (_cameraData.repeating) {
                        _cameraData.repeatEndTime -= offsetCrop;
                        if (Mathf.Approximately(_cameraData.repeatEndTime, 0)) {
                            _gpc.StopRepeat(0);
                            _cameraData.repeatEndTime = -1.0f;
                            _cameraData.repeating = false;
                        } else if (_cameraData.repeatEndTime < leastOffset) {
                            leastOffset = _cameraData.repeatEndTime;
                        }
                    }
                    if (!_cameraData.repeating) {
                        var cmds = _cameraData.cmds;
                        if (cmds != null) {
                            while (cmds.Count > 0) {
                                var cmd = cmds.First.Value;
                                cmds.RemoveFirst();
                                cmd.timeOffset -= offsetCrop;
                                if (Mathf.Approximately(cmd.timeOffset, 0)) {
                                    ExecuteAnimCommand(cmd);
                                } else {
                                    if (cmd.timeOffset < leastOffset) {
                                        leastOffset = cmd.timeOffset;
                                    }
                                    cmds.AddFirst(cmd);
                                    break;
                                }
                            }
                        }
                        if (cmds == null || (cmds != null && cmds.Count <= 0)) {
                            cmds = _cameraData.cmds = null;
                            if (_cameraData.repeatStartOffsetTime >= 0) {
                                _cameraData.repeatStartOffsetTime -= offsetCrop;
                                if (Mathf.Approximately(_cameraData.repeatStartOffsetTime, 0)) {
                                    _gpc.RepeatDeclareBegin();
                                    foreach (var repeatCmd in _cameraData.repeatCmds) {
                                        ExecuteAnimCommand(repeatCmd);
                                    }
                                    _gpc.RepeatDeclareEnd(0);
                                    _cameraData.repeatStartOffsetTime = -1.0f;
                                    _cameraData.repeatCmds = null;
                                    _cameraData.repeating = true;
                                } else if (_cameraData.repeatStartOffsetTime < leastOffset) {
                                    leastOffset = _cameraData.repeatStartOffsetTime;
                                }
                            }
                        }
                    }
                }
                var ids = new int[_imgDict.Keys.Count];
                _imgDict.Keys.CopyTo(ids, 0);
                foreach (var id in ids) {
                    var imgData = _imgDict[id];
                    if (!imgData.repeating ^ imgData.repeatEndTime >= 0) {
                        if (imgData.repeating) {
                            imgData.repeatEndTime -= offsetCrop;
                            if (Mathf.Approximately(imgData.repeatEndTime, 0)) {
                                _gpc.StopRepeat(id);
                                imgData.repeatEndTime = -1.0f;
                                imgData.repeating = false;
                            } else if (imgData.repeatEndTime < leastOffset) {
                                leastOffset = imgData.repeatEndTime;
                            }
                        }
                        if (!imgData.repeating) {
                            var cmds = imgData.cmds;
                            if (cmds != null) {
                                while (cmds.Count > 0) {
                                    var cmd = cmds.First.Value;
                                    cmds.RemoveFirst();
                                    cmd.timeOffset -= offsetCrop;
                                    if (Mathf.Approximately(cmd.timeOffset, 0)) {
                                        ExecuteAnimCommand(id, cmd);
                                    } else {
                                        if (cmd.timeOffset < leastOffset) {
                                            leastOffset = cmd.timeOffset;
                                        }
                                        cmds.AddFirst(cmd);
                                        break;
                                    }
                                }
                            }
                            if (cmds == null || (cmds != null && cmds.Count <= 0)) {
                                cmds = imgData.cmds = null;
                                if(imgData.repeatStartOffsetTime >= 0) {
                                    imgData.repeatStartOffsetTime -= offsetCrop;
                                    if (Mathf.Approximately(imgData.repeatStartOffsetTime, 0)) {
                                        _gpc.RepeatDeclareBegin();
                                        foreach (var repeatCmd in imgData.repeatCmds) {
                                            ExecuteAnimCommand(id, repeatCmd);
                                        }
                                        _gpc.RepeatDeclareEnd(id);
                                        imgData.repeatStartOffsetTime = -1.0f;
                                        imgData.repeatCmds = null;
                                        imgData.repeating = true;
                                    } else if (imgData.repeatStartOffsetTime < leastOffset) {
                                        leastOffset = imgData.repeatStartOffsetTime;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteAnimCommand(CameraAnimCommand cameraAnimCommand) {
			if (cameraAnimCommand.isEased) {
                var easeCmd = (EaseCameraAttribute)cameraAnimCommand;
				CameraAttrType attrType = easeCmd.attrType;
				EaseType esType = easeCmd.esType;
				float duration = easeCmd.duration;
				switch (attrType) {
					case CameraAttrType.Position: {
                            var easePosCmd = (EaseCameraPosition)easeCmd;
                            _gpc.SetCamAttrEased(CamAttr.Position, duration, ConvertEsType(esType), (float)easePosCmd.val.X, (float)easePosCmd.val.Y);
						}
						break;
					case CameraAttrType.Zoom: {
							var easeZoomCmd = (EaseCameraZoom)easeCmd;
                            _gpc.SetCamAttrEased(CamAttr.Zoom, duration, ConvertEsType(esType), (float)easeZoomCmd.val);
						}
						break;
				}
			} else {
				var setCmd = (SetCameraAttribute)cameraAnimCommand;
                CameraAttrType attrType = setCmd.attrType;
				switch (attrType) {
					case CameraAttrType.Position: {
							var setPosCmd = (SetCameraPosition)setCmd;
                            _gpc.SetCamAttr(CamAttr.Position, (float)setPosCmd.val.X, (float)setPosCmd.val.Y);
						}
						break;
					case CameraAttrType.Zoom: {
							var setZoomCmd = (SetCameraZoom)setCmd;
                            _gpc.SetCamAttr(CamAttr.Zoom, (float)setZoomCmd.val);
						}
						break;
				}
			}
        }

        private void ExecuteAnimCommand(int id, ObjectAnimCommand objectAnimCommand) {
			if (objectAnimCommand.isEased) {
                var easeCmd = (EaseObjectAttribute)objectAnimCommand;
				ObjectAttrType attrType = easeCmd.attrType;
				EaseType esType = easeCmd.esType;
				float duration = easeCmd.duration;
				switch (attrType) {
					case ObjectAttrType.Position: {
							var easePosCmd = (EaseObjectPosition)easeCmd;
                            _gpc.SetImgAttrEased(id, ImgAttr.Position, duration, ConvertEsType(esType), (float)easePosCmd.val.X, (float)easePosCmd.val.Y);
						}
						break;
					case ObjectAttrType.Rotation: {
							var easeRotCmd = (EaseObjectRotation)easeCmd;
                            _gpc.SetImgAttrEased(id, ImgAttr.Rotation, duration, ConvertEsType(esType), (float)easeRotCmd.angle);
						}
						break;
					case ObjectAttrType.Scale: {
							var easeScaleCmd = (EaseObjectScale)easeCmd;
                            _gpc.SetImgAttrEased(id, ImgAttr.Scale, duration, ConvertEsType(esType), (float)easeScaleCmd.val.X, (float)easeScaleCmd.val.Y);
						}
						break;
					case ObjectAttrType.Alpha: {
							var easeAlphaCmd = (EaseObjectAlpha)easeCmd;
                            _gpc.SetImgAttrEased(id, ImgAttr.Alpha, duration, ConvertEsType(esType), (float)easeAlphaCmd.val);
						}
						break;
					case ObjectAttrType.Tint: {
							var easeTintCmd = (EaseObjectTint)easeCmd;
                            _gpc.SetImgAttrEased(id, ImgAttr.Tint, duration, ConvertEsType(esType), (float)easeTintCmd.color.X, (float)easeTintCmd.color.Y, (float)easeTintCmd.color.Z);
						}
						break;
					case ObjectAttrType.Sprite: {
							var easeSpriteCmd = (EaseObjectSprite)easeCmd;
                            var imgData = _imgDict[id];
                            _gpc.SetImgAttr(id, ImgAttr.Position, (int)imgData.spriteGrp[easeSpriteCmd.spriteIndex].uid);
						}
						break;
				}
			} else {
                var setCmd = (SetObjectAttribute)objectAnimCommand;
				ObjectAttrType attrType = setCmd.attrType;
				switch (attrType) {
					case ObjectAttrType.Position: {
							var setPosCmd = (SetObjectPosition)setCmd;
                            var imgData = _imgDict[id];
                            _gpc.SetImgAttr(id, ImgAttr.Position, (float)setPosCmd.val.X, (float)setPosCmd.val.Y, (float)imgData.zIdx);
                            imgData.pos = setPosCmd.val;
						}
						break;
					case ObjectAttrType.ZIndex: {
							var setZCmd = (SetObjectZIndex)setCmd;
                            var imgData = _imgDict[id];
                            _gpc.SetImgAttr(id, ImgAttr.Position, (float)imgData.pos.X, (float)imgData.pos.Y, (float)setZCmd.val);
                            imgData.zIdx = setZCmd.val;
						}
						break;
					case ObjectAttrType.Rotation: {
							var setRotCmd = (SetObjectRotation)setCmd;
                            _gpc.SetImgAttr(id, ImgAttr.Rotation, (float)setRotCmd.angle);
						}
						break;
					case ObjectAttrType.Scale: {
							var setScaleCmd = (SetObjectScale)setCmd;
                            _gpc.SetImgAttr(id, ImgAttr.Scale, (float)setScaleCmd.val.X, (float)setScaleCmd.val.Y);
						}
						break;
					case ObjectAttrType.Alpha: {
							var setAlphaCmd = (SetObjectAlpha)setCmd;
                            _gpc.SetImgAttr(id, ImgAttr.Alpha, (float)setAlphaCmd.val);
						}
						break;
					case ObjectAttrType.Tint: {
							var setTintCmd = (SetObjectTint)setCmd;
                            _gpc.SetImgAttr(id, ImgAttr.Tint, (float)setTintCmd.color.X, (float)setTintCmd.color.Y, (float)setTintCmd.color.Z);
						}
						break;
					case ObjectAttrType.Sprite: {
							var setSpriteCmd = (SetObjectSprite)setCmd;
                            var imgData = _imgDict[id];
                            _gpc.SetImgAttr(id, ImgAttr.Position, (int)imgData.spriteGrp[setSpriteCmd.spriteIndex].uid);
						}
						break;
				}
			}
        }

        public void CompleteAnimations() {
            if (_gpc == null) return;
            _gpc.Skip();
        }

        private void SetObjectCallback(int id, Action<int> callback) {
            if (callback == null) _gpc.SetImgInteractable(id, null);
            else _gpc.SetImgInteractable(id, (phase, x, y) => {
                Debug.LogFormat("hit img1: phase {0}, x {1}, y {2}", phase, x, y);
                callback(id);
            });
        }

        public void ClearObjectInteractionListeners() {
            if (_gpc == null) return;
            foreach (int id in _interactableIDs) {
                SetObjectCallback(id, null);
            }
            _interactableIDs.Clear();
        }
        
        public void SetObjectInteractionListener(int id, Action<int> callback) {
            if (_gpc == null) return;
            if (!_interactableIDs.Contains(id)) _interactableIDs.Add(id);
            SetObjectCallback(id, callback);
        }
        
        public void EnabledFreedomCameraView(GameCore.Vec2 camPos, float camZoom) {
            if (_gpc == null) return;
            _cameraFreedom = true;
            _gpc.Wait(-1);
            if (_gpc.IsRepeatIDAllocated(0)) _gpc.StopRepeat(0);
            _gpc.SetCamAttr(CamAttr.Position, (float)camPos.X, (float)camPos.Y);
            _gpc.SetCamAttr(CamAttr.Zoom, (float)camZoom);
        }
        
        public void DisableFreedomCameraView() {
            if (_gpc == null) return;
            _cameraFreedom = false;
        }

        public void ShowDialog() {
            dialog.Show();
        }

        public void HideDialog() {
            dialog.Hide();
        }
        
        public void DisplayDialogText(string text, GameCore.Vec3 color) {
            dialog.SetTextColor(new Color(color.X, color.Y, color.Z));
            dialog.DisplayText(text);
        }

        public void DisplayDialogPortrait(int objID) {// -1 is clear
            if (objID == -1) {
                dialog.ClearPortrait();
            } else {
                
            }
        }

        public void EnabledDialogTextInput(Action<string> callback) {
            dialog.EnabledTextInput(callback);
        }

        public void DisableDialogTextInput() {
            dialog.DisableTextInput();
        }

        public void HideSelections() {
            selectionList.Hide();
        }

        public void ShowSelections(string[] selections, Action<int> callback) {
            selectionList.Show(selections, callback);
        }

        public void DisplaySelectionVoter(int selectionIdx, AssetReference userAvatar) {
            
        }
    }
}
