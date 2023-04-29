using AutomaticTypeMapper;

namespace EOLib.Net.FileTransfer
{
    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    public interface ILoginFileChecksumRepository
    {
        int EIFChecksum { get; set; }

        int ENFChecksum { get; set; }

        int ESFChecksum { get; set; }

        int ECFChecksum { get; set; }

        int EIFLength { get; set; }

        int ENFLength { get; set; }

        int ESFLength { get; set; }

        int ECFLength { get; set; }

        byte[] MapChecksum { get; set; }

        int MapLength { get; set; }
    }

    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    public interface ILoginFileChecksumProvider
    {
        int EIFChecksum { get; }

        int ENFChecksum { get; }

        int ESFChecksum { get; }

        int ECFChecksum { get; }

        int EIFLength { get; }

        int ENFLength { get; }

        int ESFLength { get; }

        int ECFLength { get; }

        byte[] MapChecksum { get; }

        int MapLength { get; }
    }

    /// <summary>
    /// Contains file checksums from the Welcome Request Granted packet
    /// </summary>
    [AutoMappedType(IsSingleton = true)]
    public class LoginFileChecksumRepository : ILoginFileChecksumRepository, ILoginFileChecksumProvider
    {
        public int EIFChecksum { get; set; }

        public int ENFChecksum { get; set; }

        public int ESFChecksum { get; set; }

        public int ECFChecksum { get; set; }

        public int EIFLength { get; set; }

        public int ENFLength { get; set; }

        public int ESFLength { get; set; }

        public int ECFLength { get; set; }

        public byte[] MapChecksum { get; set; }

        public int MapLength { get; set; }
    }
}
