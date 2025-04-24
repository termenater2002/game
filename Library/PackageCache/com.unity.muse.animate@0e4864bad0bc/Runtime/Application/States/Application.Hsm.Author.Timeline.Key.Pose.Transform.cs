using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyPoseTransform : ApplicationState<AuthorTimelineKeyPoseContext>, IKeyDownHandler
            {
                // Note: Maybe handles could be even more broadly shared
                UniversalEffectorHandleView m_HandleView;
                UniversalEffectorHandleViewModel m_HandleViewModel;
                UniversalEffectorHandleModel m_HandleModel;

                Vector3 m_PreviousPosition;
                Quaternion m_PreviousRotation;
                
                bool HasSelection => Context.PosingLogic.HasEffectorSelection || Context.EntityEffectorSelection.HasSelection;
                
                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    InitializeHandle();

                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors += OnRequestedDisableSelectedEffectors;
                    Context.EntityEffectorSelection.OnSelectionChanged += OnEntityEffectorSelectionChanged;
                    Context.PosingLogic.OnEffectorSelectionChanged += OnEffectorSelectionChanged;
                    Context.PosingLogic.OnPosingChanged += OnPosingChanged;

                    UpdateHandleToSelection();
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
                    m_HandleViewModel.Step(aDeltaTime);
                    
                    if (!m_HandleViewModel.IsVisible)
                        return;

                    var rotationOffset = m_HandleModel.CurrentRotation * Quaternion.Inverse(m_PreviousRotation);
                    var positionOffset = m_HandleModel.CurrentPosition - m_PreviousPosition;
                    var moved = !positionOffset.magnitude.NearlyEquals(0f, 1e-4f);
                    var rotated = !rotationOffset.NearlyEquals(Quaternion.identity, 1e-5f);

                    if (!moved && !rotated)
                        return;

                    EnableSelectedEffectors(moved, rotated);

                    if (moved)
                    {
                        TranslateSelectedEffectors(positionOffset);
                        m_PreviousPosition = m_HandleModel.CurrentPosition;
                    }

                    if (rotated)
                    {
                        RotateSelectedEffectors(m_HandleModel.CurrentPosition, rotationOffset);
                        m_PreviousRotation = m_HandleModel.CurrentRotation;
                    }
                }

                void InitializeHandle()
                {
                    m_HandleModel = new UniversalEffectorHandleModel(Vector3.zero, Quaternion.identity);
                    m_HandleViewModel = new UniversalEffectorHandleViewModel(m_HandleModel, Context.Camera)
                    {
                        IsVisible = HasSelection
                    };

                    if (m_HandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("EffectorUniversalHandle");
                        m_HandleView = handleGo.AddComponent<UniversalEffectorHandleView>();
                        m_HandleView.OnHandleRightClick += Context.PosingLogic.OpenEffectorsContextMenu;
                        // m_HandleView.CreateShapes();
                    }

                    m_HandleView.SetModel(m_HandleViewModel);
                    m_HandleView.gameObject.SetActive(true);
                }

                void CloseHandle()
                {
                    m_HandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_HandleView.gameObject);
                }

                void TranslateSelectedEffectors(Vector3 offset)
                {
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        effectorModel.Position += offset;
                    }

                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        Context.PosingLogic.Translate(id, offset);
                    }
                }

                void RotateSelectedEffectors(Vector3 pivot, Quaternion rotationOffset)
                {
                    using var tmpList = TempList<DeepPoseEffectorModel>.Allocate();
                    Context.PosingLogic.GetSelectedEffectorModels(tmpList.List);

                    var rotationMode = IsInRotateMode() ? EffectorUtils.RotationMode.Orientation : EffectorUtils.RotationMode.PositionAndLookAtTargetAndEnabledRotations;
                    EffectorUtils.RotateEffectors(tmpList.List, pivot, rotationOffset, rotationMode);

                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        Context.PosingLogic.Rotate(id, pivot, rotationOffset);
                    }
                }

                void EnableSelectedEffectors(bool moved, bool rotated)
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;

                    if (selectionCount == 1 && Context.PosingLogic.HasEffectorSelection)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(0);
                        if (effectorModel.HandlesRotation && rotated)
                            effectorModel.RotationEnabled = true;
                    }

                    if (moved || selectionCount > 1)
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
                    if (m_HandleViewModel.IsDragging)
                        return;

                    UpdateHandleToSelection();
                }

                void OnEffectorSelectionChanged(PoseAuthoringLogic logic)
                {
                    UpdateHandleToSelection();
                }

                void OnEntityEffectorSelectionChanged(SelectionModel<EntityID> model)
                {
                    UpdateHandleToSelection();
                }

                void UpdateHandleToSelection()
                {
                    var hasSelection = Context.PosingLogic.HasEffectorSelection || Context.EntityEffectorSelection.HasSelection;
                    m_HandleViewModel.IsVisible = hasSelection;
                    if (!hasSelection)
                        return;

                    var handleRotation = Quaternion.identity;

                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;
                    var center = Vector3.zero;
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        center += effectorModel.Position;

                        if (i == 0 && selectionCount == 1)
                            handleRotation = effectorModel.Rotation;
                    }
                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        var effectorModel = Context.PosingLogic.GetEntityEffector(id);
                        center += effectorModel.Position;

                        if (i == 0 && selectionCount == 1)
                            handleRotation = effectorModel.Rotation;
                    }
                    center /= selectionCount;

                    m_PreviousPosition = center;
                    m_PreviousRotation = handleRotation;
                    m_HandleModel.Reset(center, handleRotation);

                    m_HandleModel.RotationEnabled = IsInRotateMode() || selectionCount > 1;
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

                bool IsInRotateMode()
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;
                    if (selectionCount == 1 && Context.PosingLogic.HasEffectorSelection)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(0);

                        if (effectorModel.HandlesRotation)
                            return true;
                    }

                    return false;
                }

                void DisableSelectedEffectors()
                {
                    UserActionsManager.Instance.StartUserEdit("Disabled effector(s) (all)");

                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        effectorModel.PositionEnabled = false;
                        effectorModel.RotationEnabled = false;
                        effectorModel.LookAtEnabled = false;
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
