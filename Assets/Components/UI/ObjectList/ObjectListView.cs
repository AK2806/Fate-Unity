using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI.ObjectList {
	public sealed class ObjectListViewClosedEvent {
		public bool close = true;
		public Vector2Int[] selections = null;
	}
	
	public sealed class ObjectListView : MonoBehaviour, IObjectListObserver {
		public RectTransform content;
		public ObjectListGroupView groupPrefab;
		public Button cancelButton;
		public Button okButton;

		private IObjectListModel _model = null;
		private List<ObjectListGroupView> _groups = new List<ObjectListGroupView>();
		private ObjectListGroupView _selectedGroupInSingleChosenMode = null;
		private ObjectListItemView _selectedItemInSingleChosenMode = null;

		public IObjectListModel Model { get { return _model; } }
		public int GroupViewCount { get { return _groups.Count; } }
		public ObjectListGroupView this[int idx] { get { return _groups[idx]; } }

		public event Action<ListObjectSelectedEvent> SelectionStatusChanged = null;
		public event Action<ObjectListViewClosedEvent> WindowClosed = null;

		public void BindModel(IObjectListModel model) {
			if (_model != null) _model.RemoveObserver(this);
			_model = model;
			if (_model != null) _model.AddObserver(this);
		}

		void IObjectListObserver.Cleared() {
			_groups.Clear();
			foreach (Transform child in content) {
				Destroy(child.gameObject);
			}
		}

        void IObjectListObserver.GroupInserted(int groupIdx, string name) {
			var groupView = Instantiate(groupPrefab);
			var groupTransform = (RectTransform)groupView.transform;
			groupTransform.SetParent(content, false);
			groupView.TitleText = name;
			groupView.InitObjectListView(this);
			_groups.Insert(groupIdx, groupView);
		}

        void IObjectListObserver.GroupRemoved(int groupIdx) {
			var group = _groups[groupIdx];
			_groups.RemoveAt(groupIdx);
			Destroy(group.gameObject);
		}

        void IObjectListObserver.ItemCleared(int groupIdx) {
			_groups[groupIdx].Clear();
		}

        void IObjectListObserver.ItemInserted(int groupIdx, int itemIdx, IListObject item) {
			var group = _groups[groupIdx];
			group.InsertItem(itemIdx, item);
		}

        void IObjectListObserver.ItemModified(int groupIdx, int itemIdx, IListObject newItem) {
			var group = _groups[groupIdx];
			group.SetItem(itemIdx, newItem);
		}

        void IObjectListObserver.ItemRemoved(int groupIdx, int itemIdx) {
			var group = _groups[groupIdx];
			group.RemoveItemAt(itemIdx);
		}

		public int IndexOfGroupView(ObjectListGroupView component) {
			return _groups.IndexOf(component);
		}

		public void ClickItem(int groupIdx, int itemIdx) {
			ListObjectSelectedEvent e;
			if (_model.MultipleChosen) {
				bool select = !IsItemSelected(groupIdx, itemIdx);
				SelectItem(groupIdx, itemIdx, select);
				e = new ListObjectSelectedEvent() { groupIdx = groupIdx, itemIdx = itemIdx, chosen = select};
				if (SelectionStatusChanged != null) SelectionStatusChanged(e);
			} else {
				var group = _groups[groupIdx];
				var item = group[itemIdx];
				if (_selectedItemInSingleChosenMode != item) {
					if (_selectedGroupInSingleChosenMode != null && _selectedItemInSingleChosenMode != null) {
						int selectedGroupIdx = IndexOfGroupView(_selectedGroupInSingleChosenMode);
						int selectedItemIdx = _selectedGroupInSingleChosenMode.IndexOfItemView(_selectedItemInSingleChosenMode);
						SelectItem(selectedGroupIdx, selectedItemIdx, false);
						e = new ListObjectSelectedEvent() { groupIdx = selectedGroupIdx, itemIdx = selectedItemIdx, chosen = false};
						if (SelectionStatusChanged != null) SelectionStatusChanged(e);
					}
					SelectItem(groupIdx, itemIdx, true);
					_selectedGroupInSingleChosenMode = group;
					_selectedItemInSingleChosenMode = item;
					e = new ListObjectSelectedEvent() { groupIdx = groupIdx, itemIdx = itemIdx, chosen = true};
					if (SelectionStatusChanged != null) SelectionStatusChanged(e);
				}
			}
		}

		public void SelectItem(int groupIdx, int itemIdx, bool selected) {
			_groups[groupIdx].SelectItem(itemIdx, selected);
		}

		public bool IsItemSelected(int groupIdx, int itemIdx) {
			return _groups[groupIdx].IsItemSelected(itemIdx);
		}

		private void Start() {
			okButton.onClick.AddListener(() => {
				if (WindowClosed != null) {
					var selections = new LinkedList<Vector2Int>();
					for (int i = 0; i < _groups.Count; ++i) {
						var groupView = _groups[i];
						for (int j = 0; j < groupView.ItemViewCount; ++j) {
							var itemView = groupView[j];
							if (itemView.IsSelected()) {
								selections.AddLast(new Vector2Int(i, j));
							}
						}
					}
					var ret = new Vector2Int[selections.Count];
					selections.CopyTo(ret, 0);
					var e = new ObjectListViewClosedEvent();
					WindowClosed(e);
					if (e.close) Destroy(this.gameObject);
				}
			});
			cancelButton.onClick.AddListener(() => {
				if (WindowClosed != null) {
					var e = new ObjectListViewClosedEvent();
					WindowClosed(e);
					if (e.close) {
						Destroy(this.gameObject);
					}
				} else {
					Destroy(this.gameObject);
				}
			});
		}

		private void LateUpdate() {
			float contentHeight = 0.0f;
			for (int i = 0; i < _groups.Count; ++i) {
				var group = (RectTransform)_groups[i].transform;
				var pos = group.anchoredPosition;
				if (pos.y != -contentHeight) {
					pos.y = -contentHeight;
					group.anchoredPosition = pos;
				}
				contentHeight += group.rect.height;
			}
			var contentSize = content.sizeDelta;
			if (contentSize.y != contentHeight) {
				contentSize.y = contentHeight;
				content.sizeDelta = contentSize;
			}
		}
	}
}
