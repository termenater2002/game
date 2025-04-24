namespace Unity.Muse.Chat.UI.Utils
{
    static class ChatUIUtils
    {
        public static string GetLogIconClassName(LogDataType logType)
        {
            switch (logType)
            {
                case LogDataType.Warning:
                    return "warn";
                case LogDataType.Error:
                    return "error";
                default:
                    return "info";
            }
        }
    }
}
