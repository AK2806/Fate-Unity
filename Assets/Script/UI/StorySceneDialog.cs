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

		public StorySceneDialogInputField input;
		private Action<string> _inputCallback = null;
		private Vector2 _touchBeginPoint = new Vector2(0, 0);
		private bool _touchBeginOnThis = false;

		// Use this for initialization
		private void Start() {
			_portraitOriginPos = portrait.GetComponent<RectTransform>().anchoredPosition;
		}
		
		// Update is called once per frame
		private void Update() {
			if (_inputCallback != null && !input.gameObject.activeSelf && Input.touchCount > 0) {
				var touch = Input.GetTouch(0);
				switch (touch.phase) {
					case TouchPhase.Began: {
							_touchBeginPoint = touch.position;
							_touchBeginOnThis = UIHelper.IsPointerOnOverlayUI(input.GetComponent<RectTransform>(), touch.position);
						}
						break;
					case TouchPhase.Ended: {
							if (!_touchBeginOnThis) break;
							var direction = touch.position - _touchBeginPoint;
							float percent = direction.y / Screen.height;
							if (percent > 0.05f) {
								input.GetComponent<InputField>().text = "";
								input.SetClosedCallback(_inputCallback);
								input.Show();
							}
						}
						break;
				}
			}
		}

		public void Show() {
			gameObject.SetActive(true);
		}

		public void Hide() {
			gameObject.SetActive(false);
		}

		public void EnabledTextInput(Action<string> callback) {
			_inputCallback = callback;
		}

		public void DisableTextInput() {
			input.Hide();
			_inputCallback = null;
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
