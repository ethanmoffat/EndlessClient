using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Domain.Notifiers
{
    public interface IEmoteNotifier
    {
        void NotifyEmote(short playerId, Emote emote);
    }

    [AutoMappedType]
    public class NoOpEmoteNotifier : IEmoteNotifier
    {
        public void NotifyEmote(short playerId, Emote emote) { }
    }
}
