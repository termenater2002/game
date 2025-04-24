namespace Unity.Muse.Chat.WebApi
{
    interface IOrganizationIdProvider
    {
        bool GetOrganizationId(out string organizationId);
    }
}
