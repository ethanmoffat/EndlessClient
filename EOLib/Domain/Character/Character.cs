using System.Collections.Generic;

namespace EOLib.Domain.Character
{
    public class Character : ICharacter
    {
        public int ID { get; private set; }

        public int Index => ID;

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string GuildName { get; private set; }

        public string GuildRank { get; private set; }

        public string GuildTag { get; private set; }

        public byte ClassID { get; private set; }

        public AdminLevel AdminLevel { get; private set; }

        public CharacterRenderProperties RenderProperties { get; private set; }

        public ICharacterStats Stats { get; private set; }

        public int MapID { get; private set; }

        public bool NoWall { get; private set; }

        public Character()
        {
            RenderProperties = new CharacterRenderProperties.Builder().ToImmutable();
            Stats = new CharacterStats();
        }

        public ICharacter WithID(int id)
        {
            var character = MakeCopy(this);
            character.ID = id;
            return character;
        }

        public ICharacter WithName(string name)
        {
            var character = MakeCopy(this);
            character.Name = name;
            return character;
        }

        public ICharacter WithTitle(string title)
        {
            var character = MakeCopy(this);
            character.Title = title;
            return character;
        }

        public ICharacter WithGuildName(string guildName)
        {
            var character = MakeCopy(this);
            character.GuildName = guildName;
            return character;
        }

        public ICharacter WithGuildRank(string guildRank)
        {
            var character = MakeCopy(this);
            character.GuildRank = guildRank;
            return character;
        }

        public ICharacter WithGuildTag(string guildTag)
        {
            var character = MakeCopy(this);
            character.GuildTag = guildTag;
            return character;
        }

        public ICharacter WithClassID(byte newClassID)
        {
            var character = MakeCopy(this);
            character.ClassID = newClassID;
            return character;
        }

        public ICharacter WithAdminLevel(AdminLevel level)
        {
            var character = MakeCopy(this);
            character.AdminLevel = level;
            return character;
        }

        public ICharacter WithRenderProperties(CharacterRenderProperties renderProperties)
        {
            var character = MakeCopy(this);
            character.RenderProperties = renderProperties;
            return character;
        }

        public ICharacter WithStats(ICharacterStats stats)
        {
            var character = MakeCopy(this);
            character.Stats = stats;
            return character;
        }

        public ICharacter WithMapID(int mapID)
        {
            var character = MakeCopy(this);
            character.MapID = mapID;
            return character;
        }

        public ICharacter WithNoWall(bool noWall)
        {
            var character = MakeCopy(this);
            character.NoWall = noWall;
            return character;
        }

        private static Character MakeCopy(ICharacter source)
        {
            return new Character
            {
                ID = source.ID,
                Name = source.Name,
                Title = source.Title,
                GuildName = source.GuildName,
                GuildRank = source.GuildRank,
                GuildTag = source.GuildTag,
                ClassID = source.ClassID,
                AdminLevel = source.AdminLevel,
                RenderProperties = source.RenderProperties,
                Stats = source.Stats,
                MapID = source.MapID,
                NoWall = source.NoWall
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Character character &&
                   ID == character.ID &&
                   Name == character.Name &&
                   Title == character.Title &&
                   GuildName == character.GuildName &&
                   GuildRank == character.GuildRank &&
                   GuildTag == character.GuildTag &&
                   ClassID == character.ClassID &&
                   AdminLevel == character.AdminLevel &&
                   RenderProperties.Equals(character.RenderProperties) &&
                   Stats.Equals(character.Stats) &&
                   MapID == character.MapID &&
                   NoWall == character.NoWall;
        }

        public override int GetHashCode()
        {
            int hashCode = 170256730;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GuildName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GuildRank);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GuildTag);
            hashCode = hashCode * -1521134295 + ClassID.GetHashCode();
            hashCode = hashCode * -1521134295 + AdminLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<CharacterRenderProperties>.Default.GetHashCode(RenderProperties);
            hashCode = hashCode * -1521134295 + EqualityComparer<ICharacterStats>.Default.GetHashCode(Stats);
            hashCode = hashCode * -1521134295 + MapID.GetHashCode();
            hashCode = hashCode * -1521134295 + NoWall.GetHashCode();
            return hashCode;
        }
    }
}