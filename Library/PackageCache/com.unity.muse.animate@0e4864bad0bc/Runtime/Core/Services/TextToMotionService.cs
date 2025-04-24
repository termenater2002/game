using System;
using Unity.Muse.Animate;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Animate
{
    class TextToMotionService : MotionGenerationService<TimelineBakerTextToMotionCloud, TextToMotionTake, TextToMotionRequest>
    {
        public new ITimelineBakerTextToMotion Baker => base.Baker;

        public TextToMotionService()
            : base(new TimelineBakerTextToMotionCloud()) { }
        
        public void Request(TextToMotionTake target)
        {
            Model.SendAnalytics(new TextToMotionAnalytic(target.Prompt, target.Length));

            if (TryGetRequest(target, out var recycledRequest))
            {
                QueueRequest(recycledRequest);
            }

            var newRequest = new TextToMotionRequest(Baking, Baker, target);
            target.TrackRequest(newRequest);

            QueueRequest(newRequest);
        }
    }
}
