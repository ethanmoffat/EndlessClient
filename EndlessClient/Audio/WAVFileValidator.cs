using System;
using System.IO;
using System.Text;

namespace EndlessClient.Audio
{
    public static class WAVFileValidator
    {
        //some of the original SFX files will fail to load because the file length is stored incorrectly in the WAV header.
        //this helper method returns a stream around the corrected raw bytes of the file.
        public static Stream GetStreamWithCorrectLengthHeader(string filename)
        {
            var wavBuffer = File.ReadAllBytes(filename);

            var riff = Encoding.ASCII.GetString(wavBuffer[..4]);
            if (riff != "RIFF" || wavBuffer.Length < 8) //check for RIFF tag and length
                throw new ArgumentException("Invalid WAV file", nameof(filename));

            var reportedLength = wavBuffer[4] + wavBuffer[5] * 256 + wavBuffer[6] * 65536 + wavBuffer[7] * 16777216;
            var actualLength = wavBuffer.Length - 8;

            if (reportedLength != actualLength)
            {
                wavBuffer[4] = (byte)(actualLength & 0xFF);
                wavBuffer[5] = (byte)((actualLength >> 8) & 0xFF);
                wavBuffer[6] = (byte)((actualLength >> 16) & 0xFF);
                wavBuffer[7] = (byte)((actualLength >> 24) & 0xFF);
            }

            return new MemoryStream(wavBuffer);
        }
    }
}
