using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI {
    public static class UIHelper {
        public static bool IsPointerOnOverlayUI(RectTransform uiRect, Vector2 screenPoint) {
            var worldCorners = new Vector3[4];
            uiRect.GetWorldCorners(worldCorners);
            var rect = new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);
            return rect.Contains(screenPoint);
        }
    }
}