using System;

namespace EOLib.Net
{
    public class MalformedPacketException : Exception
    {
        public IPacket Packet { get; private set; }

        public MalformedPacketException(string message, IPacket packet)
            : base(message)
        {
            Packet = packet;
        }
    }
}
