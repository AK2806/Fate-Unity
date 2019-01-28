namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe static class Vec2 {
		public static float *Copy(float *o, float *a) {
			o[0] = a[0];
			o[1] = a[1];
			return o;
		}

		public static float *Zero(float *o) {
			o[0] = 0;
			o[1] = 0;
			return o;
		}

		public static float *One(float *o) {
			o[0] = 1;
			o[1] = 1;
			return o;
		}

		public static float *Set(float *o, float x, float y) {
			o[0] = x;
			o[1] = y;
			return o;
		}

		/**
		 * m00 m01 m02   x   x * m00 + y * m01 + m02
		 * m10 m11 m12 . y = x * m10 + y * m11 + m12
		 *  0   0   1    1   1
		 */
		public static float *TransformMat2D(float *o, float *m, float *a) {
			o[0] = a[0] * m[0] + a[1] * m[1] + m[2];
			o[1] = a[0] * m[3] + a[1] * m[4] + m[5];
			return o;
		}
		public static float *TransformMat2D(float *o, float *m, float x, float y) {
			o[0] = x * m[0] + y * m[1] + m[2];
			o[1] = x * m[3] + y * m[4] + m[5];
			return o;
		}

		public static string Str(float *a) {
			return string.Format("vec2({0}, {1})", a[0], a[1]);
		}
	}
}