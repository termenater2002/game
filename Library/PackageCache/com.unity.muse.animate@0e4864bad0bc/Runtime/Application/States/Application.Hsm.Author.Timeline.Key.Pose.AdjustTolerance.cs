using Hsm;
using Unity.Muse.Animate.UserActions;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyPoseAdjustTolerance : ApplicationState<AuthorTimelineKeyPoseContext>, IKeyDownHandler
            {
                
                // Note: Maybe handles could be even more broadly shared
                ToleranceHandleView m_ToleranceHandleView;
                ToleranceHandleModel m_ToleranceHandleModel;
                ToleranceHandleViewModel m_ToleranceHandleViewModel;

                float m_PreviousTolerance;

                bool IsDirty;

                public override void OnEnter(object[] args)
                {
                    IsDirty = false;

                    base.OnEnter(args);

                    InitializeHandle();

                    Context.AuthoringModel.Timeline.OnRequestedDisableSelectedEffectors += OnRequestedDisableSelectedEffectors;
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
                    Context.PosingLogic.OnEffectorSelectionChanged -= OnEffectorSelectionChanged;
                    Context.PosingLogic.OnPosingChanged -= OnPosingChanged;
                    
                    UpdateCanDisableSelectedEffectors();
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    m_ToleranceHandleViewModel.Step(aDeltaTime);
                }

                void CheckTolerance()
                {
                    var newTolerance = m_ToleranceHandleModel.Tolerance;
                    if (newTolerance.NearlyEquals(m_PreviousTolerance, 1e-4f))
                        return;

                    EnableSelectedEffectors();
                    SetTolerance(newTolerance);

                    m_PreviousTolerance = newTolerance;
                    UpdateCanDisableSelectedEffectors();
                }
                
                void OnToleranceHandleChanged(ToleranceHandleModel model, float newTolerance)
                {
                    CheckTolerance();
                }
                
                void InitializeHandle()
                {
                    m_ToleranceHandleModel = new ToleranceHandleModel();
                    m_ToleranceHandleViewModel = new ToleranceHandleViewModel(m_ToleranceHandleModel, Context.Camera)
                    {
                        IsVisible = Context.PosingLogic.HasEffectorSelection
                    };

                    if (m_ToleranceHandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("EffectorToleranceHandle");
                        m_ToleranceHandleView = handleGo.AddComponent<ToleranceHandleView>();
                        m_ToleranceHandleView.Initialize();
                        m_ToleranceHandleView.OnHandleRightClick += Context.PosingLogic.OpenEffectorsContextMenu;
                    }

                    m_ToleranceHandleModel.OnToleranceChanged += OnToleranceHandleChanged;
                    m_ToleranceHandleView.SetModel(m_ToleranceHandleViewModel);
                    m_ToleranceHandleView.gameObject.SetActive(true);
                }

                void CloseHandle()
                {
                    m_ToleranceHandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_ToleranceHandleView.gameObject);
                }

                void SetTolerance(float tolerance)
                {
                    if (!IsDirty)
                    {
                        if (Context.PosingLogic.EffectorSelectionCount > 0)
                        {
                            if (DeepPoseAnalyticsUtils.TryGetSelectedEffectorNames(Context.EntitySelection.GetSelection(0),
                                    Context.PosingLogic, out var effectorNames))
                            {
                                var result = string.Join(", ", effectorNames.ToArray());
                                DeepPoseAnalytics.SendEffectorUsed(DeepPoseAnalytics.EffectorAction.AdjustTolerance, result);
                            }
                        }

                        IsDirty = true;
                    }
                    
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount;
                    for (var i = 0; i < selectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        effectorModel.PositionTolerance = tolerance;
                    }
                }

                void EnableSelectedEffectors()
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount;
                    for (var i = 0; i < selectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        if (effectorModel.HandlesPosition)
                            effectorModel.PositionEnabled = true;
                    }
                }

                void OnPosingChanged(PoseAuthoringLogic logic, EntityID entityID)
                {
                    if (m_ToleranceHandleViewModel.IsDragging)
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

                void UpdateHandleToSelection()
                {
                    var selectionCount = Context.PosingLogic.EffectorSelectionCount;

                    if (selectionCount != 1)
                    {
                        m_ToleranceHandleViewModel.IsVisible = false;
                        m_ToleranceHandleViewModel.CancelDrag();
                        return;
                    }

                    var effectorModel = Context.PosingLogic.GetSelectedEffector(0);
                    
                    if (!effectorModel.HandlesPosition)
                    {
                        m_ToleranceHandleViewModel.IsVisible = false;
                        m_ToleranceHandleViewModel.CancelDrag();
                        return;
                    }

                    m_ToleranceHandleViewModel.IsVisible = true;
                    m_PreviousTolerance = effectorModel.PositionTolerance;
                    m_ToleranceHandleModel.Position = effectorModel.Position;
                    m_ToleranceHandleModel.Tolerance = effectorModel.PositionTolerance;
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
                    UserActionsManager.Instance.StartUserEdit("Disabled effector(s) (tolerance)");

                    var selectionCount = Context.PosingLogic.EffectorSelectionCount;
                    for (var i = 0; i < selectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        effectorModel.PositionTolerance = 0f;
                    }
                }

                void OnRequestedDisableSelectedEffectors()
                {
                    DisableSelectedEffectors();
                }

                void UpdateCanDisableSelectedEffectors()
                {
                    bool canDisable = false;
                    for (var i = 0; i < Context.PosingLogic.EffectorSelectionCount; i++)
                    {
                        var effectorModel = Context.PosingLogic.GetSelectedEffector(i);
                        if (effectorModel.PositionTolerance > 0f)
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
