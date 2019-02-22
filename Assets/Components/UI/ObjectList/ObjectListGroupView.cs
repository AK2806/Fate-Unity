using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI.ObjectList {
	public sealed class ObjectListGroupView : MonoBehaviour {
		public RectTransform title;
		public RectTransform content;
		public ObjectListItemView itemPrefab;
		public float foldMargin = 50.0f;

		private ObjectListView _listView;
		private List<ObjectListItemView> _items = new List<ObjectListItemView>();
		private bool _folded = false;

		private int cols = 1;

		public string TitleText {
			get { return title.GetComponentInChildren<Text>().text; }
			set {
				title.GetComponentInChildren<Text>().text = value;
			}
		}
		public int ItemViewCount { get { return _items.Count; } }
		public ObjectListItemView this[int idx] { get { return _items[idx]; } }

		public void InitObjectListView(ObjectListView component) {
			_listView = component;
		}

		public int IndexOfItemView(ObjectListItemView component) {
			return _items.IndexOf(component);
		}

		public void ToggleFold() {
			_folded = !_folded;
			if (_folded) {
				foreach (var item in _items) {
					item.gameObject.SetActive(false);
				}
				title.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
			} else {
				foreach (var item in _items) {
					item.gameObject.SetActive(true);
				}
				title.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
			}
		}

		public void Clear() {
			_items.Clear();
			foreach (Transform child in content) {
				Destroy(child.gameObject);
			}
		}

		public void SelectItem(int idx, bool selected) {
			_items[idx].Select(selected);
		}

		public bool IsItemSelected(int idx) {
			return _items[idx].IsSelected();
		}

		public void InsertItem(int idx, IListObject obj) {
			var item = Instantiate(itemPrefab);
			item.transform.SetParent(content, false);
			item.UpdateView(obj);
			item.SetClickListener(() => _listView.ClickItem(_listView.IndexOfGroupView(this), IndexOfItemView(item)));
			_items.Insert(idx, item);
			UpdateLayout(idx);
			if (_folded) item.gameObject.SetActive(false);
        }

		public void SetItem(int idx, IListObject newObj) {
			var item = _items[idx];
			item.UpdateView(newObj);
		}

		public void RemoveItemAt(int idx) {
			var removingItem = _items[idx];
			_items.RemoveAt(idx);
			Destroy(removingItem.gameObject);
			UpdateLayout(idx);
		}

		private void UpdateLayout(int startIdx) {
			for (int i = startIdx; i < _items.Count; ++i) {
				var item = (RectTransform)_items[i].transform;
				var size = item.sizeDelta;
				int line = i / cols;
				int col = i - line * cols;
				item.anchoredPosition = new Vector2(col * size.x, line * -size.y);
			}
		}

		private void Start() {
			float c_w = content.rect.width;
			float w = ((RectTransform)itemPrefab.transform).rect.width;
			cols = Math.Max((int)Math.Floor(c_w / w), 1);
			var c_pos = content.anchoredPosition;
			c_pos.y = -title.rect.height;
			content.anchoredPosition = c_pos;
			UpdateLayout(0);
		}

		private void LateUpdate() {
			float contentHeight = 0.0f;
			for (int i = 0; i < _items.Count; ++i) {
				var item = _items[i];
				var obj = _listView.Model.GetItem(_listView.IndexOfGroupView(this), i);
				item.UpdateView(obj);
				if (i % cols == 0) contentHeight += ((RectTransform)item.transform).rect.height;
			}
			if (_folded) contentHeight = foldMargin;
			var contentSize = content.sizeDelta;
			if (contentSize.y != contentHeight) {
				contentSize.y = contentHeight;
				content.sizeDelta = contentSize;
			}
			var self = ((RectTransform)transform);
			var size = self.sizeDelta;
			float height = title.rect.height + contentSize.y;
			if (size.y != height) {
				size.y = height;
				self.sizeDelta = size;
			}
		}
	}
}
