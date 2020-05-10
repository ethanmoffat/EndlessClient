using System;

namespace EOLib.Logger
{
    public interface ILogger : IDisposable
    {
        void Log(string format, params object[] parameters);
    }
}