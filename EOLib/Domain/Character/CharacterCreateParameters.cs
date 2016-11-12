// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
    public class CharacterCreateParameters : ICharacterCreateParameters
    {
        public string Name { get; private set; }

        public int Gender { get; private set; }

        public int HairStyle { get; private set; }

        public int HairColor { get; private set; }

        public int Race { get; private set; }

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