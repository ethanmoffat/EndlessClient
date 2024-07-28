using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Skill;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when failing to learn a skill from skillmaster
    /// </summary>
    [AutoMappedType]
    public class StatskillReplyHandler : InGameOnlyPacketHandler<StatSkillReplyServerPacket>
    {
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Reply;

        public StatskillReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                     IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(StatSkillReplyServerPacket packet)
        {
            var skillmasterReply = (SkillmasterReply)packet.ReplyCode;
            var classId = skillmasterReply == SkillmasterReply.ErrorWrongClass
                ? ((StatSkillReplyServerPacket.ReplyCodeDataWrongClass)packet.ReplyCodeData).ClassId
                : 0;

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifySkillLearnFail(skillmasterReply, classId);

            return true;
        }
    }
}