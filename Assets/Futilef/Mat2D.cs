using Math = System.Math;

namespace Futilef {
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	public unsafe static class Mat2D {
		public static float *Identity(float *o) {
			o[0] = 1;
			o[1] = 0;
			o[2] = 0;
			o[3] = 1;
			o[4] = 0;
			o[5] = 0;
			return o;
		}

		public static float *Copy(float *o, float *a) {
			o[0] = a[0];
			o[1] = a[1];
			o[2] = a[2];
			o[3] = a[3];
			o[4] = a[4];
			o[5] = a[5];
			return o;
		}

		/**
		 * a . b = o
		 */
		public static float *Mul(float *o, float *a, float *b) {
			float a0 = a[0], a1 = a[1], a2 = a[2], a3 = a[3], a4 = a[4], a5 = a[5];
			float b0 = b[0], b1 = b[1], b2 = b[2], b3 = b[3], b4 = b[4], b5 = b[5];
			o[0] = a0 * b0 + a2 * b1;
			o[1] = a1 * b0 + a3 * b1;
			o[2] = a0 * b2 + a2 * b3;
			o[3] = a1 * b2 + a3 * b3;
			o[4] = a0 * b4 + a2 * b5 + a4;
			o[5] = a1 * b4 + a3 * b5 + a5;
			return o;
		}

		/**
		 * 1 0 x   cos -sin 0   sx 0 0   1 0 0   (sx * cos) (sy * -sin) x
		 * 0 1 y . sin cos  0 . 0 sy 0 . 0 1 0 = (sx * sin) (sy * cos)  y
		 * 0 0 1   0   0    1   0 0  1   0 0 1   0          0           1
		 */
		public static float *FromScalingRotationTranslation(float *o, float *t, float *s, float r) {
			float sx = s[0], sy = s[1];
			float x = t[0], y = t[1];
			float sin = (float)Math.Sin(r), cos = (float)Math.Cos(r);
			o[0] = sx * cos;
			o[1] = sy * -sin;
			o[2] = x;
			o[3] = sx * sin;
			o[4] = sy * cos;
			o[5] = y;
			return o;
		}

		public static string Str(float *o) {
			return string.Format("mat2d({0}, {1}, {2}, {3}, {4}, {5})", o[0], o[1], o[2], o[3], o[4], o[5]);
		}
	}
}