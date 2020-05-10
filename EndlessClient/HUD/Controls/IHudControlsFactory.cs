using System.Collections.Generic;
using EndlessClient.Controllers;
using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.Controls
{
    public interface IHudControlsFactory
    {
        void InjectChatController(IChatController chatController);

        IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud();
    }
}
