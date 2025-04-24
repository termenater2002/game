using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class OrganizationSelection : Dropdown
    {
        List<OrganizationInfo> m_Orgs;
        bool m_UpdatingSourceItems;

        public OrganizationSelection()
        {
            RegisterCallback<PointerDownEvent>(FetchOrganizations);
            this.RegisterValueChangedCallback(OnOrgDropdownChanged);

            bindItem = (item, i) => item.label = GetOrganizationDisplayName(m_Orgs[i]);

            RefreshOrganizations();
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                AccountInfo.Instance.OnOrganizationChanged += RefreshSelectedOrganization;
                AccountInfo.Instance.OnOrganizationListChanged += RefreshOrganizations;
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                AccountInfo.Instance.OnOrganizationChanged -= RefreshSelectedOrganization;
                AccountInfo.Instance.OnOrganizationListChanged -= RefreshOrganizations;
            });
        }

        static void FetchOrganizations(PointerDownEvent _) =>
            AccountInfo.Instance.UpdateAccountInformation();

        void OnOrgDropdownChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            if (m_UpdatingSourceItems)
                return;
            if (m_Orgs == null || m_Orgs.Count == 0 || !evt.newValue.Any())
                return;

            var selected = evt.newValue.FirstOrDefault();
            AccountInfo.Instance.Organization = selected != -1 ? m_Orgs[selected] : null;
        }

        void RefreshOrganizations()
        {
            m_UpdatingSourceItems = true;
            var orgs = AccountInfo.Instance.Organizations.OrderByDescending(org => org.IsEntitled).ToList();
            if (m_Orgs != null && orgs.Count == m_Orgs.Count && m_Orgs.TrueForAll(org => orgs.Contains(org)))
            {
                m_UpdatingSourceItems = false;
                return;     // No changes, no need to do anything.
            }
            m_Orgs = orgs;
            defaultValue = value.ToArray();
            sourceItems = m_Orgs;
            RefreshSelectedOrganization();
            m_UpdatingSourceItems = false;

            EnableInClassList("hidden", m_Orgs.Count <= 1);
        }

        void RefreshSelectedOrganization()
        {
            var selected = m_Orgs.FindIndex(org => org.Id == AccountInfo.Instance.Organization?.Id);
            if (selected != -1)
                value = new[] {selected};
        }

        static string GetOrganizationDisplayName(OrganizationInfo org) => 
            $"{org.Label.Replace("/", "|")} ({org.Status})";
    }
}
