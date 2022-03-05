using System.Collections.Generic;
using EOLib;
using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterStateCache
    {
        Optional<ICharacter> MainCharacter { get; }

        IReadOnlyDictionary<int, ICharacter> OtherCharacters { get; }

        IReadOnlyList<RenderFrameActionTime> DeathStartTimes { get; }

        bool HasCharacterWithID(int id);

        void UpdateMainCharacterState(ICharacter updatedCharacter);

        void UpdateCharacterState(int id, ICharacter updatedCharacter);

        void RemoveCharacterState(int id);

        void AddDeathStartTime(int id);

        void RemoveDeathStartTime(int id);

        void ClearAllOtherCharacterStates();

        void Reset();
    }
}