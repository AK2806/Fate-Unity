using System;
using System.Collections;
using System.Collections.Generic;
using FateUnity.Components.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FateUnity.Script.UI {
	public class StorySceneDialogInputField : MonoBehaviour {
		public Vector2 animatePosition;
		public Vector2 animatePositionOffset = new Vector2(0, 60.0f);
		public float animateDuration = 0.2f;

		private Vector2 _touchBeginPoint = new Vector2(0, 0);
		private bool _touchBeginOnThis = false;
		private Action<string> _inputFieldClosedCallback = null;

		private void OnValidate() {
			animatePosition = GetComponent<RectTransform>().anchoredPosition;
		}

		// Update is called once per frame
		private void Update () {
			if (Input.touchCount > 0) {
				var touch = Input.GetTouch(0);
				switch (touch.phase) {
					case TouchPhase.Began:
						_touchBeginPoint = touch.position;
						_touchBeginOnThis = UIHelper.IsPointerOnOverlayUI(GetComponent<RectTransform>(), touch.position);
						break;
					case TouchPhase.Ended:
						var direction = touch.position - _touchBeginPoint;
						float percent = direction.y / Screen.height;
						var dstPos = animatePosition;
						string result = null;
						if (percent > 0.05f) {
							if (!_touchBeginOnThis) break;
							dstPos += animatePositionOffset;
							result = GetComponent<InputField>().text;
						} else if (percent < -0.05f) {
							if (!UIHelper.IsPointerOnOverlayUI(GetComponent<RectTransform>(), touch.position)) break;
							dstPos -= animatePositionOffset;
						} else break;
						var animator = GetComponent<UIAnimator>();
						animator.DeclareConcurrentActionsBegin();
						animator.EasePosition(dstPos, animateDuration);
						animator.EaseColor(new Color(1, 1, 1, 0), animateDuration);
						animator.DeclareConcurrentActionsEnd();
						animator.CallFunc(() => {
							if (_inputFieldClosedCallback != null) {
								_inputFieldClosedCallback(result);
							}
							gameObject.SetActive(false);
						});
						break;
				}
			}
		}

		public void Show() {
			var srcPos = animatePosition - animatePositionOffset;
			var dstPos = animatePosition;
			gameObject.SetActive(true);
			GetComponent<Image>().color = new Color(1, 1, 1, 0);
			if (gameObject.activeInHierarchy) {
				GetComponent<RectTransform>().anchoredPosition = srcPos;
				var animator = GetComponent<UIAnimator>();
				animator.AbortAllActions();
				animator.DeclareConcurrentActionsBegin();
				animator.EasePosition(dstPos, animateDuration);
				animator.EaseColor(new Color(1, 1, 1, 1), animateDuration);
				animator.DeclareConcurrentActionsEnd();
			} else {
				GetComponent<RectTransform>().anchoredPosition = dstPos;
			}
		}

		public void Hide() {
			if (gameObject.activeInHierarchy) {
				var animator = GetComponent<UIAnimator>();
				var dstPos = animatePosition - animatePositionOffset;
				animator.DeclareConcurrentActionsBegin();
				animator.EasePosition(dstPos, animateDuration);
				animator.EaseColor(new Color(1, 1, 1, 0), animateDuration);
				animator.DeclareConcurrentActionsEnd();
				animator.CallFunc(() => {
					if (_inputFieldClosedCallback != null) {
						_inputFieldClosedCallback(null);
					}
					gameObject.SetActive(false);
				});
			} else {
				if (_inputFieldClosedCallback != null) {
					_inputFieldClosedCallback(null);
				}
				gameObject.SetActive(false);
			}
		}

		public void SetClosedCallback(Action<string> callback) {
			_inputFieldClosedCallback = callback;
		}
	}
}
