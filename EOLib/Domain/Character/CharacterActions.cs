using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Spells;
using EOLib.IO.Repositories;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class CharacterActions : ICharacterActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IESFFileProvider _spellFileProvider;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public CharacterActions(IPacketSendService packetSendService,
                                ICharacterRepository characterRepository,
                                IESFFileProvider spellFileProvider,
                                IFixedTimeStepRepository fixedTimeStepRepository)
        {
            _packetSendService = packetSendService;
            _characterRepository = characterRepository;
            _spellFileProvider = spellFileProvider;
            _fixedTimeStepRepository = fixedTimeStepRepository;
        }

        public void Face(EODirection direction)
        {
            var packet = new FacePlayerClientPacket { Direction = (Direction)direction };
            _packetSendService.SendPacket(packet);
        }

        public void Walk(bool ghosted)
        {
            var admin = _characterRepository.MainCharacter.NoWall &&
                        _characterRepository.MainCharacter.AdminLevel != AdminLevel.Player;
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            var walkAction = new WalkAction
            {
                Direction = (Direction)renderProperties.Direction,
                Timestamp = (int)_fixedTimeStepRepository.TickCount,
                Coords = new Coords
                {
                    X = renderProperties.GetDestinationX(),
                    Y = renderProperties.GetDestinationY()
                }
            };

            var packet = admin
                ? (IPacket)new WalkAdminClientPacket { WalkAction = walkAction }
                : ghosted ? (IPacket)new WalkSpecClientPacket { WalkAction = walkAction }
                : (IPacket)new WalkPlayerClientPacket { WalkAction = walkAction };
            _packetSendService.SendPacket(packet);
        }

        public void Attack()
        {
            var c = _characterRepository.MainCharacter;
            var sp = Math.Max(0, c.Stats[CharacterStat.SP] - 1);
            _characterRepository.MainCharacter = c.WithStats(c.Stats.WithNewStat(CharacterStat.SP, sp));

            var packet = new AttackUseClientPacket
            {
                Direction = (Direction)_characterRepository.MainCharacter.RenderProperties.Direction,
                Timestamp = (int)_fixedTimeStepRepository.TickCount
            };
            _packetSendService.SendPacket(packet);
        }

        /// <inheritdoc />
        public void Sit(MapCoordinate coords, bool isChair = false)
        {
            if (coords.X < 0 || coords.Y < 0)
                coords = MapCoordinate.Zero;

            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            var sitAction = renderProperties.SitState == SitState.Standing
                ? SitAction.Sit
                : SitAction.Stand;

            IPacket packet = isChair || renderProperties.SitState == SitState.Chair
                ? new ChairRequestClientPacket
                {
                    SitAction = sitAction,
                    SitActionData = sitAction == SitAction.Sit
                        ? new ChairRequestClientPacket.SitActionDataSit
                        {
                            Coords = new Coords { X = coords.X, Y = coords.Y },
                        }
                        : null
                }
                : (IPacket)new SitRequestClientPacket
                {
                    SitAction = sitAction,
                    SitActionData = sitAction == SitAction.Sit
                        ? new SitRequestClientPacket.SitActionDataSit
                        {
                            CursorCoords = new Coords { X = coords.X, Y = coords.Y },
                        }
                        : null
                };
            _packetSendService.SendPacket(packet);
        }

        public void PrepareCastSpell(int spellId)
        {
            var packet = new SpellRequestClientPacket
            {
                SpellId = spellId,
                Timestamp = (int)_fixedTimeStepRepository.TickCount
            };
            _packetSendService.SendPacket(packet);
        }

        public void CastSpell(int spellId, ISpellTargetable target)
        {
            var data = _spellFileProvider.ESFFile.Single(x => x.ID == spellId);

            var c = _characterRepository.MainCharacter;
            var sp = Math.Max(0, c.Stats[CharacterStat.SP] - data.SP);
            _characterRepository.MainCharacter = c.WithStats(c.Stats.WithNewStat(CharacterStat.SP, sp));

            IPacket packet = data.Target switch
            {
                IO.SpellTarget.Self => new SpellTargetSelfClientPacket
                {
                    SpellId = spellId,
                    Direction = (Direction)c.RenderProperties.Direction,
                    Timestamp = (int)_fixedTimeStepRepository.TickCount,
                },
                IO.SpellTarget.Normal => new SpellTargetOtherClientPacket
                {
                    SpellId = spellId,
                    VictimId = target.Index,
                    TargetType = target is NPC.NPC
                        ? SpellTargetType.Npc
                        : target is Character
                            ? SpellTargetType.Player
                            : throw new InvalidOperationException("Unknown SpellTargetType (must be character or NPC)"),
                    // todo: previous time stamp tracking. this was previously sent to eoserv as a char(1) and short (1)
                    PreviousTimestamp = NumberEncoder.DecodeNumber(new byte[] { 2, 2, 254 }),
                    Timestamp = (int)_fixedTimeStepRepository.TickCount,
                },
                IO.SpellTarget.Group => new SpellTargetGroupClientPacket
                {
                    SpellId = spellId,
                    Timestamp = (int)_fixedTimeStepRepository.TickCount,
                },
                _ => throw new ArgumentOutOfRangeException("Unknown spell target (should be Self, Normal, or Group)")
            };
            _packetSendService.SendPacket(packet);
        }

        public void Emote(Emote whichEmote)
        {
            var packet = new EmoteReportClientPacket { Emote = (Moffat.EndlessOnline.SDK.Protocol.Emote)whichEmote };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface ICharacterActions
    {
        void Face(EODirection direction);

        void Walk(bool ghosted);

        void Attack();

        /// <summary>
        /// Request sit action
        /// </summary>
        /// <param name="coord">The chair coordinate for sitting in a chair, the mouse cursor coordinates for floor sit.</param>
        /// <param name="isChair">True if the sit action is for a chair.</param>
        void Sit(MapCoordinate coord, bool isChair = false);

        void PrepareCastSpell(int spellId);

        void CastSpell(int spellId, ISpellTargetable target);

        void Emote(Emote whichEmote);
    }
}