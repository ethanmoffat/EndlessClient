// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterStateCache
    {
        Optional<ICharacterRenderProperties> MainCharacterRenderProperties { get; }

        IReadOnlyDictionary<int, ICharacterRenderProperties> CharacterRenderProperties { get; }

        IReadOnlyList<RenderFrameActionTime> DeathStartTimes { get; }

        bool HasCharacterWithID(int id);

        void UpdateMainCharacterState(ICharacterRenderProperties newMainCharacterState);

        void UpdateCharacterState(int id, ICharacterRenderProperties newCharacterState);

        void RemoveCharacterState(int id);

        void AddDeathStartTime(int id, DateTime startTime);

        void RemoveDeathStartTime(int id);

        void ClearAllOtherCharacterStates();

        void Reset();
    }
}