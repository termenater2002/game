namespace Unity.Muse.Common.Account
{
    class ExperimentalProgramLimitReachedNotificationView : NotificationView
    {
        public ExperimentalProgramLimitReachedNotificationView(bool inlineButton)
            : base(new ()
            {
                titleText = TextContent.experimentalProgramLimitReachedTitle,
                buttonText = TextContent.experimentalProgramLimitReachedAction,
                description = TextContent.experimentalProgramLimitReachedDescription,
                inlineButton = inlineButton,
                action = AccountLinks.ViewPricing
            }) { }
    }
}
