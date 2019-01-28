namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe static class Vec4 {
		public static float *Copy(float *o, float *a) {
			o[0] = a[0];
			o[1] = a[1];
			o[2] = a[2];
			o[3] = a[3];
			return o;
		}

		public static float *Set(float *o, float x, float y, float z, float w) {
			o[0] = x;
			o[1] = y;
			o[2] = z;
			o[3] = w;
			return o;
		}

		public static UnityEngine.Color Color(float *o) {
			return new UnityEngine.Color(o[0], o[1], o[2], o[3]);
		}
		public static UnityEngine.Color Color(float *o, float s) {
			return new UnityEngine.Color(o[0] * s, o[1] * s, o[2] * s, o[3] * s);
		}
	}
}