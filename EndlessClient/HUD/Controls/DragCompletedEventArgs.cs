using EOLib.IO.Pub;

namespace EndlessClient.HUD.Controls
{
    public class DragCompletedEventArgs<TRecord>
    {
        public bool ContinueDrag { get; set; } = false;

        public bool RestoreOriginalSlot { get; set; } = false;

        public TRecord Data { get; }

        public DragCompletedEventArgs(TRecord data) => Data = data;
    }
}
