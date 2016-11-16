// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IMapChangedNotifier
    {
        void NotifyMapChanged(WarpAnimation warpAnimation, bool showMapTransition);
    }

    public class NoOpMapChangedNotifier : IMapChangedNotifier
    {
        public void NotifyMapChanged(WarpAnimation warpAnimation, bool showMapTransition) { }
    }
}
