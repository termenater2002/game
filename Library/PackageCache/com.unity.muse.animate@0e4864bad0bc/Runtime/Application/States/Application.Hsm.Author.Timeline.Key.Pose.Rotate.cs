using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyPoseRotate : ApplicationState<AuthorTimelineKeyPoseContext>, IKeyDownHandler
            {
                // Note: Maybe handles could be even more broadly shared
                RotationHandleView m_RotationHandleView;
                RotationHandleViewModel m_RotationHandleViewModel;
                RotationHandleModel m_RotationHandleModel;

                Quaternion m_PreviousRotation;

                bool IsDirty;
                
                bool HasSelection => Context.PosingLogic.HasEffectorSelection || Context.EntityEffectorSelection.HasSelection;
                
                public override void OnEnter(object[] args)
                {
                    IsDirty = false;

                    base.OnEnter(args);

                    InitializeHandle();

                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors += OnRequestedDisableSelectedEffectors;
                    Context.EntityEffectorSelection.OnSelectionChanged += OnEntityEffectorSelectionChanged;
                    Context.PosingLogic.OnEffectorSelectionChanged += OnEffectorSelectionChanged;
                    Context.PosingLogic.OnPosingChanged += OnPosingChanged;

                    UpdateHandleToSelection();
                    UpdateCanDisableSelectedEffectors();
                }

                public override void OnExit()
                {
                    base.OnExit();

                    CloseHandle();

                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors -= OnRequestedDisableSelectedEffectors;
                    Context.EntityEffectorSelection.OnSelectionChanged -= OnEntityEffectorSelectionChanged;
                    Context.PosingLogic.OnEffectorSelectionChanged -= OnEffectorSelectionChanged;
                    Context.PosingLogic.OnPosingChanged -= OnPosingChanged;
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    m_RotationHandleViewModel.Step(aDeltaTime);
                    
                    if (!m_RotationHandleViewModel.IsVisible)
                        return;

                    var rotationOffset = m_RotationHandleModel.CurrentRotation * Quaternion.Inverse(m_PreviousRotation);
                    if (rotationOffset.NearlyEquals(Quaternion.identity, 1e-5f))
                        return;

                    EnableSelectedEffectors();
                    RotateSelectedEffectors(m_RotationHandleModel.CurrentPosition, rotationOffset);
                    UpdateCanDisableSelectedEffectors();

                    m_PreviousRotation = m_RotationHandleModel.CurrentRotation;
                }

                void InitializeHandle()
                {
                    m_RotationHandleModel = new RotationHandleModel(Vector3.zero, Quaternion.identity);
                    m_RotationHandleViewModel = new RotationHandleViewModel(m_RotationHandleModel, Context.Camera)
                    {
                        IsVisible = HasSelection
                    };

                    if (m_RotationHandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("EffectorRotationHandle");
                        m_RotationHandleView = handleGo.AddComponent<RotationHandleView>();
                        m_RotationHandleView.OnHandleRightClick += Context.PosingLogic.OpenEffectorsContextMenu;
                        m_RotationHandleView.Initialize();
                    }

                    m_RotationHandleView.SetModel(m_RotationHandleViewModel);
                }

                void CloseHandle()
                {
                    m_RotationHandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_RotationHandleView.gameObject);
                }

                void RotateSelectedEffectors(Vector3 pivot, Quaternion rotationOffset)
                {
                    if (!IsDirty)
                    {
                        if (Context.PosingLogic.EffectorSelectionCount > 0)
                        {
                            if (DeepPoseAnalyticsUtils.TryGetSelectedEffectorNames(Context.EntitySelection.GetSelection(0),
                                    Context.PosingLogic, out var effectorNames))
                            {
                                var result = string.Join(", ", effectorNames.ToArray());
                                DeepPoseAnalytics.SendEffectorUsed(DeepPoseAnalytics.EffectorAction.Rotate, result);
                            }
                        }

                        if (Context.EntityEffectorSelection.Count > 0)
                        {
                            DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Rotate);
                        }

                        IsDirty = true;
                    }
                    
                    using var tmpList = TempList<DeepPoseEffectorModel>.Allocate();
                    Context.PosingLogic.GetSelectedEffectorModels(tmpList.List);

                    var rotationMode = GetRotationMode();
                    EffectorUtils.RotateEffectors(tmpList.List, pivot, rotationOffset, rotationMode);

                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        Context.PosingLogic.Rotate(id, pivot, rotationOffset);
                    }
                }

                
                EffectorUtils.RotationMode GetRotationMode()
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;
                    if (selectionCount == 1 && Context.PosingLogic.HasEffectorSelection)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(0);

                        if (effectorModel.HandlesRotation)
                            return EffectorUtils.RotationMode.Orientation;
                    }

                    return EffectorUtils.RotationMode.PositionAndLookAtTargetAndEnabledRotations;
                }

                void EnableSelectedEffectors()
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;

                    // Note: single effector means controlling its orientation
                    // Multiple effectors means controlling their position

                    if (selectionCount == 1 && Context.PosingLogic.HasEffectorSelection)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(0);

                        if (effectorModel.HandlesRotation)
                            effectorModel.RotationEnabled = true;
                    }
                    else
                    {
                        for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                        {
                            var effectorModel = Context.PosingLogic.GetSelectedEffector(i);

                            if (effectorModel.HandlesPosition)
                                effectorModel.PositionEnabled = true;
                            if (effectorModel.HandlesLookAt)
                                effectorModel.LookAtEnabled = true;
                        }
                    }
                }

                void OnPosingChanged(PoseAuthoringLogic logic, EntityID entityID)
                {
                    if (m_RotationHandleViewModel.IsDragging)
                        return;

                    UpdateHandleToSelection();
                    UpdateCanDisableSelectedEffectors();
                }

                void OnEffectorSelectionChanged(PoseAuthoringLogic logic)
                {
                    IsDirty = false;
                    UpdateHandleToSelection();
                    UpdateCanDisableSelectedEffectors();
                }

                void OnEntityEffectorSelectionChanged(SelectionModel<EntityID> model)
                {
                    IsDirty = false;
                    UpdateHandleToSelection();
                }

                void UpdateHandleToSelection()
                {
                    var handleRotation = Quaternion.identity;
                    var totalSelectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;
                    var center = Vector3.zero;
                    bool isSingledSelectedEffectorCompatible = false;
                    
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);

                        center += effectorModel.Position;

                        if (i == 0 && totalSelectionCount == 1)
                            handleRotation = effectorModel.Rotation;

                        if (effectorModel.HandlesRotation || (effectorModel.HandlesLookAt && ApplicationConstants.EnableGeneralizedLookAt))
                            isSingledSelectedEffectorCompatible = true;
                    }

                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        var effectorModel = Context.PosingLogic.GetEntityEffector(id);
                        center += effectorModel.Position;
                        
                        if (i == 0 && totalSelectionCount == 1)
                            handleRotation = effectorModel.Rotation;
                    }

                    var showHandle = totalSelectionCount > 1
                        || (totalSelectionCount == 1 && isSingledSelectedEffectorCompatible)
                        || Context.EntityEffectorSelection.Count > 0;

                    if (showHandle)
                    {
                        center /= totalSelectionCount;

                        m_PreviousRotation = handleRotation;
                        m_RotationHandleViewModel.IsVisible = true;
                        m_RotationHandleModel.Reset(center, handleRotation);
                    }
                    else
                    {
                        m_RotationHandleViewModel.IsVisible = false;
                        m_RotationHandleViewModel.CancelDrag();
                    }
                }

                void UpdateCanDisableSelectedEffectors()
                {
                    bool canDisable = false;
                
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        if (effectorModel.RotationEnabled)
                        {
                            canDisable = true;
                            break;
                        }
                    }
                    
                    Context.AuthoringModel.Timeline.CanDisableSelectedEffectors = canDisable;
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
                    UserActionsManager.Instance.StartUserEdit("Disabled effector(s) (rotation)");

                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        effectorModel.RotationEnabled = false;
                    }

                    Context.PosingLogic.ClearEffectorsSelection();
                }

                void OnRequestedDisableSelectedEffectors()
                {
                    DisableSelectedEffectors();
                }
            }
        }
    }
}
