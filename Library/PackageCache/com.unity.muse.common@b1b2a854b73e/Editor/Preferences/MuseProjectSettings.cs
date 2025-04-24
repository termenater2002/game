using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Muse.Common.Account;
using TextField = UnityEngine.UIElements.TextField;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Unity.Muse.Common.Editor
{
    class MuseProjectSettings : ScrollView
    {
        internal const string mainUssClassName = "muse-project-settings";

        internal static readonly string headerUssClassName = $"{mainUssClassName}__header";

        internal static readonly string descriptionUssClassName = $"{mainUssClassName}__description";

        internal static readonly string sectionUssClassName = $"{mainUssClassName}__section";

        internal static readonly string subHeaderUssClassName = $"{mainUssClassName}__sub-header";

        internal static readonly string rowUssClassName = $"{mainUssClassName}__row";

        internal static readonly string helpBoxUssClassName = $"{mainUssClassName}__help-box";

        internal static readonly string containerUssClassName = $"{mainUssClassName}__container";

        internal static readonly string propertyLabelUssClassName = $"{mainUssClassName}__property-label";

        internal static readonly string fullWidthDropdownUssClassName = $"{mainUssClassName}__full-width-dropdown";

        Delegate m_ConnectChanged;

        bool m_SignedIn;

        List<OrganizationInfo> m_Orgs;

        readonly DropdownField m_OrganizationDropdown;
        readonly HelpBox m_SignedOutWarning;
        readonly Label m_SelectOrgText;
        readonly Toggle m_DeleteWithoutWarningToggle;
        readonly TextField m_SpriteAssetGeneratedPathField;
        readonly TextField m_TextureAssetGeneratedPathField;
        readonly TextField m_AnimateAssetGeneratedPathField;
        readonly EnumField m_CanvasControlSchemeField;
        readonly VisualElement m_SettingsContainer;

        static readonly Dictionary<string, IMuseEditorPreferencesView> k_Sections =
            new Dictionary<string, IMuseEditorPreferencesView>();

        public MuseProjectSettings()
        {
            AddToClassList(mainUssClassName);
            styleSheets.Add(ResourceManager.Load<StyleSheet>(PackageResources.projectSettingsStyleSheet));

            var header = new Label { text = TextContent.projectSettingsTitle };
            header.AddToClassList(headerUssClassName);
            Add(header);

            /*
             * Account settings
             */

            var accountCategory = new Label {text = TextContent.accountSettingsCategory};
            accountCategory.AddToClassList(subHeaderUssClassName);
            Add(accountCategory);

            m_SignedOutWarning = new HelpBox(TextContent.projectSettingsSignedOut, HelpBoxMessageType.Warning);
            m_SignedOutWarning.AddToClassList(helpBoxUssClassName);
            Add(m_SignedOutWarning);

            m_SettingsContainer = new VisualElement();
            m_SettingsContainer.AddToClassList(containerUssClassName);
            Add(m_SettingsContainer);

            m_SelectOrgText = new Label {text = TextContent.projectSettingsOrgDesc};
            m_SelectOrgText.AddToClassList(propertyLabelUssClassName);
            m_SettingsContainer.Add(m_SelectOrgText);
            m_OrganizationDropdown = new DropdownField();
            m_OrganizationDropdown.RegisterCallback<PointerDownEvent>(FetchOrganizations);
            m_OrganizationDropdown.RegisterValueChangedCallback(OnOrgDropdownChanged);
            m_SettingsContainer.Add(m_OrganizationDropdown);

            /*
             * General settings
             */

            var generalSettingsCategory = new Label {text = TextContent.generalSettingsCategory};
            generalSettingsCategory.AddToClassList(subHeaderUssClassName);
            m_SettingsContainer.Add(generalSettingsCategory);

            m_DeleteWithoutWarningToggle = new Toggle {label = TextContent.deleteWithoutWarning};
            m_DeleteWithoutWarningToggle.RegisterValueChangedCallback(evt =>
            {
                GlobalPreferences.deleteWithoutWarning = evt.newValue;
            });
            m_SettingsContainer.Add(m_DeleteWithoutWarningToggle);

            m_CanvasControlSchemeField = new EnumField {label = TextContent.canvasControlScheme};
            m_CanvasControlSchemeField.Init(GlobalPreferences.canvasControlScheme);
            m_CanvasControlSchemeField.RegisterValueChangedCallback(evt =>
            {
                GlobalPreferences.canvasControlScheme = (Unity.Muse.AppUI.UI.CanvasControlScheme)evt.newValue;
            });
            m_SettingsContainer.Add(m_CanvasControlSchemeField);

            /*
             * Others packages settings
             */

            foreach (var (sectionName, view) in k_Sections.OrderBy(kvp => kvp.Key))
            {
                var foldout = new Foldout {text = sectionName};
                foldout.AddToClassList(sectionUssClassName);
                foldout.Add(view.CreateGUI());
                foldout.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                        view.Refresh();
                });
                m_SettingsContainer.Add(foldout);
            }

            RefreshOrganizations();
            Refresh();

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                AccountInfo.Instance.OnOrganizationChanged += RefreshSelectedOrganization;
                AccountInfo.Instance.OnOrganizationListChanged += RefreshOrganizations;
                SignInUtility.OnChanged += Refresh;
                m_ConnectChanged = UnityConnectUtils.RegisterConnectStateChangedEvent(ConnectChanged);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                AccountInfo.Instance.OnOrganizationChanged -= RefreshSelectedOrganization;
                AccountInfo.Instance.OnOrganizationListChanged -= RefreshOrganizations;
                SignInUtility.OnChanged -= Refresh;
                UnityConnectUtils.UnregisterConnectStateChangedEvent(m_ConnectChanged);
            });
        }

        void FetchOrganizations(PointerDownEvent _)
        {
            AccountInfo.Instance.UpdateAccountInformation();
        }

        void ConnectChanged(object obj)
        {
            Refresh();
        }

        void Refresh()
        {
            /*
             * Account settings
             */

            m_SignedIn = SignInUtility.state == SignInState.SignedIn || SignInUtility.state == SignInState.NotReady;
            m_SignedOutWarning.style.display = m_SignedIn ? DisplayStyle.None : DisplayStyle.Flex;
            m_SettingsContainer.SetEnabled(m_SignedIn);

            /*
             * General settings
             */

            var deleteWithoutWarning = GlobalPreferences.deleteWithoutWarning;
            m_DeleteWithoutWarningToggle.SetValueWithoutNotify(deleteWithoutWarning);

            var canvasControlScheme = GlobalPreferences.canvasControlScheme;
            m_CanvasControlSchemeField.SetValueWithoutNotify(canvasControlScheme);

            /*
             * Others packages settings
             */

            foreach (var (_, view) in k_Sections)
            {
                view.Refresh();
            }
        }

        void RefreshOrganizations()
        {
            m_Orgs = AccountInfo.Instance.Organizations.OrderByDescending(org => org.IsEntitled).ToList();
            m_OrganizationDropdown.choices = m_Orgs.Select(GetOrganizationDisplayName).ToList();
            RefreshSelectedOrganization();
        }

        void RefreshSelectedOrganization()
        {
            var selected = m_Orgs.FindIndex(org => org.Id == AccountInfo.Instance.Organization?.Id);
            var selectedString = selected != -1 ? GetOrganizationDisplayName(m_Orgs[selected]) : null;
            m_OrganizationDropdown.SetValueWithoutNotify(selectedString);
        }

        void OnOrgDropdownChanged(ChangeEvent<string> evt)
        {
            if (m_Orgs == null || m_Orgs.Count == 0)
                return;

            var selected = m_Orgs.FindIndex(org => GetOrganizationDisplayName(org) == evt.newValue);
            AccountInfo.Instance.Organization = selected != -1 ? m_Orgs[selected] : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetOrganizationDisplayName(OrganizationInfo org)
        {
            return $"{org.Label.Replace("/", "|")} ({org.Status})";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string SanitizePath(string path)
        {
            var root = Path.GetDirectoryName(Application.dataPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(root) && path.StartsWith(root) && path.Length > root.Length + 1)
                return path[(root.Length + 1)..];
            return null;
        }

        internal static void RegisterSection(string sectionName, IMuseEditorPreferencesView view)
        {
            k_Sections[sectionName] = view;
        }
    }
}
