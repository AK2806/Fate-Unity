using System;

namespace FateHelper {
    public static class NumberHelper {
        public static bool AlmostEquals(this float x, float y, float precision) {
            return (Math.Abs(x - y) <= precision);
        }
    }
}