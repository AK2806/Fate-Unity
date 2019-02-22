using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateUnity.Components.UI.ObjectList {
    public interface IListObject {
        string Name { get; }
        Sprite Image { get; }
        Vector2 ImagePos { get; }
        float ImageScale { get; }
    }

    public interface IObjectListModel {
        bool MultipleChosen { get; }
        int GetGroupCount();
        int GetItemCount(int groupIdx);
        IListObject GetItem(int groupIdx, int itemIdx);
        void AddObserver(IObjectListObserver observer);
        void RemoveObserver(IObjectListObserver observer);
    }
    
    public sealed class ListObjectSelectedEvent {
        public int groupIdx;
        public int itemIdx;
        public bool chosen;
    }

    public interface IObjectListObserver {
        void Cleared();
        void GroupInserted(int groupIdx, string name);
        void GroupRemoved(int groupIdx);
        void ItemCleared(int groupIdx);
        void ItemInserted(int groupIdx, int itemIdx, IListObject item);
        void ItemModified(int groupIdx, int itemIdx, IListObject newItem);
        void ItemRemoved(int groupIdx, int itemIdx);
    }

    public struct DefaultListObject : IListObject {
        public string name;
        public Sprite image;
        public Vector2 imagePos;
        public float imageScale;

        string IListObject.Name { get { return name; } }
        Sprite IListObject.Image { get { return image; } }
        Vector2 IListObject.ImagePos { get { return imagePos; } }
        float IListObject.ImageScale { get { return imageScale; } }
    }

    public class DefaultObjectListModel<T> : IObjectListModel where T : IListObject {
        private struct Group {
            public string name;
            public List<T> items;
        }

        private bool _multichosen = false;
        private LinkedList<IObjectListObserver> _observers = new LinkedList<IObjectListObserver>();
        private List<Group> _groups = new List<Group>();
        private readonly string _defaultGroupName;

        public bool MultipleChosen { get { return _multichosen; } set { _multichosen = value; } }
        public string DefaultGroupName { get { return _defaultGroupName; } }
        public T this[int groupIdx, int itemIdx] {
            get { return _groups[groupIdx].items[itemIdx]; }
            set {
                _groups[groupIdx].items[itemIdx] = value;
                foreach (var observer in _observers) {
                    observer.ItemModified(groupIdx, itemIdx, value);
                }
            }
        }

        public DefaultObjectListModel(string defaultGroupName = "全部") {
            _defaultGroupName = defaultGroupName;
            _groups.Add(new Group() { name = _defaultGroupName, items = new List<T>() });
        }

        public int GetGroupCount() {
            return _groups.Count;
        }

        public int GetItemCount(int groupIdx) {
            var group = _groups[groupIdx];
            return group.items.Count;
        }

        IListObject IObjectListModel.GetItem(int groupIdx, int itemIdx) {
            return this[groupIdx, itemIdx];
        }

        void IObjectListModel.AddObserver(IObjectListObserver observer) {
            _observers.AddLast(observer);
            observer.Cleared();
            int i = 0, j = 0;
            foreach (var group in _groups) {
                observer.GroupInserted(i, group.name);
                foreach (var item in group.items) {
                    observer.ItemInserted(i, j++, item);
                }
                j = 0; ++i;
            }
        }

        void IObjectListModel.RemoveObserver(IObjectListObserver observer) {
            _observers.Remove(observer);
            observer.Cleared();
        }

        public void Clear() {
            _groups.Clear();
            foreach (var observer in _observers) {
                observer.Cleared();
            }
        }

        public void AddGroup(string name) {
            _groups.Add(new Group() { name = name, items = new List<T>() });
            foreach (var observer in _observers) {
                observer.GroupInserted(_groups.Count - 1, name);
            }
        }

        public void InsertGroup(int idx, string name) {
            _groups.Insert(idx, new Group() { name = name, items = new List<T>() });
            foreach (var observer in _observers) {
                observer.GroupInserted(idx, name);
            }
        }

        public void RemoveGroup(string name) {
            RemoveGroupAt(_groups.FindIndex(group => group.name == name));
        }

        public void RemoveGroupAt(int idx) {
            _groups.RemoveAt(idx);
            foreach (var observer in _observers) {
                observer.GroupRemoved(idx);
            }
        }

        public bool ContainsGroup(string name) {
            return _groups.Exists(group => group.name == name);
        }

        public int IndexOfGroup(string name, int idx = 0, int cnt = -1) {
            if (cnt == -1) return _groups.FindIndex(idx, group => group.name == name);
            else return _groups.FindIndex(idx, cnt, group => group.name == name);
        }
        
        public void ClearItems(int groupIdx = 0) {
            _groups[groupIdx].items.Clear();
            foreach (var observer in _observers) {
                observer.ItemCleared(groupIdx);
            }
        }

        public void AddItem(T item, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.Add(item);
            foreach (var observer in _observers) {
                observer.ItemInserted(groupIdx, list.Count - 1, item);
            }
        }

        public void InsertItem(int itemIdx, T item, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.Insert(itemIdx, item);
            foreach (var observer in _observers) {
                observer.ItemInserted(groupIdx, itemIdx, item);
            }
        }

        public bool RemoveItem(T item, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            int idx = list.IndexOf(item);
            if (idx == -1) {
                return false;
            } else {
                RemoveItemAt(idx, groupIdx);
                return true;
            }
        }

        public void RemoveItemAt(int itemIdx, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.RemoveAt(itemIdx);
            foreach (var observer in _observers) {
                observer.ItemRemoved(groupIdx, itemIdx);
            }
        }

        public bool ContainsItem(T item, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
			return list.Contains(item);
		}

        public int IndexOfItem(T item, int idx = 0, int cnt = -1, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            if (cnt == -1) return list.IndexOf(item, idx);
            else return list.IndexOf(item, idx, cnt);
        }
        
        public bool ExistsItem(Predicate<T> match, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            return list.Exists(match);
        }

        public void ForEachItem(Action<T> action, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.ForEach(action);
        }

        public T[] ToItemArray(int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            return list.ToArray();
        }

        public void CopyItemsTo(T[] array, int arrayIndex, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.CopyTo(array, arrayIndex);
        }

        public void CopyItemsTo(int index, T[] array, int arrayIndex, int count, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyItemsTo(T[] array, int groupIdx = 0) {
            var list = _groups[groupIdx].items;
            list.CopyTo(array);
        }
    }
}
