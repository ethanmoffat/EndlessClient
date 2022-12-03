using AutomaticTypeMapper;
using EOLib.Domain.Interact;
using EOLib.Domain.Interact.Skill;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when opening a skillmaster dialog
    /// </summary>
    [AutoMappedType]
    public class StatskillOpenHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            _skillDataRepository.ID = packet.ReadShort();
            _skillDataRepository.Title = packet.ReadBreakString();
            _skillDataRepository.Skills.Clear();

            while (packet.ReadPosition < packet.Length)
            {
                var skill = new Domain.Interact.Skill.Skill.Builder
                {
                    Id = packet.ReadShort(),
                    LevelRequirement = packet.ReadChar(),
                    ClassRequirement = packet.ReadChar(),
                    GoldRequirement = packet.ReadInt(),
                    SkillRequirements = new List<short>
                    {
                        packet.ReadShort(),
                        packet.ReadShort(),
                        packet.ReadShort(),
                        packet.ReadShort()
                    },
                    StrRequirement = packet.ReadShort(),
                    IntRequirement = packet.ReadShort(),
                    WisRequirement = packet.ReadShort(),
                    AgiRequirement = packet.ReadShort(),
                    ConRequirement = packet.ReadShort(),
                    ChaRequirement = packet.ReadShort()
                }.ToImmutable();

                _skillDataRepository.Skills.Add(skill);
            }

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyInteractionFromNPC(IO.NPCType.Skills);

            return true;
        }
    }
}
