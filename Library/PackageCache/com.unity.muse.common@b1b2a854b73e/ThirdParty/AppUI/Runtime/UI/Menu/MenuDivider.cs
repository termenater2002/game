using System;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.Muse.AppUI.UI
{
    /// <summary>
    /// A special <see cref="Divider"/> intended to be used inside <see cref="Menu"/> elements.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    internal partial class MenuDivider : Divider
    {
        /// <summary>
        /// The MenuDivider main styling class.
        /// </summary>
        public const string dividerClassName = "appui-menu__divider";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuDivider()
        {
            AddToClassList(dividerClassName);
        }

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// UXML factory for the <see cref="MenuDivider"/>.
        /// </summary>
        internal new class UxmlFactory : UxmlFactory<MenuDivider, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="MenuDivider"/>.
        /// </summary>
        internal new class UxmlTraits : Divider.UxmlTraits { }

#endif
    }
}
