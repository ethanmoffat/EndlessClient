using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Notifiers
{
    public interface IMapChangedNotifier
    {
        void NotifyMapChanged(WarpEffect warpEffect, bool differentMapID);

        void NotifyMapMutation();
    }

    [AutoMappedType]
    public class NoOpMapChangedNotifier : IMapChangedNotifier
    {
        public void NotifyMapChanged(WarpEffect warpEffect, bool differentMapID) { }

        public void NotifyMapMutation() { }
    }
}
