namespace Unity.Muse.Animate
{
    interface IKeyDownHandler
    {
        public void OnKeyDown(KeyPressEvent eventData);
    }

    interface IKeyUpHandler
    {
        public void OnKeyUp(KeyPressEvent eventData);
    }

    interface IKeyHoldHandler
    {
        public void OnKeyHold(KeyPressEvent eventData);
    }
}
