namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Interface used on UI elements which handle user input.
    /// </summary>
    /// <typeparam name="TValueType">The type of the `value`.</typeparam>
    internal interface IInputElement<TValueType> : IValidatableElement<TValueType>
    { }
}
