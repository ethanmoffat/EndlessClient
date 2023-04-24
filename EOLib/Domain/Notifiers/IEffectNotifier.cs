using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.IO.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IEffectNotifier
    {
        void NotifyWarpLeaveEffect(int characterId, WarpAnimation anim);

        void NotifyWarpEnterEffect(int characterId, WarpAnimation anim);

        void NotifyPotionEffect(int playerId, int effectId);

        void NotifyMapEffect(MapEffect effect, int strength = 0);

        void NotifyEffectAtLocation(MapCoordinate location, int effectId);
    }

    [AutoMappedType]
    public class NoOpEffectNotifier : IEffectNotifier
    {
        public void NotifyWarpLeaveEffect(int characterId, WarpAnimation anim) { }

        public void NotifyWarpEnterEffect(int characterId, WarpAnimation anim) { }

        public void NotifyPotionEffect(int playerId, int effectId) { }

        public void NotifyMapEffect(MapEffect effect, int strength = 0) { }

        public void NotifyEffectAtLocation(MapCoordinate location, int effectId) { }
    }
}
