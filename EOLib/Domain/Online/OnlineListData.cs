using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Online
{
    public class OnlineListData : IOnlineListData
    {
        public IReadOnlyList<OnlinePlayerInfo> OnlineList { get; }

        public OnlineListData(IReadOnlyList<OnlinePlayerInfo> onlineList)
        {
            OnlineList = onlineList;
        }
    }

    public interface IOnlineListData : ITranslatedData
    {
        IReadOnlyList<OnlinePlayerInfo> OnlineList { get; }
    }
}
