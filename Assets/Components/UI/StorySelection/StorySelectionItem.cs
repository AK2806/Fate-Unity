using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace FateUnity.Components.UI.StorySelection {
	public class StorySelectionItem : MonoBehaviour {
		public Button button;
		public Text text;
		public Image[] avatars = new Image[4];

		public string SelectionText { get { return text.text; } set { text.text = value; } }

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetButtonClickListener(UnityAction callback) {
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(callback);
		}

		public void ShowVoter(int idx, Sprite avatar, Vector2 imgPos, float scale) {
			var img = avatars[idx];
			img.transform.parent.gameObject.SetActive(true);
			img.sprite = avatar;
			img.SetNativeSize();
			var trans = img.GetComponent<RectTransform>();
			trans.anchoredPosition = imgPos;
			trans.localScale = new Vector3(scale, scale, 1.0f);
		}

		public void HideVoter(int idx) {
			avatars[idx].transform.parent.gameObject.SetActive(false);
		}
	}
}
