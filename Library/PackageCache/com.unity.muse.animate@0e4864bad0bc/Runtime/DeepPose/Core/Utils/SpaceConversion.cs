using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    class SpaceConversion
    {
        public enum AxisName
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        public enum AxisDirection
        {
            Positive = 0,
            Negative = 1
        }

        [Serializable]
        public struct Axis
        {
            public AxisName Name;
            public AxisDirection Direction;

            public Axis(AxisName name, AxisDirection direction = AxisDirection.Positive)
            {
                Name = name;
                Direction = direction;
            }
        }
        Matrix4x4 m_ExternalToUnityMatrix;
        Matrix4x4 m_UnityToExternalMatrix;

        public SpaceConversion(Axis xAxis, Axis yAxis, Axis zAxis)
        {
            m_ExternalToUnityMatrix = CreateExternalToUnityMatrix(xAxis, yAxis, zAxis);
            m_UnityToExternalMatrix = m_ExternalToUnityMatrix.inverse;
        }

        public Matrix4x4 TransformToUnity(Matrix4x4 externalTransform)
        {
            return m_ExternalToUnityMatrix * externalTransform * m_UnityToExternalMatrix;
        }

        public Matrix4x4 TransformToExternal(Matrix4x4 unityTransform)
        {
            return m_UnityToExternalMatrix * unityTransform * m_ExternalToUnityMatrix;
        }

        public Vector3 PointToUnity(Vector3 externalVector)
        {
            return m_ExternalToUnityMatrix.MultiplyPoint(externalVector);
        }

        public Vector3 PointToExternal(Vector3 unityVector)
        {
            return m_UnityToExternalMatrix.MultiplyPoint(unityVector);
        }

        public Vector3 VectorToUnity(Vector3 externalVector)
        {
            return m_ExternalToUnityMatrix.MultiplyVector(externalVector);
        }

        public Vector3 VectorToExternal(Vector3 unityVector)
        {
            return m_UnityToExternalMatrix.MultiplyVector(unityVector);
        }

        public Quaternion RotationToUnity(Quaternion externalRotation)
        {
            var externalTransform = Matrix4x4.Rotate(externalRotation);
            var unityTransform = TransformToUnity(externalTransform);
            return unityTransform.rotation;
        }

        public Quaternion RotationToExternal(Quaternion unityRotation)
        {
            var unityTransform = Matrix4x4.Rotate(unityRotation);
            var externalTransform = TransformToExternal(unityTransform);
            return externalTransform.rotation;
        }

        static Matrix4x4 CreateExternalToUnityMatrix(Axis xAxis, Axis yAxis, Axis zAxis)
        {
            Assert.IsTrue(IsValidTransform(xAxis, yAxis, zAxis), "At least two axis are parallel");

            var matrix = Matrix4x4.identity;

            matrix.SetRow(0, CreateAxisVector(xAxis));
            matrix.SetRow(1, CreateAxisVector(yAxis));
            matrix.SetRow(2, CreateAxisVector(zAxis));
            matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

            return matrix;
        }

        static bool IsValidTransform(Axis xAxis, Axis yAxis, Axis zAxis)
        {
            return xAxis.Name != yAxis.Name && xAxis.Name != zAxis.Name && yAxis.Name != zAxis.Name;
        }

        static Vector4 CreateAxisVector(Axis axis)
        {
            var axisValues = axis.Direction == AxisDirection.Positive ? 1f : -1f;
            return axis.Name switch
            {
                AxisName.X => new Vector4(axisValues, 0f, 0f, 0f),
                AxisName.Y => new Vector4(0f, axisValues, 0f, 0f),
                AxisName.Z => new Vector4(0f, 0f, axisValues, 0f),
                _ => throw new NotImplementedException($"Unknown axis: {axis.Name}")
            };
        }
    }
}
