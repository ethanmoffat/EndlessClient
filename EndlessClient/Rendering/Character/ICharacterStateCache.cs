using System.Collections.Generic;
using Optional;
using DomainCharacter = EOLib.Domain.Character.Character;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterStateCache
    {
        Option<DomainCharacter> MainCharacter { get; }

        IReadOnlyDictionary<int, DomainCharacter> OtherCharacters { get; }

        IReadOnlyList<RenderFrameActionTime> DeathStartTimes { get; }

        bool HasCharacterWithID(int id);

        void UpdateMainCharacterState(DomainCharacter updatedCharacter);

        void UpdateCharacterState(int id, DomainCharacter updatedCharacter);

        void RemoveCharacterState(int id);

        void AddDeathStartTime(int id);

        void RemoveDeathStartTime(int id);

        void ClearAllOtherCharacterStates();

        void Reset();
    }
}