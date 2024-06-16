using AutomaticTypeMapper;
using EOLib.Domain.Map;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Notifiers
{
    public interface IEffectNotifier
    {
        void NotifyWarpLeaveEffect(int characterId, WarpEffect anim);

        void NotifyWarpEnterEffect(int characterId, WarpEffect anim);

        void NotifyPotionEffect(int playerId, int effectId);

        void NotifyMapEffect(IO.Map.MapEffect effect, int strength = 0);

        void NotifyEffectAtLocation(MapCoordinate location, int effectId);

        void NotifyAdminHideEffect(int playerId);
    }

    [AutoMappedType]
    public class NoOpEffectNotifier : IEffectNotifier
    {
        public void NotifyWarpLeaveEffect(int characterId, WarpEffect anim) { }

        public void NotifyWarpEnterEffect(int characterId, WarpEffect anim) { }

        public void NotifyPotionEffect(int playerId, int effectId) { }

        public void NotifyMapEffect(IO.Map.MapEffect effect, int strength = 0) { }

        public void NotifyEffectAtLocation(MapCoordinate location, int effectId) { }

        public void NotifyAdminHideEffect(int playerId) { }
    }
}
