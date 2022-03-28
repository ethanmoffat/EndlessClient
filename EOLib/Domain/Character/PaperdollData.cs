﻿using EOLib.Domain.Online;
using EOLib.IO;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Character
{
    public class PaperdollData : IPaperdollData
    {
        public string Name { get; private set; }

        public string Home { get; private set; }

        public string Partner { get; private set; }

        public string Title { get; private set; }

        public string Guild { get; private set; }

        public string Rank { get; private set; }

        public short PlayerID { get; private set; }

        public byte Class { get; private set; }

        public byte Gender { get; private set; }

        public IReadOnlyDictionary<EquipLocation, short> Paperdoll { get; private set; }

        public OnlineIcon Icon { get; private set; }

        public PaperdollData()
        {
            Paperdoll = new Dictionary<EquipLocation, short>();
        }

        private PaperdollData(string name,
            string home,
            string partner,
            string title,
            string guild,
            string rank,
            short playerID,
            byte @class,
            byte gender,
            IReadOnlyDictionary<EquipLocation, short> paperdoll,
            OnlineIcon icon)
        {
            Name = name;
            Home = home;
            Partner = partner;
            Title = title;
            Guild = guild;
            Rank = rank;
            PlayerID = playerID;
            Class = @class;
            Gender = gender;
            Paperdoll = paperdoll;
            Icon = icon;
        }

        public IPaperdollData WithName(string name)
        {
            return new PaperdollData(name, Home, Partner, Title, Guild, Rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }
        
        public IPaperdollData WithHome(string home)
        {
            return new PaperdollData(Name, home, Partner, Title, Guild, Rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithPartner(string partner)
        {
            return new PaperdollData(Name, Home, partner, Title, Guild, Rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithTitle(string title)
        {
            return new PaperdollData(Name, Home, Partner, title, Guild, Rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithGuild(string guild)
        {
            return new PaperdollData(Name, Home, Partner, Title, guild, Rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithRank(string rank)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, rank, PlayerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithPlayerID(short playerID)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, Rank, playerID, Class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithClass(byte @class)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, Rank, PlayerID, @class, Gender, Paperdoll, Icon);
        }

        public IPaperdollData WithGender(byte gender)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, Rank, PlayerID, Class, gender, Paperdoll, Icon);
        }

        public IPaperdollData WithPaperdoll(IReadOnlyDictionary<EquipLocation, short> paperdoll)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, Rank, PlayerID, Class, Gender, paperdoll, Icon);
        }

        public IPaperdollData WithIcon(OnlineIcon icon)
        {
            return new PaperdollData(Name, Home, Partner, Title, Guild, Rank, PlayerID, Class, Gender, Paperdoll, icon);
        }

        public override bool Equals(object obj)
        {
            var other = obj as PaperdollData;
            if (other == null)
                return false;

            return Name == other.Name && Home == other.Home && Partner == other.Partner && Title == other.Title &&
                Guild == other.Guild && Rank == other.Rank && PlayerID == other.PlayerID &&
                Class == other.Class && Gender == other.Gender && Icon == other.Icon && Paperdoll.SequenceEqual(other.Paperdoll);
        }
    }

    public interface IPaperdollData
    {
        string Name { get; }
        string Home { get; }
        string Partner { get; }
        string Title { get; }
        string Guild { get; }
        string Rank { get; }

        short PlayerID { get; }
        byte Class { get; }
        byte Gender { get; }

        IReadOnlyDictionary<EquipLocation, short> Paperdoll { get; }

        OnlineIcon Icon { get; }

        IPaperdollData WithName(string name);

        IPaperdollData WithHome(string home);

        IPaperdollData WithPartner(string partner);

        IPaperdollData WithTitle(string title);

        IPaperdollData WithGuild(string guild);

        IPaperdollData WithRank(string rank);

        IPaperdollData WithPlayerID(short playerID);

        IPaperdollData WithClass(byte @class);

        IPaperdollData WithGender(byte gender);

        IPaperdollData WithPaperdoll(IReadOnlyDictionary<EquipLocation, short> paperdoll);

        IPaperdollData WithIcon(OnlineIcon icon);
    }
}