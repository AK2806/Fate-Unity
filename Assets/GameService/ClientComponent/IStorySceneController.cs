using System;
using System.Collections;
using System.Collections.Generic;
using GameCore;
using GameCore.StoryScene;
using GameCore.StoryScene.AnimationCommand;
using GameService.Util;

namespace GameService.ClientComponent {
    public interface IStorySceneController {
        // take effect after animations done
        void ClearObjects();
        void AddObject(int id, AssetReference[] spriteGroup);
        void RemoveObject(int id);
        ////////////////////////////////////

        // (animation settings) take effect while playing animations
        void SetCameraAnimation(CameraAnimCommand[] cameraAnimation);
        void SetObjectAnimation(int id, ObjectAnimCommand[] objectAnimation);
        void SetCameraNextRepeatStartAfter(CameraAnimCommand[] repeatAnimation, float offsetTime);
        void SetCameraCurrentRepeatStopAt(float endTime);
        void SetObjectNextRepeatStartAfter(int id, ObjectAnimCommand[] repeatAnimation, float offsetTime);
        void SetObjectCurrentRepeatStopAt(int id, float endTime);
        ////////////////////////////////////////////////////////////

        void PlayAnimations(); // Enqueue animation settings
        void CompleteAnimations(); // Skip animations in queue
        
        void ClearInvestigationListeners();
        void SetInvestigationListener(int id, Action<int> callback);

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