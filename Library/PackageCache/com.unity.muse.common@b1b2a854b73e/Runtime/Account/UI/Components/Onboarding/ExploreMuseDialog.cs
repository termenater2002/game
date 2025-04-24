#if UNITY_EDITOR
using System;
using System.Reflection;
using Unity.Muse.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.Muse.AppUI.UI.Button;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Muse.Common.Account
{
    class ExploreMuseDialog : AccountDialog
    {
        interface IClientHandler
        {
            void Open();
            void Install();
            bool IsInstalled { get; }
        }
        class ClientHandler : IClientHandler
        {
            public Action open;
            public Action installOverride;
            public string packageName;

            static MethodInfo s_InstallMethod;

            static ClientHandler()
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = asm.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
                    if (type != null)
                    {
                        var method = type.GetMethod("OpenURL", BindingFlags.NonPublic | BindingFlags.Static);
                        if (method != null)
                        {
                            s_InstallMethod = method;
                            break;
                        }
                    }
                }
            }

            // If the package is visible in the package manager window without any project specific settings (such as
            // "showPreReleasePackages" project setting turned on). If this is false, it means we can't simply open the
            // package manager window to the package's page as it might not be visible. We instead provide an alternative
            // path to install the package.
            public bool IsVisibleInPackageManagerWindow = true;

            // Consider no package to always be installed
            public bool IsInstalled =>
                string.IsNullOrEmpty(packageName) ||
                PackageInfo.FindForAssetPath($"Packages/{packageName}/package.json") != null;
            public void Open() => open();
            public void Install()
            {
                if (installOverride != null)
                {
                    installOverride.Invoke();
                    return;
                }

                try
                {
                    // `Open` throws an exception in 2022 when a package is not present but not in 6000.
                    // This means calling `Open` may not result in the package being shown in package manager under
                    // various conditions. There is no alternative API that lets a client know if a package can be
                    // seen in the package manager window, as per this slack thread: https://unity.slack.com/archives/C0111GBRLTC/p1726513457204939
                    // Therefore, we hard-code which package is known to be visible and otherwise will always open the package manager with
                    // the package name to be installed filled in.
                    if (IsVisibleInPackageManagerWindow)
                        UnityEditor.PackageManager.UI.Window.Open(packageName);
                    else
                        InstallByOpenURL();
                }
                catch (Exception exception)
                {
                    if (s_InstallMethod == null)
                        Debug.LogException(exception);
                    InstallByOpenURL();
                }
            }

            /// <summary>
            /// Opens the package manager with the "install package by name" dropdown opened and filled in with the package name.
            /// </summary>
            void InstallByOpenURL()
            {
                if (s_InstallMethod == null)
                    Debug.LogError($"Package {packageName} is not available in the registry. It is possible you don't have preview packages visible.");
                else
                    s_InstallMethod.Invoke(null, new object[] { $"com.unity3d.kharma:upmpackage/{packageName}" });
            }
        }

        class Section : VisualElement
        {
            Text m_SectionTitle = new();
            VisualElement m_Container = new();

            public Section(string title)
            {
                m_SectionTitle.text = title;
                m_SectionTitle.AddToClassList("muse-dialog-section-title");

                Add(m_SectionTitle);
                Add(m_Container);
            }
        }

        class SectionCard : VisualElement
        {
            VisualElement m_TextContainer = new();
            Text m_Title = new();
            Text m_Description = new();

            public SectionCard(string titleText, string descriptionText, IClientHandler handler, string iconName = "arrow-square-in")
            {
                m_Title.AddToClassList("muse-dialog-card-title");
                m_Title.text = titleText;

                m_Description.AddToClassList("muse-dialog-card-description");
                m_Description.text = descriptionText;

                m_TextContainer.Add(m_Title);
                m_TextContainer.Add(m_Description);
                Add(m_TextContainer);

                if (handler.IsInstalled)
                {
                    var action = new Icon { iconName = iconName, size = IconSize.M};
                    action.AddToClassList("action-icon");
                    Add(action);
                    this.AddManipulator(new Pressable(handler.Open));
                }
                else
                {
                    m_TextContainer.SetEnabled(false);

                    var action = new Button(handler.Install) {title = "Install"};
                    action.AddToClassList("action-icon");
                    Add(action);
                    this.AddManipulator(new Pressable(handler.Install));
                }
            }
        }

        public Action OnAccept;

        public ExploreMuseDialog()
        {
            AddToClassList("muse-subscription-dialog-explore");

            dialogDescription.Add(new Text {text = TextContent.exploreTitle, name="muse-description-title"});
            var descriptionText = new Text {text = TextContent.exploreDescription, name = "muse-description-secondary", enableRichText = true};
            descriptionText.AddToClassList("muse-description-section");
            dialogDescription.Add(descriptionText);
            dialogDescription.AddToClassList("muse-description-section");

            var learnSection = new VisualElement();
            learnSection.AddToClassList("muse-dialog-explore-section-learn");
            learnSection.AddToClassList("muse-description-section");
            learnSection.Add(new SectionCard(TextContent.exploreLearnSectionTitle, TextContent.exploreCardLearnDescription, Learn, "arrow-square-out"));

            var exploreSection = new Section(TextContent.exploreMuseSectionTitle);
            exploreSection.AddToClassList("muse-dialog-explore-section-muse");
            exploreSection.Add(new SectionCard(TextContent.exploreCardChatTitle, TextContent.exploreCardChatDescription, Chat));
            exploreSection.Add(new SectionCard(TextContent.exploreCardSpriteTitle, TextContent.exploreCardSpriteDescription, Sprite));
            exploreSection.Add(new SectionCard(TextContent.exploreCardTextureTitle, TextContent.exploreCardTextureDescription, Texture));
            exploreSection.Add(new SectionCard(TextContent.exploreCardAnimateTitle, TextContent.exploreCardAnimateDescription, Animate));
            exploreSection.Add(new SectionCard(TextContent.exploreCard2dWorkflowsTitle, TextContent.exploreCard2dWorkflowsDescription, SpriteEnhancers));

            dialogDescription.Add(learnSection);
            dialogDescription.Add(exploreSection);

            AddPrimaryButton(TextContent.exploreAccept, () =>
            {
                var chat = Chat;
                if (chat.IsInstalled)
                    chat.Open();

                OnAccept?.Invoke();
            });
        }

        static ClientHandler Chat => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/Chat"),
            packageName = "com.unity.muse.chat",
            IsVisibleInPackageManagerWindow = false
        };

        static ClientHandler Texture => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Texture Generator"),
            packageName = "com.unity.muse.texture"
        };

        static ClientHandler Sprite => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Sprite Generator"),
            packageName = "com.unity.muse.sprite"
        };

        static ClientHandler Animate => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Muse/New Animate Generator"),
            packageName = "com.unity.muse.animate",
            IsVisibleInPackageManagerWindow = false
        };

        static ClientHandler SpriteEnhancers => new()
        {
            open = () => EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor"),

#if !UNITY_6000_0_OR_NEWER
            installOverride = () => EditorUtility.DisplayDialog("Package could not be installed", "This package require Unity 6 to be installed and cannot be installed in your current Unity version.", "Ok."),
#endif
            packageName = "com.unity.2d.muse",
            IsVisibleInPackageManagerWindow = false
        };

        static ClientHandler Learn => new()
        {
            open = AccountLinks.LearnMuse,
            packageName = string.Empty
        };
    }
}
#endif
