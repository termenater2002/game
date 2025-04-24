using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using Unity.Muse.Sprite.Artifacts;
using Unity.Muse.Sprite.Editor.Analytics;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Sprite.Editor.DragAndDrop
{
    sealed class SpriteArtifactDragAndDropHandler : IArtifactDragAndDropHandler
    {
        SpriteMuseArtifact m_Artifact;

        public event Action<string, Artifact> ArtifactDropped;

        public static void Register()
        {
            DragAndDropFactory.SetHandlerForArtifact("Sprite", typeof(SpriteArtifactDragAndDropHandler));
        }

        public SpriteArtifactDragAndDropHandler(IList<Artifact> artifact)
        {
            m_Artifact = (SpriteMuseArtifact)artifact.FirstOrDefault();
        }

        public bool CanDropSceneView(GameObject dropUpon, Vector3 worldPosition) => true;

        public void HandleDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            Model.SendAnalytics(new SaveSpriteAnalytic(SpriteSaveDestination.SceneView, true, ""));

            CreateNewSprite(worldPosition);

            ArtifactDropped?.Invoke(null, m_Artifact);
        }

        public bool CanDropHierarchy(GameObject dropUpon) => true;

        public void HandleDropHierarchy(GameObject dropUpon)
        {
            Model.SendAnalytics(new SaveSpriteAnalytic(SpriteSaveDestination.HierarchyWindow, true, ""));

            CreateNewSprite(Vector3.zero);

            ArtifactDropped?.Invoke(null, m_Artifact);
        }

        public bool CanDropProject(string path) => true;

        public void HandleDropProject(string path)
        {
            if(File.Exists(path))
                path = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(path))
                path = ExporterHelpers.assetsRoot;

            Model.SendAnalytics(new SaveSpriteAnalytic(SpriteSaveDestination.ProjectWindow, true, ""));

            m_Artifact.ExportToDirectory(path, true, exportedPath =>
            {
                ArtifactDropped?.Invoke(exportedPath, m_Artifact);
            });
        }

        void CreateNewSprite(Vector3 position)
        {
            m_Artifact.ExportToDirectory(ExporterHelpers.assetsRoot, true, savePath =>
            {
                if (!string.IsNullOrEmpty(savePath) && ExporterHelpers.IsInAssets(savePath, out var relativePath))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(relativePath);
                    var go = new GameObject(sprite.name);
                    go.transform.position = position;
                    var spriteRenderer = go.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = sprite;

                    Undo.RegisterCreatedObjectUndo(go, $"Create Sprite ({sprite.name})");
                }
            });
        }
    }
}
