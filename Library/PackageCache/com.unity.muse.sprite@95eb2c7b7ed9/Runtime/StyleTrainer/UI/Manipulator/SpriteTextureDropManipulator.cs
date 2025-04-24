using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Sprite.Common.Backend;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.StyleTrainer.Manipulator
{
    class SpriteTextureDropManipulator : UnityEngine.UIElements.Manipulator
    {
        public event Action onDragStart;
        public event Action onDragEnd;
        public event Action<IList<Texture2D>> onDrop;

        bool isDragging { get; set; }

        public SpriteTextureDropManipulator()
        { }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
#if UNITY_EDITOR
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            target.RegisterCallback<DragUpdatedEvent>(OnSpriteDragUpdate);
            target.RegisterCallback<DragExitedEvent>(OnSpriteDragExit);
#endif
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
#if UNITY_EDITOR
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragUpdatedEvent>(OnSpriteDragUpdate);
            target.UnregisterCallback<DragExitedEvent>(OnSpriteDragExit);
#endif
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            OnEnter();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            OnExit();
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            OnExit();
        }
#if UNITY_EDITOR
        void OnDragPerform(DragPerformEvent evt)
        {
            OnPerform();
        }

        void OnSpriteDragUpdate(DragUpdatedEvent evt)
        {
            OnMove();
        }

        void OnSpriteDragExit(DragExitedEvent evt)
        {
            OnExit();
        }
#endif

        void OnEnter()
        {
            var textures = GetTextures();
            if (textures != null && textures.Count > 0)
            {
                isDragging = true;
                onDragStart?.Invoke();
            }
        }

        void OnExit()
        {
            if (!isDragging)
                return;

            isDragging = false;
            onDragEnd?.Invoke();
        }

        void OnPerform()
        {
            if (!isDragging)
                return;

            var textures = GetTextures();
            if (isDragging && textures is { Count: > 0 })
            {
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.AcceptDrag();
#endif
                onDrop?.Invoke(textures);
            }

            OnExit();
        }

        void OnMove()
        {
            if (!isDragging)
                return;

            var texture = GetTextures();
            if (texture == null || texture.Count == 0)
            {
                isDragging = false;
                onDragEnd?.Invoke();
            }
            else
            {
                isDragging = true;
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
#endif
            }
        }

        static IList<Texture2D> GetTextures()
        {
            var textures = new List<Texture2D>();
#if UNITY_EDITOR

            var spriteRefs = UnityEditor.DragAndDrop.objectReferences
                .OfType<UnityEngine.Sprite>()
                .Select(BackendUtilities.SpriteAsTexture);
            textures.AddRange(spriteRefs);

            var gameObjects = UnityEditor.DragAndDrop.objectReferences
                .Where(obj => obj is GameObject go && go.GetComponentInChildren<SpriteRenderer>()?.sprite != null)
                .Select(obj => BackendUtilities.SpriteAsTexture(((GameObject)obj).GetComponentInChildren<SpriteRenderer>().sprite));
            textures.AddRange(gameObjects);

            var textureRefs = UnityEditor.DragAndDrop.objectReferences
                .OfType<Texture2D>()
                .Select(tex => BackendUtilities.CreateTemporaryDuplicate(tex, tex.width, tex.height));
            textures.AddRange(textureRefs);

#endif

            return textures;
        }
    }
}