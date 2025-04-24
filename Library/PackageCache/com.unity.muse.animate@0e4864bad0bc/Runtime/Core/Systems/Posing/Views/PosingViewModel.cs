using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    class PosingViewModel
    {
        PosingModel m_PosingModel;
        SelectionModel<DeepPoseEffectorIndex> m_SelectionModel;
        CameraModel m_CameraModel;
        ArmatureStaticPoseModel m_SolvedPoseModel;
        List<PosingEffectorControlViewModel> m_EffectorViewModels;
        bool m_IsVisible;

        public bool IsVisible
        {
            get => m_IsVisible;
            set
            {
                if (m_IsVisible == value)
                    return;

                m_IsVisible = value;
                UpdateEffectorsVisibility();
            }
        }
        
        public int EffectorCount => m_PosingModel.EffectorCount;
        
        // [Section] UI Events

        public delegate void EffectorClicked(PosingViewModel viewModel, DeepPoseEffectorIndex effectorIndex);
        public delegate void EffectorRightClicked(PosingViewModel viewModel, DeepPoseEffectorIndex effectorIndex, Vector2 pointerPosition);
        public event EffectorClicked OnEffectorClicked;
        public event EffectorRightClicked OnEffectorRightClicked;
        public event Action<PosingEffectorControlViewModel, DeepPoseEffectorIndex, Vector2> OnEffectorBeginDrag;
        
        

        public PosingViewModel(PosingModel posingModel, SelectionModel<DeepPoseEffectorIndex> selectionModel,
            ArmatureStaticPoseModel solvedPose, CameraModel cameraModel)
        {
            Assert.IsNotNull(posingModel, "You must provide a posing model");
            Assert.IsNotNull(selectionModel, "You must provide a selection model");

            m_PosingModel = posingModel;
            m_SelectionModel = selectionModel;
            m_CameraModel = cameraModel;
            m_SolvedPoseModel = solvedPose;

            m_EffectorViewModels = new List<PosingEffectorControlViewModel>(EffectorCount);
            
            for (var i = 0; i < m_PosingModel.EffectorCount; i++)
            {
                m_EffectorViewModels.Add(CreateEffectorViewModel(i));
            }
            
            m_SelectionModel.OnSelectionChanged += OnSelectionModelSelectionChanged;
            UpdateEffectorsVisibility();
        }

        public void UpdateEffectorsVisibility()
        {
            foreach (var viewModel in m_EffectorViewModels)
            {
                viewModel.IsVisible = m_IsVisible;
            }
        }
        
        void OnSelectionModelSelectionChanged(SelectionModel<DeepPoseEffectorIndex> model)
        {
            foreach (var viewModel in m_EffectorViewModels)
            {
                viewModel.IsSelected = m_SelectionModel.IsSelected(viewModel.Index);
            }
        }

        PosingEffectorControlViewModel CreateEffectorViewModel(int effectorIdx)
        {
            var effectorModel = m_PosingModel.GetEffectorModel(effectorIdx);
            var solvedJointTransformModel = m_SolvedPoseModel.GetTransformModel(effectorModel.ArmatureJointIndex);
            var effectorViewModel = new PosingEffectorControlViewModel(effectorModel, m_SelectionModel, m_CameraModel, solvedJointTransformModel);
            effectorViewModel.IsVisible = false;
            effectorViewModel.OnEffectorClicked += OnPosingEffectorClicked;
            effectorViewModel.OnEffectorRightClicked += OnPosingEffectorRightClicked;
            effectorViewModel.OnEffectorBeginDrag += OnPosingEffectorBeginDrag;
            return effectorViewModel;
        }

        void OnPosingEffectorClicked(PosingEffectorControlViewModel viewModel)
        {
            OnEffectorClicked?.Invoke(this, viewModel.Index);
        }

        void OnPosingEffectorRightClicked(PosingEffectorControlViewModel viewModel, Vector2 pointerPosition)
        {
            OnEffectorRightClicked?.Invoke(this, viewModel.Index, pointerPosition);
        }

        public PosingEffectorControlViewModel GetEffector(int i)
        {
            return m_EffectorViewModels[i];
        }

        void OnPosingEffectorBeginDrag(PosingEffectorControlViewModel viewModel, Vector2 pointerPosition)
        {
            OnEffectorBeginDrag?.Invoke(viewModel, viewModel.Index, pointerPosition);
        }

        public void Step(float delta)
        {
            foreach (var viewModel in m_EffectorViewModels)
            {
                viewModel.Step(delta);
            }
        }
    }
}
