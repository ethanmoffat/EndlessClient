// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Map;

namespace EndlessClient.Input
{
    public class WalkErrorHandler : IWalkErrorHandler
    {
        private readonly IMapCellStateProvider _mapCellStateProvider;

        public WalkErrorHandler(IMapCellStateProvider mapCellStateProvider)
        {
            _mapCellStateProvider = mapCellStateProvider;
        }

        public void HandleWalkError()
        {
        }
    }

    public interface IWalkErrorHandler
    {
        void HandleWalkError();
    }
}
