using System;
using System.Collections;
using System.Collections.Generic;
using GameCore;

namespace GameService.Util {
	public interface IIdentifiable {
		int ID { get; }
	}
	
	public interface IReadonlyIdentifiedObjectList<T> : IEnumerable<T>, IEnumerable where T : IIdentifiable {
		int Count { get; }
		T this[int id] { get; }
		bool Contains(int id);
		bool Contains(T item);
		bool TryGetValue(int id, out T value);
		T[] ToArray();
		void CopyTo(T[] array, int arrayIndex);
	}

	public sealed class IdentifiedObjectList<T> : IEnumerable<T>, IEnumerable, IReadonlyIdentifiedObjectList<T> where T : IIdentifiable {
		private Dictionary<int, T> _table;

		public int Count { get { return _table.Count; } }

		public T this[int id] { get { return _table[id]; } }

		public IdentifiedObjectList() {
			_table = new Dictionary<int, T>();
		}

		public IdentifiedObjectList(IEnumerable<T> list) :
			this() {
			foreach (T e in list) {
				_table.Add(e.ID, e);
			}
		}

		public void Clear() {
			_table.Clear();
		}

		public void Add(T obj) {
			_table.Add(obj.ID, obj);
		}

		public bool Remove(T obj) {
			if (obj == null) return false;
			T val;
			if (!_table.TryGetValue(obj.ID, out val)) return false;
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(obj, val)) return false;
			return _table.Remove(obj.ID);
		}

		public bool Remove(int id) {
			return _table.Remove(id);
		}
		
		public bool Contains(T obj) {
			if (obj == null) return false;
			T val;
			if (!_table.TryGetValue(obj.ID, out val)) return false;
			var comparer = EqualityComparer<T>.Default;
			return comparer.Equals(val, obj);
		}

		public bool Contains(int id) {
			return _table.ContainsKey(id);
		}

		public bool Exists(Predicate<T> match) {
			foreach (var kv in _table) {
				if (match(kv.Value)) return true;
			}
			return false;
		}

		public void ForEach(Action<T> action) {
			foreach (T e in _table.Values) {
				action(e);
			}
		}

		public bool TryGetValue(int id, out T value) {
			return _table.TryGetValue(id, out value);
		}
		
		public T[] ToArray() {
			T[] ret = new T[_table.Count];
			CopyTo(ret, 0);
			return ret;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			_table.Values.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator() {
			return ((IEnumerable<T>)_table.Values).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
