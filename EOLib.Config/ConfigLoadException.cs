using System;
using EOLib.Shared;

namespace EOLib.Config
{
    public class ConfigLoadException : Exception
    {
        public ConfigLoadException()
            : base("Unable to load the configuration file! Make sure there is a file in " + Constants.Default_Config_File) { }
    }
}
