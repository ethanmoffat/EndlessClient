using System.IO;

namespace EOLib.IO.Map
{
    public class MapPathToIDConverter
    {
        public int ConvertFromPathToID(string pathToMapFile)
        {
            var lastDot = pathToMapFile.LastIndexOf('.');
            if (lastDot < 5)
                throw new IOException();

            var strID = pathToMapFile.Substring(lastDot - 5, 5);
            return int.Parse(strID);
        }
    }
}