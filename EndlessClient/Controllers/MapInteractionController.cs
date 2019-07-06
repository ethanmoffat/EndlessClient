// Original Work Copyright (c) Ethan Moffat 2014-2019

using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Extensions;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class MapInteractionController : IMapInteractionController
    {
        private readonly IMapActions _mapActions;

        public MapInteractionController(IMapActions mapActions)
        {
            _mapActions = mapActions;
        }

        public void LeftClick(IMapCellState cellState)
        {
            var item = cellState.Items.OptionalFirst();
            if (item.HasValue)
            {
                _mapActions.PickUpItem(item.Value);
            }
        }

        public void RightClick(IMapCellState cellState)
        {
            if (!cellState.Character.HasValue)
                return;

            //todo: context menu
        }
    }

    public interface IMapInteractionController
    {
        void LeftClick(IMapCellState cellState);

        void RightClick(IMapCellState cellState);
    }
}
