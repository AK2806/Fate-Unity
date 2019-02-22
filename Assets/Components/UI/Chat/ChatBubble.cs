using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI.Chat {
	public class ChatBubble : MonoBehaviour {
		public Text textComp;
		public Vector2 textLeftTopMargin = new Vector2(42, 15);
		public Vector2 textRightBottomMargin = new Vector2(20, 15);
		public float textMaxWidth = 600.0f;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			var transform = GetComponent<RectTransform>();
			var textTransform = textComp.GetComponent<RectTransform>();
			var textPos = new Vector2(textLeftTopMargin.x, -textLeftTopMargin.y);
			if (textTransform.anchoredPosition != textPos) {
				textTransform.anchoredPosition = textPos;
			}
			if (textTransform.rect.width > textMaxWidth) {
				var sizeFitter = textComp.GetComponent<ContentSizeFitter>();
				sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
				var textSizeDelta = textTransform.sizeDelta;
				textSizeDelta.x = textMaxWidth;
				textTransform.sizeDelta = textSizeDelta;
			}
			var transformSizeDelta = textLeftTopMargin + textTransform.rect.size + textRightBottomMargin;
			if (transform.sizeDelta != transformSizeDelta) {
				transform.sizeDelta = transformSizeDelta;
			}
		}

		public void SetChatText(string text) {
			var sizeFitter = textComp.GetComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			textComp.text = text;
		}

		public void Show() {
			gameObject.SetActive(true);
		}

		public void Hide() {
			gameObject.SetActive(false);
		}
	}
}