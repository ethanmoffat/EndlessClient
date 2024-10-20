using System;
using System.Collections.Generic;
using Amadevus.RecordGenerator;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Interact.Guild
{
    [Record(Features.Default | Features.ObjectEquals | Features.EquatableEquals)]
    public sealed partial class GuildInfo
    {
        public string Name { get; }

        public string Tag { get; }

        public DateTime CreateDate { get; }

        public string Description { get; }

        public string Wealth { get; }

        public IReadOnlyList<string> Ranks { get; }

        public IReadOnlyList<GuildStaff> Staff { get; }

        public static GuildInfo FromPacket(GuildReportServerPacket packet)
        {
            return new Builder
            {
                Name = packet.Name,
                Tag = packet.Tag,
                CreateDate = DateTime.TryParse(packet.CreateDate, out var created) ? created : new DateTime(0, DateTimeKind.Utc),
                Description = packet.Description,
                Wealth = packet.Wealth,
                Ranks = packet.Ranks,
                Staff = packet.Staff
            }.ToImmutable();
        }

    }
}
