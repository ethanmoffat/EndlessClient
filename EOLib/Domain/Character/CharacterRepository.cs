using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterRepository
    {
        bool HasAvatar { get; set; }

        Character MainCharacter { get; set; }
    }

    public interface ICharacterProvider
    {
        bool HasAvatar { get; }

        Character MainCharacter { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterRepository : ICharacterRepository, ICharacterProvider
    {
        public bool HasAvatar { get; set; } = true;

        public Character MainCharacter { get; set; }
    }
}