using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterStateCache), IsSingleton = true)]
    public class CharacterStateCache : ICharacterStateCache
    {
        public Optional<ICharacter> MainCharacter { get; private set; }

        private readonly Dictionary<int, ICharacter> _otherCharacters;
        private readonly List<RenderFrameActionTime> _deathStartTimes;

        public IReadOnlyDictionary<int, ICharacter> OtherCharacters => _otherCharacters;

        public IReadOnlyList<RenderFrameActionTime> DeathStartTimes => _deathStartTimes;

        public CharacterStateCache()
        {
            MainCharacter = new Optional<ICharacter>();
            _otherCharacters = new Dictionary<int, ICharacter>();
            _deathStartTimes = new List<RenderFrameActionTime>();
        }

        public bool HasCharacterWithID(int id)
        {
            return _otherCharacters.ContainsKey(id);
        }

        public void UpdateMainCharacterState(ICharacter updatedCharacter)
        {
            MainCharacter = new Optional<ICharacter>(updatedCharacter);
        }

        public void UpdateCharacterState(int id, ICharacter updatedCharacter)
        {
            _otherCharacters[id] = updatedCharacter;
        }

        public void RemoveCharacterState(int id)
        {
            _otherCharacters.Remove(id);
        }

        public void AddDeathStartTime(int id, DateTime startTime)
        {
            if (_deathStartTimes.Any(x => x.UniqueID == id))
                throw new ArgumentException("That character already started dying...", nameof(id));

            _deathStartTimes.Add(new RenderFrameActionTime(id, DateTime.Now));
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
            MainCharacter = Optional<ICharacter>.Empty;
            ClearAllOtherCharacterStates();
        }
    }
}