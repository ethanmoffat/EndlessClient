using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Skill;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when opening a skillmaster dialog
    /// </summary>
    [AutoMappedType]
    public class StatskillOpenHandler : InGameOnlyPacketHandler<StatSkillOpenServerPacket>
    {
        private readonly ISkillDataRepository _skillDataRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Open;

        public StatskillOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                    ISkillDataRepository skillDataRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _skillDataRepository = skillDataRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(StatSkillOpenServerPacket packet)
        {
            _skillDataRepository.ID = packet.SessionId;
            _skillDataRepository.Title = packet.ShopName;
            _skillDataRepository.Skills = new HashSet<Skill>(packet.Skills.Select(x =>
                new Skill.Builder
                {
                    Id = x.Id,
                    LevelRequirement = x.LevelRequirement,
                    ClassRequirement = x.ClassRequirement,
                    GoldRequirement = x.Cost,
                    SkillRequirements = x.SkillRequirements,
                    StrRequirement = x.StatRequirements.Str,
                    IntRequirement = x.StatRequirements.Intl,
                    WisRequirement = x.StatRequirements.Wis,
                    AgiRequirement = x.StatRequirements.Agi,
                    ConRequirement = x.StatRequirements.Con,
                    ChaRequirement = x.StatRequirements.Cha,
                }.ToImmutable()));

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Skills);

            return true;
        }
    }
}