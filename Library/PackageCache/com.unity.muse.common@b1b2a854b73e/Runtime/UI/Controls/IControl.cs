using UnityEngine;

namespace Unity.Muse.Common
{
    internal interface IControl
    {
        public void SetModel(Model model);

        public void UpdateView();
    }
}
