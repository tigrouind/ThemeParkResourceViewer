using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThemeParkResourceViewer
{
	public class MoviePlayer : IDisposable
	{
		readonly byte[] pixels = new byte[64000];
		readonly uint[] palette = new uint[256];
		readonly Dictionary<int, (long Offset, uint[] Palette, byte[] Pixels)> cache = [];

		readonly byte[] firstPixels;
		readonly uint[] firstPalette;

		readonly BinaryReader reader;
		readonly long startPosition;
		readonly long fileLength;
		public int CurrentFrame = -1;
		public int FrameCount; //last frame might be same as first frame
		bool disposed;

		public MoviePlayer(string filename, int header = 6)
		{
			reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 256 * 1024));
			fileLength = reader.BaseStream.Length;

			reader.BaseStream.Seek(header, SeekOrigin.Begin); //header size (4) + 0xAF12 magic
			FrameCount = reader.ReadUInt16();
			if (filename.Contains("ANM")) FrameCount--;

			reader.BaseStream.Seek(4, SeekOrigin.Current); //frame count(2) + width(2) + height(2)
			startPosition = reader.BaseStream.Position;
		}

		public MoviePlayer(string filename, byte[] pixels, uint[] palette)
			: this(filename, 0)
		{
			firstPixels = pixels;
			firstPalette = palette;
			LoadFirstFrame();
		}

		public static TimeSpan GetDuration(string filename, int header = 6)
		{
			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
			reader.BaseStream.Seek(header, SeekOrigin.Begin);
			return TimeSpan.FromSeconds(reader.ReadUInt16() / 15);
		}

		public void RenderBitmap(DirectBitmap bitmap)
		{
			for (int i = 0; i < 64000; i++)
			{
				bitmap.Bits[i] = palette[pixels[i]];
			}
		}

		bool GetFrameFromCache(int frame)
		{
			if (cache.TryGetValue(frame, out var item))
			{
				Array.Copy(item.Palette, palette, item.Palette.Length);
				Array.Copy(item.Pixels, pixels, item.Pixels.Length);
				reader.BaseStream.Seek(item.Offset, SeekOrigin.Begin);
				return true;
			}

			return false;
		}

		public void Seek(int frame) //already in cache?
		{
			if (GetFrameFromCache(frame))
			{
				CurrentFrame = frame;
			}
			else
			{
				while (CurrentFrame != frame)
				{
					NextFrame();
				}
			}
		}

		public bool NextFrame()
		{
			if (reader.BaseStream.Position == fileLength) //eof
			{
				Reset();
			}

			CurrentFrame++;

			if (!GetFrameFromCache(CurrentFrame))
			{
				reader.BaseStream.Seek(6, SeekOrigin.Current); //frame length (4) + 0xF1FA magic
				ushort frameChunks = reader.ReadUInt16();
				reader.BaseStream.Seek(8, SeekOrigin.Current); //padding

				for (int j = 0; j < frameChunks; j++)
				{
					long nextChunk = reader.BaseStream.Position + reader.ReadUInt32();
					ushort typeChunk = reader.ReadUInt16();
					switch (typeChunk)
					{
						case 4:
							Palette();
							break;

						case 7:
							DeltaFLC();
							break;

						case 12:
							DeltaFLI();
							break;

						case 15: //first frame
							RLEFrame();
							break;

						case 16: //uncompressed
							reader.Read(pixels, 0, 64000);
							break;
					}

					reader.BaseStream.Seek(nextChunk, SeekOrigin.Begin);
				}

				cache.Add(CurrentFrame, (reader.BaseStream.Position, [.. palette], [.. pixels]));
			}

			//rewind if needed
			if (reader.BaseStream.Position == fileLength) //eof
			{
				return false;
			}

			return true;

			void Palette()
			{
				int position = 0;
				ushort numPackets = reader.ReadUInt16();
				byte skip = reader.ReadByte();
				int changes = reader.ReadByte();

				if (skip == 0 && changes == 0) //special case
				{
					changes = 256;
					numPackets = 1;
				}

				for (int i = 0; i < numPackets; i++)
				{
					position += skip;
					for (int j = 0; j < changes; j++)
					{
						uint r = reader.ReadByte();
						uint g = reader.ReadByte();
						uint b = reader.ReadByte();

						r = r << 2 | r >> 4;
						g = g << 2 | g >> 4;
						b = b << 2 | b >> 4;

						palette[position++] = 0xFF000000 | r << 16 | g << 8 | b;
					}

					if (i < (numPackets - 1)) //last one?
					{
						skip = reader.ReadByte();
						changes = reader.ReadByte();
					}
				}
			}

			void RLEFrame()
			{
				int position = 0;
				while (position < 64000)
				{
					byte chunks = reader.ReadByte();
					for (int i = 0; i < chunks; i++)
					{
						sbyte count = reader.ReadSByte();
						if (count > 0)
						{
							byte data = reader.ReadByte();
							for (int j = 0; j < count; j++)
							{
								pixels[position++] = data;
							}
						}
						else if (count < 0)
						{
							for (int j = 0; j < -count; j++)
							{
								pixels[position++] = reader.ReadByte();
							}
						}
					}
				}
			}

			void DeltaFLI()
			{
				int position = reader.ReadUInt16() * 320;
				ushort lineCount = reader.ReadUInt16();

				for (int i = 0; i < lineCount; i++)
				{
					int packetCount = reader.ReadByte();

					int previousPos = position;
					for (int j = 0; j < packetCount; j++)
					{
						int skip = reader.ReadByte();
						int count = reader.ReadSByte();
						position += skip;

						if (count > 0)
						{
							for (int k = 0; k < count; k++)
							{
								pixels[position++] = reader.ReadByte();
							}
						}
						else if (count < 0)
						{
							byte data = reader.ReadByte();
							for (int k = 0; k < -count; k++)
							{
								pixels[position++] = data;
							}
						}
					}

					position = previousPos + 320; //next line
				}
			}

			void DeltaFLC()
			{
				ushort lineCount = reader.ReadUInt16();
				int position = 0;

				for (int i = 0; i < lineCount; i++)
				{
					ushort packetCount = 0;

					//process opcodes
					ushort opcode;
					do
					{
						opcode = reader.ReadUInt16();

						switch (opcode & 0xC000)
						{
							case 0: //last instruction
								packetCount = opcode;
								break;

							case 0xC000: //skip lines
								unchecked
								{
									position += -(short)opcode * 320;
								}
								break;
						}
					}
					while ((opcode & 0xC000) != 0);

					int previousPos = position;
					for (int j = 0; j < packetCount; j++)
					{
						position += reader.ReadByte();
						sbyte count = reader.ReadSByte();

						if (count > 0)
						{
							for (int k = 0; k < count * 2; k++)
							{
								pixels[position++] = reader.ReadByte();
							}
						}
						else if (count < 0)
						{
							byte data0 = reader.ReadByte();
							byte data1 = reader.ReadByte();

							for (int k = 0; k < -count; k++)
							{
								pixels[position++] = data0;
								pixels[position++] = data1;
							}
						}
					}

					position = previousPos + 320; //next line
				}
			}
		}

		public void Reset()
		{
			reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
			CurrentFrame = -1;
			LoadFirstFrame();
		}

		void LoadFirstFrame()
		{
			if (firstPixels != null && firstPalette != null)
			{
				Array.Copy(firstPixels, pixels, 64000);
				Array.Copy(firstPalette, palette, 256);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
				if (disposing)
				{
					reader.Dispose();
				}
			}
		}
	}
}
