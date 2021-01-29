using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class FunctionKeyController : IFunctionKeyController
    {
        private readonly IMapActions _mapActions;
        private readonly ICharacterActions _characterActions;

        public FunctionKeyController(IMapActions mapActions,
                                     ICharacterActions characterActions)
        {
            _mapActions = mapActions;
            _characterActions = characterActions;
        }

        public bool Sit()
        {
            _characterActions.ToggleSit();
            return true;
        }

        public bool RefreshMapState()
        {
            _mapActions.RequestRefresh();
            return true;
        }
    }

    public interface IFunctionKeyController
    {
        bool Sit();

        bool RefreshMapState();
    }
}
