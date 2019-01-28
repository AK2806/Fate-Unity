using System.Collections;
using System.Collections.Generic;
using System;
using GameCore.Network;
using GameCore.Network.ClientMessages;
using GameCore.Network.ServerMessages;
using GameCore;
using GameCore.Proxy;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.Util;
using GameService.CharacterData;
using GameService.ClientComponent;
using GameService.ServerProxy.DataComponent.StoryScene;

namespace GameService.ServerProxy {
    public class StorySceneProxy : ServerComponentProxy {
        private IStorySceneController _controller = null;
        private IdentifiedObjectList<SceneObject> _objectList = new IdentifiedObjectList<SceneObject>();
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
            connection.AddMessageReceiver(StorySceneFinishNonRepeatAnimsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneFinishRepeatAnimsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterStoryModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterTalkModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneTurnTalkMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneEnterInvestigationModeMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneShowSelectionsMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(StorySceneUpdateDialogMessage.MESSAGE_TYPE, this);
        }

        public void InteractWithObject(int id) {
            if (_investigating) return;
            _controller.FinishNonRepeatAnimations();
            var message = new StorySceneInvestigateObjectMessage();
            message.objID = IdentificationConverter.GetIdentification(_objectList[id]);
            ConnectionRef.SendMessage(message);
        }

        public void SelectSelection(int selectionIdx) {
            if (!_selectionShowed) return;
            var message = new StorySceneSelectSelectionMessage();
            message.index = selectionIdx;
            ConnectionRef.SendMessage(message);
        }

