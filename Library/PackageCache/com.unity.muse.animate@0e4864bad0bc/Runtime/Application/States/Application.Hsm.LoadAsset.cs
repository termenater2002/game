using Hsm;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Muse.AppUI.UI;
using Unity.AppUI.Core;
using UnityEditor;
using AnimationMode = Unity.Muse.AppUI.UI.AnimationMode;

namespace Unity.Muse.Animate
{
    partial class Application
    {
        partial class ApplicationHsm
        {
            /// <summary>
            /// Loads a <see cref="LibraryItemAsset"/> in order to author it.
            /// </summary>
            public class LoadAsset : ApplicationState<ApplicationContext>
            {
                bool m_IsAssetLoaded;
                string m_AssetPath;
                LibraryItemAsset m_AssetToLoad;
                LoadSteps m_LoadStep;
                string m_LoadingError;

                enum LoadSteps
                {
                    LoadingAsset,
                    LoadingStage,
                    Loaded,
                    Failed
                }

                public override Transition GetTransition()
                {
                    return Transition.None();
                }

                public override void OnEnter(object[] args)
                {
                    Assert.AreEqual(2, args.Length);
                    base.OnEnter(new[] { args[0] });
                    
                    m_IsAssetLoaded = false;
                    m_AssetToLoad = null;
                    m_AssetPath = args[1] as string;
                    m_LoadStep = LoadSteps.LoadingAsset;

                    Context.SidePanelUIModel.IsVisible = false;
                    
                    LoadStep();
                }

                void LoadingError(string message)
                {
                    m_LoadingError = message;
                    m_LoadStep = LoadSteps.Failed;
                    
                    DevLogger.LogError(m_LoadingError);

                    Toast.Build(Context.RootVisualElement, m_LoadingError,
                            NotificationDuration.Short)
                        .SetStyle(NotificationStyle.Warning)
                        .SetAnimationMode(AnimationMode.Slide)
                        .Show();
                }

                public override void Update(float aDeltaTime)
                {
                    base.Update(aDeltaTime);
                    LoadStep();
                }

                void LoadStep()
                {
                    if (m_IsAssetLoaded)
                        return;
                    
                    if (m_LoadStep == LoadSteps.LoadingAsset)
                    {
                        m_AssetToLoad = AssetDatabase.LoadAssetAtPath<LibraryItemAsset>(m_AssetPath);
                    
                        if (m_AssetToLoad == null)
                        {
                            LoadingError("Loading Failed: m_AssetToLoad is null.");
                            return;
                        }

                        if (m_AssetToLoad.Version < ApplicationConstants.MinimumAssetVersion)
                        {
                            LoadingError("Loading Failed: Asset version is unsupported.");
                            return;
                        }
                    
                        if (m_AssetToLoad.Data == null)
                        {
                            LoadingError("Loading Failed: m_AssetToLoad.Data is null.");
                            return;
                        }
                    
                        if (m_AssetToLoad.StageModel == null)
                        {
                            LoadingError("Loading Failed: m_AssetToLoad.StageModel is null.");
                            return;
                        }
                    
                        if (m_AssetToLoad.StageModel.NumActors == 0)
                        {
                            LoadingError("Loading Failed: m_AssetToLoad.StageModel has no actors!");
                            return;
                        }

                        // Set the active library item
                        Context.ApplicationLibraryModel.ActiveLibraryItem = m_AssetToLoad;

                        // Match the selection with the item we want to edit
                        Context.ApplicationLibraryModel.RequestSelectLibraryItem(Context.ItemsSelection, m_AssetToLoad);
                    
                        m_LoadStep = LoadSteps.LoadingStage;
                    }

                    if (m_LoadStep == LoadSteps.LoadingStage)
                    {
                        Context.TextToMotionService.Stop();
                        Context.VideoToMotionService.Stop();
                        
                        Context.Stage.Unload();
                        Context.Stage.Load(Context, m_AssetToLoad.StageModel);
                        
                        Context.ApplicationLibraryModel.ResumeItems();
                        
                        RestoreCameraViewpoint();
                        
                        m_LoadStep = LoadSteps.Loaded;
                    }

                    if (m_LoadStep == LoadSteps.Loaded)
                    {
                        var toast = Toast.Build(Context.RootVisualElement, $"Loaded: {m_AssetToLoad.Title}",
                            NotificationDuration.Short);
                        toast.SetStyle(NotificationStyle.Positive);
                        toast.SetAnimationMode(AnimationMode.Slide);
                        toast.Show();

                        // Loading is done
                        m_IsAssetLoaded = true;
                        Instance.IsAuthoringAssetLoaded = true;
                    }

                    if (m_LoadStep == LoadSteps.Failed)
                    {
                        // Set the flag so that LoadStep() logical doesn't get executed again.
                        m_IsAssetLoaded = true;
                        Instance.IsAuthoringAssetLoaded = false;
                    }
                }

                void RestoreCameraViewpoint()
                {
                    if (Context.Stage.NumCameraViewpoints == 0)
                        return;

                    var cameraCoordinates = Context.Stage.GetCameraViewpoint(0);
                    Context.CameraMovement.SetCoordinates(cameraCoordinates.Pivot, cameraCoordinates.CameraPosition);
                }
            }
        }
    }
}
