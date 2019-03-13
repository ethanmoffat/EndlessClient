// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

        short EIFLength { get; set; }

        short ENFLength { get; set; }

        short ESFLength { get; set; }

        short ECFLength { get; set; }

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

        short EIFLength { get; }

        short ENFLength { get; }

        short ESFLength { get; }

        short ECFLength { get; }

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

        public short EIFLength { get; set; }

        public short ENFLength { get; set; }

        public short ESFLength { get; set; }

        public short ECFLength { get; set; }

        public byte[] MapChecksum { get; set; }

        public int MapLength { get; set; }
    }
}
