using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Editor
{
    class MuseEnvironment : EditorWindow
    {
        const string k_TestDefineSymbol = "UNITY_MUSE_CLOUD_TEST";
        const string k_StagingDefineSymbol = "UNITY_MUSE_CLOUD_STAGING";
        const string k_LocalDefineSymbol = "UNITY_MUSE_CLOUD_LOCAL";

        internal enum TestEnvironment
        {
            Production,
            Staging,
            Test,
            Local
        }

        [MenuItem("internal:Muse/Internals/Set Muse Test Environment", false, 904)]
        public static void ShowWindow()
        {
            var window = GetWindow<MuseEnvironment>(true, "Muse Test Environment");
            window.minSize = new Vector2(300, 245);
            window.maxSize = new Vector2(300, 245);
            window.ShowModalUtility();
        }

        void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.muse.common/Editor/Internal/MuseEnvironmentDialog.uxml");
            visualTree.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.unity.muse.common/Editor/Internal/MuseEnvironmentDialog.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            var envLabel = rootVisualElement.Q<Label>("environmentLabel");
            if (envLabel != null)
            {
                envLabel.text = "Current environment: " + GetCurrentEnvironment();
            }

            rootVisualElement.Q<Button>("production").clicked += () =>
            {
                SetTestEnvironment(TestEnvironment.Production);
                Close();
            };
            rootVisualElement.Q<Button>("staging").clicked += () =>
            {
                SetTestEnvironment(TestEnvironment.Staging);
                Close();
            };
            rootVisualElement.Q<Button>("test").clicked += () =>
            {
                SetTestEnvironment(TestEnvironment.Test);
                Close();
            };
            rootVisualElement.Q<Button>("local").clicked += () =>
            {
                SetTestEnvironment(TestEnvironment.Local);
                Close();
            };
        }

        internal static void SetTestEnvironment(TestEnvironment environment)
        {
            RemoveDefineSymbols(k_TestDefineSymbol);
            RemoveDefineSymbols(k_StagingDefineSymbol);
            RemoveDefineSymbols(k_LocalDefineSymbol);

            if (environment == TestEnvironment.Staging)
            {
                AddDefineSymbols(k_StagingDefineSymbol);
            }
            else if (environment == TestEnvironment.Test)
            {
                AddDefineSymbols(k_TestDefineSymbol);
            }
            else if (environment == TestEnvironment.Local)
            {
                AddDefineSymbols(k_LocalDefineSymbol);
            }
        }

        internal static TestEnvironment GetCurrentEnvironment()
        {
            if (ContainsDefineSymbol(k_StagingDefineSymbol))
                return TestEnvironment.Staging;
            if (ContainsDefineSymbol(k_TestDefineSymbol))
                return TestEnvironment.Test;
            if (ContainsDefineSymbol(k_LocalDefineSymbol))
                return TestEnvironment.Local;

            return TestEnvironment.Production;
        }

        internal static void AddDefineSymbols(string environmentSymbol)
        {
            var namedBuildTarget = GetCurrentNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            if (!ContainsDefineSymbol(environmentSymbol))
            {
                var newSymbols = new List<string>(defines) { environmentSymbol };
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newSymbols.ToArray());
            }
        }

        internal static void RemoveDefineSymbols(string environmentSymbol)
        {
            var namedBuildTarget = GetCurrentNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            var index = Array.IndexOf(defines, environmentSymbol);
            if (index > -1)
            {
                var newSymbols = new List<string>(defines);
                newSymbols.RemoveAt(index);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newSymbols.ToArray());
            }
        }

        internal static bool ContainsDefineSymbol(string environmentSymbol)
        {
            var namedBuildTarget = GetCurrentNamedBuildTarget();
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);

            var index = Array.IndexOf(defines, environmentSymbol);
            return index != -1;
        }

        static NamedBuildTarget GetCurrentNamedBuildTarget()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            return namedBuildTarget;
        }
    }
}
