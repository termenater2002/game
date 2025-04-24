namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// Interface for components that need to be initialized.
    /// </summary>
    internal interface IInitializableComponent
    {
        /// <summary>
        /// Initializes the component.
        /// </summary>
        void InitializeComponent();
    }
}
