using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Common.Editor
{
    [InitializeOnLoad]
    static class ArtifactDragAndDropHandler
    {
        public const string dragAndDropKey = nameof(ArtifactDragAndDropHandler);

        const string k_RootFolderName = "Assets";

        static ArtifactDragAndDropHandler()
        {
            DragAndDrop.AddDropHandler(HandleDropHierarchy);
            DragAndDrop.AddDropHandler(HandleDropScene);
            DragAndDrop.AddDropHandler(HandleDropProject);
        }

        public static void StartDrag(IArtifactDragAndDropHandler handler, string dragHint)
        {
            DragAndDrop.AcceptDrag();
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(dragAndDropKey, handler);
            DragAndDrop.StartDrag(dragHint);
        }

        static DragAndDropVisualMode HandleDropHierarchy(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            CleanupData();
            return HandleDropHierarchyInternal(dropTargetInstanceID, dropMode, perform);
        }

        static DragAndDropVisualMode HandleDropProject(int id, string path, bool perform)
        {
            CleanupData();
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith(k_RootFolderName))
                return DragAndDropVisualMode.None;

            var draggable = GetHandler();
            if(draggable == null || !draggable.CanDropProject(path))
                return DragAndDropVisualMode.None;

            if (perform)
            {
                draggable.HandleDropProject(path);
            }

            return DragAndDropVisualMode.Copy;
        }

        static DragAndDropVisualMode HandleDropSceneInternal(Object[] objectReferences, Object dropUpon, Vector3 worldPosition, bool perform)
        {
            var draggable = GetHandler();
            if(draggable == null || !draggable.CanDropSceneView((GameObject)dropUpon, worldPosition))
                return DragAndDropVisualMode.None;

            if (perform)
                draggable.HandleDropSceneView((GameObject)dropUpon, worldPosition);

            return DragAndDropVisualMode.Copy;
        }

        static DragAndDropVisualMode HandleDropHierarchyInternal(int dropTargetInstanceID, HierarchyDropFlags dropMode, bool perform)
        {
            var parentGo = (GameObject)EditorUtility.InstanceIDToObject(dropTargetInstanceID);
            var draggable = GetHandler();
            if(draggable == null || !draggable.CanDropHierarchy(parentGo))
                return DragAndDropVisualMode.None;

            if (perform)
                draggable.HandleDropHierarchy(parentGo);

            return DragAndDropVisualMode.Copy;
        }

        static DragAndDropVisualMode HandleDropScene(Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            CleanupData();
            return HandleDropSceneInternal(DragAndDrop.objectReferences, dropUpon, worldPosition, perform);
        }

        static void CleanupData()
        {
            if(DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0 || DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                DragAndDrop.SetGenericData(dragAndDropKey, null);
        }

        static IArtifactDragAndDropHandler GetHandler() => (IArtifactDragAndDropHandler)DragAndDrop.GetGenericData(dragAndDropKey);
    }
}