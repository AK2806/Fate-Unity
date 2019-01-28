namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe struct PtrIntDict {
		public int tag;
		#if FDB
		public static int Type = Fdb.NewType("NumDict");
		public int type;
		#endif

		static class Const {
			public static readonly int[] Lens = { 3, 7, 13, 31, 61, 127, 251, 509, 1021, 2039, 4093, 8191, 16381, 32749, 65521, 131071, 262139, 524287, 1048573, 2097143, 4194301, 8388593, 16777213, 33554393, 67108859, 134217689, 268435399, 536870909, 1073741789, 2147483647 };
			public static readonly int EntrySize = sizeof(Entry);
		}

		const int IntSize = sizeof(int);

		public unsafe struct Entry {
			public int hash;
			public int next;
			public void *val;
		}

		public int level, len, count, free;
		public Entry *entries;
		public int *arr;  // at the end of entries, not allocated

		public static PtrIntDict *New() {
			var self = (PtrIntDict *)Mem.Malloc(sizeof(PtrIntDict));
			Init(self);
			return self;
		}

		public static void Decon(PtrIntDict *self) {
			#if FDB
			Verify(self);
			#endif
			Mem.Free(self->entries);
		}

		public static void Init(PtrIntDict *self) {
			#if FDB
			Should.NotNull("self", self);
			self->type = Type;
			#endif
			self->tag = Tag.PtrIntDict;
			int level = self->level = 0;
			int len = self->len = Const.Lens[level];
			self->count = 0;
			self->free = 0;
			// | (Entry)entries... | (int)arr... |
			// used entry: next: next entry in the bucket
			// free entry: next: next free entry
			int EntrySize = Const.EntrySize;
			var entries = self->entries = (Entry *)Mem.Malloc(len * (EntrySize + IntSize));
			Entry *entry = null;
			for (int i = 0; i < len; i += 1) {
				entry = entries + i;
				entry->val = null;
				entry->next = i + 1;
			}
			entry->next = -1;  // the last free entry
			int *arr = self->arr = (int *)((byte *)entries + len * EntrySize);
			for (int i = 0; i < len; i += 1) arr[i] = -1;
		}

		public static void *Get(PtrIntDict *self, int hash) {
			#if FDB
			Verify(self);
			#endif
			int cur = self->arr[(uint)hash % self->len];
			var entries = self->entries;
			while (cur != -1) {
				var entry = entries + cur;
				if (entry->hash == hash) {  // found the key
					return entry->val;
				}
				cur = entry->next;
			}
			return null;
		}

		public static bool Contains(PtrIntDict *self, int hash) {
			#if FDB
			Verify(self);
			#endif
			int cur = self->arr[(uint)hash % self->len];
			var entries = self->entries;
			while (cur != -1) {
				var entry = entries + cur;
				if (entry->hash == hash) {  // found the key
					return true;
				}
				cur = entry->next;
			}
			return false;
		}

		public static void Set(PtrIntDict *self, int hash, void *val) {
			#if FDB
			Verify(self);
			Should.NotNull("val", val);
			#endif
			int *arr = self->arr;
			long mod = (uint)hash % self->len;
			int idx = arr[mod], cur = idx;
			var entries = self->entries;
			Entry *entry = null; 
			while (cur != -1) {
				entry = entries + cur;
				if (entry->hash == hash) {  // found the key
					entry->val = val;
					#if FDB
					Verify(self);
					#endif
					return;
				}
				cur = entry->next;
			}

			// find a free entry
			int free = self->free;
			if (free == -1) {  // expand
				int level = self->level += 1;
				int oldLen = self->len;
				int len = self->len = Const.Lens[level];

				// rehash
				int EntrySize = Const.EntrySize;
				entries = self->entries = (Entry *)Mem.Realloc(entries, len * (EntrySize + IntSize));
				arr = self->arr = (int *)((byte *)entries + len * EntrySize);
				for (int i = 0; i < len; i += 1) arr[i] = -1;  // clear the buckets
				for (int i = 0; i < oldLen; i += 1) entries[i].next = -1;  // clear old links
				for (int i = 0; i < oldLen; i += 1) {  // insert all old entries
					entry = entries + i;
					mod = (uint)entry->hash % len;
					entry->next = arr[mod];
					arr[mod] = i;
				}

				// link all new entries
				self->free = oldLen + 1;  // point to new allocated entries
				for (int i = oldLen + 1; i < len; i += 1) {
					entry = entries + i;
					entry->val = null;
					entry->next = i + 1;
				}
				entry->next = -1;  // the last free entry

				entry = entries + oldLen;
				idx = arr[mod = (uint)hash % len];  // different idx after expand
				free = oldLen;
			} else {
				entry = entries + free;
				self->free = entry->next;
			}

			entry->hash = hash;
			entry->next = idx;
			entry->val = val;
			arr[mod] = free;

			self->count += 1;
			#if FDB
			Verify(self);
			#endif
		}

		public static void *Remove(PtrIntDict *self, int hash) {
			#if FDB
			Verify(self);
			#endif
			int *arr = self->arr;
			long mod = (uint)hash % self->len;
			int cur = arr[mod], prev = -1;
			var entries = self->entries;
			while (cur != -1) {
				var entry = entries + cur;
				if (entry->hash == hash) {  // found the key
					if (prev != -1) {
						entries[prev].next = entry->next;
					} else {  // the first entry in the bucket
						arr[mod] = entry->next;
					}
					// insert into the front of free list
					void *val = entry->val;
					entry->val = null;
					entry->next = self->free;
					self->free = cur;
					self->count -= 1;
					#if FDB
					Verify(self);
					#endif
					return val;
				}
				prev = cur;
				cur = entry->next;
			}
			// do nothing if no key is found
			#if FDB
			Fdb.Error("hash {0:X} does not exist in NumDict {1:X}", hash, (long)self);
			#endif
			return null;
		}

		public static void Clear(PtrIntDict *self) {
			int len = self->len;
			self->count = 0;
			self->free = 0;

			var entries = self->entries;
			Entry *entry = null;
			for (int i = 0; i < len; i += 1) {
				entry = entries + i;
				entry->val = null;
				entry->next = i + 1;
			}
			entry->next = -1;  // the last free entry

			int *arr = self->arr;
			for (int i = 0; i < len; i += 1) arr[i] = -1;
		}

		public static void ShiftBase(PtrIntDict *self, long shift) {
			#if FDB
			Verify(self);
			#endif
			var entries = self->entries;
			for (int i = 0, len = self->len; i < len; i += 1) {
				var entry = entries + i;
				void *val = entry->val;
				if (val != null) entry->val = (byte *)val + shift;
			}
		}

		#if FDB
		public static void Verify(PtrIntDict *self) {
			Should.NotNull("self", self);
			Should.InRange("self->level", self->level, 0, Const.Lens.Length - 1);
			Should.Equal("self->len", self->len, Const.Lens[self->level]);
			Should.InRange("self->count", self->count, 0, self->len);
			Should.InRange("self->free", self->free, -1, self->len - 1);
			Mem.Verify(self->entries);
			Should.NotNull("self->arr", self->arr);

			int len = self->len;
			int count = self->count;
			int j = 0;
			for (int i = 0; i < len; i += 1) {
				var entry = self->entries + i;
				if (entry->val != null) {  // a real key
					j += 1;
				}
			}
			Should.Equal("j", j, count);

			int freeCount = len - count;
			int free = self->free;
			while (free != -1) {
				freeCount -= 1;
				free = self->entries[free].next;
			}
			Should.Zero("freeCount", freeCount);

			j = 0;
			for (int i = 0; i < len; i += 1) {
				int idx = self->arr[i];
				while (idx != -1) {
					var entry = self->entries + idx;
					Should.Equal("entry->hash % len", (uint)entry->hash % len, i);
					j += 1;
					idx = entry->next;
				}
			}
//			Fdb.Dump(self->arr, self->len * IntSize);
//			Fdb.Dump(self->entries, self->len * EntrySize, EntrySize);
			Should.Equal("j", j, count);
		}

		public static void Test() {
//			TestBasic();
//			TestSetGet();
//			TestSetGetRemove();
			TestRandomSetGetRemove();
		}

		static void TestBasic() {
			var dict = stackalloc PtrIntDict[1]; Init(dict);
			Set(dict, 1, (void *)1);
			Should.Equal("(int)Get(dict, 1)", (int)Get(dict, 1), 1);
			Remove(dict, 1);
			Should.Null("Get(dict, 1)", Get(dict, 1));
		}

		static void TestSetGet() {
			var dict = stackalloc PtrIntDict[1]; Init(dict);
			for (int i = 2; i < 100; i += 1) {
				Set(dict, i, (void *)i);
				for (int j = 2; j <= i; j += 1) {
					Should.Equal("Get(dict, j)", (int)Get(dict, j), j);
				}
			}
		}

		static void TestSetGetRemove() {
			var dict = stackalloc PtrIntDict[1]; Init(dict);
			for (int i = 2; i < 100; i += 1) {
				Set(dict, i, (void *)i);
			}
			for (int i = 2; i < 100; i += 1) {
				Remove(dict, i);
				for (int j = 2; j < 100; j += 1) {
					if (j <= i) {
						Should.Null("Get(dict, j)", Get(dict, j));
					} else {
						Should.Equal("Get(dict, j)", (int)Get(dict, j), j);			
					}
				}
			}
		}

		static void TestRandomSetGetRemove() {
			var dict = stackalloc PtrIntDict[1]; Init(dict);
			var keyList = new System.Collections.Generic.List<int>();
			var valList = new System.Collections.Generic.List<int>();
			for (int i = 2; i < 100; i += 1) {
				int key = Fdb.Random(-0x8000000, 0x7fffffff);
				int val = Fdb.Random(-0x8000000, 0x7fffffff);
				if (val == 0) val = 1;
				Set(dict, key, (void *)val);
				keyList.Add(key);
				valList.Add(val);
				for (int j = 0; j < keyList.Count; j += 1) {
					Should.Equal("Get(dict, j)", (int)Get(dict, keyList[j]), valList[j]);	
				}
			}
			for (int i = 0; i < keyList.Count; i += 1) {
				Remove(dict, keyList[i]);
				for (int j = 0; j < keyList.Count; j += 1) {
					if (j <= i) {
						Should.Null("Get(dict, j)", Get(dict, keyList[j]));
					} else {
						Should.Equal("Get(dict, j)", (int)Get(dict, keyList[j]), valList[j]);			
					}
				}
			}
		}
		#endif
	}
}