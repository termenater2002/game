using System;

namespace Unity.Muse.Common
{
    internal interface IVariateArtifact
    {
        public void Variate(Model model, int variationNbr = 4);
    }
}
