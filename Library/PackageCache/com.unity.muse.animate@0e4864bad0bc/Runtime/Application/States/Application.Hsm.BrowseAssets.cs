using Hsm;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// State used when the user is browsing the asset library and no LibraryItemAsset is being authored.
            /// </summary>
            public class BrowseAssets : ApplicationState<ApplicationContext>
            {                
                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    Context.ApplicationLibraryModel.ActiveLibraryItem = null;

                    Context.LibraryUI.BringToFront();
                    Context.SidePanelUIModel.AddPanel(SidePanelUtils.PageType.TakesLibrary, Context.TakesUI);
                    Context.SidePanelUIModel.SelectedPageIndex = (int)SidePanelUtils.PageType.TakesLibrary;
                    Context.SidePanelUIModel.IsVisible = true;
                    Context.ReturnToLibraryButton.style.display = DisplayStyle.None;
                }
                
                public override void OnExit()
                {
                    base.OnExit();
                    Context.LibraryUI.SendToBack();
                    Context.SidePanelUIModel.RemovePanel(SidePanelUtils.PageType.TakesLibrary, Context.TakesUI);
                    Context.ReturnToLibraryButton.style.display = DisplayStyle.Flex;
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }
            }
        }
    }
}
