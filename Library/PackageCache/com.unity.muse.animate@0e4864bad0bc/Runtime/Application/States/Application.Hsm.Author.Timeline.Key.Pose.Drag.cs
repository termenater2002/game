using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using AppUI = Unity.Muse.AppUI.UI;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyPoseDrag : ApplicationState<AuthorTimelineKeyPoseContext>, IKeyDownHandler
            {
                
                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);
                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors += OnRequestedDisableSelectedEffectors;
                    Context.PosingLogic.OnEffectorSelectionChanged += OnEffectorSelectionChanged;
                    
                    UpdateCanDisableSelectedEffectors();
                }

                public override void OnExit()
                {
                    base.OnExit();
                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors -= OnRequestedDisableSelectedEffectors;
                    Context.PosingLogic.OnEffectorSelectionChanged -= OnEffectorSelectionChanged;
                }

                public virtual void OnKeyDown(KeyPressEvent eventData)
                {
                    switch (eventData.KeyCode)
                    {
                        case KeyCode.Delete:
                            if (Context.PosingLogic.EffectorSelectionCount > 0 && Context.AuthoringModel.LastSelectionType == AuthoringModel.SelectionType.Effector)
                            {
                                DisableSelectedEffectors();
                                eventData.Use();
                            }
                            break;
                    }
                }

                void DisableSelectedEffectors()
                {
                    UserActionsManager.Instance.StartUserEdit("Disabled effector(s) (position & look-at)");

                    var selectionCount = Context.PosingLogic.EffectorSelectionCount;
                    for (var i = 0; i < selectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);

                        effectorModel.PositionEnabled = false;
                        effectorModel.LookAtEnabled = false;
                    }

                    Context.PosingLogic.ClearEffectorsSelection();
                }

                void OnRequestedDisableSelectedEffectors()
                {
                    DisableSelectedEffectors();
                }
                
                void OnEffectorSelectionChanged(PoseAuthoringLogic logic)
                {
                    UpdateCanDisableSelectedEffectors();
                }
                
                void UpdateCanDisableSelectedEffectors()
                {
                    bool canDisable = false;
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        if (effectorModel.PositionEnabled || effectorModel.LookAtEnabled)
                        {
                            canDisable = true;
                            break;
                        }
                    }
                    
                    Context.AuthoringModel.Timeline.CanDisableSelectedEffectors = canDisable;
                }
            }
        }
    }
}
