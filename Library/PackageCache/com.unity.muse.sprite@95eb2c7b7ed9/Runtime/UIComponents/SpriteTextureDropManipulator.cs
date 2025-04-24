using System;
using System.Linq;
using Unity.Muse.Common;
using Unity.Muse.Sprite.Artifacts;
using Unity.Muse.Sprite.Common.Backend;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.Muse.Sprite.UIComponents
{
    class SpriteTextureDropManipulator : Texture2DDropManipulator
    {
        public SpriteTextureDropManipulator(Model model)
            : base(model) { }
        
        protected override bool GetDroppableObjectForArtifact(Artifact artifact, out Texture2D obj)
        {
            if (artifact is not SpriteMuseArtifact)
            {
                obj = null;
                return false;
            }
            
            return base.GetDroppableObjectForArtifact(artifact, out obj);
        }

        protected override bool GetDroppableObjectForUnityObjects(Object[] objects, out Texture2D obj)
        {
            var spriteRef = objects.FirstOrDefault(obj => obj is UnityEngine.Sprite);
            if (spriteRef)
            {
                obj = BackendUtilities.SpriteAsTexture((UnityEngine.Sprite) spriteRef);
                return true;
            }
            
            var go = objects.FirstOrDefault(obj => obj is GameObject go && go.GetComponentInChildren<SpriteRenderer>()?.sprite != null);
            if (go)
            {
                obj = BackendUtilities.SpriteAsTexture(((GameObject) go).GetComponent<SpriteRenderer>().sprite);
                return true;
            }
            
            var textureRef = objects.FirstOrDefault(obj => obj is Texture2D);
            if (textureRef)
            {
                var texture = (Texture2D)textureRef;
                obj = BackendUtilities.CreateTemporaryDuplicate(texture, texture.width, texture.height);
                return true;
            }
            
            obj = null;
            return false;
        }

        // Sprite Muse Alpha - WebGL
        // void OnPreviewDrop(IEnumerable<Texture2D> previews, Vector3 pos)
        // {
        //     if(!isDragging)
        //         return;
        //     
        //     var preview = previews.FirstOrDefault();
        //     if (preview != null)
        //     {
        //         m_Texture = preview;
        //         onDrop?.Invoke(m_Texture);
        //             
        //         OnExit();
        //     }
        // }
        // Sprite Muse Alpha - WebGL
    }
}