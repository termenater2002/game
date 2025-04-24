using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.Muse.Common
{
    internal abstract class DropManipulator<T> : Manipulator
        where T : Object
    {
        public Artifact artifact { get; protected set; }
        
        public event Action onDragStart;
        
        public event Action onDragEnd;
        
        public event Action<T> onDrop;

        Model m_Model;

        T m_Object;
        
        static readonly Dictionary<string, T> k_CacheForPath = new Dictionary<string, T>();

        bool isDragging => m_Object != null;

        public DropManipulator(Model model)
        {
            m_Model = model;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
#if UNITY_EDITOR
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragExitedEvent>(OnDragExit);
#endif
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
#if UNITY_EDITOR
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
#endif
        }

        /// <summary>
        /// This method is called when the user drops an artifact on the target.
        /// </summary>
        /// <param name="artifact"> The artifact dropped on the target. </param>
        /// <param name="obj"> The object to be dropped. </param>
        /// <returns> True if the object is valid and can be dropped, false otherwise. </returns>
        protected virtual bool GetDroppableObjectForArtifact(Artifact artifact, out T obj)
        {
            if (artifact is not null && ArtifactCache.IsInCache(artifact))
            {
                obj = (T)ArtifactCache.Read(artifact);
                this.artifact = artifact;
                return true;
            }
            
            obj = null;
            this.artifact = null;
            return false;
        }
        
        /// <summary>
        /// This method is called when the user drops a Unity object on the target.
        /// </summary>
        /// <param name="objects"> The objects dropped on the target. </param>
        /// <param name="obj"> The object to be dropped. </param>
        /// <returns> True if the object is valid and can be dropped, false otherwise. </returns>
        protected virtual bool GetDroppableObjectForUnityObjects(Object[] objects, out T obj)
        {
            var firstObject = objects.FirstOrDefault(o => o is T);
            
            if (firstObject)
            {
                obj = (T)firstObject;
                return true;
            }
            
            obj = null;
            return false;
        }
        
        /// <summary>
        /// This method is called when the user drops a file on the target.
        /// </summary>
        /// <remarks>
        /// The result will be cached and this cache will be invalidated when the current drag operation ends.
        /// </remarks>
        /// <param name="path"> The path of the file dropped on the target. </param>
        /// <param name="obj"> The object to be dropped. </param>
        /// <returns> True if the object is valid and can be dropped, false otherwise. </returns>
        protected virtual bool GetDroppableObjectForPath(string path, out T obj)
        {
            obj = null;
            return false;
        }

        void OnArtifactsDrop(IEnumerable<Artifact> artifacts, Vector3 pos)
        {
            if(!isDragging)
                return;
            
            var artifact = artifacts.FirstOrDefault();

            if (GetDroppableObjectForArtifact(artifact, out var obj))
            {
                m_Object = obj;
                onDrop?.Invoke(m_Object);
                
                OnExit();
            }
        }

        void OnPointerEnter(PointerEnterEvent evt) => OnEnter();
        void OnPointerLeave(PointerLeaveEvent evt) => OnExit();
        void OnPointerCancel(PointerCancelEvent evt) => OnExit();
#if UNITY_EDITOR
        void OnDragPerform(DragPerformEvent evt) => OnPerform();
        void OnDragUpdate(DragUpdatedEvent evt) => OnMove();
        void OnDragExit(DragExitedEvent evt) => OnExit();
#endif

        void OnEnter()
        {
            var obj = GetObject();
            if (obj)
            {
                m_Object = obj;
                onDragStart?.Invoke();

                m_Model.OnItemsDropped += OnArtifactsDrop;
            }
        }

        void OnExit()
        {
            if (!isDragging)
                return;

            m_Object = null;
            InvokeDragEnd();

            m_Model.OnItemsDropped -= OnArtifactsDrop;
        }

        void OnPerform()
        {
            if(!isDragging)
                return;

            var obj = GetObject();
            if (obj)
            {
                m_Object = obj;
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.AcceptDrag();
#endif
                onDrop?.Invoke(obj);
            }
            
            OnExit();
        }

        void OnMove()
        {
            if (!isDragging)
                return;
            
            var obj = GetObject();
            if (!obj)
            {
                m_Object = null;
                InvokeDragEnd();
            }
            else
            {
                m_Object = obj;
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
#endif
            }
        }

        void InvokeDragEnd()
        {
            onDragEnd?.Invoke();
            k_CacheForPath.Clear();
        }

        T GetObject()
        {
            var artifact = m_Model.DraggedArtifacts?.FirstOrDefault();
            
            if (GetDroppableObjectForArtifact(artifact, out var obj))
                return obj;
            
#if UNITY_EDITOR
            if (GetDroppableObjectForUnityObjects(UnityEditor.DragAndDrop.objectReferences, out var unityObj))
                return unityObj;


            foreach (var path in UnityEditor.DragAndDrop.paths)
            {
                if (k_CacheForPath.TryGetValue(path, out var cached))
                    return cached;

                if (GetDroppableObjectForPath(path, out var pathObj))
                {
                    k_CacheForPath[path] = pathObj;
                    return pathObj;
                }
            }
#endif

            return null;
        }
    }
}
