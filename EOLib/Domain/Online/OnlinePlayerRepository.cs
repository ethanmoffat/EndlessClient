using AutomaticTypeMapper;
using System.Collections.Generic;

namespace EOLib.Domain.Online
{
    public interface IOnlinePlayerRepository : IResettable
    {
        HashSet<OnlinePlayerInfo> OnlinePlayers { get; }
    }

    public interface IOnlinePlayerProvider
    {
        IReadOnlyCollection<OnlinePlayerInfo> OnlinePlayers { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class OnlinePlayerRepository : IOnlinePlayerRepository, IOnlinePlayerProvider
    {
        public HashSet<OnlinePlayerInfo> OnlinePlayers { get; private set; }

        IReadOnlyCollection<OnlinePlayerInfo> IOnlinePlayerProvider.OnlinePlayers => OnlinePlayers;

        public OnlinePlayerRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            OnlinePlayers = new HashSet<OnlinePlayerInfo>();
        }
    }
}
