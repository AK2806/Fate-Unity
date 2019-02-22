using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace FateUnity.Components.UI.ObjectList {
	public class ObjectListItemView : MonoBehaviour {
		public Text nameComp;
		public Image imageComp;
		public Button interactionComp;
		public Image selectedIconComp;

		public void UpdateView(IListObject obj) {
			if (nameComp.text != obj.Name) nameComp.text = obj.Name;
			if (imageComp.sprite != obj.Image) {
				imageComp.sprite = obj.Image;
				imageComp.SetNativeSize();
			}
			var rectTransform = ((RectTransform)imageComp.transform);
			if (rectTransform.anchoredPosition != obj.ImagePos) rectTransform.anchoredPosition = obj.ImagePos;
			if (rectTransform.localScale != new Vector3(obj.ImageScale, obj.ImageScale, 1.0f)) rectTransform.localScale = new Vector3(obj.ImageScale, obj.ImageScale, 1.0f);
		}

		public void SetClickListener(UnityAction action) {
			interactionComp.onClick.RemoveAllListeners();
			interactionComp.onClick.AddListener(action);
		}

		public void Select(bool selected) {
			selectedIconComp.gameObject.SetActive(selected);
		}

		public bool IsSelected() {
			return selectedIconComp.gameObject.activeSelf;
		}
	}
}