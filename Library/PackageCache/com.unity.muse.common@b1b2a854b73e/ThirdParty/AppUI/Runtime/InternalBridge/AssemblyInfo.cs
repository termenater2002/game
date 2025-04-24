using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Muse.AppUI")]
[assembly: InternalsVisibleTo("Unity.Muse.AppUI.MVVM")]
[assembly: InternalsVisibleTo("Unity.AppUI.Navigation")]
[assembly: InternalsVisibleTo("Unity.Muse.AppUI.Redux")]
[assembly: InternalsVisibleTo("Unity.Muse.AppUI.Undo")]
[assembly: InternalsVisibleTo("Unity.AppUI.Tests")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Unity.Muse.AppUI.Editor")]
[assembly: InternalsVisibleTo("Unity.AppUI.Navigation.Editor")]
[assembly: InternalsVisibleTo("Unity.AppUI.Tests.Editor")]
#endif