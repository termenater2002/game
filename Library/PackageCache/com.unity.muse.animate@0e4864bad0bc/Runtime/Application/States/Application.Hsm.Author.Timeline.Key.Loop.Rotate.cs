using Hsm;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyLoopRotate : ApplicationState<AuthorTimelineKeyLoopContext>
            {
                // Note: Maybe handles could be even more broadly shared
                RotationHandleView m_RotationHandleView;
                RotationHandleViewModel m_RotationHandleViewModel;
                RotationHandleModel m_RotationHandleModel;

                Quaternion m_PreviousRotation;

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    InitializeHandle();

                    Context.KeySelection.OnSelectionChanged += OnKeySelectionChanged;
                    Context.EntitySelection.OnSelectionChanged += OnEntitySelectionChanged;
                    Context.LoopAuthoringLogic.OnLoopChanged += OnLoopChanged;

                    UpdateHandleToSelection();
                }

                public override void OnExit()
                {
                    base.OnExit();

                    CloseHandle();

                    Context.KeySelection.OnSelectionChanged -= OnKeySelectionChanged;
                    Context.EntitySelection.OnSelectionChanged -= OnEntitySelectionChanged;
                    Context.LoopAuthoringLogic.OnLoopChanged -= OnLoopChanged;
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    m_RotationHandleViewModel.Step(aDeltaTime);
                    
                    var rotationOffset = m_RotationHandleModel.CurrentRotation * Quaternion.Inverse(m_PreviousRotation);
                    if (rotationOffset.NearlyEquals(Quaternion.identity, 1e-5f))
                        return;

                    RotateSelection(rotationOffset);

                    m_PreviousRotation = m_RotationHandleModel.CurrentRotation;
                }

                void RotateSelection(Quaternion rotationOffset)
                {
                    if (!Context.KeySelection.HasSelection)
                        return;

                    var key = Context.KeySelection.GetSelection(0);

                    for (var i = 0; i < Context.EntitySelection.Count; i++)
                    {
                        var entityID = Context.EntitySelection.GetSelection(i);
                        if (!key.Key.Loop.TryGetOffset(entityID, out var loopOffset))
                            continue;

                        loopOffset.Rotation = rotationOffset * loopOffset.Rotation;
                    }
                }

                void InitializeHandle()
                {
                    m_RotationHandleModel = new RotationHandleModel(Vector3.zero, Quaternion.identity);
                    m_RotationHandleViewModel = new RotationHandleViewModel(m_RotationHandleModel, Context.Camera)
                    {
                        IsVisible = Context.KeySelection.HasSelection
                    };

                    if (m_RotationHandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("LoopRotationHandle");
                        m_RotationHandleView = handleGo.AddComponent<RotationHandleView>();
                        m_RotationHandleView.Initialize();
                    }

                    m_RotationHandleView.SetModel(m_RotationHandleViewModel);
                    m_RotationHandleView.gameObject.SetActive(true);
                }

                void CloseHandle()
                {
                    m_RotationHandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_RotationHandleView.gameObject);
                }

                void OnEntitySelectionChanged(SelectionModel<EntityID> model)
                {
                    UpdateHandleToSelection();
                }

                void OnKeySelectionChanged(SelectionModel<TimelineModel.SequenceKey> model)
                {
                    UpdateHandleToSelection();
                }
                
                void OnLoopChanged(LoopKeyModel loopKeyModel)
                {
                    if (m_RotationHandleViewModel.IsDragging) return;
                    
                    UpdateHandleToSelection();
                }

                void UpdateHandleToSelection()
                {
                    m_RotationHandleViewModel.IsVisible = Context.KeySelection.HasSelection && Context.EntitySelection.HasSelection;
                    if (!m_RotationHandleViewModel.IsVisible)
                        return;

                    var key = Context.KeySelection.GetSelection(0);

                    var center = Vector3.zero;
                    var rotation = Quaternion.identity;

                    var entityCount = 0;
                    for (var i = 0; i < Context.EntitySelection.Count; i++)
                    {
                        var entityID = Context.EntitySelection.GetSelection(i);
                        if (!Context.LoopAuthoringLogic.HasEntity(entityID))
                            continue;

                        if (!Context.LoopAuthoringLogic.TryGetEntityReferencePosition(entityID, out var referencePosition))
                            continue;

                        if (!key.Key.Loop.TryGetOffset(entityID, out var loopOffset))
                            continue;

                        center += referencePosition + loopOffset.Position;
                        if (Context.EntitySelection.Count == 1)
                            rotation = loopOffset.Rotation;

                        entityCount++;
                    }

                    if (entityCount > 0)
                        center /= entityCount;

                    m_PreviousRotation = rotation;
                    m_RotationHandleModel.Reset(center, rotation);
                }
            }
        }
    }
}
