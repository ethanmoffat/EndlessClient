using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using EOLib.IO.Pub;
using EOLib.IO.Services;

namespace EOLib.IO.Test.Pub
{
    [ExcludeFromCodeCoverage]
    internal class DummyFile : BasePubFile<DummyRecord>
    {
        public override string FileType => "   ";

        public override void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var num = ms.ReadByte();

                for (int i = 0; i < num; ++i)
                {
                    var nameLen = ms.ReadByte();
                    var rawName = new byte[nameLen];
                    ms.Read(rawName, 0, nameLen);

                    _data.Add(new DummyRecord {ID = i + 1, Name = Encoding.ASCII.GetString(rawName)});
                }
            }
        }
    }
}
