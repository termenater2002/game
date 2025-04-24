using System;
using Unity.Muse.Chat.FunctionCalling;

namespace Unity.Muse.Chat.Context.SmartContext
{
    internal partial class SmartContextToolbox
    {
        internal class ExtractedContext
        {
            public string Payload { get; set; }
            public string ContextType { get; set; }

            public bool Truncated { get; set; }
        }

        internal class ContextSelection : IContextSelection
        {
            private IContextSelection _contextSelectionImplementation;
            string IContextSelection.DownsizedPayload => Payload;

            string IContextSelection.ContextType => m_ContextType;

            string IContextSelection.TargetName => "";

            bool? IContextSelection.Truncated => m_Truncated;

            public string Classifier { get; } = "smart";
            public string Description { get; }
            public string Payload { get; }

            private string m_ContextType;
            private bool? m_Truncated;

            public ContextSelection(CachedFunction selector, ExtractedContext result)
            {
                Description = selector.FunctionDefinition.Description;
                Payload = result.Payload;
                m_ContextType = result.ContextType;
                m_Truncated = result.Truncated;
            }

            public bool Equals(IContextSelection other)
            {
                return Classifier == other.Classifier
                       && Description == other.Description
                       && Payload == other.Payload;
            }
        }
    }
}