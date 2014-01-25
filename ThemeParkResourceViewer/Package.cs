using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ThemeParkResourceViewer
{
	static class Package
	{
		public static SpriteTab[] SpriteTabs { get; private set; }
		public static byte[] SpriteData { get; private set; }
		public static uint[] Palette { get; private set; }

		public static ushort[] FirstFrame { get; private set; }
		public static SpriteFrame[] SpriteFrames { get; private set; }
		public static SpriteElement[] SpriteElements { get; private set; }
		public static Animation[] Animations { get; private set; }

		public static byte[] SoundData { get; private set; }
		public static SoundTab[] SoundTabs { get; private set; }

		static string lastSprite, lastSound, lastAnim, lastPalette;
		static SpriteTab[] lastSpriteTab;
		static readonly Dictionary<uint, byte[]> pixelsCache = [];

		#region Load

		public static void Clear()
		{
			lastPalette = lastAnim = lastSound = lastSound = string.Empty;
		}

		public static void LoadSprite(string gameFolder, string filename)
		{
			LoadPalette(gameFolder, filename);

			if (filename != lastSprite)
			{
				SpriteData = SpriteViewer.GetSpriteData(Path.Combine(gameFolder, filename + ".DAT"));
				SpriteTabs = SpriteViewer.GetSpriteTabs(Path.Combine(gameFolder, filename + ".TAB"));
				lastSprite = filename;
			}
		}

		public static void LoadBackground(string gameFolder, string filename)
		{
			LoadPalette(gameFolder, filename);

			if (filename != lastSprite)
			{
				SpriteData = SpriteViewer.GetSpriteData(Path.Combine(gameFolder, filename + ".DAT"));
				if (SpriteData.Length == 65536)
				{
					SpriteTabs = [new SpriteTab() { Offset = 0, Width = 256, Height = 256 }];
				}
				else
				{
					SpriteTabs = [new SpriteTab() { Offset = 0, Width = 320, Height = 200 }];
				}
				lastSprite = filename;
			}
		}

		public static void LoadSound(string gameFolder, string filename)
		{
			if (filename != lastSound)
			{
				SoundData = SpriteViewer.GetSoundData(Path.Combine(gameFolder, filename + ".DAT"));
				SoundTabs = SpriteViewer.GetSoundTabs(Path.Combine(gameFolder, filename + ".TAB"));
				lastSound = filename;
			}
		}

		public static void LoadAnim(string gameFolder, string filename)
		{
			switch (filename)
			{
				case "MSTA-0":
					LoadSprite(gameFolder, "MSPR-0");
					if (filename != lastAnim)
					{
						SpriteFrames = SpriteViewer.GetSpriteFrames(Path.Combine(gameFolder, "MFRA-0.ANI"));
						SpriteElements = SpriteViewer.GetSpriteElements(Path.Combine(gameFolder, "MELE-0.ANI"));
						FirstFrame = SpriteViewer.GetSpriteAnims(Path.Combine(gameFolder, "MSTA-0.ANI"));
						Animations = [.. FirstFrame.Select(GetAnimation)];
						lastAnim = filename;
					}
					break;

				case "MUSSTA":
					LoadSprite(gameFolder, "MUS");
					if (filename != lastAnim)
					{
						SpriteFrames = SpriteViewer.GetSpriteFrames(Path.Combine(gameFolder, "MUSFRA.ANI"));
						SpriteElements = SpriteViewer.GetSpriteElements(Path.Combine(gameFolder, "MUSELE.ANI"));
						FirstFrame = SpriteViewer.GetSpriteAnims(Path.Combine(gameFolder, "MUSSTA.ANI"));
						Animations = [.. FirstFrame.Select(GetAnimation)];
						lastAnim = filename;
					}
					break;

				case "MDSTA-0":
					LoadSprite(gameFolder, "MDSPR-0");
					if (filename != lastAnim)
					{
						SpriteFrames = SpriteViewer.GetSpriteFrames(Path.Combine(gameFolder, "MDFRA-0.ANI"));
						SpriteElements = SpriteViewer.GetSpriteElements(Path.Combine(gameFolder, "MDELE-0.ANI"));
						FirstFrame = SpriteViewer.GetSpriteAnims(Path.Combine(gameFolder, "MDSTA-0.ANI"));
						Animations = [.. FirstFrame.Select(GetAnimation)];
						lastAnim = filename;
					}
					break;

				default:
					throw new NotSupportedException(filename);
			}

			static Animation GetAnimation(ushort frameIndex)
			{
				int minHeight = int.MaxValue;
				int minWidth = int.MaxValue;
				int maxHeight = int.MinValue;
				int maxWidth = int.MinValue;

				foreach (SpriteFrame spriteFrame in GetAllFrames(SpriteFrames[frameIndex]))
				{
					foreach (SpriteElement element in GetAllElements(SpriteElements[spriteFrame.FirstElement]))
					{
						SpriteTab tab = SpriteTabs[element.Sprite];
						minWidth = Math.Min(minWidth, element.XOffset);
						maxWidth = Math.Max(maxWidth, element.XOffset + tab.Width);

						minHeight = Math.Min(minHeight, element.YOffset);
						maxHeight = Math.Max(maxHeight, element.YOffset + tab.Height);
					}
				}

				return new Animation
				{
					Left = -minWidth,
					Top = -minHeight,
					Width = maxWidth - minWidth,
					Height = maxHeight - minHeight,
					FirstFrame = frameIndex,
					CurrentFrame = frameIndex,
					LastFrame = frameIndex
				};
			}
		}

		public static string[][] LoadText(string filename)
		{
			string[][] result;
			using (BinaryReader reader = new(File.OpenRead(filename)))
			{
				var sectionCount = reader.ReadUInt16();
				result = new string[sectionCount][];
				for(int i = 0; i < sectionCount; i++)
				{
					var entryCount = reader.ReadUInt16();
					result[i] = new string[entryCount];
				}

				foreach (var section in result)
				{
					GetEntries(reader, section);
				}

				Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
			}

			return result;

			static void GetEntries(BinaryReader reader, string[] section)
			{
				List<byte> data = [];

				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				var encoding = Encoding.GetEncoding(850);

				int i = 0;
				while(i < section.Length)
				{
					var ch = reader.ReadByte();
					if (ch == 0) // null byte separator
					{
						section[i++] = encoding.GetString([.. data]);
						data.Clear();
					}
					else
					{
						data.Add(ch);
					}
				}
			}
		}

		#endregion

		public static byte[] GetPixels(SpriteTab tab, byte[] data)
		{
			if (lastSpriteTab != SpriteTabs)
			{
				pixelsCache.Clear();
				lastSpriteTab = SpriteTabs;
			}

			if (!pixelsCache.TryGetValue(tab.Offset, out byte[] pixels))
			{
				pixels = [.. SpriteViewer.GetPixels(tab, data)];
				pixelsCache.Add(tab.Offset, pixels);
			}

			return pixels;
		}

		static void LoadPalette(string gameFolder, string filename)
		{
			filename = GetPalette();
			if (filename != lastPalette)
			{
				try
				{
					Palette = SpriteViewer.GetPalette(Path.Combine(gameFolder, filename));
				}
				catch (FileNotFoundException)
				{
					Palette = SpriteViewer.GetDefaultPalette();
				}

				lastPalette = filename;
			}

			string GetPalette()
			{
				return filename switch
				{
					"BUSTED" => "BUSPAL.DAT",
					"MAWAR0-0" or "MAWAR1-0" or "MCUP-0" => "MAWPAL-0.DAT",
					"MGLOBE-0" => "MGLPAL-0.DAT",
					"MHAND-0" or "MNEG-0" => "MNGPAL-0.DAT",
					"MUS" => "INPAL.DAT",
					"MRSSPR-0" or "MRES-0" => "MRSPAL-0.DAT",
					"MSTATE-0" => "MSTAP-0.DAT",
					"MSTSPR-0" or "MSTOCK-0" => "MSTPAL-0.DAT",
					"TAKOVER" => "TAKPAL.DAT",
					_ => "MPALETTE.DAT",
				};
			}
			;
		}

		#region Animation

		public static IEnumerable<SpriteFrame> GetAllFrames(SpriteFrame initialFrame)
		{
			SpriteFrame frame = initialFrame;
			yield return frame;

			while (SpriteFrames[frame.Next] != initialFrame)
			{
				frame = SpriteFrames[frame.Next];
				yield return frame;
			}
		}

		public static IEnumerable<SpriteElement> GetAllElements(SpriteElement element)
		{
			yield return element;
			while (element.Next != 0)
			{
				element = SpriteElements[element.Next];
				yield return element;
			}
		}

		#endregion
	}
}
