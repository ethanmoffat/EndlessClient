using Amadevus.RecordGenerator;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Online
{
    [Record(Features.ObjectEquals | Features.Constructor)]
    public sealed partial class OnlineListData : ITranslatedData
    {
        public IReadOnlyList<OnlinePlayerInfo> OnlineList { get; }
    }
}
