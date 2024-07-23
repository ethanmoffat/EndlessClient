namespace EOLib.Domain.Interact.Guild
{
    public class GuildCreationRequest
    {
        public int CreatorPlayerID { get; set; }
        public string GuildIdentity { get; set; }

        public GuildCreationRequest(int creatorPlayerID, string guildIdentity)
        {
            CreatorPlayerID = creatorPlayerID;
            GuildIdentity = guildIdentity;
        }
    }
}
