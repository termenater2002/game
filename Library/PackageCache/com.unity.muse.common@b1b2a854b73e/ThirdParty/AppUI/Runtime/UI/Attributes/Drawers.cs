using UnityEngine;

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// Use this attribute to draw a property with the optional ScaleDrawer.
    /// </summary>
    internal class OptionalScaleDrawerAttribute : PropertyAttribute {}

    /// <summary>
    /// Use this attribute to draw a property with the ScaleDrawer.
    /// </summary>
    internal class ScaleDrawerAttribute : PropertyAttribute { }

    /// <summary>
    /// Use this attribute to draw a property with the optional ThemeDrawer.
    /// </summary>
    internal class OptionalThemeDrawerAttribute : PropertyAttribute {}

    /// <summary>
    /// Use this attribute to draw a property with the ThemeDrawer.
    /// </summary>
    internal class ThemeDrawerAttribute : PropertyAttribute { }

    /// <summary>
    /// Use this attribute to draw a property with the default PropertyField.
    /// </summary>
    internal class DefaultPropertyDrawerAttribute : PropertyAttribute { }
}
