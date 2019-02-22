using System.Collections;
using System.Collections.Generic;
using System;
using GameCore.Network;
using GameCore.Network.ClientMessages;
using GameCore.Network.ServerMessages;
using GameCore;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.Util;
using GameService.CharacterData;
using GameService.ClientComponent;
using GameService.ServerProxy.DataComponent.StoryScene;

namespace GameService.ServerProxy {
    public class StorySceneProxy : ServerComponentProxy {
        private IStorySceneController _controller = null;
        private bool _controllerActive = false;

        private IdentifiedObjectList<SceneObject> _objectList = new IdentifiedObjectList<SceneObject>();
        private readonly SceneCamera _camera = new SceneCamera();

        private bool _investigating = false; //Indicate using InvestigationView or StoryView
        private Character _talker1 = null; //Indicate talk mode when talker1 and talker2 is not null
        private Character _talker2 = null;
        private bool _isTalker1Talking = true;
        private bool _selectionShowed = false;

        public StorySceneProxy(Connection connection) :
            base(connection) {
            connection.AddMessageReceiver(StorySceneResetMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneCreateObjectMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneDestroyObjectMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneUpdateObjectDataMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneExecuteAnimCommandsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneCompleteAnimCommandsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterStoryModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterTalkModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneTurnTalkMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterInvestigationModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneShowSelectionsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneUpdateDialogMessage.MESSAGE_TYPE, this);
        }

        public void BindController(IStorySceneController controller) {
            _controller = controller;
        }

        public void ResetController() {
            _controller.ClearObjectInteractionListeners();
            _controller.CompleteAnimations();
            _controller.ClearObjects();
            _controller.DisableFreedomCameraView();
            _controller.DisableDialogTextInput();
            _controller.ShowDialog();
            _controller.DisplayDialogText("", new Vec3(1, 1, 1));
            _controller.DisplayDialogPortrait(-1);
            _controller.HideSelections();
        }

        public void ActiveController() {
            _controllerActive = true;
            ResetController();
        }

        public void InactiveController() {
            _controllerActive = false;
            _controller.HideSelections();
            _controller.HideDialog();
            _controller.DisableFreedomCameraView();
            _controller.CompleteAnimations();
            _controller.ClearObjects();
        }

        public void InteractWithObject(int id) {
            if (_investigating) return;
            _controller.CompleteAnimations();
            var message = new StorySceneInvestigateObjectMessage();
            message.objID = new RuntimeId { value = _objectList[id].ID };
            ConnectionRef.SendMessage(message);
        }

        public void SelectSelection(int selectionIdx) {
            if (!_selectionShowed) return;
            var message = new StorySceneSelectSelectionMessage();
            message.index = selectionIdx;
            ConnectionRef.SendMessage(message);
        }

        public void InputDialogText(string text) {
            InputDialogText(text, null);
        }

