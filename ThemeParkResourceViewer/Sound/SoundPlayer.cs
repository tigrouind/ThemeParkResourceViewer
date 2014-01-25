using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ThemeParkResourceViewer
{
	public static class AudioPlayer
	{
		[DllImport("winmm.dll")]
		static extern bool PlaySound(byte[] pszSound, IntPtr hmod, uint fdwSound);

		const uint SND_ASYNC = 0x0001;
		const uint SND_MEMORY = 0x0004;
		const uint SND_LOOP = 0x0008;
		const uint SND_PURGE = 0x0040;

		public static byte[] CreateWave(byte[] data, uint offset, uint length, ushort bitsPerSample, uint sampleRate, ushort channels)
		{
			using MemoryStream audioStream = new();
			using BinaryWriter writer = new(audioStream);
			//header
			writer.Write("RIFF".ToCharArray());
			writer.Write(length + 44 - 8); //file size
			writer.Write("WAVE".ToCharArray());

			//format chunk
			writer.Write("fmt ".ToCharArray());
			writer.Write(16); //length of data listed above
			writer.Write((ushort)1); //PCM
			writer.Write(channels);
			writer.Write(sampleRate);
			writer.Write(sampleRate * bitsPerSample * channels / 8);
			writer.Write((ushort)(bitsPerSample * channels / 8));
			writer.Write(bitsPerSample);

			//data
			writer.Write("data".ToCharArray());
			writer.Write(length);
			writer.Write(data, (int)offset, (int)length);

			return audioStream.ToArray();
		}

		public static TimeSpan GetDuration(byte[] data, ushort bitsPerSample, uint sampleRate)
		{
			return TimeSpan.FromMilliseconds(data.Length * 1000L / (bitsPerSample / 8 * sampleRate));
		}

		public static void Play(byte[] audioBytes, bool loop)
		{
			PlaySound(audioBytes, IntPtr.Zero, SND_ASYNC | SND_MEMORY | (loop ? SND_LOOP : 0));
		}

		public static void Stop()
		{
			PlaySound(null, IntPtr.Zero, SND_PURGE);
		}
	}
}
