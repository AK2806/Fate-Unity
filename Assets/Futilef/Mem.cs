using System.Runtime.InteropServices;

namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe static class Mem {
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		[DllImport("msvcrt.dll", EntryPoint = "malloc", CallingConvention=CallingConvention.Cdecl)]
		static extern void *msvcrt_malloc(int size);
		[DllImport("msvcrt.dll", EntryPoint = "realloc", CallingConvention=CallingConvention.Cdecl)]
		static extern void *msvcrt_realloc(void *ptr, int size);
		[DllImport("msvcrt.dll", EntryPoint = "free", CallingConvention=CallingConvention.Cdecl)]
		static extern void msvcrt_free(void *ptr);
		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
		static extern void *msvcrt_memcpy(void *dst, void *src, int count);
		[DllImport("msvcrt.dll", EntryPoint = "memmove", CallingConvention = CallingConvention.Cdecl)]
		static extern void *msvcrt_memmove(void *dst, void *src, int count);
		[DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl)]
		static extern void *msvcrt_memset(void *ptr, int value, int count);
		#elif UNITY_ANDROID
		[DllImport("native")]
		static extern void *ndk_malloc(int size);
		[DllImport("native")]
		static extern void *ndk_realloc(void *ptr, int size);
		[DllImport("native")]
		static extern void ndk_free(void *ptr);
		[DllImport("native")]
		static extern void *ndk_memcpy(void *dst, void *src, int count);
		[DllImport("native")]
		static extern void *ndk_memmove(void *dst, void *src, int count);
		[DllImport("native")]
		static extern void *ndk_memset(void *ptr, int value, int count);
		#endif

		#if FDB
		public static readonly int Type = Fdb.NewType("Mem");

		// | (int)len | (byte)mem... | (int)Type |
		public static int Verify(void *mem) {
			Should.NotNull("mem", mem);
			int len = *(int *)((byte *)mem - sizeof(int));
			Should.GreaterThanZero("*(int *)((byte *)mem - sizeof(int))", len);
			int type = *(int *)((byte *)mem + len);
			Should.TypeEqual("*(int *)((byte *)mem + len)", type, Type);
			return len;
		}
		#endif

		public static void *Malloc(int size) {
			#if FDB
			Should.GreaterThanZero("size", size);
			size += sizeof(int) * 2;
			byte *p = (byte *)
			#else
			return
			#endif
				#if UNITY_EDITOR || UNITY_STANDALONE_WIN
				msvcrt_malloc(size);
				#elif UNITY_ANDROID
				ndk_malloc(size);
				#endif
			#if FDB
			*(int *)p = size - sizeof(int) * 2;
			*(int *)(p + size - sizeof(int)) = Type;
			return p + sizeof(int);
			#endif
		}

		public static void *Realloc(void *ptr, int size) {
			#if FDB
			Verify(ptr);
			Should.GreaterThan("n", size, 0);
			ptr = ((byte *)ptr - sizeof(int));
			size += sizeof(int) * 2;
			byte *p = (byte *)
			#else
			return
			#endif
				#if UNITY_EDITOR || UNITY_STANDALONE_WIN
				msvcrt_realloc(ptr, size);
				#elif UNITY_ANDROID
				ndk_realloc(ptr, size);
				#endif
			#if FDB
			*(int *)p = size - sizeof(int) * 2;
			*(int *)(p + size - sizeof(int)) = Type;
			return p + sizeof(int);
			#endif
		}

		public static void Free(void *ptr) {
			#if FDB
			Verify(ptr);
			ptr = ((byte *)ptr - sizeof(int));
			#endif
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			msvcrt_free(ptr);
			#elif UNITY_ANDROID
			ndk_free(ptr);
			#endif
		}

		public static void Memcpy(void *dst, void *src, int count) {
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			msvcrt_memcpy(dst, src, count);
			#elif UNITY_ANDROID
			ndk_memcpy(dst, src, count);
			#endif
		}

		public static void Memmove(void *dst, void *src, int count) {
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			msvcrt_memmove(dst, src, count);
			#elif UNITY_ANDROID
			ndk_memmove(dst, src, count);
			#endif
		}

		public static void Memset(void *ptr, int value, int count) {
			#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			msvcrt_memset(ptr, value, count);
			#elif UNITY_ANDROID
			ndk_memset(ptr, value, count);
			#endif
		}

		#if FDB
		public static void Test() {
			void *a = Malloc(64);
			Should.Equal("Verify(a)", Verify(a), 64);
			*(int *)a = 100;
			a = Realloc(a, 128);
			Should.Equal("Verify(a)", Verify(a), 128);
			Should.Equal("*(int *)a", *(int *)a, 100);
			Free(a);
		}
		#endif
	}
}