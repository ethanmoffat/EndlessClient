using System;

namespace EOLib.Config
{
    public class MalformedConfigException : Exception
    {
        public MalformedConfigException(string message) : base(message)
        {
        }

        public MalformedConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}