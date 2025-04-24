namespace Unity.Muse.Common.Account
{
    class ExperimentalProgramSignUpNotificationView : NotificationView
    {
        public ExperimentalProgramSignUpNotificationView(bool inlineButton)
            : base(new()
            {
                titleText = TextContent.experimentalProgram,
                buttonText = TextContent.experimentalProgramSignUpAction,
                description = TextContent.experimentalProgramSignUpDescription,
                inlineButton = inlineButton,
                style = MuseNotificationStyle.Warning,
                action = AccountLinks.SignUpExperimentalProgram
            }) { }
    }
}
