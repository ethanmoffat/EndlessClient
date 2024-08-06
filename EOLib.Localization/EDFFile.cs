using System.Collections.Generic;

namespace EOLib.Localization
{
    public class EDFFile : IEDFFile
    {
        private readonly Dictionary<int, string> _data;

        public IReadOnlyDictionary<int, string> Data => _data;

        public DataFiles WhichFile { get; private set; }

        public EDFFile(DataFiles whichFile)
            : this(whichFile, new Dictionary<int, string>()) { }

        public EDFFile(DataFiles whichFile, Dictionary<int, string> data)
        {
            WhichFile = whichFile;
            _data = data;
        }

        public IEDFFile WithDataEntry(int key, string data)
        {
            var copy = MakeCopy(this);
            copy._data[key] = data;
            return copy;
        }

        private EDFFile MakeCopy(EDFFile input)
        {
            return new EDFFile(WhichFile, new Dictionary<int, string>(input._data));
        }
    }

    public interface IEDFFile
    {
        IReadOnlyDictionary<int, string> Data { get; }

        DataFiles WhichFile { get; }

        IEDFFile WithDataEntry(int key, string data);
    }
}
