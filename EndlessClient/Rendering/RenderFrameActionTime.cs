using System;
using EOLib;

namespace EndlessClient.Rendering
{
    public class RenderFrameActionTime
    {
        public int UniqueID { get; private set; }

        public Optional<DateTime> ActionStartTime { get; private set; }

        public RenderFrameActionTime(int uniqueID, Optional<DateTime> actionStartTime)
        {
            UniqueID = uniqueID;
            ActionStartTime = actionStartTime;
        }

        public void UpdateActionStartTime(Optional<DateTime> now)
        {
            ActionStartTime = now;
        }
    }
}