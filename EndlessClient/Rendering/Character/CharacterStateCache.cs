using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib;
using Optional;
using DomainCharacter = EOLib.Domain.Character.Character;

namespace EndlessClient.Rendering.Character
{
    [AutoMappedType(IsSingleton = true)]
    public class CharacterStateCache : ICharacterStateCache
    {
        public Option<DomainCharacter> MainCharacter { get; private set; }

        private readonly Dictionary<int, DomainCharacter> _otherCharacters;
        private readonly List<RenderFrameActionTime> _deathStartTimes;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public IReadOnlyDictionary<int, DomainCharacter> OtherCharacters => _otherCharacters;

        public IReadOnlyList<RenderFrameActionTime> DeathStartTimes => _deathStartTimes;

        public CharacterStateCache(IFixedTimeStepRepository fixedTimeStepRepository)
        {
            MainCharacter = Option.None<DomainCharacter>();
            _otherCharacters = new Dictionary<int, DomainCharacter>();
            _deathStartTimes = new List<RenderFrameActionTime>();
            _fixedTimeStepRepository = fixedTimeStepRepository;
        }

        public bool HasCharacterWithID(int id)
        {
            return _otherCharacters.ContainsKey(id);
        }

        public void UpdateMainCharacterState(DomainCharacter updatedCharacter)
        {
            MainCharacter = Option.Some(updatedCharacter);
        }

        public void UpdateCharacterState(int id, DomainCharacter updatedCharacter)
        {
            _otherCharacters[id] = updatedCharacter;
        }

        public void RemoveCharacterState(int id)
        {
            _otherCharacters.Remove(id);
        }

        public void AddDeathStartTime(int id)
        {
            if (_deathStartTimes.Any(x => x.UniqueID == id))
                throw new ArgumentException("That character already started dying...", nameof(id));

            _deathStartTimes.Add(new RenderFrameActionTime(id, _fixedTimeStepRepository.TickCount));
        }

        public void RemoveDeathStartTime(int id)
        {
            if (_deathStartTimes.All(x => x.UniqueID != id))
                throw new ArgumentException("That character isn't dying...", nameof(id));

            _deathStartTimes.RemoveAll(x => x.UniqueID == id);
        }

        public void ClearAllOtherCharacterStates()
        {
            _otherCharacters.Clear();
        }

        public void Reset()
        {
            MainCharacter = Option.None<DomainCharacter>();
            ClearAllOtherCharacterStates();
        }
    }
}