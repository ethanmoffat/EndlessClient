using System;

namespace EOLib.Net
{
    public class NoDataSentException : Exception
    {
        public NoDataSentException()
            : base("No data was sent to the server.") { }
    }
}