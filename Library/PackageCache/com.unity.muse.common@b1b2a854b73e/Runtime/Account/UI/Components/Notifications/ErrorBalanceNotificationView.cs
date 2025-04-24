namespace Unity.Muse.Common.Account
{
    class ErrorBalanceNotificationView : NotificationView
    {
        public ErrorBalanceNotificationView(bool inlineButton)
            : base(new ()
            {
                titleText = "Experimental Program Error",
                description = "An error has occured trying to get your points balance. Please try again later.",
            }) { }
    }
}
