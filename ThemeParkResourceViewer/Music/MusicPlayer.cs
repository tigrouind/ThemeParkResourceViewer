using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;

namespace ThemeParkResourceViewer
{
	public static class MusicPlayer
	{
		static OutputDevice outputDevice;
		static Playback playback;

		static MidiFile Load(byte[] data)
		{
			int position = 48;
			int channels = ReadInt32();
			position = 772; //skip header

			var chunks = new List<TrackChunk>
			{
				new(new SetTempoEvent(60000000 / 75)) //75 BPM
			};

			for (int i = 0; i < channels; i++)
			{
				position += 12; //chunk number (4) / chunk size (4) / track number (4)
				var chunk = CreateChunk();
				chunks.Add(chunk);
			}

			var midi = new MidiFile(chunks);
			return midi;

			TrackChunk CreateChunk()
			{
				var trackChunk = new TrackChunk();
				var events = trackChunk.Events;

				while (true)
				{
					int delay = ReadVarInt(); //delay

					var command = data[position++];
					if (command == 0xff) //stop
					{
						events.Add(new StopEvent());
						position += 2;
						break;
					}

					int channel = command & 0x0f;

					switch (command & 0xf0)
					{
						case 0xc0: //patch change
							int patch = data[position++];
							events.Add(new ProgramChangeEvent((SevenBitNumber)patch) { DeltaTime = delay, Channel = (FourBitNumber)channel });
							break;

						case 0x90: //note on
							int noteNumber = data[position++];
							int velocity = data[position++];
							events.Add(new NoteOnEvent((SevenBitNumber)noteNumber, (SevenBitNumber)velocity) { DeltaTime = delay, Channel = (FourBitNumber)channel });
							break;

						case 0xb0: //continuous controller (channel volume)
							int controlNumber = data[position++];
							int controlValue = data[position++];
							events.Add(new ControlChangeEvent((SevenBitNumber)controlNumber, (SevenBitNumber)controlValue) { DeltaTime = delay, Channel = (FourBitNumber)channel });
							break;

						case 0xe0: //pitch bend
							int pitch = data[position++] | data[position++] << 7;
							events.Add(new PitchBendEvent((ushort)pitch) { DeltaTime = delay, Channel = (FourBitNumber)channel });
							break;

						default:
							throw new NotSupportedException();
					}
				}

				return trackChunk;

				int ReadVarInt()
				{
					int result = 0;
					int shift = 0;
					byte b;
					do
					{
						b = data[position++];
						result |= (b & 0x7f) << shift;
						shift += 7;
					}
					while ((b & 0x80) == 0);

					return result;
				}
			}

			int ReadInt32() => data[position++] | (data[position++] << 8) | (data[position++] << 16) | (data[position++] << 24);
		}

		public static TimeSpan GetDuration(byte[] data)
		{
			int position = 0x3C;
			return TimeSpan.FromSeconds(ReadInt32());
			int ReadInt32() => data[position++] | (data[position++] << 8) | (data[position++] << 16) | (data[position++] << 24);
		}

		public static TimeSpan CurrentTime => playback != null ? playback.GetCurrentTime<MetricTimeSpan>() : TimeSpan.Zero;

		public static TimeSpan TotalTime => playback != null ? playback.GetDuration<MetricTimeSpan>() : TimeSpan.Zero;

		public static void Seek(TimeSpan value)
		{
			if (playback != null)
			{
				MuteChannels();
				playback.MoveToTime(new MetricTimeSpan((int)value.TotalMicroseconds));
			}
		}

		public static void Play()
		{
			if (playback != null && !playback.IsRunning)
			{
				playback.Start();
			}
		}

		public static void Play(byte[] data)
		{
			Stop();
			var midi = Load(data);

			if (outputDevice == null)
			{
				outputDevice = OutputDevice.GetByIndex(0);
			}

			playback = midi.GetPlayback(outputDevice);
			playback.Start();
		}

		public static void Save(string filePath, byte[] data)
		{
			var midi = Load(data);
			midi.Write(filePath, true);
		}

		public static bool Loop
		{
			set
			{
				if (playback != null)
				{
					playback.Loop = value;
				}
			}
		}

		public static void Mute()
		{
			if (playback != null)
			{
				playback.Stop();
				MuteChannels();
			}
		}

		public static void Stop()
		{
			if (playback != null)
			{
				playback.Dispose();
				playback = null;

				MuteChannels();
			}
		}

		static void MuteChannels()
		{
			if (outputDevice != null)
			{
				for (int channel = 0; channel < 16; channel++) //silence all channels
				{
					outputDevice.SendEvent(new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0)
					{
						Channel = (FourBitNumber)channel
					});
				}
			}
		}
	}
}
