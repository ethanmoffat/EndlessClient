using System;

namespace EOLib.Config
{
    public class ConfigLoadException : Exception
    {
        public ConfigLoadException()
            : base("Unable to load the configuration file! Make sure there is a file in " + ConfigStrings.Default_Config_File) { }
    }
}
