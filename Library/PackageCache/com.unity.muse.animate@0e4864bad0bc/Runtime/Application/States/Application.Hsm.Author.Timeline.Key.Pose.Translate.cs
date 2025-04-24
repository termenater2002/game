using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyPoseTranslate : ApplicationState<AuthorTimelineKeyPoseContext>, IKeyDownHandler
            {
                // Note: Maybe handles could be even more broadly shared
                TranslationHandleView m_TranslationHandleView;
                TranslationHandleModel m_TranslationHandleModel;
                TranslationHandleViewModel m_TranslationHandleViewModel;

                bool HasSelection => Context.PosingLogic.HasEffectorSelection || Context.EntityEffectorSelection.HasSelection;
                
                Vector3 m_PreviousPosition;

                bool IsDirty;
                
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

                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors -= OnRequestedDisableSelectedEffectors;
                    Context.EntityEffectorSelection.OnSelectionChanged -= OnEntityEffectorSelectionChanged;
                    Context.PosingLogic.OnEffectorSelectionChanged -= OnEffectorSelectionChanged;
                    Context.PosingLogic.OnPosingChanged -= OnPosingChanged;
                    
                    CloseHandle();
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    m_TranslationHandleViewModel.Step(aDeltaTime);
                    
                    if (!m_TranslationHandleViewModel.IsVisible)
                        return;

                    var positionOffset = m_TranslationHandleModel.CurrentPosition - m_PreviousPosition;
                    if (positionOffset.magnitude < 1e-4f)
                        return;

                    EnableSelectedEffectors();
                    TranslateSelectedEffectors(positionOffset);

                    m_PreviousPosition = m_TranslationHandleModel.CurrentPosition;
                }

                void InitializeHandle()
                {
                    m_TranslationHandleModel = new TranslationHandleModel(Vector3.zero, Quaternion.identity);
                    m_TranslationHandleViewModel = new TranslationHandleViewModel(m_TranslationHandleModel, Context.Camera)
                    {
                        IsVisible = HasSelection
                    };

                    if (m_TranslationHandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("EffectorTranslationHandle");
                        m_TranslationHandleView = handleGo.AddComponent<TranslationHandleView>();
                        m_TranslationHandleView.OnHandleRightClick += Context.PosingLogic.OpenEffectorsContextMenu;
                        m_TranslationHandleView.Initialize();
                    }
                    
                    m_TranslationHandleView.SetModel(m_TranslationHandleViewModel);
                    m_TranslationHandleView.gameObject.SetActive(true);
                }

                void CloseHandle()
                {
                    m_TranslationHandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_TranslationHandleView.gameObject);
                }

                void TranslateSelectedEffectors(Vector3 offset)
                {
                    if (!IsDirty)
                    {
                        if (Context.PosingLogic.EffectorSelectionCount > 0)
                        {
                            if (DeepPoseAnalyticsUtils.TryGetSelectedEffectorNames(Context.EntitySelection.GetSelection(0),
                                    Context.PosingLogic, out var effectorNames))
                            {
                                var result = string.Join(", ", effectorNames.ToArray());
                                DeepPoseAnalytics.SendEffectorUsed(DeepPoseAnalytics.EffectorAction.Translate, result);
                            }
                        }

                        if (Context.EntityEffectorSelection.Count > 0)
                        {
                            DeepPoseAnalytics.SendEntityEffectorUsed(DeepPoseAnalytics.EffectorAction.Translate);
                        }

                        IsDirty = true;
                    }
                    
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

                void EnableSelectedEffectors()
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

                void OnPosingChanged(PoseAuthoringLogic logic, EntityID entityID)
                {
                    if (m_TranslationHandleViewModel.IsDragging)
                        return;

                    UpdateHandleToSelection();
                    UpdateCanDisableSelectedEffectors();
                }

                void OnEffectorSelectionChanged(PoseAuthoringLogic logic)
                {
                    IsDirty = false;
                    DeselectIncompatibleEffectors();
                    m_TranslationHandleViewModel.CancelDrag();
                    UpdateHandleToSelection();
                    UpdateCanDisableSelectedEffectors();
                }

                void OnEntityEffectorSelectionChanged(SelectionModel<EntityID> model)
                {
                    IsDirty = false;
                    m_TranslationHandleViewModel.CancelDrag();
                    UpdateHandleToSelection();
                }

                void DeselectIncompatibleEffectors()
                {
                    using var toDeselect = TempList<EntityEffectorIndex>.Allocate();

                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorIndex = Context.PosingLogic.GetSelectedEffectorIndex(i);

                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        if (!effectorModel.HandlesPosition && !effectorModel.HandlesLookAt)
                            toDeselect.Add(effectorIndex);
                    }

                    foreach (var effectorIndex in toDeselect)
                    {
                        Context.PosingLogic.SetEffectorSelected(effectorIndex, false);
                    }
                }

                void UpdateHandleToSelection()
                {
                    var hasSelection = Context.PosingLogic.HasEffectorSelection || Context.EntityEffectorSelection.HasSelection;
                    m_TranslationHandleViewModel.IsVisible = hasSelection;
                    if (!hasSelection)
                        return;

                    var selectionCount = Context.PosingLogic.EffectorSelectionCount + Context.EntityEffectorSelection.Count;
                    var center = Vector3.zero;
                    var rotation = Quaternion.identity;
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        center += effectorModel.Position;

                        // Note: for now, rotate handle kep always in Global model (design review feedback)
                        // if (i == 0 && selectionCount == 1 && effectorModel.HandlesRotation && effectorModel.RotationEnabled)
                        //     rotation = effectorModel.Rotation;
                    }
                    for (var i = 0; i < Context.EntityEffectorSelection.Count; i++)
                    {
                        var id = Context.EntityEffectorSelection.GetSelection(i);
                        var effectorModel = Context.PosingLogic.GetEntityEffector(id);
                        center += effectorModel.Position;
                    }
                    center /= selectionCount;

                    m_PreviousPosition = center;
                    m_TranslationHandleModel.Reset(center, rotation);
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

                public virtual void OnKeyDown(KeyPressEvent eventData)
                {
                    if (ApplicationConstants.DebugStatesInputEvents) Log($"OnKeyDown({eventData.KeyCode})");
                    
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

                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
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
            }
        }
    }
}
