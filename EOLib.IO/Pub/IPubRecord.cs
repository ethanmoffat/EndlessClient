using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public interface IPubRecord
    {
        int RecordSize { get; }

        int ID { get; set; }

        string Name { get; set; }

        TValue Get<TValue>(PubRecordProperty type);

        byte[] SerializeToByteArray(INumberEncoderService numberEncoderService);

        void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService);
    }
}