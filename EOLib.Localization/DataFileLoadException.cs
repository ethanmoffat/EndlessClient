using System;

namespace EOLib.Localization
{
    public class DataFileLoadException : Exception
    {
        public const string ExceptionMessage = "Unable to find data files! Check that the data directory exists and has ALL the edf files copied over";

        public DataFileLoadException()
            : base(ExceptionMessage) { }
    }
}
