using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateUnity.UI {
	public class StoryDialogGesturesHandler : MonoBehaviour {
		private Vector2 _startPoint = new Vector2(0, 0);

		// Use this for initialization
		private void Start () {
			
		}
		
		// Update is called once per frame
		private void Update () {
			if (Input.touchCount > 0) {
				var touch = Input.GetTouch(0);
				switch (touch.phase) {
					case TouchPhase.Began:
						_startPoint = touch.position;
						break;
					case TouchPhase.Ended:
						var direction = touch.position - _startPoint;
						Debug.Log(direction);
						break;
				}
			}
		}

		public void Log(string s) {
			Debug.Log(s);
		}
	}
}
