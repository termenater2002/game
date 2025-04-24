using System;
using Unity.DeepPose.Core;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Animate.Editor
{
    static class AnimationClipUtils
    {
        const string RootPositionPropertyName = "RootT";
        const string RootRotationPropertyName = "RootQ";

        public struct PositionCurve
        {
            public AnimationCurve X;
            public AnimationCurve Y;
            public AnimationCurve Z;

            public static PositionCurve New()
            {
                return new PositionCurve
                {
                    X = new AnimationCurve(),
                    Y = new AnimationCurve(),
                    Z = new AnimationCurve()
                };
            }
        }

        public struct RotationCurve
        {
            public AnimationCurve X;
            public AnimationCurve Y;
            public AnimationCurve Z;
            public AnimationCurve W;

            public static RotationCurve New()
            {
                return new RotationCurve
                {
                    X = new AnimationCurve(),
                    Y = new AnimationCurve(),
                    Z = new AnimationCurve(),
                    W = new AnimationCurve()
                };
            }
        }

        public static void SetPositionCurve(this AnimationClip clip, PositionCurve curve, string relativePath, Type type, string propertyName)
        {
            clip.SetCurve(relativePath, type, $"{propertyName}.x", curve.X);
            clip.SetCurve(relativePath, type, $"{propertyName}.y", curve.Y);
            clip.SetCurve(relativePath, type, $"{propertyName}.z", curve.Z);
        }

        public static void SetRotationCurve(this AnimationClip clip, RotationCurve curve, string relativePath, Type type, string propertyName)
        {
            clip.SetCurve(relativePath, type, $"{propertyName}.x", curve.X);
            clip.SetCurve(relativePath, type, $"{propertyName}.y", curve.Y);
            clip.SetCurve(relativePath, type, $"{propertyName}.z", curve.Z);
            clip.SetCurve(relativePath, type, $"{propertyName}.w", curve.W);
        }

        public static void AddPositionKey(this PositionCurve rootPositionCurve, float time, Vector3 position)
        {
            rootPositionCurve.X.AddKey(time, position.x);
            rootPositionCurve.Y.AddKey(time, position.y);
            rootPositionCurve.Z.AddKey(time, position.z);
        }

        public static void AddRotationKey(this RotationCurve rootRotationCurve, float time, Quaternion rotation)
        {
            rootRotationCurve.X.AddKey(time, rotation.x);
            rootRotationCurve.Y.AddKey(time, rotation.y);
            rootRotationCurve.Z.AddKey(time, rotation.z);
            rootRotationCurve.W.AddKey(time, rotation.w);
        }

        public static void SetHumanoidCurves(AnimationClip clip, PositionCurve rootPositionCurve, RotationCurve rootRotationCurve, AnimationCurve[] animationCurves)
        {
            clip.ClearCurves();
            clip.SetPositionCurve(rootPositionCurve, "", typeof(Animator), RootPositionPropertyName);
            clip.SetRotationCurve(rootRotationCurve, "", typeof(Animator), RootRotationPropertyName);
            for (var j = 0; j < animationCurves.Length; j++)
            {
                var curve = animationCurves[j];
                var muscleName = HumanoidUtils.GetMuscleBindingName(j);
                clip.SetCurve("", typeof(Animator), muscleName, curve);
            }

            clip.EnsureQuaternionContinuity();
        }
        
        public static void DeepCopyAnimation(AnimationClip from, AnimationClip to)
        {
            to.ClearCurves();
            
            var curveBindings = AnimationUtility.GetCurveBindings(from);
            var targetCurves = new AnimationCurve [curveBindings.Length];
            for (var i = 0; i < curveBindings.Length; i++)
            {
                var binding = curveBindings[i];
                targetCurves[i] = AnimationUtility.GetEditorCurve(from, binding);
            }

            //SetEditorCurves is much more efficient because it only executes the synchronization of the editor curves once
            //instead of once per curve.
            AnimationUtility.SetEditorCurves(to, curveBindings, targetCurves);
        }
    }
}
