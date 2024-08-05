using System;

namespace EOLib.Net
{
    public class EmptyPacketReceivedException : Exception
    {
        public EmptyPacketReceivedException()
            : base("No data was received from the server") { }
    }
}