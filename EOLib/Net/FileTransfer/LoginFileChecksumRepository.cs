using System.Collections.Generic;
using AutomaticTypeMapper;

namespace EOLib.Net.FileTransfer
{
    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    public interface ILoginFileChecksumRepository
    {
        List<int> EIFChecksum { get; set; }

        List<int> ENFChecksum { get; set; }

        List<int> ESFChecksum { get; set; }

        List<int> ECFChecksum { get; set; }

        int EIFLength { get; set; }

        int ENFLength { get; set; }

        int ESFLength { get; set; }

        int ECFLength { get; set; }

        List<int> MapChecksum { get; set; }

        int MapLength { get; set; }
    }

    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    public interface ILoginFileChecksumProvider
    {
        IReadOnlyList<int> EIFChecksum { get; }

        IReadOnlyList<int> ENFChecksum { get; }

        IReadOnlyList<int> ESFChecksum { get; }

        IReadOnlyList<int> ECFChecksum { get; }

        int EIFLength { get; }

        int ENFLength { get; }

        int ESFLength { get; }

        int ECFLength { get; }

        IReadOnlyList<int> MapChecksum { get; }

        int MapLength { get; }
    }

    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    [AutoMappedType(IsSingleton = true)]
    public class LoginFileChecksumRepository : ILoginFileChecksumRepository, ILoginFileChecksumProvider
    {
        public List<int> EIFChecksum { get; set; }

        public List<int> ENFChecksum { get; set; }

        public List<int> ESFChecksum { get; set; }

        public List<int> ECFChecksum { get; set; }

        public int EIFLength { get; set; }

        public int ENFLength { get; set; }

        public int ESFLength { get; set; }

        public int ECFLength { get; set; }

        public List<int> MapChecksum { get; set; }

        public int MapLength { get; set; }

        IReadOnlyList<int> ILoginFileChecksumProvider.EIFChecksum => EIFChecksum;

        IReadOnlyList<int> ILoginFileChecksumProvider.ENFChecksum => ENFChecksum;

        IReadOnlyList<int> ILoginFileChecksumProvider.ESFChecksum => ESFChecksum;

        IReadOnlyList<int> ILoginFileChecksumProvider.ECFChecksum => ECFChecksum;

        IReadOnlyList<int> ILoginFileChecksumProvider.MapChecksum => MapChecksum;
    }
}
