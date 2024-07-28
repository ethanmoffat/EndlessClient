using EndlessClient.Controllers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace EndlessClient.HUD.Controls
{
    public interface IHudControlsFactory
    {
        void InjectChatController(IChatController chatController, IMainButtonController mainButtonController);

        IReadOnlyDictionary<HudControlIdentifier, IGameComponent> CreateHud();
    }
}