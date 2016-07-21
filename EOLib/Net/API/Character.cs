// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using EOLib.Domain.Character;
using EOLib.Localization;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    /// <summary>
    /// Represents data for a single character
    /// </summary>
    public struct CharacterData
    {
        private readonly string m_name;
        public string Name { get { return m_name; } }

        private readonly short m_id, m_map, m_x, m_y;
        public short ID { get { return m_id; } }
        public short Map { get { return m_map; } }
        public short X { get { return m_x; } }
        public short Y { get { return m_y; } }

        private readonly string m_guildTag;
        public string GuildTag { get { return m_guildTag; } }

        private readonly EODirection m_facing;
        public EODirection Direction { get { return m_facing; } }

        private readonly byte m_level, m_gender, m_hairstyle, m_haircolor, m_race;
        public byte Level { get { return m_level; } }
        public byte Gender { get { return m_gender; } }
        public byte HairStyle { get { return m_hairstyle; } }
        public byte HairColor { get { return m_haircolor; } }
        public byte Race { get { return m_race; } }

        private readonly short m_hp, m_maxhp, m_tp, m_maxtp;
        public short MaxHP { get { return m_maxhp; } }
        public short HP { get { return m_hp; } }
        public short MaxTP { get { return m_maxtp; } }
        public short TP { get { return m_tp; } }

        private readonly short m_boots, m_armor, m_hat, m_shield, m_weapon;
        public short Boots { get { return m_boots; } }
        public short Armor { get { return m_armor; } }
        public short Hat { get { return m_hat; } }
        public short Shield { get { return m_shield; } }
        public short Weapon { get { return m_weapon; } }

        private readonly SitState m_sit;
        private readonly bool m_hidden;
        public SitState Sitting { get { return m_sit; } }
        public bool Hidden { get { return m_hidden; } }

        internal CharacterData(OldPacket pkt)
        {
            m_name = pkt.GetBreakString();
            if (m_name.Length > 1)
                m_name = char.ToUpper(m_name[0]) + m_name.Substring(1);

            m_id = pkt.GetShort();
            m_map = pkt.GetShort();
            m_x = pkt.GetShort();
            m_y = pkt.GetShort();

            m_facing = (EODirection)pkt.GetChar();
            pkt.GetChar(); //value is always 6? unknown
            m_guildTag = pkt.GetFixedString(3);

            m_level = pkt.GetChar();
            m_gender = pkt.GetChar();
            m_hairstyle = pkt.GetChar();
            m_haircolor = pkt.GetChar();
            m_race = pkt.GetChar();

            m_maxhp = pkt.GetShort();
            m_hp = pkt.GetShort();
            m_maxtp = pkt.GetShort();
            m_tp = pkt.GetShort();

            m_boots = pkt.GetShort();
            pkt.Skip(3 * sizeof(short)); //other paperdoll data is 0'd out
            m_armor = pkt.GetShort();
            pkt.Skip(sizeof(short));
            m_hat = pkt.GetShort();
            m_shield = pkt.GetShort();
            m_weapon = pkt.GetShort();

            m_sit = (SitState)pkt.GetChar();
            m_hidden = pkt.GetChar() != 0;
        }
    }

    /// <summary>
    /// Represents render data for a single character
    /// </summary>
    public struct CharacterLoginData
    {
        private readonly string name;
        private readonly int id;
        private readonly byte level, gender, hairstyle, haircolor, race;
        private readonly AdminLevel admin;
        private readonly short boots, armor, hat, shield, weapon;

        public string Name { get { return name; } }
        public int ID { get { return id; } }
        public byte Level { get { return level; } }
        public byte Gender { get { return gender; } }
        public byte HairStyle { get { return hairstyle; } }
        public byte HairColor { get { return haircolor; }}
        public byte Race { get { return race; } }
        public AdminLevel AdminLevel { get { return admin; } }
        public short Boots { get { return boots; } }
        public short Armor { get { return armor; } }
        public short Hat { get { return hat; } }
        public short Shield { get { return shield; } }
        public short Weapon { get { return weapon; } }

        internal CharacterLoginData(OldPacket pkt)
        {
            name = pkt.GetBreakString();
            id = pkt.GetInt();
            level = pkt.GetChar();
            gender = pkt.GetChar();
            hairstyle = pkt.GetChar();
            haircolor = pkt.GetChar();
            race = pkt.GetChar();
            admin = (AdminLevel)pkt.GetChar();
            boots = pkt.GetShort();
            armor = pkt.GetShort();
            hat = pkt.GetShort();
            shield = pkt.GetShort();
            weapon = pkt.GetShort();
        }
    }

    partial class PacketAPI
    {
        private AutoResetEvent m_character_responseEvent;
        private CharacterReply m_character_reply;
        private CharacterLoginData[] m_character_data;
        private int m_character_takeID;

        private void _createCharacterMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Character, PacketAction.Player), _handleCharacterPlayer, false);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Character, PacketAction.Reply), _handleCharacterReply, false);

            m_character_responseEvent = new AutoResetEvent(false);
            m_character_reply = CharacterReply.THIS_IS_WRONG;
            m_character_takeID = -1;
            m_character_data = null;
        }

        private void _disposeCharacterMembers()
        {
            if (m_character_responseEvent != null)
            {
                m_character_responseEvent.Dispose();
                m_character_responseEvent = null;
            }
        }

        public bool CharacterRequest(out CharacterReply reply)
        {
            reply = CharacterReply.THIS_IS_WRONG;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Character, PacketAction.Request);

            if (!m_client.SendPacket(builder) || !m_character_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            reply = CharacterReply.Ok;

            return true;
        }

        public bool CharacterCreate(byte gender, byte hairStyle, byte hairColor, byte race, string name, out CharacterReply reply, out CharacterLoginData[] data)
        {
            data = null;
            reply = CharacterReply.THIS_IS_WRONG;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Character, PacketAction.Create);
            builder.AddShort(255);
            builder.AddShort(gender);
            builder.AddShort(hairStyle);
            builder.AddShort(hairColor);
            builder.AddShort(race);
            builder.AddByte(255);
            builder.AddBreakString(name);

            if (!m_client.SendPacket(builder) || !m_character_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            reply = m_character_reply;

            if (reply == CharacterReply.THIS_IS_WRONG || m_character_data == null || m_character_data.Length == 0)
                return false;

            data = m_character_data;

            return true;
        }

        public bool CharacterTake(int id, out int takeID)
        {
            takeID = -1;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Character, PacketAction.Take);
            builder.AddInt(id);

            if (!m_client.SendPacket(builder) || !m_character_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            takeID = m_character_takeID;

            return true;
        }

        public bool CharacterRemove(int id, out CharacterLoginData[] data)
        {
            data = null;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Character, PacketAction.Remove);
            builder.AddShort(255);
            builder.AddInt(id);

            if (!m_client.SendPacket(builder) || !m_character_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            if (m_character_reply != CharacterReply.Deleted || m_character_data == null)
                return false;

            data = m_character_data;

            return true;
        }

        private void _handleCharacterReply(OldPacket pkt)
        {
            m_character_reply = (CharacterReply)pkt.GetShort();

            if (m_character_reply == CharacterReply.Ok || m_character_reply == CharacterReply.Deleted)
            {
                byte numCharacters = pkt.GetChar();
                pkt.GetByte();
                pkt.GetByte();

                m_character_data = new CharacterLoginData[numCharacters];

                for (int i = 0; i < numCharacters; ++i)
                {
                    CharacterLoginData nextData = new CharacterLoginData(pkt);
                    m_character_data[i] = nextData;
                    if (255 != pkt.GetByte())
                        return; //malformed packet - time out and signal error
                }
            }

            m_character_responseEvent.Set();
        }

        //handler function for when server sends CHARACTER_PLAYER (in response to CHARACTER_TAKE)
        private void _handleCharacterPlayer(OldPacket pkt)
        {
            pkt.Skip(2);
            m_character_takeID = pkt.GetInt();
            m_character_responseEvent.Set();
        }

        public DialogResourceID CharacterResponseMessage()
        {
            DialogResourceID message = DialogResourceID.NICE_TRY_HAXOR;
            switch (m_character_reply)
            {
                case CharacterReply.Ok:
                    message = DialogResourceID.CHARACTER_CREATE_SUCCESS;
                    break;
                case CharacterReply.Full:
                    message = DialogResourceID.CHARACTER_CREATE_TOO_MANY_CHARS;
                    break;
                case CharacterReply.Exists:
                    message = DialogResourceID.CHARACTER_CREATE_NAME_EXISTS;
                    break;
                case CharacterReply.NotApproved:
                    message = DialogResourceID.CHARACTER_CREATE_NAME_NOT_APPROVED;
                    break;
            }
            return message;
        }
    }
}
