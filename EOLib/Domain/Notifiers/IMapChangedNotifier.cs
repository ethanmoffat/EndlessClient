using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IMapChangedNotifier
    {
        void NotifyMapChanged(WarpAnimation warpAnimation, bool differentMapID);
    }

    [AutoMappedType]
    public class NoOpMapChangedNotifier : IMapChangedNotifier
    {
        public void NotifyMapChanged(WarpAnimation warpAnimation, bool differentMapID) { }
    }
}
