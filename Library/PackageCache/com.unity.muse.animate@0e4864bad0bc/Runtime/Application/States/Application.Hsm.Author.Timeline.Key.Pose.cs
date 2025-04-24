using System;
using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// The user authors a <see cref="KeySequenceTake"/>'s <see cref="EntityKeyModel"/>.
            /// </summary>
            public class AuthorTimelineKeyPose : ApplicationState<AuthorTimelineKeyPoseContext>, IPointerClickHandler, IKeyDownHandler
            {
                Transition m_NextTransition = Transition.None();

                public override Transition GetTransition()
                {
                    return m_NextTransition;
                }

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);
                    
                    SetupUI();
                    RegisterEvents();
                    
                    // Show the Posing
                    Context.PosingLogic.IsVisible = true;

                    // If no posing tool is currently selected, select the default tool
                    if (Context.AuthoringModel.PosingTool == AuthoringModel.PosingToolType.None)
                    {
                        SwitchToPosingTool(AuthoringModel.PosingToolType.Translate);
                    }
                    
                    UpdatePosingTool();

                    // If no entity selected, select all actors
                    if (!Context.EntitySelection.HasSelection)
                    {
                        for (var i = 0; i < Context.Stage.NumActors; i++)
                        {
                            var actorId = Context.Stage.GetActorID(i);
                            Context.EntitySelection.Select(actorId.EntityID);
                        }
                    }
                    
                    UserActionsManager.Instance.BackupForUndo();
                }

                public override void OnExit()
                {
                    base.OnExit();

                    CloseUI();
                    UnregisterEvents();
                    
                    Context.PosingLogic.IsVisible = false;
                    // Force an update since the value might not have changed
                    Context.PosingLogic.ForceUpdateAllViews();
                }

                void RegisterEvents()
                {
                    // Selection Events
                    Context.EntityEffectorSelection.OnSelectionChanged += OnEffectorSelectionChanged;
                    
                    // Authoring Events
                    Context.AuthoringModel.OnPosingToolChanged += OnPosingToolChanged;
                }

                void UnregisterEvents()
                {
                    // Selection Events
                    Context.EntityEffectorSelection.OnSelectionChanged -= OnEffectorSelectionChanged;
                    
                    // Authoring Events
                    Context.AuthoringModel.OnPosingToolChanged -= OnPosingToolChanged;
                }
                
                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    Context.PosingLogic.Step(aDeltaTime);
                }

                public void OnPointerClick(PointerEventData eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnPointerClick({eventData.button})");

                    if (Context.PosingLogic.HasEffectorSelection)
                    {
                        DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Deselect);
                        Context.PosingLogic.ClearEffectorsSelection();
                        eventData.Use();
                    }
                    else if (Context.EntityEffectorSelection.HasSelection)
                    {
                        DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Deselect);
                        Context.EntityEffectorSelection.Clear();
                        eventData.Use();
                    }
                    else
                    {
                        Context.EntitySelection.Clear();
                        eventData.Use();
                    }
                }

                public void OnKeyDown(KeyPressEvent eventData)
                {
                    if (Context.TakesUI.IsWriting)
                    {
                        eventData.Use();
                        return;
                    }
                    
                    switch (eventData.KeyCode)
                    {
                        case KeyCode.F when !eventData.IsControlOrCommand:
                            if (FrameCameraOnEffectorSelection())
                            {
                                eventData.Use();
                            }
                            break;

                        case KeyCode.Q:
                            if (!eventData.IsControlOrCommand)
                            {
                                SwitchToPosingTool(AuthoringModel.PosingToolType.Drag);
                                eventData.Use();
                            }
                            break;

                        case KeyCode.W:
                            if (!eventData.IsControlOrCommand)
                            {
                                SwitchToPosingTool(AuthoringModel.PosingToolType.Translate);
                                eventData.Use();
                            }
                            break;

                        case KeyCode.E:
                            if (!eventData.IsControlOrCommand)
                            {
                                SwitchToPosingTool(AuthoringModel.PosingToolType.Rotate);
                                eventData.Use();
                            }
                            break;

                        case KeyCode.R:
                            if (!eventData.IsControlOrCommand)
                            {
                                SwitchToPosingTool(AuthoringModel.PosingToolType.Universal);
                                eventData.Use();
                            }
                            break;

                        case KeyCode.T:
                            if (!eventData.IsControlOrCommand)
                            {
                                SwitchToPosingTool(AuthoringModel.PosingToolType.Tolerance);
                                eventData.Use();
                            }
                            break;
                    }
                }

                void SetupUI()
                {
                    Context.AuthoringModel.Title = "Editing Pose";
                }

                void CloseUI()
                {
                    
                }

                void SwitchToPosingTool(AuthoringModel.PosingToolType tool)
                {
                    Context.AuthoringModel.PosingTool = tool;
                }

                void UpdateToolbar()
                {
                    Context.Toolbars.Update();
                }

                void OnPosingToolChanged()
                {
                    UpdatePosingTool();
                }

                void UpdatePosingTool()
                {
                    UpdateToolbar();

                    switch (Context.AuthoringModel.PosingTool)
                    {
                        case AuthoringModel.PosingToolType.None:
                            m_NextTransition = Transition.None();
                            break;

                        case AuthoringModel.PosingToolType.Drag:
                            m_NextTransition = Transition.Inner<AuthorTimelineKeyPoseDrag>(Context);
                            break;

                        case AuthoringModel.PosingToolType.Rotate:
                            m_NextTransition = Transition.Inner<AuthorTimelineKeyPoseRotate>(Context);
                            break;

                        case AuthoringModel.PosingToolType.Translate:
                            m_NextTransition = Transition.Inner<AuthorTimelineKeyPoseTranslate>(Context);
                            break;

                        case AuthoringModel.PosingToolType.Universal:
                            m_NextTransition = Transition.Inner<AuthorTimelineKeyPoseTransform>(Context);
                            break;

                        case AuthoringModel.PosingToolType.Tolerance:
                            m_NextTransition = Transition.Inner<AuthorTimelineKeyPoseAdjustTolerance>(Context);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(Context.AuthoringModel.PosingTool), Context.AuthoringModel.PosingTool, null);
                    }
                }
                
                void OnEffectorSelectionChanged(SelectionModel<EntityID> model)
                {
                    if (model.HasSelection)
                    {
                        Context.AuthoringModel.LastSelectionType = AuthoringModel.SelectionType.Effector;
                    }
                }
                
                bool FrameCameraOnEffectorSelection()
                {
                    if (!Context.PosingLogic.HasEffectorSelection)
                        return false;

                    var effectorModel = Context.PosingLogic.GetSelectedEffector(0);
                    var bounds = new Bounds(effectorModel.Position, Vector3.zero);
                    for (var i = 1; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        bounds.Encapsulate(effectorModel.Position);
                    }

                    // Add some margin
                    bounds.Expand(0.2f);

                    if (DeepPoseAnalyticsUtils.TryGetSelectedEffectorNames(Context.EntitySelection.GetSelection(0),
                        Context.PosingLogic, out var effectorNames))
                    {
                        var result = string.Join(", ", effectorNames.ToArray());
                        DeepPoseAnalytics.SendFrameCameraOnEffectorSelection(result);
                    }

                    Context.CameraMovement.Frame(bounds, 3f);

                    return true;
                }

                Bounds GetEntityBounds(EntityID entityID)
                {
                    var viewArmature = Context.PosingLogic.GetViewArmature(entityID);
                    var viewGameObject = viewArmature.gameObject;
                    var bounds = viewGameObject.GetRenderersWorldBounds();
                    return bounds;
                }
            }
        }
    }
}
