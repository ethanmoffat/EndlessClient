using AutomaticTypeMapper;
using EndlessClient.HUD;
using EndlessClient.Rendering.Character;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Localization;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(ITabKeyController))]
    public class TabKeyController : ITabKeyController
    {
        private readonly IHudStateActions _hudStateActions;

        public TabKeyController(IHudStateActions hudStateActions)
        {
            _hudStateActions = hudStateActions;
        }

        public void ToggleMapView()
        {
            _hudStateActions.ToggleMapView();
        }
    }

    public interface ITabKeyController
    {
        void ToggleMapView();
    }
}
