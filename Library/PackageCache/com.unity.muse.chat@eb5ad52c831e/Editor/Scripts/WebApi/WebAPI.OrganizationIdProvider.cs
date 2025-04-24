using Unity.Muse.Common.Account;
using UnityEngine;

namespace Unity.Muse.Chat.WebApi
{
    partial class WebAPI
    {
        class OrganizationIdProvider : IOrganizationIdProvider
        {
            public string OrganizationIdBackup { get; set; }

            public bool GetOrganizationId(out string id)
            {
                id = AccountInfo.Instance.Organization?.Id;

                if (string.IsNullOrWhiteSpace(id))
                {
                    id = OrganizationIdBackup;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        Debug.LogWarning("Cannot find a valid organization.");
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
