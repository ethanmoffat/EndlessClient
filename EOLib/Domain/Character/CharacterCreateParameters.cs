namespace EOLib.Domain.Character
{
    public class CharacterCreateParameters : ICharacterCreateParameters
    {
        public string Name { get; }

        public int Gender { get; }

        public int HairStyle { get; }

        public int HairColor { get; }

        public int Race { get; }

        public CharacterCreateParameters(string name, int gender, int hairStyle, int hairColor, int race)
        {
            Name = name;
            Gender = gender;
            HairStyle = hairStyle;
            HairColor = hairColor;
            Race = race;
        }
    }

    public interface ICharacterCreateParameters
    {
        string Name { get; }

        int Gender { get; }
        int HairStyle { get; }
        int HairColor { get; }
        int Race { get; }
    }
}
