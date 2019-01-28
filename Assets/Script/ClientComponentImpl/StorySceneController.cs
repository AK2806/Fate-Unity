using System;
using System.Collections;
using System.Collections.Generic;
using GameCore;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.ClientComponent;
using GameService.Util;
using Futilef;
using ImgAttr = Futilef.GpController.ImgAttr;
using CamAttr = Futilef.GpController.CamAttr;
using UnityEngine;

namespace ClientComponentImpl.StoryScene {
    public sealed class StorySceneController : IStorySceneController {
        private struct ImgData {
            public AssetReference[] spriteGrp;
            public GameCore.Vec2 pos;
            public int zIdx;
        }

        private readonly GpController _gpc;
        private readonly Dictionary<int, ImgData> _imgDict;
        private HashSet<int> _interactableIDs = new HashSet<int>();

        public StorySceneController(Camera camera) {
            _gpc = new GpController(camera);
            _imgDict = new Dictionary<int, ImgData>();
        }

        public void Active(bool enabled) {
            if (!enabled) {
                _gpc.RmImg(-1);
            }
        }

        public void ClearObjects() {
            _gpc.RmImg(-1);
        }

        public void Update(float deltaTime) {
            _gpc.Update(deltaTime);
        }

        public void AddObject(int id, AssetReference[] spriteGroup) {
            _imgDict.Add(id, new ImgData() { spriteGrp = spriteGroup });
            _gpc.AddImg(id, spriteGroup[0].uid);
        }

        public void RemoveObject(int id) {
            _imgDict.Remove(id);
            _gpc.Wait(-1);
            _gpc.RmImg(id);
        }
        /*
        public void SetPos(int id, GameCore.Vec2 pos) {
            var data = _imgDict[id];
            _gpc.Wait(-1);
            _gpc.SetImgAttr(id, ImgAttr.Position, pos.X, pos.Y, (float)data.zIdx);
            data.pos = pos;
            _imgDict[id] = data;
        }

        public void SetZIndex(int id, int z) {
            var data = _imgDict[id];
            _gpc.Wait(-1);
            _gpc.SetImgAttr(id, ImgAttr.Position, data.pos.X, data.pos.Y, (float)z);
            data.zIdx = z;
            _imgDict[id] = data;
        }
        */
        public void ExecuteAnimations(GameCore.StoryScene.Animation cameraAnimation, int[] ids, GameCore.StoryScene.Animation[] objectsAnim) {

        }

        public void ExecuteAnimations(int[] ids, GameCore.StoryScene.Animation[] objectsAnim) {
            _gpc.Wait(-1);
            
        }
        
        public void FinishNonRepeatAnimations() {
            
        }

        public void FinishRepeatAnimations() {
            
        }

        private void SetInteractCallback(int id, Action<int> callback) {
            if (callback == null) _gpc.SetImgInteractable(id, null);
            else _gpc.SetImgInteractable(id, (phase, x, y) => {
                Debug.LogFormat("hit img1: phase {0}, x {1}, y {2}", phase, x, y);
                callback(id);
            });
        }

        public void ClearObjectTouchListeners() {
            foreach (int id in _interactableIDs) {
                SetInteractCallback(id, null);
            }
            _interactableIDs.Clear();
        }
        
        public void SetObjectTouchListener(int id, Action<int> callback) {
            if (!_interactableIDs.Contains(id)) _interactableIDs.Add(id);
            else if (callback == null) _interactableIDs.Remove(id);
            SetInteractCallback(id, callback);
        }
        
        public void EnabledInvestigationView() {
            
        }
        
        public void DisableInvestigationView() {

        }

        public void ClearDialog() {

        }

        public void SetDialogText(string text, GameCore.Vec3 color) {

        }

        public void SetDialogPortrait(int objID) {// -1 is clear

        }

        public void SetTextInputListener(Action<string, IList<User>> callback) {

        }

        public void EnabledTextInput() {

        }
        
        public void DisableTextInput() {

        }

        public void HideSelections() {

        }

        public void ShowSelections(string[] selections) {

        }

        public void SetSelectListener(Action<int> callback) {

        }

        public void SetSelectionVoter(int selectionIdx, User user) {

        }

    }
}