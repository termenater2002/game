using UnityEngine;

namespace Unity.Muse.Animate
{
    static class MathUtils
    {
        public static float SmoothLerp(float start, float end, float amount)
        {
            amount = Mathf.Clamp01(amount);

            // Cubicly adjust the amount value.
            amount = (amount * amount) * (3f - (2f * amount));

            // Interpolate
            var v = Mathf.Lerp(start, end, amount);

            return v;
        }

        public static bool NearlyEquals(this float v1, float v2, float epsilon, bool zeroMustBeExactlyEqual = false)
        {
            if (zeroMustBeExactlyEqual && (v1 == 0f || v2 == 0f))
                return v1 == v2;

            return Mathf.Abs(v1 - v2) < epsilon;
        }

        //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        //to each other. This function finds those two points. If the lines are not parallel, the function
        //outputs true, otherwise false.
        public static bool ClosestPointsOnTwoLines(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, out Vector3 closestPointLine1, out Vector3 closestPointLine2)
        {
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            var a = Vector3.Dot(lineVec1, lineVec1);
            var b = Vector3.Dot(lineVec1, lineVec2);
            var e = Vector3.Dot(lineVec2, lineVec2);
            var d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {
                var r = linePoint1 - linePoint2;
                var c = Vector3.Dot(lineVec1, r);
                var f = Vector3.Dot(lineVec2, r);
                var s = (b * f - c * e) / d;
                var t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            return false;
        }
    }
}
