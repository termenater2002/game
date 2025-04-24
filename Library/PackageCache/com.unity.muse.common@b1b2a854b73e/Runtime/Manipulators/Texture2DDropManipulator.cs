using System.Linq;
using UnityEngine;

namespace Unity.Muse.Common
{
    internal class Texture2DDropManipulator : DropManipulator<Texture2D>
    {
        static readonly string[] k_Extensions = new[]
        {
            ".png",
            ".jpg",
            ".jpeg"
        };
        
        public Texture2DDropManipulator(Model model)
            : base(model) { }

        protected override bool GetDroppableObjectForPath(string path, out Texture2D obj)
        {
            var ext = System.IO.Path.GetExtension(path)?.ToLower() ?? string.Empty;
            if (!k_Extensions.Contains(ext))
            {
                obj = null;
                return false;
            }
                
            var data = System.IO.File.ReadAllBytes(path);
            obj = new Texture2D(2, 2);
            obj.LoadImage(data);
            return true;
        }
    }
}
