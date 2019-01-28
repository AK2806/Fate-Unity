namespace Futilef {
	public static class TchPhase {
		public const int Down = 0, Stay = 1, Up = 2, None = 3;
		public static int FromUnityTouch(UnityEngine.TouchPhase uPhase) {
			switch (uPhase) {
			case UnityEngine.TouchPhase.Began: return Down;
			case UnityEngine.TouchPhase.Moved:
			case UnityEngine.TouchPhase.Stationary: return Stay;
			case UnityEngine.TouchPhase.Canceled:
			case UnityEngine.TouchPhase.Ended: return Up;
			}
			return None;
		}

		public static int FromUnityMouse(int mouse) {
			if (UnityEngine.Input.GetMouseButtonDown(mouse)) return Down;
			if (UnityEngine.Input.GetMouseButtonUp(mouse)) return Up;
			if (UnityEngine.Input.GetMouseButton(mouse)) return Stay;
			return None;
		}
	}
}

