using Math = System.Math;

namespace Futilef {
	public static class EsType {
		public const int In = 1, Out = 2, InOut = 3;

		public const int Linear = 4, 
			Quad = 8, QuadIn = 9, QuadOut = 10, QuadInOut = 11, 
			Cubic = 12, CubicIn = 13, CubicOut = 14, CubicInOut = 15, 
			Quart = 16, QuartIn = 17, QuartOut = 18, QuartInOut = 19, 
			Quint = 20, QuintIn = 21, QuintOut = 22, QuintInOut = 23, 
			Sine = 24, SineIn = 25, SineOut = 26, SineInOut = 27, 
			Expo = 28, ExpoIn = 29, ExpoOut = 30, ExpoInOut = 31, 
			Circ = 32, CircIn = 33, CircOut = 34, CircInOut = 35, 
			Back = 36, BackIn = 37, BackOut = 38, BackInOut = 39, 
			Elastic = 40, ElasticIn = 41, ElasticOut = 42, ElasticInOut = 43, 
			Bounce = 44, BounceIn = 45, BounceOut = 46, BounceInOut = 47,
			End = 48;
	}

	public static class Es {
		public const float Pi = 3.14159265359f, HalfPi = 1.57079632679f, TwoPi = 6.28318530718f;

		public static float Ease(int esType, float t) {
			switch (esType) {
			case EsType.Quad:
			case EsType.QuadIn:       return t * t;
			case EsType.QuadOut:      return t * (2 - t);
			case EsType.QuadInOut:    return (t *= 2) < 1 ? .5f * t * t : .5f * (1 - (t - 1) * (t - 3));
			
			case EsType.Cubic:
			case EsType.CubicIn:      return t * t * t;
			case EsType.CubicOut:     return ((t -= 1) * t * t + 1);
			case EsType.CubicInOut:   return (t *= 2) < 1 ? .5f * t * t * t : .5f * ((t -= 2) * t * t + 2);
			
			case EsType.Quart:
			case EsType.QuartIn:      return t * t * t * t;
			case EsType.QuartOut:     return 1 - (t -= 1) * t * t * t;
			case EsType.QuartInOut:   return (t *= 2) < 1 ? .5f * t * t * t * t : .5f * (2 - (t -= 2) * t * t * t);
			
			case EsType.Quint:
			case EsType.QuintIn:      return t * t * t * t * t;
			case EsType.QuintOut:     return ((t -= 1) * t * t * t * t + 1);
			case EsType.QuintInOut:   return (t *= 2) < 1 ? .5f * t * t * t * t * t : .5f * ((t -= 2) * t * t * t * t + 2);
			
			case EsType.Sine:
			case EsType.SineIn:       return 1 - (float)Math.Cos(t * HalfPi);
			case EsType.SineOut:      return (float)Math.Sin(t * HalfPi);
			case EsType.SineInOut:    return .5f * (1 - (float)Math.Cos(t * Pi));
			
			case EsType.Expo:
			case EsType.ExpoIn:       return (float)Math.Exp(7 * (t - 1));
			case EsType.ExpoOut:      return 1 - (float)Math.Exp(-7 * t);
			case EsType.ExpoInOut:    return (t *= 2) < 1 ? .5f * (float)Math.Exp(7 * (t - 1)) : .5f * (2 - (float)Math.Exp(-7 * (t - 1)));
			
			case EsType.Circ:
			case EsType.CircIn:       return 1 - (float)Math.Sqrt(1 - t * t);
			case EsType.CircOut:      return (float)Math.Sqrt(1 - (t -= 1) * t);
			case EsType.CircInOut:    return (t *= 2) < 1 ? .5f * (1 - (float)Math.Sqrt(1 - t * t)) : .5f * ((float)Math.Sqrt(1 - (t -= 2) * t) + 1);
			
			case EsType.Back:
			case EsType.BackIn:       return t * t * (2.70158f * t - 1.70158f);
			case EsType.BackOut:      return (t -= 1) * t * (2.70158f * t + 1.70158f) + 1;
			case EsType.BackInOut:    return (t *= 2) < 1 ? .5f * (t * t * (3.5949095f * t - 2.5949095f)) : .5f * ((t -= 2) * t * (3.5949095f * t + 2.5949095f) + 2);
			
			case EsType.Elastic:
			case EsType.ElasticIn:    return (float)( -Math.Exp(7 * (t -= 1)) * Math.Sin((t - 0.075) * 20.9439510239) );
			case EsType.ElasticOut:   return (float)( Math.Exp(-7 * t) * Math.Sin((t - 0.075) * 20.9439510239) + 1 );
			case EsType.ElasticInOut: return (t *= 2) < 1 ? (float)(-.5 * Math.Exp(7 * (t -= 1)) * Math.Sin((t - 0.1125) * 13.962634016)) : (float)(Math.Exp(-7 * (t -= 1)) * Math.Sin((t - 0.1125) * 13.962634016) * .5 + 1);
			
			case EsType.Bounce:
			case EsType.BounceIn:     return 1 - Es.Ease(EsType.BounceOut, 1 - t);
			case EsType.BounceOut:    return t < 0.363636363636f ? 7.5625f * t * t : t < 0.727272727273f ? 7.5625f * (t -= 0.545454545455f) * t + .75f : t < 0.909090909091f ? 7.5625f * (t -= 0.818181818182f) * t + .9375f : 7.5625f * (t -= 0.954545454545f) * t + .984375f;
			case EsType.BounceInOut:  return (t *= 2) < 1 ? .5f * (1 - Es.Ease(EsType.BounceOut, 1 - t)) : .5f * (Es.Ease(EsType.BounceOut, t - 1) + 1);
			}

			return t;
		}
	}
}