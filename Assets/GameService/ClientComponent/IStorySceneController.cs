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
        /////////////////////////////////////////////////////////////////////////////////

        void PlayAnimations(); // Enqueue animation settings
        void CompleteAnimations(); // Skip animations in queue
        //////////////////////////////////////////////////

        void ClearObjectInteractionListeners();
        void SetObjectInteractionListener(int id, Action<int> callback);

        void EnabledFreedomCameraView(Vec2 camPos, float camZoom); // allow to controll camera moving
        void DisableFreedomCameraView();
        ////////////////////////////////////////////////

        void ShowDialog();
        void HideDialog();
        void DisplayDialogText(string text, Vec3 color);
        void DisplayDialogPortrait(int objID); // -1 is clear
        void EnabledDialogTextInput(Action<string> callback);
        void DisableDialogTextInput();
        /////////////////////////////////////////////////
        
        void HideSelections();
        void ShowSelections(string[] selections, Action<int> callback);
        void DisplaySelectionVoter(int selectionIdx, AssetReference userAvatar);
    }
}