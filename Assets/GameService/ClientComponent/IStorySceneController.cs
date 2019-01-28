using System;
using System.Collections;
using System.Collections.Generic;
using GameCore;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.Util;

namespace GameService.ClientComponent {
    public interface IStorySceneController {
        void ClearObjects();

        void AddObject(int id, AssetReference[] spriteGroup);
        void RemoveObject(int id);

        void ExecuteAnimations(Animation cameraAnimation, int[] ids, Animation[] animations);
        void ExecuteAnimations(int[] ids, Animation[] animations);
        void FinishNonRepeatAnimations();
        void FinishRepeatAnimations();
        
        void ClearObjectTouchListeners();
        void SetObjectTouchListener(int id, Action<int> callback);

        void EnabledInvestigationView();
        void DisableInvestigationView();
        
        void ClearDialog();
        void SetDialogText(string text, Vec3 color);
        void SetDialogPortrait(int objID); // -1 is reset
        void SetTextInputListener(Action<string, IList<User>> callback);
        void EnabledTextInput();
        void DisableTextInput();

        void HideSelections();
        void ShowSelections(string[] selections);
        void SetSelectListener(Action<int> callback);
        void SetSelectionVoter(int selectionIdx, User user);
    }
}