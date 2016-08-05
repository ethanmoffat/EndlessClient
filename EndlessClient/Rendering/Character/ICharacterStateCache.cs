// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterStateCache
    {
        ICharacterRenderProperties ActiveCharacterRenderProperties { get; }

        IReadOnlyDictionary<int, ICharacterRenderProperties> CharacterRenderProperties { get; }

        bool HasCharacterWithID(int id);

        void UpdateActiveCharacterState(ICharacterRenderProperties newActiveCharacterState);

        void UpdateCharacterState(int id, ICharacterRenderProperties newCharacterState);

        void RemoveCharacterState(int id);

        void ClearAll();
    }
}