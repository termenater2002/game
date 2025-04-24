using Unity.Mathematics;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class NoiseUtils
    {
        public static Vector3 AddNoise(this Vector3 position, float std)
        {
            var nx = std * GaussianNoise();
            var ny = std * GaussianNoise();
            var nz = std * GaussianNoise();

            return new Vector3(
                position.x + (float)nx,
                position.y + (float)ny,
                position.z + (float)nz
                );
        }

        public static Quaternion AddNoise(this Quaternion rotation, float stdDegrees)
        {
            var randomAxis = new Vector3((float)GaussianNoise(), (float)GaussianNoise(), (float)GaussianNoise());
            var stdRadians = stdDegrees * Mathf.PI / 180f;
            var delta = GetNormalRandomRotationAroundAxis(randomAxis, stdRadians);
            var newRotation = delta * rotation;
            return newRotation;
        }

        public static Quaternion GetNormalRandomRotationAroundAxis(Vector3 axis, float stdRadians)
        {
            var normalizedAxis = axis.normalized;
            var theta = Mathf.Clamp((float)(stdRadians * GaussianNoise()), -Mathf.PI, Mathf.PI);
            var sin = Mathf.Sin(theta);

            var quat = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, Mathf.Cos(theta));
            return quat.normalized;
        }

        public static double GaussianNoise()
        {
            double u, v, s;

            do
            {
                u = 2.0 * UnityEngine.Random.value - 1.0;
                v = 2.0 * UnityEngine.Random.value - 1.0;
                s = u * u + v * v;
            }
            while (s >= 1.0);

            var fac = math.sqrt(-2.0 * math.log(s) / s);
            return u * fac;
        }
    }
}
