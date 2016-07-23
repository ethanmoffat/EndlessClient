// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Config
{
    public class ConfigLoadException : Exception
    {
        public ConfigLoadException()
            : base("Unable to load the configuration file! Make sure there is a file in " + ConfigStrings.Default_Config_File) { }
    }
}