using EOLib.Domain.Spells;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
    public interface IMapActor
    {
        int NameLabelY { get; }

        int HorizontalCenter { get; }

        bool IsAlive { get; }

        ISpellTargetable SpellTarget { get; }

        Rectangle DrawArea { get; }

        bool MouseOver { get; }

        bool MouseOverPreviously { get; }

        void ShowDamageCounter(int damage, int percentHealth, bool isHeal);

        void ShowChatBubble(string text, bool isGroupChat = false);

        bool EffectIsPlaying();

        void PlayEffect(int effectId);
    }
}
