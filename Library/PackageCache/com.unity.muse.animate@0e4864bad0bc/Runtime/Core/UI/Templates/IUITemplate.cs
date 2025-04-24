namespace Unity.Muse.Animate
{
    interface IUITemplate
    {
        public void InitComponents() { }
        public void Update() { }
        public void FindComponents() { }
        public void RegisterComponents() { }
        public void UnregisterComponents() { }
    }
}
