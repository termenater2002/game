using Hsm;
using UnityEngine;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            public class AuthorTimelineKeyLoopTranslate : ApplicationState<AuthorTimelineKeyLoopContext>
            {
                // Note: Maybe handles could be even more broadly shared
                TranslationHandleView m_TranslationHandleView;
                TranslationHandleModel m_TranslationHandleModel;
                TranslationHandleViewModel m_TranslationHandleViewModel;

                Vector3 m_PreviousPosition;

                public override void OnEnter(object[] args)
                {
                    base.OnEnter(args);

                    InitializeHandle();

                    Context.KeySelection.OnSelectionChanged += OnKeySelectionChanged;
                    Context.EntitySelection.OnSelectionStateChanged += OnEntitySelectionChanged;
                    Context.LoopAuthoringLogic.OnLoopChanged += OnLoopChanged;

                    UpdateHandleToSelection();
                }

                public override void OnExit()
                {
                    base.OnExit();

                    CloseHandle();

                    Context.KeySelection.OnSelectionChanged -= OnKeySelectionChanged;
                    Context.EntitySelection.OnSelectionStateChanged -= OnEntitySelectionChanged;
                    Context.LoopAuthoringLogic.OnLoopChanged -= OnLoopChanged;
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    m_TranslationHandleViewModel.Step(aDeltaTime);
                    
                    var positionOffset = m_TranslationHandleModel.CurrentPosition - m_PreviousPosition;
                    if (positionOffset.magnitude < 1e-4f)
                        return;

                    TranslateSelection(positionOffset);

                    m_PreviousPosition = m_TranslationHandleModel.CurrentPosition;
                }

                void TranslateSelection(Vector3 positionOffset)
                {
                    if (!Context.KeySelection.HasSelection)
                        return;

                    var key = Context.KeySelection.GetSelection(0);
                    for (var i = 0; i < Context.EntitySelection.Count; i++)
                    {
                        var entityID = Context.EntitySelection.GetSelection(i);
                        if (!key.Key.Loop.TryGetOffset(entityID, out var loopOffset))
                            continue;

                        loopOffset.Position += positionOffset;
                    }
                }

                void InitializeHandle()
                {
                    m_TranslationHandleModel = new TranslationHandleModel(Vector3.zero, Quaternion.identity);
                    m_TranslationHandleViewModel = new TranslationHandleViewModel(m_TranslationHandleModel, Context.Camera)
                    {
                        IsVisible = Context.KeySelection.HasSelection
                    };

                    if (m_TranslationHandleView == null)
                    {
                        var handleGo = Locator.Get<IRootObjectSpawner<GameObject>>().CreateGameObject("LoopTranslationHandle");
                        m_TranslationHandleView = handleGo.AddComponent<TranslationHandleView>();
                        m_TranslationHandleView.Initialize();
                        
                    }
                    
                    m_TranslationHandleView.SetModel(m_TranslationHandleViewModel);
                }

                void CloseHandle()
                {
                    m_TranslationHandleViewModel.CancelDrag();
                    GameObjectUtils.Destroy(m_TranslationHandleView.gameObject);
                }

                void OnKeySelectionChanged(SelectionModel<TimelineModel.SequenceKey> model)
                {
                    UpdateHandleToSelection();
                }

                void OnEntitySelectionChanged(SelectionModel<EntityID> model, EntityID index, bool isselected)
                {
                    UpdateHandleToSelection();
                }

                void OnLoopChanged(LoopKeyModel loopKeyModel)
                {
                    if (m_TranslationHandleViewModel.IsDragging) return;
                    
                    UpdateHandleToSelection();
                }

                void UpdateHandleToSelection()
                {
                    m_TranslationHandleViewModel.IsVisible = Context.KeySelection.HasSelection && Context.EntitySelection.HasSelection;
                    if (!m_TranslationHandleViewModel.IsVisible)
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

                        // Note: for now, rotate handle kep always in Global model (design review feedback)
                        //if (Context.EntitySelection.Count == 1)
                        //    rotation = loopOffset.Rotation;

                        entityCount++;
                    }

                    if (entityCount > 0)
                        center /= entityCount;

                    m_PreviousPosition = center;
                    m_TranslationHandleModel.Reset(center, rotation);
                }
            }
        }
    }
}
