namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe static class Vec3 {
		public static float *Copy(float *o, float *a) {
			o[0] = a[0];
			o[1] = a[1];
			o[2] = a[2];
			return o;
		}

		public static float *Zero(float *o) {
			o[0] = 0;
			o[1] = 0;
			o[2] = 0;
			return o;
		}

		public static float *Set(float *o, float x, float y, float z) {
			o[0] = x;
			o[1] = y;
			o[2] = z;
			return o;
		}
	}
}