        public void InputDialogText(string text, IList<User> to) {
            if (!_investigating
            && ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self))) {
                if (to != null && GameServerProxy.Instance.Self.IsDM) {
                    var message = new StorySceneDMDialogSendPrivateTextMessage();
                    var players = new Identification[to.Count];
                    for (int i = 0; i < to.Count; ++i) {
                        if (to[i].IsDM) continue;
                        players[i] = IdentificationConverter.GetIdentification(to[i]);
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

        public void SetController(IStorySceneController controller) {
            _controller = controller;
            _controller.SetSelectListener(SelectSelection);
            _controller.SetTextInputListener(InputDialogText);
        }

        public override void MessageReceived(Message message) {
            try {
                switch (message.MessageType) {
                    case StorySceneResetMessage.MESSAGE_TYPE: {
                            _investigating = false;
                            _talker1 = _talker2 = null;
                            _controller.DisableTextInput();
                            _controller.HideSelections();
                            _controller.ClearDialog();
                            _controller.FinishNonRepeatAnimations();
                            _controller.FinishRepeatAnimations();
                            _controller.ClearObjectTouchListeners();
                            _controller.ClearObjects();
                            _controller.DisableInvestigationView();
                        }
                        break;
                    case StorySceneCreateObjectMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneCreateObjectMessage)message;
                            var character = CharacterManager.Instance.FindCharacter(IdentificationConverter.GetID(msg.objID));
                            int id = IdentificationConverter.GetID(msg.objID);
                            var sceneObj = new SceneObject(id, character, msg.portrait);
                            _objectList.Add(sceneObj);
                            _controller.AddObject(id, msg.portrait.sprites);
                        }
                        break;
                    case StorySceneDestroyObjectMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneDestroyObjectMessage)message;
                            int id = IdentificationConverter.GetID(msg.objID);
                            _objectList.Remove(id);
                            _controller.RemoveObject(id);
                        }
                        break;
                    case StorySceneUpdateObjectDataMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneUpdateObjectDataMessage)message;
                            int id = IdentificationConverter.GetID(msg.objID);
                            _objectList[id].Interactable = msg.interactable;
                            if (_investigating) {
                                if (msg.interactable) {
                                    _controller.SetObjectTouchListener(id, this.InteractWithObject);
                                } else {
                                    _controller.SetObjectTouchListener(id, null);
                                }
                            }
                        }
                        break;
                    case StorySceneExecuteAnimCommandsMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneExecuteAnimCommandsMessage)message;
                            var idList = new List<int>();
                            var animList = new List<Animation>();
                            int cameraAnimIdx = -1;
                            for (int i = 0; i < msg.objectsID.Length; ++i) {
                                int id = IdentificationConverter.GetID(msg.objectsID[i]);
                                if (id == -1) {
                                    cameraAnimIdx = i;
                                } else {
                                    idList.Add(id);
                                    animList.Add(msg.animations[i]);
                                }
                            }
                            int[] ids = idList.ToArray();
                            Animation[] animations = animList.ToArray();
                            if (cameraAnimIdx != -1) {
                                _controller.ExecuteAnimations(msg.animations[cameraAnimIdx], ids, animations);
                            } else {
                                _controller.ExecuteAnimations(ids, animations);
                            }
                        }
                        break;
                    case StorySceneFinishNonRepeatAnimsMessage.MESSAGE_TYPE: {
                            _controller.FinishNonRepeatAnimations();
                        }
                        break;
                    case StorySceneFinishRepeatAnimsMessage.MESSAGE_TYPE: {
                            _controller.FinishRepeatAnimations();
                        }
                        break;
                    case StorySceneEnterStoryModeMessage.MESSAGE_TYPE: {
                            _investigating = false;
                            _talker1 = null;
                            _talker2 = null;
                            _controller.DisableInvestigationView();
                            _controller.DisableTextInput();
                            _controller.ClearObjectTouchListeners();
                        }
                        break;
                    case StorySceneEnterTalkModeMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneEnterTalkModeMessage)message;
                            _talker1 = CharacterManager.Instance.FindCharacter(IdentificationConverter.GetID(msg.talkCharacter1));
                            _talker2 = CharacterManager.Instance.FindCharacter(IdentificationConverter.GetID(msg.talkCharacter2));
                            _isTalker1Talking = msg.chara1Start;
                            _controller.DisableInvestigationView();
                            if ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                            || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self)) {
                                _controller.EnabledTextInput();
                            } else {
                                _controller.DisableTextInput();
                            }
                            _controller.ClearObjectTouchListeners();
                        }
                        break;
                    case StorySceneTurnTalkMessage.MESSAGE_TYPE: {
                            _isTalker1Talking = !_isTalker1Talking;
                            if ((_isTalker1Talking && _talker1.Controller == GameServerProxy.Instance.Self)
                            || (!_isTalker1Talking && _talker2.Controller == GameServerProxy.Instance.Self))  {
                                _controller.EnabledTextInput();
                            } else {
                                _controller.DisableTextInput();
                            }
                        }
                        break;
                    case StorySceneEnterInvestigationModeMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneEnterInvestigationModeMessage)message;
                            var userID = IdentificationConverter.GetID(msg.investigatingPlayer);
                            if (userID == GameServerProxy.Instance.Self.ID) {
                                _investigating = true;
                                _controller.EnabledInvestigationView();
                            } else {
                                _investigating = false;
                                _controller.DisableInvestigationView();
                            }
                            foreach (var obj in _objectList) {
                                if (obj.Interactable) {
                                    _controller.SetObjectTouchListener(obj.ID, this.InteractWithObject);
                                }
                            }
                        }
                        break;
                    case StorySceneShowSelectionsMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneShowSelectionsMessage)message;
                            _selectionShowed = true;
                            _controller.ShowSelections(msg.selections);
                        }
                        break;
                    case StorySceneHideSelectionsMessage.MESSAGE_TYPE: {
                            _selectionShowed = false;
                            _controller.HideSelections();
                        }
                        break;
                    case StorySceneShowSelectionVoterMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneShowSelectionVoterMessage)message;
                            var voter = GameServerProxy.Instance.FindUser(IdentificationConverter.GetID(msg.voterID));
                            _controller.SetSelectionVoter(msg.selectionIndex, voter);
                        }
                        break;
                    case StorySceneUpdateDialogMessage.MESSAGE_TYPE: {
                            var msg = (StorySceneUpdateDialogMessage)message;
                            int objID = IdentificationConverter.GetID(msg.portraitObjectID);
                            _controller.SetDialogPortrait(objID);
                            _controller.SetDialogText(msg.text, msg.isPrivate ? new Vec3(0.6f, 0.6f, 0.6f) : new Vec3(1.0f, 1.0f, 1.0f));
                        }
                        break;
                }
            } catch (System.Exception e) {
                Logger.WriteLine(e.Message);
            }
        }
    }
}