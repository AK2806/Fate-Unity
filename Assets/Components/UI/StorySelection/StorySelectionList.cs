using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI.StorySelection {
	public class StorySelectionList : MonoBehaviour {
		public ScrollRect scrollRect;
		public RectTransform content;
		public StorySelectionItem itemPrefab;
		public float maxHeight = 1080.0f;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void Show(string[] selections, Action<int> callback) {
			gameObject.SetActive(true);
			content.DetachChildren();
			float contentHeight = 0.0f;
			for (int i = 0; i < selections.Length; ++i) {
				var item = Instantiate(itemPrefab);
				var itemTransform = (RectTransform)item.transform;
				itemTransform.SetParent(content, false);
				var pos = itemTransform.anchoredPosition;
				pos.y = -contentHeight;
				itemTransform.anchoredPosition = pos;
				contentHeight += itemTransform.rect.height;
				item.SelectionText = selections[i];
				int idx = i;
				item.SetButtonClickListener(() => callback(idx));
			}
			var contentSize = content.sizeDelta;
			contentSize.y = contentHeight;
			content.sizeDelta = contentSize;
			var scrollTransform = scrollRect.GetComponent<RectTransform>();
			var scrollSize = scrollTransform.sizeDelta;
			scrollSize.y = contentSize.y;
			if (scrollSize.y > maxHeight) {
				scrollSize.y = maxHeight;
			}
			scrollTransform.sizeDelta = scrollSize;
		}

		public void Hide() {
			gameObject.SetActive(false);
		}

		public void ShowVoter(int itemIdx, int voterIdx, Sprite avatar, Vector2 imgPos, float scale) {
			var item = content.GetChild(itemIdx).GetComponent<StorySelectionItem>();
			item.ShowVoter(voterIdx, avatar, imgPos, scale);
		}

		public void HideVoter(int itemIdx, int voterIdx) {
			var item = content.GetChild(itemIdx).GetComponent<StorySelectionItem>();
			item.HideVoter(voterIdx);
		}
	}
}
