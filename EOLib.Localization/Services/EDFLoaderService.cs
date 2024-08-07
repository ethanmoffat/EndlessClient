using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Data;

namespace EOLib.Localization
{
    [AutoMappedType]
    public class EDFLoaderService : IEDFLoaderService
    {
        public IEDFFile LoadFile(string fileName, DataFiles whichFile)
        {
            switch (whichFile)
            {
                case DataFiles.CurseFilter:
                    return LoadCurseFilter(fileName);
                case DataFiles.Checksum:
                case DataFiles.Credits:
                    return LoadUnencodedFile(fileName, whichFile);
                default:
                    return LoadEncodedFile(fileName, whichFile);
            }
        }

        public void SaveFile(string fileName, IEDFFile file)
        {
            switch (file.WhichFile)
            {
                case DataFiles.CurseFilter:
                    {
                        var data = string.Join(":", file.Data.Values);
                        var encoded = EncodeDatString(data, DataFiles.CurseFilter);
                        File.WriteAllText(fileName, encoded);
                        break;
                    }
                case DataFiles.Checksum:
                case DataFiles.Credits:
                    {
                        File.WriteAllLines(fileName, file.Data.Values.ToArray());
                        break;
                    }
                default:
                    {
                        var data = file.Data.Values
                            .Select(x => EncodeDatString(x, file.WhichFile))
                            .ToArray();
                        File.WriteAllLines(fileName, data);
                        break;
                    }
            }
        }

        private IEDFFile LoadCurseFilter(string fileName)
        {
            var data = new Dictionary<int, string>();
            var lines = File.ReadAllLines(fileName);

            var i = 0;
            foreach (string encoded in lines)
            {
                string decoded = DecodeDatString(encoded, DataFiles.CurseFilter);
                string[] curses = decoded.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string curse in curses)
                    data.Add(i++, curse);
            }

            return new EDFFile(DataFiles.CurseFilter, data);
        }

        private IEDFFile LoadEncodedFile(string fileName, DataFiles whichFile)
        {
            var data = new Dictionary<int, string>();
            var lines = File.ReadAllLines(fileName, Encoding.Default);

            var i = 0;
            foreach (string encoded in lines)
                data.Add(i++, DecodeDatString(encoded, whichFile));

            return new EDFFile(whichFile, data);
        }

        private IEDFFile LoadUnencodedFile(string fileName, DataFiles whichFile)
        {
            var data = new Dictionary<int, string>();
            var lines = File.ReadAllLines(fileName, Encoding.Default);

            var i = 0;
            foreach (string decoded in lines)
                data.Add(i++, decoded);

            return new EDFFile(whichFile, data);
        }

        private string DecodeDatString(string content, DataFiles whichFile)
        {
            var res = DataEncrypter.Deinterleave(Encoding.ASCII.GetBytes(content));

            if (whichFile != DataFiles.CurseFilter)
                res = DataEncrypter.SwapMultiples(res, 7);

            return Encoding.ASCII.GetString(res.ToArray());
        }

        private string EncodeDatString(string content, DataFiles whichFile)
        {
            byte[] res = null;

            if (whichFile != DataFiles.CurseFilter)
                res = DataEncrypter.SwapMultiples(Encoding.ASCII.GetBytes(content), 7);

            res = DataEncrypter.Interleave(res ?? Encoding.ASCII.GetBytes(content));

            return Encoding.ASCII.GetString(res);
        }
    }
}
