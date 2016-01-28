
using System.IO;
using System.Text;
using EOLib;

namespace EndlessClient.Audio
{
	public static class WAVFileValidator
	{
		//some of the original SFX files will fail to load because the file length is stored incorrectly in the WAV header.
		//this method fixes those in-place. make sure to have backups! :)
		public static void CorrectTheFileLength(string filename)
		{
			byte[] wav = File.ReadAllBytes(filename);

			string riff = Encoding.ASCII.GetString(wav.SubArray(0, 4));
			if (riff != "RIFF" || wav.Length < 8) //check for RIFF tag and length
				return;

			int reportedLength = wav[4] + wav[5] * 256 + wav[6] * 65536 + wav[7] * 16777216;
			int actualLength = wav.Length - 8;

			if (reportedLength != actualLength)
			{
				wav[4] = (byte)(actualLength & 0xFF);
				wav[5] = (byte)((actualLength >> 8) & 0xFF);
				wav[6] = (byte)((actualLength >> 16) & 0xFF);
				wav[7] = (byte)((actualLength >> 24) & 0xFF);
				File.WriteAllBytes(filename, wav);
			}
		}
	}
}
