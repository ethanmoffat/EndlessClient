using System.Diagnostics;

namespace EndlessClient.Rendering
{
    public class RenderFrameActionTime
    {
        public int UniqueID { get; private set; }

        public Stopwatch ActionTimer { get; private set; }

        public bool Replay { get; set; }

        public RenderFrameActionTime(int uniqueID)
        {
            UniqueID = uniqueID;
            UpdateActionStartTime();
        }

        public void UpdateActionStartTime()
        {
            ActionTimer = Stopwatch.StartNew();
        }
    }
}