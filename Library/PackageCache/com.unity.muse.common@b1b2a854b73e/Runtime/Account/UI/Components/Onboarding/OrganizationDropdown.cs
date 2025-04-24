using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Common.Account
{
    class OrganizationDropdown : Dropdown
    {
        public Action<OrganizationInfo> OnChange;
        public OrganizationInfo Selected => sourceItems[Enumerable.FirstOrDefault(value)] as OrganizationInfo;

        public OrganizationDropdown(Action<OrganizationInfo> onChange = null, string notEntitledClass = null) :
            this(AccountInfo.Instance.Organizations.OrderByDescending(org => org.IsEntitled).ToList(),
                onChange,
                notEntitledClass,
                true) { }

        public OrganizationDropdown(List<OrganizationInfo> organizations,
            Action<OrganizationInfo> onChange = null,
            string notEntitledClass = null,
            bool showStatus = false)
        {
            OnChange = onChange;

            name = "muse-organization-dropdown";
            bindItem = (item, i) =>
            {
                var organization = organizations[i];
                item.AddToClassList("muse-account-label");
                if (!organization.IsEntitled)
                {
                    item.AddToClassList(notEntitledClass);
                    item.tooltip = TextContent.subNotEntitled;
                }

                if (showStatus)
                {
                    item.labelElement.AddToClassList("show-status");
                    var label = new Text {text = organization.Label};
                    var status = new Text {text = $"{SubscriptionStatusUtils.ToString(organization.Status)}"};
                    status.AddToClassList("status");
                    item.labelElement.Add(label);
                    item.labelElement.Add(status);
                }
                else
                {
                    item.label = organization.Label;
                }
            };
            sourceItems = organizations;
            value = organizations.Any() ? new [] {0} : new int[] { };
            var selected = organizations.FindIndex(org => org.Id == AccountInfo.Instance.Organization?.Id);
            if (selected != -1)
                value = new[] {selected};

            this.RegisterValueChangedCallback(_ => OnChange?.Invoke(Selected));
        }
    }
}
