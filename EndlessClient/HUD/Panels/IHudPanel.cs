using Microsoft.Xna.Framework;

namespace EndlessClient.HUD.Panels
{
    public interface IHudPanel : IGameComponent
    {
        bool Visible { get; set; }
    }
}
