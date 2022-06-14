using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.Domain.Spells;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class CharacterActions : ICharacterActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IESFFileProvider _spellFileProvider;

        public CharacterActions(IPacketSendService packetSendService,
                                ICharacterRepository characterRepository,
                                IESFFileProvider spellFileProvider)
        {
            _packetSendService = packetSendService;
            _characterRepository = characterRepository;
            _spellFileProvider = spellFileProvider;
        }

        public void Face(EODirection direction)
        {
            var packet = new PacketBuilder(PacketFamily.Face, PacketAction.Player)
                .AddChar((byte) direction)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Walk()
        {
            var admin = _characterRepository.MainCharacter.NoWall &&
                        _characterRepository.MainCharacter.AdminLevel != AdminLevel.Player;
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;

            var packet = new PacketBuilder(PacketFamily.Walk, admin ? PacketAction.Admin : PacketAction.Player)
                .AddChar((byte) renderProperties.Direction)
                .AddThree(DateTime.Now.ToEOTimeStamp())
                .AddChar((byte)renderProperties.GetDestinationX())
                .AddChar((byte)renderProperties.GetDestinationY())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Attack()
        {
            var c = _characterRepository.MainCharacter;
            var sp = Math.Max(0, c.Stats[CharacterStat.SP] - 1);
            _characterRepository.MainCharacter = c.WithStats(c.Stats.WithNewStat(CharacterStat.SP, sp));

            var packet = new PacketBuilder(PacketFamily.Attack, PacketAction.Use)
                .AddChar((byte) _characterRepository.MainCharacter.RenderProperties.Direction)
                .AddThree(DateTime.Now.ToEOTimeStamp())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void ToggleSit()
        {
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            var sitAction = renderProperties.SitState == SitState.Standing
                ? SitAction.Sit
                : SitAction.Stand;

            var packetFamily = renderProperties.SitState == SitState.Chair
                ? PacketFamily.Chair
                : PacketFamily.Sit;

            var packet = new PacketBuilder(packetFamily, PacketAction.Request)
                .AddChar((byte)sitAction)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void SitInChair()
        {
            var rp = _characterRepository.MainCharacter.RenderProperties;
            var action = rp.SitState == SitState.Chair ? SitAction.Stand : SitAction.Sit;
            var packet = new PacketBuilder(PacketFamily.Chair, PacketAction.Request)
                .AddChar((byte)action)
                .AddChar((byte)rp.GetDestinationX())
                .AddChar((byte)rp.GetDestinationY())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void PrepareCastSpell(int spellId)
        {
            var packet = new PacketBuilder(PacketFamily.Spell, PacketAction.Request)
                .AddShort((short)spellId)
                .AddThree(DateTime.Now.ToEOTimeStamp())
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void CastSpell(int spellId, ISpellTargetable target)
        {
            var data = _spellFileProvider.ESFFile.Single(x => x.ID == spellId);

            var c = _characterRepository.MainCharacter;
            var sp = Math.Max(0, c.Stats[CharacterStat.SP] - data.SP);
            _characterRepository.MainCharacter = c.WithStats(c.Stats.WithNewStat(CharacterStat.SP, sp));

            var action = data.Target == IO.SpellTarget.Self
                ? PacketAction.TargetSelf
                : data.Target == IO.SpellTarget.Normal
                    ? PacketAction.TargetOther
                    : data.Target == IO.SpellTarget.Group
                        ? PacketAction.TargetGroup
                        : throw new InvalidOperationException("Spell ID has unknown spell target");

            IPacketBuilder builder = new PacketBuilder(PacketFamily.Spell, action);

            if (data.Target == IO.SpellTarget.Group)
            {
                builder = builder
                    .AddShort((short)spellId)
                    .AddThree(DateTime.Now.ToEOTimeStamp());
            }
            else
            {
                var spellTargetType = target is NPC.NPC
                    ? SpellTargetType.NPC
                    : target is Character
                        ? SpellTargetType.Player
                        : throw new InvalidOperationException("Invalid spell target specified, must be player or character");
                builder = builder.AddChar((byte)spellTargetType);

                if (data.Target == IO.SpellTarget.Normal)
                {
                    builder = builder
                        .AddChar(1) // unknown
                        .AddShort(1) // unknown
                        .AddShort((short)spellId)
                        .AddShort((short)target.Index)
                        .AddThree(DateTime.Now.ToEOTimeStamp());
                }
                else
                {
                    builder = builder
                        .AddShort((short)spellId)
                        .AddInt(DateTime.Now.ToEOTimeStamp());
                }
            }

            _packetSendService.SendPacket(builder.Build());
        }

        public void Emote(Emote whichEmote)
        {
            var packet = new PacketBuilder(PacketFamily.Emote, PacketAction.Report)
                .AddChar((byte)whichEmote)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface ICharacterActions
    {
        void Face(EODirection direction);

        void Walk();

        void Attack();

        void ToggleSit();

        void SitInChair();

        void PrepareCastSpell(int spellId);

        void CastSpell(int spellId, ISpellTargetable target);

        void Emote(Emote whichEmote);
    }
}
