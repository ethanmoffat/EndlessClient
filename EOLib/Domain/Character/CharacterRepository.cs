using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterRepository
    {
        Character MainCharacter { get; set; }
    }

    public interface ICharacterProvider
    {
        Character MainCharacter { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterRepository : ICharacterRepository, ICharacterProvider
    {
        public Character MainCharacter { get; set; }
    }
}
