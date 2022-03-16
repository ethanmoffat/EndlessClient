using AutomaticTypeMapper;
using EOLib.IO.Pub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOLib.IO.Services.Serializers
{
    [AutoMappedType]
    public class PubRecordSerializer : IPubRecordSerializer
    {
        private readonly INumberEncoderService _numberEncoderService;

        public PubRecordSerializer(INumberEncoderService numberEncoderService)
        {
            _numberEncoderService = numberEncoderService;
        }

        public IPubRecord DeserializeFromByteArray(byte[] data, Func<IPubRecord> recordFactory)
        {
            var record = recordFactory();

            var nameLengths = new List<byte>();
            for (int i = 0; i < record.NumberOfNames; i++)
                nameLengths.Add((byte)_numberEncoderService.DecodeNumber(data[i]));

            int offset = nameLengths.Count;
            var names = new List<string>(nameLengths.Count);
            foreach (var nameLength in nameLengths)
            {
                names.Add(Encoding.ASCII.GetString(data, offset, nameLength));
                offset += names.Last().Length;
            }

            record = record.WithNames(names);
            foreach (var propertyKvp in record.Bag)
            {
                var property = propertyKvp.Value;
                var propertyRawBytes = data.Skip(offset + property.Offset).Take(property.Length).ToArray();
                record = record.WithProperty(propertyKvp.Key, _numberEncoderService.DecodeNumber(propertyRawBytes));
            }

            return record;
        }

        public byte[] SerializeToByteArray(IPubRecord record)
        {
            var retList = new List<byte>();

            foreach (var name in record.Names)
            {
                var nameLength = _numberEncoderService.EncodeNumber(name.Length, 1)[0];
                retList.Add(nameLength);
            }

            foreach (var name in record.Names)
            {
                var nameBytes = Encoding.ASCII.GetBytes(name);
                retList.AddRange(nameBytes);
            }

            var distinctOffsets = record.Bag
                .GroupBy(x => x.Value.Offset)
                .Select(x => new KeyValuePair<PubRecordProperty, RecordData>(x.First().Key, x.First().Value));

            foreach (var propertyKvp in distinctOffsets)
            {
                var property = propertyKvp.Value;
                var propertyRawBytes = _numberEncoderService.EncodeNumber(property.Value, property.Length);
                Array.Resize(ref propertyRawBytes, property.Length);
                retList.AddRange(propertyRawBytes);
            }

            return retList.ToArray();
        }
    }
}
