// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Character
{
    public class CharacterStateCache : ICharacterStateCache
    {
        public Optional<ICharacterRenderProperties> MainCharacterRenderProperties { get; private set; }

        private readonly Dictionary<int, ICharacterRenderProperties> _characterRenderProperties;

        public IReadOnlyDictionary<int, ICharacterRenderProperties> CharacterRenderProperties => _characterRenderProperties;

        public CharacterStateCache()
        {
            MainCharacterRenderProperties = new Optional<ICharacterRenderProperties>();
            _characterRenderProperties = new Dictionary<int, ICharacterRenderProperties>();
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