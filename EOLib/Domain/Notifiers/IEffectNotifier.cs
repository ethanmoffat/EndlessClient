using AutomaticTypeMapper;
using EOLib.Domain.Map;

namespace EOLib.Domain.Notifiers
{
    public interface IEffectNotifier
    {
        void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim);
        void NotifyWarpEnterEffect(short characterId, WarpAnimation anim);
        void NotifyPotionEffect(short playerId, int effectId);
    }

    [AutoMappedType]
    public class NoOpEffectNotifier : IEffectNotifier
    {
        public void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim) { }
        public void NotifyWarpEnterEffect(short characterId, WarpAnimation anim) { }
        public void NotifyPotionEffect(short playerId, int effectId) { }
    }
}
