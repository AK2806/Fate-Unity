namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe struct PtrLst {
		public int tag;
		#if FDB
		public static readonly int Type = Fdb.NewType("PtrLst2");
		public int type;
		#endif

		public int len, count, size;
		public void **arr;

		public static PtrLst *New() {
			var self = (PtrLst *)Mem.Malloc(sizeof(PtrLst));
			Init(self);
			return self;
		}

		public static void Decon(PtrLst *self) {
			#if FDB
			Verify(self);
			#endif
			Mem.Free(self->arr);
		}

		public static void Init(PtrLst *self) {
			Init(self, 4);
		}
		public static void Init(PtrLst *self, int len) {
			#if FDB
			Should.NotNull("self", self);
			Should.GreaterThanZero("len", len);
			self->type = Type;
			#endif
			self->tag = Tag.PtrLst;
			self->len = len;
			self->count = 0;
			int size = self->size = sizeof(void *);
			self->arr = (void **)Mem.Malloc(len * size);
		}

		public static void Push(PtrLst *self, void *ptr) {
			#if FDB
			Verify(self);
			#endif
			int oldCount = self->count;
			self->count = oldCount + 1;
			if (oldCount >= self->len) {  // resize
				self->arr = (void **)Mem.Realloc(self->arr, (self->len <<= 1) * self->size);
			}
			self->arr[oldCount] = ptr;
		}

		public static void RemoveAt(PtrLst *self, int idx) {
			#if FDB
			Verify(self);
			Should.InRange("idx", idx, 0, self->count - 1);
			#endif
			byte *src = (byte *)(self->arr + idx);
			int size = self->size;
			int count = self->count -= 1;
//			void **arr = self->arr;
//			for (int i = idx; i < count; i += 1) arr[i] = arr[i + 1];
			Mem.Memmove(src, src + size, (count - idx) * size);
		}

		public static void Remove(PtrLst *self, void *ptr) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			int oldCount = self->count;
			#endif
			void **arr = self->arr;
			int i = 0, len = self->count;
			while (i < len && arr[i] != ptr) i += 1;
			if (i < len) {  // arr[i] == p
				// for (len = self->count -= 1; i < len; i += 1) arr[i] = arr[i + 1];
				byte* src = (byte *)(arr + i);
				int size = self->size;
				int count = self->count -= 1;
				Mem.Memmove(src, src + size, (count - i) * size);
			}
			#if FDB
			else Fdb.Error("{0:X} does not exist in PtrLst {1:X}", (ulong)ptr, (ulong) self);
			Should.Equal("self->count", self->count, oldCount - 1);
			#endif
		}

		public static void Clear(PtrLst *self) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			self->count = 0;
		}

		public static void ShiftBase(PtrLst *self, long shift) {
			#if FDB
			Verify(self);
			#endif
			void **arr = self->arr;
			for (int i = 0, count = self->count; i < count; i += 1) {
				*(byte **)(arr + i) += shift;
			}
		}


		#if FDB
		public static void Verify(PtrLst *self) {
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			int len = Mem.Verify(self->arr) / sizeof(void *);
			Should.Equal("self->len", self->len, len);
			Should.InRange("self->count", self->count, 0, self->len);
			Should.Equal("self->size", self->size, sizeof(void *));
		}

		public static void Test() {
			TestPush();
			TestRemoveAt();
			TestPushRemovePush();
			TestShiftPtr();
		}

		static void TestPush() {
			const int len = 1000;
			var arr = stackalloc int[len];
			var lst = stackalloc PtrLst[1]; Init(lst);
			for (int i = 0; i < len; i += 1) {
				arr[i] = Fdb.Random(0, len);
				Push(lst, (void *)arr[i]);
			}
			void **ptr = lst->arr;
			for (int i = 0; i < len; i += 1) {
				Should.Equal("(int)ptr[i]", (int)ptr[i], arr[i]);
			}
		}

		static void TestRemoveAt() {
			const int len = 1000;
			var arr = new System.Collections.Generic.List<int>();
			var lst = stackalloc PtrLst[1]; Init(lst);
			for (int i = 0; i < len; i += 1) {
				int v = Fdb.Random(0, len);
				arr.Add(v);
				Push(lst, (void *)v);
			}
			for (int i = 0; i < len >> 1; i += 1) {
				int idx = Fdb.Random(0, arr.Count);
				arr.RemoveAt(idx);
				RemoveAt(lst, idx);
			}
			void **ptr = lst->arr;
			for (int i = 0; i < arr.Count; i += 1) {
				Should.Equal("(int)ptr[i]", (int)ptr[i], arr[i]);
			}
		}

		static void TestPushRemovePush() {
			const int len = 1000;
			var arr = new System.Collections.Generic.List<int>();
			var lst = stackalloc PtrLst[1]; Init(lst);
			for (int i = 0; i < len; i += 1) {
				int v = Fdb.Random(0, len);
				arr.Add(v);
				Push(lst, (void *)v);
			}
			for (int i = 0; i < len >> 1; i += 1) {
				int idx = Fdb.Random(0, arr.Count);
				arr.RemoveAt(idx);
				RemoveAt(lst, idx);
			}
			for (int i = 0; i < len; i += 1) {
				int v = Fdb.Random(0, len);
				arr.Add(v);
				Push(lst, (void *)v);
			}
			void **ptr = lst->arr;
			for (int i = 0; i < arr.Count; i += 1) {
				Should.Equal("(int)ptr[i]", (int)ptr[i], arr[i]);
			}
		}

		static void TestShiftPtr() {
			const int len = 1000;
			var arr = stackalloc int[len];
			var lst = stackalloc PtrLst[1]; Init(lst);
			for (int i = 0; i < len; i += 1) {
				arr[i] = Fdb.Random(0, len);
				Push(lst, (void *)arr[i]);
			}
			long shift = Fdb.Random(-1000, 1000);
			ShiftBase(lst, shift);
			void **ptr = lst->arr;
			for (int i = 0; i < len; i += 1) {
				Should.Equal("(int)ptr[i]", (long)ptr[i], arr[i] + shift);
			}
		}
		#endif
	}
}