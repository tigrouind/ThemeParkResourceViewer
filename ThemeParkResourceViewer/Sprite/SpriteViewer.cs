using System;
using System.Collections.Generic;
using System.IO;

namespace ThemeParkResourceViewer
{
	class SpriteViewer
	{
		public static int TileSize = 32;

		public static SpriteTab[] GetSpriteTabs(string filename)
		{
			List<SpriteTab> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					SpriteTab tab = new()
					{
						Offset = reader.ReadUInt32(),
						Width = reader.ReadByte(),
						Height = reader.ReadByte()
					};
					list.Add(tab);
				}
			}
			return [.. list];
		}

		public static ushort[] GetSpriteAnims(string filename)
		{
			List<ushort> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					ushort indexes = reader.ReadUInt16();
					list.Add(indexes);
				}
			}
			return [.. list];
		}

		public static byte[] GetSpriteData(string filename)
		{
			return Path.GetFileNameWithoutExtension(filename) switch
			{
				"TAKOVER" => ReadBytes(26, 64000),
				"BUSTED" => ReadBytes(25, 64000),
				_ => File.ReadAllBytes(filename),
			};
			byte[] ReadBytes(int offset, int count)
			{
				using var fileStream = File.OpenRead(filename);
				byte[] data = new byte[count];
				fileStream.Seek(offset, SeekOrigin.Current);
				fileStream.ReadExactly(data, 0, count);
				return data;
			}
		}

		public static bool IsRNCCompressed(string filename)
		{
			using FileStream fs = File.OpenRead(filename);
			return fs.Length >= 3 && fs.ReadByte() == 'R' && fs.ReadByte() == 'N' && fs.ReadByte() == 'C';
		}

		public static uint[] GetPalette(string filename)
		{
			List<uint> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					uint r = reader.ReadByte();
					uint g = reader.ReadByte();
					uint b = reader.ReadByte();

					r = r << 2 | r >> 4;
					g = g << 2 | g >> 4;
					b = b << 2 | b >> 4;

					uint palette = 0xFF000000 | r << 16 | g << 8 | b;
					list.Add(palette);
				}
			}
			return [.. list];
		}

		public static uint[] GetDefaultPalette()
		{
			var palette = new uint[256];
			for (uint i = 0; i < palette.Length; i++)
			{
				palette[i] = 0xFF000000 | i << 16 | i << 8 | i;
			}

			return palette;
		}

		public static SpriteFrame[] GetSpriteFrames(string filename)
		{
			List<SpriteFrame> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					SpriteFrame frame = new()
					{
						FirstElement = reader.ReadUInt16(),
						Width = reader.ReadByte(),
						Height = reader.ReadByte(),
						Flags = reader.ReadUInt16(),
						Next = reader.ReadUInt16()
					};
					list.Add(frame);
				}
			}
			return [.. list];
		}

		public static SpriteElement[] GetSpriteElements(string filename)
		{
			List<SpriteElement> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					SpriteElement element = new()
					{
						Sprite = (ushort)(reader.ReadUInt16() / 6),
						XOffset = (short)(reader.ReadInt16() / 2),
						YOffset = (short)(reader.ReadInt16() / 2),
						XFlipped = reader.ReadUInt16(),
						Next = reader.ReadUInt16()
					};
					list.Add(element);
				}
			}
			return [.. list];
		}

		public static byte[] GetSoundData(string filename)
		{
			return File.ReadAllBytes(filename);
		}

		public static SoundTab[] GetSoundTabs(string filename)
		{
			List<SoundTab> list = [];
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				long length = reader.BaseStream.Length;
				while (reader.BaseStream.Position != length)
				{
					SoundTab tab = new()
					{
						Name = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(12)).TrimEnd('\0'),
						Dummy1 = reader.ReadBytes(6), //?
						Offset = reader.ReadUInt32(),
						Dummy2 = reader.ReadBytes(4), //?
						Length = reader.ReadUInt32(),
						Dummy3 = reader.ReadBytes(2) //?
					};

					if (string.IsNullOrWhiteSpace(tab.Name))
					{
						tab.Name = $"NONAME{list.Count:D2}";
					}
					list.Add(tab);
				}
			}
			return [.. list];
		}

		public static byte[] GetPixels(SpriteTab tab, byte[] data)
		{
			uint spriteData = tab.Offset;
			byte[] pixelData = new byte[tab.Width * tab.Height];

			int currentPixel = 0;
			for (int i = 0; i < tab.Height; i++)
			{
				int runLength = unchecked((sbyte)data[spriteData++]);
				while (runLength != 0)
				{
					if (runLength > 0) // pixel run
					{
						for (int j = 0; j < runLength; j++)
						{
							pixelData[currentPixel++] = data[spriteData++];
						}
					}
					else if (runLength < 0) // transparent run
					{
						for (int j = 0; j < -runLength; j++)
						{
							pixelData[currentPixel++] = 255;
						}
					}

					runLength = unchecked((sbyte)data[spriteData++]);
				}

				runLength = (i + 1) * tab.Width - currentPixel;
				for (int j = 0; j < runLength; j++) //fill the end of the line
				{
					pixelData[currentPixel++] = 255;
				}
			}
			return pixelData;
		}

		public static void DrawToBitmap(DirectBitmap bitmap, byte[] pixelData, uint[] palette, int posx, int posy, int width, int height, bool xflipped)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					byte colorIndex;
					if (xflipped)
					{
						colorIndex = pixelData[width - 1 - x + y * width];
					}
					else
					{
						colorIndex = pixelData[x + y * width];
					}

					if (colorIndex != 255 || (width == 320 && height == 200)) //transparency
					{
						bitmap.Bits[posx + x + (posy + y) * bitmap.Width] = palette[colorIndex];
					}
				}
			}
		}

		public static void DrawSpriteRectangle(DirectBitmap bitmap, int posx, int posy, int width, int height)
		{
			for (int x = 0; x < width; x++)
			{
				bitmap.Bits[x + posx + posy * bitmap.Width] = 0xFF00FF00;
				bitmap.Bits[x + posx + (posy + height - 1) * bitmap.Width] = 0xFF00FF00;
			}

			for (int y = 0; y < height; y++)
			{
				bitmap.Bits[posx + (y + posy) * bitmap.Width] = 0xFF00FF00;
				bitmap.Bits[posx + width - 1 + (y + posy) * bitmap.Width] = 0xFF00FF00;
			}
		}

		public static void DrawSpriteRectangleScaled(DirectBitmap bitmap, int destX, int destY, int srcWidth, int srcHeight,
			int offsetX, int offsetY, int spriteWidth, int spriteHeight)
		{
			if (srcWidth <= TileSize && srcHeight <= TileSize) //does it fit?
			{
				DrawSpriteRectangle(bitmap, destX + offsetX, destY + offsetY, spriteWidth, spriteHeight);
			}
			else
			{
				int scale = Math.Max(srcWidth, srcHeight); //take biggest side
				int destWidth = srcWidth * TileSize / scale;
				int destHeight = srcHeight * TileSize / scale;

				if (destWidth > 0 && destHeight > 0)
				{
					DrawToBitmapScaled();

					void DrawToBitmapScaled()
					{
						var (x0, y0, x1, y1) = GetScaleBoundaries(srcWidth, srcHeight, destWidth, destHeight, offsetX, offsetY, spriteWidth, spriteHeight);
						if (x1 >= x0 && y1 >= y0)
						{
							DrawSpriteRectangle(bitmap, destX + x0, destY + y0, x1 - x0 + 1, y1 - y0 + 1);
						}
					}
				}
			}
		}

		public static void DrawSpriteScaled(DirectBitmap bitmap, byte[] pixelData, uint[] palette, int destX, int destY, int srcWidth, int srcHeight,
			int offsetX, int offsetY, int spriteWidth, int spriteHeight, bool xflipped)
		{
			if (srcWidth <= TileSize && srcHeight <= TileSize) //does it fit?
			{
				DrawToBitmap(bitmap, pixelData, palette, destX + offsetX, destY + offsetY, spriteWidth, spriteHeight, xflipped);
			}
			else
			{
				int scale = Math.Max(srcWidth, srcHeight); //take biggest side
				int destWidth = srcWidth * TileSize / scale;
				int destHeight = srcHeight * TileSize / scale;

				if (destWidth > 0 && destHeight > 0)
				{
					DrawToBitmapScaled();

					void DrawToBitmapScaled()
					{
						var (x0, y0, x1, y1) = GetScaleBoundaries(srcWidth, srcHeight, destWidth, destHeight, offsetX, offsetY, spriteWidth, spriteHeight);

						for (int y = y0; y <= y1; y++)
						{
							int scaleY = y * srcHeight / destHeight;
							int srcY = scaleY - offsetY;

							for (int x = x0; x <= x1; x++)
							{
								int scaleX = x * srcWidth / destWidth;
								int srcX = scaleX - offsetX;

								byte colorIndex;
								if (xflipped)
								{
									colorIndex = pixelData[spriteWidth - 1 - srcX + srcY * spriteWidth];
								}
								else
								{
									colorIndex = pixelData[srcX + srcY * spriteWidth];
								}

								if (colorIndex != 255 || (destWidth == 320 && destHeight == 200)) //transparency
								{
									bitmap.Bits[destX + x + (destY + y) * bitmap.Width] = palette[colorIndex];
								}
							}
						}
					}
				}
			}
		}

		static (int x0, int x1, int y0, int y1) GetScaleBoundaries(int srcWidth, int srcHeight, int destWidth, int destHeight, int offsetX, int offsetY, int spriteWidth, int spriteHeight)
		{
			int x0 = (offsetX * destWidth + srcWidth - 1) / srcWidth;
			int x1 = ((offsetX + spriteWidth) * destWidth - 1) / srcWidth;

			int y0 = (offsetY * destHeight + srcHeight - 1) / srcHeight;
			int y1 = ((offsetY + spriteHeight) * destHeight - 1) / srcHeight;

			return (x0, y0, x1, y1);
		}

		public static (int X, int Y) GetSpriteCenter(int srcWidth, int srcHeight, int destWidth, int destHeight)
		{
			var dest = GetDestSize(srcWidth, srcHeight, destWidth, destHeight);
			return ((destWidth - dest.X) / 2, (destHeight - dest.Y) / 2);

			static (int X, int Y) GetDestSize(int srcWidth, int srcHeight, int destWidth, int destHeight)
			{
				if (srcWidth <= destWidth && srcHeight <= destHeight)
				{
					return (srcWidth, srcHeight);
				}
				else
				{
					int scale = Math.Max(srcWidth, srcHeight); //take biggest side
					return (srcWidth * destWidth / scale, srcHeight * destHeight / scale);
				}
			}
		}

		public static void ClearSprite(DirectBitmap bitmap, int posX, int posY, int width, int height)
		{
			int scale = Math.Max(width, height); //take biggest side
			width = Math.Min(width, width * TileSize / scale);
			height = Math.Min(height, height * TileSize / scale);

			for (int y = 0; y < height; y++)
			{
				Array.Clear(bitmap.Bits, posX + (posY + y) * bitmap.Width, width);
			}
		}
	}
}