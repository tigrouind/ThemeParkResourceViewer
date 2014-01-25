using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ThemeParkResourceViewer
{
	public static class MusicPlayer
	{
		#region Native

		[DllImport("winmm.dll")]
		static extern uint midiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, IntPtr dwCallback, IntPtr dwInstance, uint dwFlags);

		[DllImport("winmm.dll")]
		static extern uint midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

		[DllImport("winmm.dll")]
		static extern uint midiOutReset(IntPtr hMidiOut);

		#endregion

		static IntPtr device;
		static MusicChannel[] channels;
		static readonly MMTimer timer = new MMTimer();
		static uint startedTime;
		public static bool Loop;

		static MusicPlayer()
		{
			timer.Timer += TimerTick;
			timer.Interval = 1;
		}

		public static void Play(byte[] data)
		{
			Stop();
			Load();
			InitMidi();

			startedTime = timer.GetTime();
			timer.Start();

			void InitMidi()
			{
				if (device == IntPtr.Zero)
				{
					//calling midiOutOpen / midiOutClose every Start / Stop is a bad idea because it's slow
					midiOutOpen(out device, 0, IntPtr.Zero, IntPtr.Zero, 0);
				}
			}

			void Load()
			{
				int position = 48;
				channels = new MusicChannel[ReadInt32()];
				position += 724; //skip header

				for (int i = 0; i < channels.Length; i++)
				{
					position += 12; //chunk number (4) / chunk size (4) / track number (4)

					var channel = new MusicChannel();
					channel.Data = GetChannelData().ToArray();
					channel.Delay = channel.Data[channel.Position++];
					channels[i] = channel;
				}

				IEnumerable<int> GetChannelData()
				{
					while (true)
					{
						yield return ReadVarInt(); //delay

						var command = data[position++];
						if (command == 0xff) //stop
						{
							position += 2;
							yield break;
						}

						switch (command & 0xf0)
						{
							case 0xc0: //patch change
								yield return command | data[position++] << 8;
								break;

							case 0x80: //note off
							case 0x90: //note on
							case 0xb0: //continuous controller (channel volume)
							case 0xe0: //pitch bend
								yield return command | data[position++] << 8 | data[position++] << 16;
								break;

							default:
								throw new NotSupportedException();
						}
					}

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
		}

		static void TimerTick(object sender, EventArgs e)
		{
			int elapsedTime = (int)(timer.GetTime() - startedTime);

			bool completed = true;
			for (int i = 0; i < channels.Length; i++)
			{
				var channel = channels[i];
				if (channel.Position < channel.Data.Length)
				{
					completed = false;
				}

				if ((elapsedTime * 120) >= (channel.Delay * 1000))  //120 BPM
				{
					if (channel.Position < channel.Data.Length)
					{
						midiOutShortMsg(device, (uint)channel.Data[channel.Position++]);
					}

					if (channel.Position < channel.Data.Length)
					{
						channel.Delay += channel.Data[channel.Position++];
					}
				}
			}

			if (completed)
			{
				if (Loop)
				{
					startedTime = timer.GetTime();
					foreach (var channel in channels)
					{
						channel.Position = 0;
						channel.Delay = channel.Data[channel.Position++];
					}
				}
				else
				{
					Stop();
				}
			}
		}

		public static bool IsRunning => timer.Enabled;

		public static void Stop()
		{
			timer.Stop();
			ResetMidi();

			void ResetMidi()
			{
				if (device != IntPtr.Zero)
				{
					midiOutReset(device); //silence all channels
				}
			}
		}
	}
}
