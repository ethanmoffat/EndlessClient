namespace EOLib
{
    public class CRC32
    {
        /// <summary>
        /// This value is used to 'seed' the CRC. It is a polynomial in integer format. It is set to a default when CRC32 is instantiated.
        /// </summary>
        public static uint Magic {get; set;}
        
        //lookup table for the CRC
        private uint[] lookup = new uint[256];

        //flag to indicate whether the lookup table has been generated
        private bool generated = false;

        public CRC32()
        {
            Magic = 0x04c11db7;
            GenerateTable(true);
        }

        public void Regenerate() { GenerateTable(true); }
        private void GenerateTable(bool over)
        {
            if (!over && generated) //'over' flag specifies that it should force regen
                return;

            for (int i = 0; i < 256; ++i)
            {
                uint temp = (uint)i;
                for (int j = 8; j > 0; --j)
                    if ((temp & 1) != 0)
                        temp = (temp >> 1) ^ Magic;
                    else
                        temp >>= 1;
                lookup[i] = temp;
            }
        }

        /// <summary>
        /// Returns the CRC32 for an ASCII-encoded character string.
        /// </summary>
        /// <param name="data">The string data to CRC</param>
        /// <returns>The CRC value as a 32-bit unsigned integer</returns>
        public uint Check(string data) { return Check(System.Text.Encoding.ASCII.GetBytes(data)); }

        /// <summary>
        /// Returns the CRC32 for an extension of bytes.
        /// </summary>
        /// <param name="data">Byte extension to process</param>
        /// <returns>The CRC value as a 32-bit unsigned integer</returns>
        public uint Check(byte[] data) { return Check(data, 0, (uint)data.Length); }

        /// <summary>
        /// Returns the CRC32 for an extension of bytes, starting at <paramref name="offset"/> and going for <paramref name="length"/> bytes.
        /// </summary>
        /// <param name="data">Byte extension to process</param>
        /// <param name="offset">Starting index for processing</param>
        /// <param name="length">Number of elements to process</param>
        /// <returns></returns>
        public uint Check(byte[] data, uint offset, uint length)
        {
            uint crc = 0xFFFFFFFF;
            if (offset > data.Length || length > data.Length || offset + length > data.Length)
                return crc;
            for (uint i = offset; i < offset + length; ++i)
            {
                uint index = crc >> 24, mod = crc << 8;
                index &= 0xFF;
                index ^= data[i];

                crc = lookup[index] ^ mod;
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}
