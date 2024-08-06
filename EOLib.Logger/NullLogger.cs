namespace EOLib.Logger
{
    public sealed class NullLogger : ILogger
    {
        public void Dispose() { }

        public void Log(string format, params object[] parameters) { }
    }
}