        public void InputDialogText(string text, IList<User> to) {
            if (!_investigating && ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self))) {
                if (to != null && GameServerProxy.Instance.Self.IsDM) {
                    var message = new StorySceneDMDialogSendPrivateTextMessage();
                    var players = new RuntimeId[to.Count];
                    for (int i = 0; i < to.Count; ++i) {
                        if (to[i].IsDM) continue;
                        players[i] = new RuntimeId { value = to[i].ID };
                    }
                    message.players = players;
                    message.text = text;
                    ConnectionRef.SendMessage(message);
                } else {
                    var message = new StorySceneDialogSendTextMessage();
                    message.text = text;
                    ConnectionRef.SendMessage(message);
                }
            }
        }

        public override void MessageReceived(Message message) {
            try {
                switch (message.MessageType) {
                    case StorySceneResetMessage.MESSAGE_TYPE: {
                            _investigating = false;
                            _talker1 = _talker2 = null;
                            ResetController();
                        }
                        break;
                    case StorySceneCreateObjectMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneCreateObjectMessage)message;
                            var character = CharacterManager.Instance.FindCharacter(msg.objID.value);
                            int id = msg.objID.value;
                            var sceneObj = new SceneObject(id, character, msg.portrait);
                            _objectList.Add(sceneObj);
                            _controller.AddObject(id, msg.portrait.sprites);
                        }
                        break;
                    case StorySceneDestroyObjectMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneDestroyObjectMessage)message;
                            int id = msg.objID.value;
                            _objectList.Remove(id);
                            _controller.RemoveObject(id);
                        }
                        break;
                    case StorySceneUpdateObjectDataMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneUpdateObjectDataMessage)message;
                            int id = msg.objID.value;
                            _objectList[id].Interactable = msg.interactable;
                            if (msg.interactable) {
                                _controller.SetObjectInteractionListener(id, this.InteractWithObject);
                            } else {
                                _controller.SetObjectInteractionListener(id, null);
                            }
                        }
                        break;
                    case StorySceneExecuteAnimCommandsMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneExecuteAnimCommandsMessage)message;
                            if (msg.hasCameraAnimation) {
                                var camAnimation = msg.cameraAnimation;
                                if (_camera.RepeatingAnimation && camAnimation.repeatEndTime >= 0) {
                                    _controller.SetCameraCurrentRepeatStopAt(camAnimation.repeatEndTime);
                                    _camera.RepeatingAnimation = false;
                                }
                                if (!_camera.RepeatingAnimation && camAnimation.commands.Length > 0) {
                                    _controller.SetCameraAnimation((CameraAnimCommand[])camAnimation.commands);
                                }
                                if (!_camera.RepeatingAnimation && camAnimation.repeatStartOffsetTime >= 0 && camAnimation.repeatCommands.Length > 0) {
                                    _controller.SetCameraNextRepeatStartAfter((CameraAnimCommand[])camAnimation.repeatCommands, camAnimation.repeatStartOffsetTime);
                                    _camera.RepeatingAnimation = true;
                                }
                            }
                            for (int i = 0; i < msg.objectsID.Length; ++i) {
                                int id = msg.objectsID[i].value;
                                var objAnimation = msg.objectsAnimation[i];
                                var obj = _objectList[id];
                                if (obj.RepeatingAnimation && objAnimation.repeatEndTime >= 0) {
                                    _controller.SetObjectCurrentRepeatStopAt(id, objAnimation.repeatEndTime);
                                    obj.RepeatingAnimation = false;
                                }
                                if (!obj.RepeatingAnimation && objAnimation.commands.Length > 0) {
                                    _controller.SetObjectAnimation(id, (ObjectAnimCommand[])objAnimation.commands);
                                }
                                if (!obj.RepeatingAnimation && objAnimation.repeatStartOffsetTime >= 0 && objAnimation.repeatCommands.Length > 0) {
                                    _controller.SetObjectNextRepeatStartAfter(id, (ObjectAnimCommand[])objAnimation.repeatCommands, objAnimation.repeatStartOffsetTime);
                                    obj.RepeatingAnimation = true;
                                }
                            }
                            _controller.PlayAnimations();
                        }
                        break;
                    case StorySceneCompleteAnimCommandsMessage.MESSAGE_TYPE: {
                            _controller.CompleteAnimations();
                        }
                        break;
                    case StorySceneEnterStoryModeMessage.MESSAGE_TYPE: {
                            _controller.DisableFreedomCameraView();
                            _controller.ShowDialog();
                            _controller.DisableDialogTextInput();
                            _investigating = false;
                            _talker1 = null;
                            _talker2 = null;
                        }
                        break;
                    case StorySceneEnterTalkModeMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneEnterTalkModeMessage)message;
                            _talker1 = CharacterManager.Instance.FindCharacter(msg.talkCharacter1.value);
                            _talker2 = CharacterManager.Instance.FindCharacter(msg.talkCharacter2.value);
                            _isTalker1Talking = msg.chara1Start;
                            _controller.DisableFreedomCameraView();
                            _controller.ShowDialog();
                            if ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                                || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self)) {
                                _controller.EnabledDialogTextInput(InputDialogText);
                            } else {
                                _controller.DisableDialogTextInput();
                            }
                        }
                        break;
                    case StorySceneTurnTalkMessage.MESSAGE_TYPE: {
                            _isTalker1Talking = !_isTalker1Talking;
                            if ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                                || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self))  {
                                _controller.EnabledDialogTextInput(InputDialogText);
                            } else {
                                _controller.DisableDialogTextInput();
                            }
                        }
                        break;
                    case StorySceneEnterInvestigationModeMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneEnterInvestigationModeMessage)message;
                            int userID = msg.investigatingPlayer.value;
                            if (userID == GameServerProxy.Instance.Self.ID) {
                                _investigating = true;
                                _controller.EnabledFreedomCameraView(new Vec2(0, 0), 540);
                                _controller.DisableDialogTextInput();
                                _controller.HideDialog();
                                _controller.HideSelections();
                            } else {
                                _investigating = false;
                                _controller.DisableFreedomCameraView();
                            }
                        }
                        break;
                    case StorySceneShowSelectionsMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneShowSelectionsMessage)message;
                            _selectionShowed = true;
                            _controller.ShowSelections(msg.selections, SelectSelection);
                        }
                        break;
                    case StorySceneHideSelectionsMessage.MESSAGE_TYPE: {
                            _selectionShowed = false;
                            _controller.HideSelections();
                        }
                        break;
                    case StorySceneShowSelectionVoterMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneShowSelectionVoterMessage)message;
                            var voter = GameServerProxy.Instance.FindUser(msg.voterID.value);
                            _controller.DisplaySelectionVoter(msg.selectionIndex, voter.Avatar);
                        }
                        break;
                    case StorySceneUpdateDialogMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneUpdateDialogMessage)message;
                            int objID = msg.portraitObjectID.value;
                            _controller.DisplayDialogPortrait(objID);
                            _controller.DisplayDialogText(msg.text, msg.isPrivate ? new Vec3(0.6f, 0.6f, 0.6f) : new Vec3(1.0f, 1.0f, 1.0f));
                        }
                        break;
                }
            } catch (System.Exception e) {
                Logger.WriteLine(e.Message);
            }
        }
    }
}