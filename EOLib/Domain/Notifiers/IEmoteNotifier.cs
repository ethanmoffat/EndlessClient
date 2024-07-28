using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Domain.Notifiers
{
    public interface IEmoteNotifier
    {
        void NotifyEmote(int playerId, Emote emote);

        void MakeMainPlayerDrunk();
    }

    [AutoMappedType]
    public class NoOpEmoteNotifier : IEmoteNotifier
    {
        public void NotifyEmote(int playerId, Emote emote) { }

        public void MakeMainPlayerDrunk() { }
    }
}