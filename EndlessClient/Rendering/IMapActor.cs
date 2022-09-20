using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
    public interface IMapActor
    {
        int TopPixel { get; }

        int TopPixelWithOffset { get; }

        Rectangle DrawArea { get; }

        Rectangle MapProjectedDrawArea { get; }

        bool MouseOver { get; }

        bool MouseOverPreviously { get; }

        void ShowDamageCounter(int damage, int percentHealth, bool isHeal);

        void ShowChatBubble(string text, bool isGroupChat = false);
    }
}
