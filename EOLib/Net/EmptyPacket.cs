using System.Collections.Generic;
using System.Linq;

namespace EOLib.Net
{
    public class EmptyPacket : Packet
    {
        private static readonly IReadOnlyList<byte> _emptyBytes = new List<byte>{ byte.MaxValue, byte.MinValue };

        public EmptyPacket() : base(_emptyBytes.ToList())
        {
        }
    }
}
