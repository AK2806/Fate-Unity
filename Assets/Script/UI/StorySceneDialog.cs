using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FateUnity.Components.UI;

namespace FateUnity.Script.UI {
	public class StorySceneDialog : MonoBehaviour {
		public Image portrait;
		private Vector2 _portraitOriginPos;

		public Text output;
		public float outputTextInterval = 0.03f;

		public InputField input;
		public Vector2 inputAnimatePositionOffset = new Vector2(0, 60.0f);
		public float inputAnimateDuration = 0.2f;
		private bool _inputActive = false;
		private Vector2 _inputAnimatePos;
		private Vector2 _touchBeginPoint = new Vector2(0, 0);
		private bool _touchBeginOnInput = false;
		private Action<string> _inputFieldClosedCallback = null;

		private void OnValidate() {
			_inputAnimatePos = input.GetComponent<RectTransform>().anchoredPosition;
			_portraitOriginPos = portrait.GetComponent<RectTransform>().anchoredPosition;
		}

		// Update is called once per frame
		private void Update() {
			if (_inputFieldClosedCallback != null && Input.touchCount > 0) {
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began) {
					_touchBeginPoint = touch.position;
					_touchBeginOnInput = UIHelper.IsPointerOnOverlayUI(input.GetComponent<RectTransform>(), touch.position);
				}
				do {
					if (touch.phase == TouchPhase.Ended) {
						if (!_inputActive) {
							if (!_touchBeginOnInput) break;
							var direction = touch.position - _touchBeginPoint;
							float percent = direction.y / Screen.height;
							if (percent > 0.05f) {
								var srcPos = _inputAnimatePos - inputAnimatePositionOffset;
								var dstPos = _inputAnimatePos;
								input.gameObject.SetActive(true);
								input.GetComponent<InputField>().text = "";
								input.GetComponent<Image>().color = new Color(1, 1, 1, 0);
								input.GetComponent<RectTransform>().anchoredPosition = srcPos;
								var animator = input.GetComponent<UIAnimator>();
								animator.AbortAllActions();
								animator.DeclareConcurrentActionsBegin();
								animator.EasePosition(dstPos, inputAnimateDuration);
								animator.EaseColor(new Color(1, 1, 1, 1), inputAnimateDuration);
								animator.DeclareConcurrentActionsEnd();
								_inputActive = true;
							}
						} else {
							var direction = touch.position - _touchBeginPoint;
							float percent = direction.y / Screen.height;
							var dstPos = _inputAnimatePos;
							string result = null;
							if (percent > 0.05f) {
								if (!_touchBeginOnInput) break;
								dstPos += inputAnimatePositionOffset;
								result = input.GetComponent<InputField>().text;
							} else if (percent < -0.05f) {
								if (!UIHelper.IsPointerOnOverlayUI(input.GetComponent<RectTransform>(), touch.position)) break;
								dstPos -= inputAnimatePositionOffset;
							} else break;
							var animator = input.GetComponent<UIAnimator>();
							animator.SkipAllActions();
							animator.DeclareConcurrentActionsBegin();
							animator.EasePosition(dstPos, inputAnimateDuration);
							animator.EaseColor(new Color(1, 1, 1, 0), inputAnimateDuration);
							animator.DeclareConcurrentActionsEnd();
							animator.CallFunc(() => input.gameObject.SetActive(false));
							_inputFieldClosedCallback(result);
							_inputActive = false;
						}
					}
				} while(false);
			}
		}

		public void Show() {
			gameObject.SetActive(true);
		}

		public void Hide() {
			gameObject.SetActive(false);
		}
		
		public void EnabledTextInput(Action<string> callback) {
			_inputFieldClosedCallback = callback;
		}

		public void DisableTextInput() {
			if (_inputActive) {
				var dstPos = _inputAnimatePos - inputAnimatePositionOffset;
				var animator = input.GetComponent<UIAnimator>();
				animator.SkipAllActions();
				animator.DeclareConcurrentActionsBegin();
				animator.EasePosition(dstPos, inputAnimateDuration);
				animator.EaseColor(new Color(1, 1, 1, 0), inputAnimateDuration);
				animator.DeclareConcurrentActionsEnd();
				animator.CallFunc(() => input.gameObject.SetActive(false));
				if (_inputFieldClosedCallback != null) _inputFieldClosedCallback(null);
				_inputActive = false;
			}
			_inputFieldClosedCallback = null;
		}

		public void DisplayText(string text) {
			StartCoroutine(TextAnimChange(text));
		}

		public void SetTextColor(Color color) {
			output.color = color;
		}

		private IEnumerator TextAnimChange(string str) {
			output.text = "";
			for (int i = 0; i < str.Length; ++i) {
				yield return new WaitForSeconds(outputTextInterval);
				output.text += str[i];
			}
		}

		public void SetPortrait(Sprite sprite, Vector2 offset, float scale) {
			var transform = portrait.GetComponent<RectTransform>();
			portrait.sprite = sprite;
			portrait.SetNativeSize();
			portrait.color = Color.white;
			transform.anchoredPosition = _portraitOriginPos + offset;
			transform.localScale = new Vector3(scale, scale, 1.0f);
		}

		public void ClearPortrait() {
			portrait.sprite = null;
			portrait.color = new Color(1, 1, 1, 0);
		}
	}
}
