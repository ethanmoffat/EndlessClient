using EOLib.IO.Pub;
using System;

namespace EOLib.IO.Services.Serializers
{
    public interface IPubRecordSerializer
    {
        IPubRecord DeserializeFromByteArray(byte[] data, Func<IPubRecord> recordFactory);

        byte[] SerializeToByteArray(IPubRecord record);
    }
}
