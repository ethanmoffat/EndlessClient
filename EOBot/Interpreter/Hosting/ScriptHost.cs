using EOBot.Interpreter.Mapping;

namespace EOBot.Interpreter.Hosting
{
    /// <summary>
    /// A script session (e.g. one bot): the .NET types its scripts can map and the conversion of values between scripts and .NET.
    /// </summary>
    public class ScriptHost
    {
        public TypeCatalog TypeCatalog { get; }

        public DotNetConverter Converter { get; } = new();

        public ScriptHost(TypeCatalog typeCatalog)
        {
            TypeCatalog = typeCatalog;
        }
    }
}
