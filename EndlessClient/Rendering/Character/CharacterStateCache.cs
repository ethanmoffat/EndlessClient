// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
        public Optional<ICharacterRenderProperties> MainCharacterRenderProperties { get; private set; }

        private readonly Dictionary<int, ICharacterRenderProperties> _characterRenderProperties;
        private readonly List<RenderFrameActionTime> _deathStartTimes;

        public IReadOnlyDictionary<int, ICharacterRenderProperties> CharacterRenderProperties => _characterRenderProperties;

        public IReadOnlyList<RenderFrameActionTime> DeathStartTimes => _deathStartTimes;

        public CharacterStateCache()
        {
            MainCharacterRenderProperties = new Optional<ICharacterRenderProperties>();
            _characterRenderProperties = new Dictionary<int, ICharacterRenderProperties>();
            _deathStartTimes = new List<RenderFrameActionTime>();
        }

        public bool HasCharacterWithID(int id)
        {
            return _characterRenderProperties.ContainsKey(id);
        }

        public void UpdateMainCharacterState(ICharacterRenderProperties newMainCharacterState)
        {
            MainCharacterRenderProperties = new Optional<ICharacterRenderProperties>(newMainCharacterState);
        }

        public void UpdateCharacterState(int id, ICharacterRenderProperties newCharacterState)
        {
            if (!HasCharacterWithID(id))
                _characterRenderProperties.Add(id, null);

            _characterRenderProperties[id] = newCharacterState;
        }

        public void RemoveCharacterState(int id)
        {
            _characterRenderProperties.Remove(id);
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
            _characterRenderProperties.Clear();
        }

        public void Reset()
        {
            MainCharacterRenderProperties = Optional<ICharacterRenderProperties>.Empty;
            ClearAllOtherCharacterStates();
        }
    }
}