using System;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.DeepPose.Core
{
    static class GeometryUtils
    {
        public static float WrapAngle(float theta)
        {
            theta = theta % (2.0f * Mathf.PI);
            if (theta < -Mathf.PI)
                theta += 2.0f * Mathf.PI;
            else if (theta > Mathf.PI)
                theta -= 2.0f * Mathf.PI;
            return theta;
        }

        public static Quaternion QuaternionFromOrtho6d(double3x2 ortho6d, bool transpose = false)
        {
            var mat = RotationMatrixFromOrtho6d(ortho6d, transpose);
            var quat = QuaternionFromRotationMatrix(mat);
            return quat;
        }

        public static double3x3 RotationMatrixFromOrtho6d(double3x2 ortho6d, bool transpose = false)
        {
            return transpose ? RotationMatrixFromOrtho6dTransposed(ortho6d) : RotationMatrixFromOrtho6dUnTransposed(ortho6d);
        }

        static double3x3 RotationMatrixFromOrtho6dUnTransposed(double3x2 ortho6d)
        {
            var x = math.normalize(ortho6d.c0);
            var z = math.cross(x, ortho6d.c1);
            z = math.normalize(z);
            var y = math.cross(z, x);

            var mat = new double3x3(x, y, z);
            return mat;
        }

        static double3x3 RotationMatrixFromOrtho6dTransposed(double3x2 ortho6d)
        {
            var a1 = ortho6d.c0;
            var a2 = ortho6d.c1;
            var b1 = math.normalize(a1);
            var b2 = a2 - math.csum(b1 * a2) * b1;
            b2 = math.normalize(b2);
            var b3 = math.cross(b1, b2);

            var mat = new double3x3(b1.x, b1.y, b1.z, b2.x, b2.y, b2.z, b3.x, b3.y, b3.z);
            return mat;
        }

        public static Quaternion QuaternionFromTanh6d(double3x2 ortho6d)
        {
            var mat = RotationMatrixFromTanh6d(ortho6d);
            var quat = QuaternionFromRotationMatrix(mat);
            return quat;
        }

        public static double3x3 RotationMatrixFromTanh6d(double3x2 ortho6d)
        {
            var alpha = new double2(ortho6d.c0.x, ortho6d.c0.y);
            var beta = new double2(ortho6d.c0.z, ortho6d.c1.x);
            var gamma = new double2(ortho6d.c1.y, ortho6d.c1.z);

            alpha = math.normalize(alpha);
            beta = math.normalize(beta);
            gamma = math.normalize(gamma);

            var cosa = alpha.y;
            var sina = alpha.x;
            var cosb = beta.y;
            var sinb = beta.x;
            var cosg = gamma.y;
            var sing = gamma.x;

            var m00 = cosa * cosb;
            var m10 = sina * cosb;
            var m20 = -sinb;
            var m01 = cosa * sinb * sing - sina * cosg;
            var m11 = sina * sinb * sing + cosa * cosg;
            var m21 = cosb * sing;
            var m02 = cosa * sinb * cosg + sina * sing;
            var m12 = sina * sinb * cosg - cosa * sing;
            var m22 = cosb * cosg;

            return new double3x3(
                m00, m01, m02,
                m10, m11, m12,
                m20, m21, m22);
        }

        //todo: optimize
        public static Quaternion QuaternionFromRotationMatrix(double3x3 rotMatrix)
        {
            var m = new double4x4();
            m[0][0] = rotMatrix.c0.x;
            m[1][0] = rotMatrix.c0.y;
            m[2][0] = rotMatrix.c0.z;
            m[3][0] = 0.0;
            m[0][1] = rotMatrix.c1.x;
            m[1][1] = rotMatrix.c1.y;
            m[2][1] = rotMatrix.c1.z;
            m[3][1] = 0.0;
            m[0][2] = rotMatrix.c2.x;
            m[1][2] = rotMatrix.c2.y;
            m[2][2] = rotMatrix.c2.z;
            m[3][2] = 0.0;
            m[0][3] = 0.0;
            m[1][3] = 0.0;
            m[2][3] = 0.0;
            m[3][3] = 1.0;

            var q = new double4();

            var t = m[0][0] + m[1][1] + m[2][2] + m[3][3];

            if (t > m[3][3])
            {
                q[0] = t;
                q[3] = m[1][0] - m[0][1];
                q[2] = m[0][2] - m[2][0];
                q[1] = m[2][1] - m[1][2];
            }
            else
            {
                int i = 0, j = 1, k = 2;
                if (m[1][1] > m[0][0])
                {
                    i = 1;
                    j = 2;
                    k = 0;
                }

                if (m[2][2] > m[i][i])
                {
                    i = 2;
                    j = 0;
                    k = 1;
                }

                t = m[i][i] - (m[j][j] + m[k][k]) + m[3][3];
                q[i] = t;
                q[j] = m[i][j] + m[j][i];
                q[k] = m[k][i] + m[i][k];
                q[3] = m[k][j] - m[j][k];

                q = new double4(q[3], q[0], q[1], q[2]);
            }

            var f = 0.5 / math.sqrt(t * m[3][3]);
            q[0] *= f;
            q[1] *= f;
            q[2] *= f;
            q[3] *= f;

            if (q[0] < 0.0)
            {
                q[0] = -q[0];
                q[1] = -q[1];
                q[2] = -q[2];
                q[3] = -q[3];
            }

            return new Quaternion((float)q[1], (float)q[2], (float)q[3], (float)q[0]);
        }
    }
}
