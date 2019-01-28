namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public static unsafe class Algo {
		public delegate int Cmp(void *a, void *b);
		public delegate bool Eq(void *a, void *b);

		public static void QuickSort(void **arr, int len, Cmp cmp) {
			#if FDB
			Should.NotNull("arr", arr);
			Should.GreaterThanZero("len", len);
			Should.NotNull("cmp", cmp);
			#endif
			QuickSort(arr, 0, len - 1, cmp);
		}
		static void QuickSort(void **arr, int low, int high, Cmp cmp) {
			if (low >= high) return;

			void *pivot = arr[low]; void *swap;
			int mid = low;
			for (int j = low + 1; j <= high; j += 1) {
				if (cmp(swap = arr[j], pivot) < 0) {
					mid += 1;
					arr[j] = arr[mid];
					arr[mid] = swap;
				}
			}
			if (mid != low) {
				swap = arr[mid];
				arr[mid] = arr[low];
				arr[low] = swap;
			}

			QuickSort(arr, low, mid - 1, cmp);
			QuickSort(arr, mid + 1, high, cmp);
		}

		public static void *QuickSelect(void **arr, int len, int k, Cmp cmp) {
			void **temp = stackalloc void *[len];
			Mem.Memcpy(temp, arr, len * sizeof(void *));
			return QuickSelect(temp, 0, len - 1, k, cmp);
		}
		static void *QuickSelect(void **arr, int low, int high, int k, Cmp cmp) {
			#if FDB
			if (low >= high) {
				Should.Equal("low", low, high);
				Should.Equal("k", k, low);
			}
			#endif
			if (low >= high) return arr[low];

			void *pivot = arr[low]; void *swap;
			int mid = low;
			for (int j = low + 1; j <= high; j += 1) {
				if (cmp(swap = arr[j], pivot) < 0) {
					mid += 1;
					arr[j] = arr[mid];
					arr[mid] = swap;
				}
			}
			if (mid != low) {
				swap = arr[mid];
				arr[mid] = arr[low];
				arr[low] = swap;
			}

			if (mid == k) return arr[mid];
			return k < mid ? QuickSelect(arr, low, mid - 1, k, cmp) : QuickSelect(arr, mid + 1, high, k, cmp);
		}

		public static void MergeSort(void **arr, int len, Cmp cmp) {
			#if FDB
			Should.NotNull("arr", arr);
			Should.GreaterThanZero("len", len);
			Should.NotNull("cmp", cmp);
			#endif
			void **temp = stackalloc void *[len];
			MergeSort(arr, temp, 0, len - 1, cmp);
		}
		static void MergeSort(void **arr, void **temp, int low, int high, Cmp cmp) {
			if (low >= high) return;

			void *swap;
			if (low == high - 1) {
				if (cmp(swap = arr[low], arr[high]) > 0) {
					arr[low] = arr[high];
					arr[high] = swap;
				}
				return;
			}

			int mid = (low + high) >> 1;
			MergeSort(arr, temp, low, mid, cmp);
			MergeSort(arr, temp, mid + 1, high, cmp);

			int i = low, j = mid + 1, k = low;
			while (i <= mid && j <= high) {
				if (cmp(arr[i], arr[j]) < 0) {
					temp[k++] = arr[i++];
				} else {
					temp[k++] = arr[j++];
				}
			}
			while (i <= mid) temp[k++] = arr[i++];
			while (j <= high) temp[k++] = arr[j++];

			for (k = low; k <= high; k += 1) arr[k] = temp[k];
		}

		public static int BinarySearch(void **arr, int len, void *val, Cmp cmp) {
			#if FDB
			Should.NotNull("arr", arr);
			Should.GreaterThanZero("len", len);
			Should.NotNull("cmp", cmp);
			#endif
			return BinarySearch(arr, 0, len - 1, val, cmp);
		}
		static int BinarySearch(void **arr, int low, int high, void *val, Cmp cmp) {
			if (low > high) return -1;

			int mid = (low + high) >> 1;
			int r = cmp(val, arr[mid]);

			if (r == 0) return mid;
			if (low == high) return -1;

			return r < 0 ? BinarySearch(arr, 0, mid - 1, val, cmp) : BinarySearch(arr, mid + 1, high, val, cmp);
		}

		#if FDB
		public static void Test() {
			TestQuickSort();
			TestQuickSelect();
			TestMergeSort();
			TestBinarySearch();
		}

		static void TestBinarySearch() {
			const int len = 100;
			var arr = stackalloc void *[len];
			for (int i = 0; i < len; i += 1) {
				arr[i] = (void *)i;
			}
			for (int i = 0; i < len; i += 1) {
				int res = BinarySearch(arr, len, (void *)i, (a, b) => (int)a - (int)b);
				Should.Equal("res", res, i);
			}
		}

		static void TestQuickSort() {
			const int len = 100;
			var arr = stackalloc void *[len];
			for (int i = 0; i < len; i += 1) {
				arr[i] = (void *)Fdb.Random(-len, len);
			}
			QuickSort(arr, len, (a, b) => (int)a - (int)b);
			for (int i = 1; i < len; i += 1) {
				Should.LessThanOrEqualTo("(int)arr[i - 1]", (int)arr[i - 1], (int)arr[i]);
			}
		}

		static void TestMergeSort() {
			const int len = 100;
			var arr = stackalloc void *[len];
			for (int i = 0; i < len; i += 1) {
				arr[i] = (void *)Fdb.Random(-len, len);
			}
			MergeSort(arr, len, (a, b) => (int)a - (int)b);
			for (int i = 1; i < len; i += 1) {
				Should.LessThanOrEqualTo("(int)arr[i - 1]", (int)arr[i - 1], (int)arr[i]);
			}
		}

		static void TestQuickSelect() {
			const int len = 100;
			var arr = stackalloc void *[len];
			for (int i = 0; i < len; i += 1) {
				arr[i] = (void *)i;
			}
			for (int i = len - 1; i >= 0; i -= 1) {
				int k = Fdb.Random(0, i);
				void *swap = arr[k];
				arr[k] = arr[i];
				arr[i] = swap;
			}
			for (int i = 0; i < len; i += 1) {
				void *res = QuickSelect(arr, len, i, (a, b) => (int)a - (int)b);
				Should.Equal("res", res, (void *)i);
			}
		}
		#endif
	}
}

