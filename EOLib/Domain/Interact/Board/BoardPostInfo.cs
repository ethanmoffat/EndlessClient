using Amadevus.RecordGenerator;

namespace EOLib.Domain.Interact.Board
{
    [Record(Features.Default | Features.ObjectEquals | Features.EquatableEquals)]
    public sealed partial class BoardPostInfo
    {
        public int PostId { get; }

        public string Author { get; }

        public string Subject { get; }
    }
}