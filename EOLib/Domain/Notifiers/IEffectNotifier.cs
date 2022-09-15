using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.IO.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IEffectNotifier
    {
        void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim);

        void NotifyWarpEnterEffect(short characterId, WarpAnimation anim);

        void NotifyPotionEffect(short playerId, int effectId);

        void NotifyMapEffect(MapEffect effect, byte strength = 0);

        void NotifyEffectAtLocation(byte x, byte y, short effectId);
    }

    [AutoMappedType]
    public class NoOpEffectNotifier : IEffectNotifier
    {
        public void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim) { }

        public void NotifyWarpEnterEffect(short characterId, WarpAnimation anim) { }

        public void NotifyPotionEffect(short playerId, int effectId) { }

        public void NotifyMapEffect(MapEffect effect, byte strength = 0) { }

        public void NotifyEffectAtLocation(byte x, byte y, short effectId) { }
    }
}
