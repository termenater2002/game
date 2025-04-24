#if UNITY_EDITOR
using System;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class OnboardWindow : EditorWindow
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            // Delay a frame to let the preferences system initialize.
            EditorApplication.delayCall += () =>
            {
                if (!GlobalPreferences.onboardingShown)
                {
                    // Wait until the organization list has been retrieved before deciding to show the onboarding window
                    if (AccountInfo.Instance.Organization == null)
                        AccountInfo.Instance.OnOrganizationChanged += OnReady;
                    else
                        OnReady();
                }
            };
        }

        static void OnReady()
        {
            GlobalPreferences.onboardingShown = true;

            // Don't show onboarding window if starting a project with an organization that is already entitled.
            // If we did, it would show the window briefly and the IntroductionManipulator would find that the
            // current state is entitled and would close it immediately.
            if(IntroductionManipulator.ResolveCurrentState(AccountState.Default, null) != AccountState.Default)
                ShowOnboarding();
        }

        public static void ShowOnboarding()
        {
            var window = GetWindow<OnboardWindow>(utility:true, title: TextContent.onboardingWindowTitle, focus: true);
            window.minSize = new Vector2(350, 200);
            window.ShowUtility();
        }

        void CreateGUI()
        {
            rootVisualElement.styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.museTheme));

            var panel = new Panel();
            rootVisualElement.Add(panel);

            AccountController.Register(this);
            panel.AddManipulator(new IntroductionManipulator(this));
        }
    }
}
#endif