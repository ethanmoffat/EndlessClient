using System.Collections.Generic;

namespace EOLib.Domain.Interact.Guild
{
    public class GuildCreationSession
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Members { get; set; }
        public bool Approved { get; set; }

        public GuildCreationSession(string tag, string name, string description)
        {
            Tag = tag;
            Name = name;
            Description = description;
            Members = new List<string>();
            Approved = false;
        }
    }
}
