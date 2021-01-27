using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class FunctionKeyController : IFunctionKeyController
    {
        private readonly IMapActions _mapActions;

        public FunctionKeyController(IMapActions mapActions)
        {
            _mapActions = mapActions;
        }

        public bool RefreshMapState()
        {
            _mapActions.RequestRefresh();
            return true;
        }
    }

    public interface IFunctionKeyController
    {
        bool RefreshMapState();
    }
}
