using AutomaticTypeMapper;

namespace EOLib.Domain.Character
{
    public interface ICharacterRepository
    {
        ICharacter MainCharacter { get; set; }
    }

    public interface ICharacterProvider
    {
        ICharacter MainCharacter { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class CharacterRepository : ICharacterRepository, ICharacterProvider
    {
        public ICharacter MainCharacter { get; set; }
    }
}
