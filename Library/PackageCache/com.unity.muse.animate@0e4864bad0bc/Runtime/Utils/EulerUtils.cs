using Unity.Mathematics;

namespace Unity.Muse.Animate
{
    static class EulerUtils
    {
        internal static float3 ClosestEuler(float3 euler, float3 hint)
        {
            var v360 = new float3(2 * math.PI);
            var closest = euler + math.round((hint - euler) / v360) * v360;
            var alternate = (euler + new float3(math.PI)) * new float3(-1, 1, 1);
            var closestAlternate = alternate + math.round((hint - alternate) / v360) * v360;

            var diff = closest - hint;
            var altDiff = closestAlternate - hint;

            var result = math.select(closestAlternate, closest, math.dot(diff, diff) <= math.dot(altDiff, altDiff));
            return result;
        }
    }
}
