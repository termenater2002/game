using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class TextToMotionRequest : BakingRequest<TextToMotionTake>
    {
        ITimelineBakerTextToMotion Baker { get; }
        
        public TextToMotionRequest(BakingLogic bakingLogic, ITimelineBakerTextToMotion timelineBaker, TextToMotionTake target)
            : base(bakingLogic)
        {
            Baker = timelineBaker;
            Target = target;
        }

        protected override void InitializeParameters()
        {
            Baker.Prompt = Target.Prompt;
            Baker.Seed = Target.RequestedSeed;
            Baker.Temperature = Target.RequestTemperature;
            Baker.Length = Target.Length;
            Baker.ModelType = Target.Model;
        }

        protected override void FinalizeBaking(BakedTimelineModel output)
        {
            // Depending on the API version, we may or may not receive seed and temperature values from the server.
            Target.Seed = Baker.Seed ?? Target.Seed;
            Target.Temperature = Baker.Temperature ?? Target.Temperature;
        }
    }
}
