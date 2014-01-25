using AnimatedGif;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace ThemeParkResourceViewer
{
	public partial class MainForm : Form
	{
		string gameFolder;
		int zoom, gridSize;
		DirectBitmap bitmap;
		int tilesPerRow;
		int tilesCount;
		MoviePlayer moviePlayer;
		byte[] soundBytes;
		readonly Stack<(TreeNode, int)> undo = new();
		readonly Stack<(TreeNode, int)> redo = new();
		bool ignoreSelectedNodeEvent;
		bool clearAnimation;
		string ffmpegPath = GetffmpegLocation();

		public MainForm()
		{
			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
		}

		void MainForm_Load(object sender, EventArgs e)
		{
			TryGetDataFolder(Path.GetFullPath("."), out gameFolder);
			if (gameFolder != null)
			{
				LoadTreeView();
			}

			SetZoom(2);
			SetGrid(1);
		}

		static bool TryGetDataFolder(string folder, out string result)
		{
			foreach (var subfolder in new[] { folder, Path.Combine(folder, "DATA") })
			{
				if (Directory.Exists(subfolder) && Directory.GetFiles(subfolder, "*.TAB").Any() && Directory.GetFiles(subfolder, "*.DAT").Any())
				{
					result = subfolder;
					return true;
				}
			}

			result = folder;
			return false;
		}

		#region Treeview

		public TreeNode GetNode(string key, string text)
		{
			return GetNode(key, text, treeView.Nodes);
		}

		public static TreeNode GetNode(string key, string text, TreeNodeCollection nodes)
		{
			var index = nodes.IndexOfKey(key);
			if (index == -1)
			{
				return nodes.Add(key, text);
			}
			else
			{
				return nodes[index];
			}
		}

		void LoadTreeView()
		{
			var allfiles = Directory.GetFiles(gameFolder)
				.Where(x => !SpriteViewer.IsRNCCompressed(x))
				.Select(Path.GetFileName)
				.ToHashSet();

			string[] anim = ["MFRA-0.ANI", "MELE-0.ANI", "MSTA-0.ANI"];
			string[] animUS = ["MUSFRA.ANI", "MUSELE.ANI", "MUSSTA.ANI"];

			foreach (string fileName in allfiles
				.OrderBy(x => x))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
				if (!fileName.StartsWith("MUSIC") && !fileName.StartsWith("SND"))
				{
					switch (Path.GetExtension(fileName))
					{
						case ".DAT":
							long filesize = new FileInfo(Path.Combine(gameFolder, fileName)).Length;
							if (filesize == 64000)
							{
								GetNode("BKG", "BACKGROUNDS").Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension + " (320x200)");
							}
							break;

						case ".TAB":
							if (allfiles.Contains(fileNameWithoutExtension + ".DAT"))
							{
								TreeNode node = GetNode("SPR", "SPRITES").Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension);
								node.Nodes.Add("");
							}
							break;
					}
				}
				if ((fileNameWithoutExtension == "MSTA-0" && anim.All(allfiles.Contains))
					|| (fileNameWithoutExtension == "MUSSTA" && animUS.All(allfiles.Contains)))
				{
					TreeNode node = GetNode("ANI", "ANIMATIONS").Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension);
					node.Nodes.Add("");
				}

				if (fileNameWithoutExtension == "INTRO" || fileNameWithoutExtension == "WINGAME")
				{
					GetNode("FLI", "MOVIES").Nodes.Add(fileName, fileNameWithoutExtension);
				}

				if ((fileName == "TAKOVER.ANM" || fileName == "BUSTED.ANM")
					&& allfiles.Contains(fileNameWithoutExtension + ".DAT"))
				{
					GetNode("FLI", "MOVIES").Nodes.Add(fileName, fileNameWithoutExtension);
				}

				if (fileNameWithoutExtension == "RIDEANI")
				{
					GetNode("FLI", "MOVIES").Nodes.Add(fileName, fileName);
				}

				if (fileName.StartsWith("SND") && Path.GetExtension(fileName) == ".TAB")
				{
					string name = string.Empty;
					string lastChar = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1, 1);
					switch (lastChar)
					{
						case "0":
							name = "11KHZ 8-BIT";
							break;

						case "1":
							name = "22KHZ 8-BIT";
							break;

						case "2":
							name = "44KHZ 16-BIT";
							break;

						default:
							continue;
					}

					var node = GetNode("SND", "SOUNDS");
					node = GetNode("SND" + lastChar, name, node.Nodes).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension);
					node.Nodes.Add("");
				}

				if (fileName.StartsWith("MUSIC") && Path.GetExtension(fileName) == ".TAB")
				{
					string name = string.Empty;
					string lastChar = fileNameWithoutExtension.Substring(fileNameWithoutExtension.Length - 1, 1);
					switch (lastChar)
					{
						case "0":
							name = "ADLIB";
							break;

						case "1":
							name = "ROLAND";
							break;

						case "2":
							name = "GENERAL MIDI";
							break;

						default:
							continue;
					}

					var node = GetNode("MUS", "MUSIC");
					node = GetNode("MUS" + lastChar, name, node.Nodes).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension);
					node.Nodes.Add("");
				}
			}
		}

		void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			var node = e.Node;
			var root = GetRoot(node);

			switch ($"{root.Name}|{node.Level}")
			{
				case "SPR|1":
					if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
					{
						Package.LoadSprite(gameFolder, node.Name);
						node.Nodes.Clear();
						int index = 0;
						foreach (SpriteTab tab in Package.SpriteTabs)
						{
							if (tab.Height > 0 && tab.Width > 0)
							{
								node.Nodes.Add(index.ToString(), $"SPRITE{index} ({tab.Width}x{tab.Height})");
							}
							index++;
						}
					}
					break;

				case "ANI|1":
					if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
					{
						Package.LoadAnim(gameFolder, node.Name);
						node.Nodes.Clear();
						int index = 0;
						for (int i = 0; i < Package.Animations.Length; i++)
						{
							var animation = Package.Animations[i];
							if (animation.Width > 0 && animation.Height > 0)
							{
								node.Nodes.Add(i.ToString(), $"ANIM{index} ({animation.Width}x{animation.Height})");
							}
							index++;
						}
					}
					break;

				case "SND|2":
				case "MUS|2":
					if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
					{
						Package.LoadSound(gameFolder, node.Name);
						node.Nodes.Clear();
						int index = 0;
						foreach (SoundTab snd in Package.SoundTabs)
						{
							if (!string.IsNullOrEmpty(snd.Name) && snd.Name != "NULL.RAW")
							{
								node.Nodes.Add(index.ToString(), snd.Name);
							}
							index++;
						}
					}
					break;
			}
		}

		void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			timer.Stop();
			AudioPlayer.Stop();
			MusicPlayer.Stop();

			var node = e.Node;
			exportToPNGToolStripMenuItem.Enabled = node.Nodes.Count == 0;
			var root = GetRoot(node);

			switch ($"{root.Name}|{node.Level}")
			{
				case "BKG|1":
					{
						Package.LoadBackground(gameFolder, node.Name);
						DisplaySprite(Package.SpriteTabs[0], Package.SpriteData, Package.Palette);
					}
					break;

				case "FLI|1":
					{
						moviePlayer?.Dispose();

						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(node.Name);
						switch (fileNameWithoutExtension)
						{
							case "BUSTED":
							case "TAKOVER":
								Package.LoadBackground(gameFolder, fileNameWithoutExtension);
								moviePlayer = new MoviePlayer(Path.Combine(gameFolder, node.Name), Package.SpriteData, Package.Palette);
								break;

							default:
								moviePlayer = new MoviePlayer(Path.Combine(gameFolder, node.Name));
								break;
						}

						SetBitmapSize(320, 200);

						timer.Interval = 66; //15 fps
						timer.Start();

						Timer1_Tick(this, EventArgs.Empty);
					}
					break;

				case "SPR|1":
					{
						Package.LoadSprite(gameFolder, node.Name);
						DrawGrid();
					}
					break;

				case "SPR|2":
					{
						Package.LoadSprite(gameFolder, node.Parent.Name);
						var tab = Package.SpriteTabs[int.Parse(node.Name)];
						var pixelData = Package.GetPixels(tab, Package.SpriteData);
						DisplaySprite(tab, pixelData, Package.Palette);
					}
					break;

				case "ANI|1":
					{
						Package.LoadAnim(gameFolder, node.Name);
						PrepareGrid(Package.Animations.Count(x => x.Width > 0 && x.Height > 0));
						clearAnimation = true;

						timer.Interval = 150;
						timer.Start();
						Timer1_Tick(this, EventArgs.Empty);
					}
					break;

				case "ANI|2":
					{
						Package.LoadAnim(gameFolder, node.Parent.Name);
						var animation = Package.Animations[int.Parse(node.Name)];
						SetBitmapSize(animation.Width, animation.Height);
						animation.CurrentFrame = animation.FirstFrame;

						timer.Interval = 150;
						timer.Start();
						Timer1_Tick(this, EventArgs.Empty);
					}
					break;

				case "SND|3":
					{
						ClearBitmap();
						string filename = node.Parent.Name;
						Package.LoadSound(gameFolder, filename);
						SoundTab tab = Package.SoundTabs[int.Parse(node.Name)];
						switch (filename.Substring(filename.Length - 1, 1))
						{
							case "0":
								soundBytes = AudioPlayer.CreateWave(Package.SoundData, tab.Offset, tab.Length - 16, 8, 11025, 1);
								break;

							case "1":
								soundBytes = AudioPlayer.CreateWave(Package.SoundData, tab.Offset, tab.Length - 16, 8, 22050, 1);
								break;

							case "2":
								soundBytes = AudioPlayer.CreateWave(Package.SoundData, tab.Offset, tab.Length - 16, 16, 44100, 1);
								break;
						}

						AudioPlayer.Play(soundBytes, loopSoundToolStripMenuItem.Checked);
					}
					break;

				case "MUS|3":
					{
						ClearBitmap();
						string filename = node.Parent.Name;
						Package.LoadSound(gameFolder, node.Parent.Name);
						SoundTab tab = Package.SoundTabs[int.Parse(node.Name)];
						soundBytes = new byte[tab.Length];
						Array.Copy(Package.SoundData, tab.Offset, soundBytes, 0, tab.Length);

						MusicPlayer.Play(soundBytes);
					}
					break;

				default:
					ClearBitmap();
					break;
			}
		}

		#endregion

		#region Drawing

		void DisplaySprite(SpriteTab tab, byte[] pixelData, uint[] palette)
		{
			if (tab.Width == 0 && tab.Height == 0)
			{
				return;
			}

			SetBitmapSize(tab.Width, tab.Height);
			Array.Clear(bitmap.Bits, 0, bitmap.Bits.Length);
			SpriteViewer.DrawToBitmap(bitmap, pixelData, palette, 0, 0, tab.Width, tab.Height, false);
		}

		void ClearBitmap()
		{
			bitmap?.Dispose();

			bitmap = null;
			pictureBox.Image = null;
			pictureBox.Visible = false;
		}

		void SetBitmapSize(int width, int height)
		{
			if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
			{
				bitmap?.Dispose();

				bitmap = new DirectBitmap(width, height);
				pictureBox.Width = width * zoom;
				pictureBox.Height = height * zoom;
				pictureBox.Visible = true;
			}

			pictureBox.Image = bitmap.Bitmap;
		}

		#endregion

		#region Grid

		void DrawGrid()
		{
			var tabs = Package.SpriteTabs.Where(x => x.Height > 0 && x.Width > 0);
			PrepareGrid(tabs.Count());

			//draw tiles
			int index = 0;
			foreach (var tab in tabs)
			{
				int posX = index % tilesPerRow * SpriteViewer.TileSize;
				int posY = index / tilesPerRow * SpriteViewer.TileSize;

				var pixelData = Package.GetPixels(tab, Package.SpriteData);
				SpriteViewer.DrawSpriteScaled(bitmap, pixelData, Package.Palette, posX, posY,
					tab.Width, tab.Height, 0, 0, tab.Width, tab.Height, false);

				if (showSpritesToolStripMenuItem.Checked)
				{
					SpriteViewer.DrawSpriteRectangleScaled(bitmap,
						posX,
						posY,
						tab.Width,
						tab.Height,
						0,
						0,
						tab.Width,
						tab.Height);
				}

				index++;
			}
		}

		int GetTilePerRow()
		{
			const int maxHeight = 32768; //max picturebox height that can handle mouse click events
			int maxRows = maxHeight / (SpriteViewer.TileSize * zoom);
			int minTilesPerRow = (tilesCount + maxRows - 1) / maxRows; //round up

			int tilePerRow = Math.Max(minTilesPerRow, splitContainer.Panel2.ClientRectangle.Width / (SpriteViewer.TileSize * zoom));
			return tilePerRow;
		}

		void PrepareGrid(int length)
		{
			tilesCount = length;
			tilesPerRow = GetTilePerRow();

			if (length <= tilesPerRow)
			{
				SetBitmapSize(length * SpriteViewer.TileSize, 1 * SpriteViewer.TileSize);
			}
			else
			{
				int tilesHeight = (length + tilesPerRow - 1) / tilesPerRow; //round up
				SetBitmapSize(tilesPerRow * SpriteViewer.TileSize, tilesHeight * SpriteViewer.TileSize);
			}

			Array.Clear(bitmap.Bits, 0, bitmap.Bits.Length);
		}

		void ResizeGrid()
		{
			var node = treeView.SelectedNode;
			if (node != null && node.Level == 1
					&& (node.Parent.Name == "ANI" || node.Parent.Name == "SPR")
					&& GetTilePerRow() != tilesPerRow)
			{
				TreeView1_AfterSelect(this, new TreeViewEventArgs(node));
			}
		}

		#endregion

		#region Zoom

		void Zoom1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetZoom(1);
		}

		void Zoom2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetZoom(2);
		}

		void Zoom3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetZoom(3);
		}

		void Zoom4ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetZoom(4);
		}

		void SetZoom(int zoomLevel)
		{
			if (zoom != zoomLevel)
			{
				zoom = zoomLevel;

				zoom1ToolStripMenuItem.Checked = zoom == 1;
				zoom2ToolStripMenuItem.Checked = zoom == 2;
				zoom3ToolStripMenuItem.Checked = zoom == 3;
				zoom4ToolStripMenuItem.Checked = zoom == 4;

				ResizeGrid();

				if (bitmap != null)
				{
					pictureBox.Width = bitmap.Width * zoom;
					pictureBox.Height = bitmap.Height * zoom;
				}
			}
		}

		void MainForm_MouseWheel(object sender, MouseEventArgs e)
		{
			switch (ModifierKeys)
			{
				case Keys.Control:
					if (e.Delta > 0 && zoom < 4)
					{
						SetZoom(zoom + 1);
					}
					else if (e.Delta < 0 && zoom > 1)
					{
						SetZoom(zoom - 1);
					}
					break;

				case Keys.Shift:
					if (e.Delta > 0 && gridSize < 2)
					{
						SetGrid(gridSize + 1);
					}
					else if (e.Delta < 0 && gridSize > 0)
					{
						SetGrid(gridSize - 1);
					}
					break;
			}
		}

		#endregion

		void Timer1_Tick(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "FLI|1":
						MoviePlay();
						break;

					case "ANI|1":
						{
							Package.LoadAnim(gameFolder, node.Name);
							AnimateGrid();
						}
						break;

					case "ANI|2":
						{
							Package.LoadAnim(gameFolder, node.Parent.Name);
							var animation = Package.Animations[int.Parse(node.Name)];
							AnimateSprite(animation);
						}
						break;
				}

				pictureBox.Invalidate();
			}

			void MoviePlay()
			{
				moviePlayer.RenderFrame(bitmap);
			}

			void AnimateSprite(Animation animation)
			{
				Array.Clear(bitmap.Bits, 0, bitmap.Bits.Length);

				var currentFrame = Package.SpriteFrames[animation.CurrentFrame];
				if (currentFrame.Width > 0 && currentFrame.Height > 0)
				{
					foreach (SpriteElement element in Package.GetAllElements(Package.SpriteElements[currentFrame.FirstElement]))
					{
						SpriteTab tab = Package.SpriteTabs[element.Sprite];
						SpriteViewer.DrawToBitmap(bitmap, Package.GetPixels(tab, Package.SpriteData), Package.Palette,
							animation.Left + element.XOffset,
							animation.Top + element.YOffset,
							tab.Width, tab.Height,
							(element.XFlipped & 0x1) == 0x1);
					}

					if (showSpritesToolStripMenuItem.Checked)
					{
						foreach (SpriteElement element in Package.GetAllElements(Package.SpriteElements[currentFrame.FirstElement]))
						{
							SpriteTab tab = Package.SpriteTabs[element.Sprite];
							SpriteViewer.DrawSpriteRectangle(bitmap,
								animation.Left + element.XOffset,
								animation.Top + element.YOffset,
								tab.Width,
								tab.Height);
						}
					}
				}

				animation.CurrentFrame = currentFrame.Next;
			}

			void AnimateGrid()
			{
				int index = 0;
				foreach (var anim in Package.Animations)
				{
					if (anim.Width > 0 && anim.Height > 0)
					{
						var currentFrame = Package.SpriteFrames[anim.CurrentFrame];
						if (currentFrame.FirstElement != Package.SpriteFrames[anim.LastFrame].FirstElement || clearAnimation)
						{
							int posX = index % tilesPerRow * SpriteViewer.TileSize;
							int posY = index / tilesPerRow * SpriteViewer.TileSize;
							SpriteViewer.ClearSprite(bitmap, posX, posY, anim.Width, anim.Height);

							if (currentFrame.Width > 0 && currentFrame.Height > 0)
							{
								DrawAnimedSprite();

								void DrawAnimedSprite()
								{
									foreach (SpriteElement element in Package.GetAllElements(Package.SpriteElements[currentFrame.FirstElement]))
									{
										SpriteTab tab = Package.SpriteTabs[element.Sprite];
										if (tab.Width > 0 && tab.Height > 0)
										{
											SpriteViewer.DrawSpriteScaled(bitmap, Package.GetPixels(tab, Package.SpriteData), Package.Palette,
												posX,
												posY,
												anim.Width,
												anim.Height,
												anim.Left + element.XOffset,
												anim.Top + element.YOffset,
												tab.Width,
												tab.Height,
												(element.XFlipped & 0x1) == 0x1);
										}
									}

									if (showSpritesToolStripMenuItem.Checked)
									{
										foreach (SpriteElement element in Package.GetAllElements(Package.SpriteElements[currentFrame.FirstElement]))
										{
											SpriteTab tab = Package.SpriteTabs[element.Sprite];
											SpriteViewer.DrawSpriteRectangleScaled(bitmap,
												posX,
												posY,
												anim.Width,
												anim.Height,
												anim.Left + element.XOffset,
												anim.Top + element.YOffset,
												tab.Width,
												tab.Height);
										}
									}
								}
							}

							anim.LastFrame = anim.CurrentFrame;
						}

						anim.CurrentFrame = currentFrame.Next;
						index++;
					}
				}

				clearAnimation = false;
			}
		}

		void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void ExportToPNGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode node = treeView.SelectedNode;
			if (node != null && node.Nodes.Count == 0)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "BKG|1":
					case "SPR|2":
						{
							string fileName;
							if (node.Parent.Name == "BKG")
							{
								fileName = $"{node.Name}.png";
							}
							else
							{
								fileName = $"{node.Parent.Text}_{node.Name}.png";
							}

							SaveFileDialog dialog = new()
							{
								Filter = "PNG (*.png)|*.png",
								FileName = fileName
							};

							if (dialog.ShowDialog() == DialogResult.OK)
							{
								pictureBox.Image.Save(dialog.FileName, ImageFormat.Png);
							}
						}
						break;

					case "MUS|3":
						{
							SaveFileDialog dialog = new()
							{
								FileName = $"{node.Text.Split('.')[0]}.mid",
								Filter = $"MIDI (*.mid)|*.mid|HMP (*.hmp)|*.hmp"
							};

							if (dialog.ShowDialog() == DialogResult.OK)
							{
								if (string.Equals(Path.GetExtension(dialog.FileName), ".hmp", StringComparison.InvariantCultureIgnoreCase))
								{
									File.WriteAllBytes(dialog.FileName, soundBytes);
								}
								else
								{
									MusicPlayer.Save(dialog.FileName, soundBytes);
								}
							}
						}
						break;

					case "SND|3":
						{
							SaveFileDialog dialog = new()
							{
								FileName = $"{node.Text.Split('.')[0]}.wav",
								Filter = "WAV (*.wav)|*.wav"
							};

							if (dialog.ShowDialog() == DialogResult.OK)
							{
								File.WriteAllBytes(dialog.FileName, soundBytes);
							}
						}
						break;

					case "ANI|2":
						{
							SaveFileDialog dialog = new()
							{
								FileName = $"{node.Parent.Text}_{node.Name}.gif",
								Filter = "GIF (*.gif)|*.gif"
							};

							if (dialog.ShowDialog() == DialogResult.OK)
							{
								string filePath = Path.Combine(Path.GetDirectoryName(dialog.FileName), $"{Path.GetFileNameWithoutExtension(dialog.FileName)}.gif");
								ExportGif(filePath, node);
							}
						}
						break;

					case "FLI|1":
						{
							SaveFileDialog saveDialog = new()
							{
								FileName = $"{node.Text}.mp4",
								Filter = "MP4 (*.mp4)|*.mp4"
							};

							if (saveDialog.ShowDialog() == DialogResult.OK)
							{
								if (string.IsNullOrEmpty(ffmpegPath))
								{
									MessageBox.Show("Can't find ffmpeg location.\nPlease indicate where ffmpeg is installed using \"File\" menu", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
								}
								else
								{
									var filePath = Path.Combine(Path.GetDirectoryName(saveDialog.FileName), $"{Path.GetFileNameWithoutExtension(saveDialog.FileName)}.mp4");
									ExportMovie(filePath);
								}

							}
						}
						break;
				}
			}
		}

		static string GetffmpegLocation()
		{
			return Environment.GetEnvironmentVariable("PATH")
				.Split(';')
				.Concat([@"c:\ffmpeg\bin"])
				.FirstOrDefault(s => File.Exists(Path.Combine(s, "ffmpeg.exe")));
		}

		void ExportGif(string filePath, TreeNode node)
		{
			Package.LoadAnim(gameFolder, node.Parent.Name);

			var animation = Package.Animations[int.Parse(node.Name)];
			var firstFrame = Package.SpriteFrames[animation.FirstFrame];

			using var gif = AnimatedGif.AnimatedGif.Create(filePath, 150);
			foreach (SpriteFrame frame in Package.GetAllFrames(Package.SpriteFrames[firstFrame.Next]))
			{
				if (frame.Width > 0 && frame.Height > 0)
				{
					Array.Clear(bitmap.Bits, 0, bitmap.Bits.Length);

					foreach (SpriteElement element in Package.GetAllElements(Package.SpriteElements[frame.FirstElement]))
					{
						SpriteTab tab = Package.SpriteTabs[element.Sprite];
						SpriteViewer.DrawToBitmap(bitmap, Package.GetPixels(tab, Package.SpriteData), Package.Palette,
							animation.Left + element.XOffset,
							animation.Top + element.YOffset,
							tab.Width, tab.Height,
							(element.XFlipped & 0x1) == 0x1);
					}

					gif.AddFrame(bitmap.Bitmap, delay: -1, quality: GifQuality.Bit8);
				}
			}
		}

		void ExportMovie(string filePath)
		{
			File.Delete(filePath);

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(ffmpegPath, "ffmpeg.exe"),
					Arguments = $"-f rawvideo -pix_fmt bgra -s {bitmap.Width}x{bitmap.Height} -r 15 -i - -c:v libx264 -crf 0 -preset veryslow -pix_fmt yuv444p \"{filePath}\"",
					RedirectStandardInput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();

			try
			{
				using (var input = process.StandardInput.BaseStream)
				{
					var stopwatch = Stopwatch.StartNew();
					moviePlayer.Reset();
					while (moviePlayer.RenderFrame(bitmap))
					{
						var byteArray = new byte[bitmap.Bits.Length * sizeof(uint)];
						Buffer.BlockCopy(bitmap.Bits, 0, byteArray, 0, byteArray.Length);
						input.Write(byteArray, 0, byteArray.Length);
						if (stopwatch.Elapsed > TimeSpan.FromSeconds(0.5))
						{
							stopwatch.Restart();
							pictureBox.Refresh();
						}
					}
				}

				process.StandardError.ReadToEnd();
				process.WaitForExit();
			}
			catch
			{
				var errorOutput = process.StandardError.ReadToEnd();
				MessageBox.Show(errorOutput, "FFmpeg failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void SetGameLocationFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool success = false;
			string folder = gameFolder;

			do
			{
				FolderBrowserDialog dialog = new()
				{
					SelectedPath = folder + @"\",
					Description = "Please select folder where game is installed :"
				};
				DialogResult result = dialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					if (TryGetDataFolder(dialog.SelectedPath, out folder))
					{
						//reset everything
						treeView.Nodes.Clear();
						exportToPNGToolStripMenuItem.Enabled = false;
						timer.Stop();
						MusicPlayer.Stop();
						AudioPlayer.Stop();
						ClearBitmap();

						gameFolder = folder;
						LoadTreeView();
						success = true;
					}
					else
					{
						MessageBox.Show("No game data files found in this folder", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				else
				{
					success = true;
				}
			}
			while (!success);
		}

		void SetFFmpegLocationPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var folderDialog = new FolderBrowserDialog()
			{
				SelectedPath = GetffmpegLocation() ?? @"c:\ffmpeg\bin\",
				Description = @"Please indicate the folder where ffmpeg.exe is located (eg: 'C:\ffmpeg\bin\')"
			};

			do
			{
				if (folderDialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}
			}
			while (!File.Exists(Path.Combine(folderDialog.SelectedPath, "ffmpeg.exe")));

			ffmpegPath = folderDialog.SelectedPath;
		}

		void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var version = Assembly.GetEntryAssembly().GetName().Version;
			var buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(TimeSpan.TicksPerDay * version.Build));

			MessageBox.Show($"{Text} v{version.Major}.{version.Minor:D2}\r\nDate : {buildDateTime:d}\r\nContact me : tigrou.ind@gmail.com");
		}

		void UseBilinearFilteringToolStripMenuItem_Click(object sender, EventArgs e)
		{
			pictureBox.InterpolationMode = useBilinearFilteringToolStripMenuItem.Checked ?
				InterpolationMode.Bilinear : InterpolationMode.NearestNeighbor;
			pictureBox.Refresh();
		}

		void LoopSoundToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "SND|3":
						if (!loopSoundToolStripMenuItem.Checked)
						{
							AudioPlayer.Stop();
						}
						else
						{
							AudioPlayer.Play(soundBytes, loopSoundToolStripMenuItem.Checked);
						}
						break;

					case "MUS|3":
						MusicPlayer.Loop = loopSoundToolStripMenuItem.Checked;
						if (!MusicPlayer.IsRunning)
						{
							MusicPlayer.Play(soundBytes);
						}
						break;
				}
			}
		}

		void SplitContainer_Panel2_Resize(object sender, EventArgs e)
		{
			ResizeGrid();
		}

		static TreeNode GetRoot(TreeNode node)
		{
			while (node.Parent != null)
			{
				node = node.Parent;
			}

			return node;
		}

		void PictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "SPR|1":
					case "ANI|1":
						int tileX = e.X / (SpriteViewer.TileSize * zoom);
						int tileY = e.Y / (SpriteViewer.TileSize * zoom);
						int nodeIndex = tileX + tilesPerRow * tileY;

						if (nodeIndex < tilesCount)
						{
							node.Expand();
							treeView.SelectedNode = node.Nodes[nodeIndex];
						}
						break;
				}
			}
		}

		void ShowAnimationElementsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "SPR|1":
					case "ANI|1":
						TreeView1_AfterSelect(this, new TreeViewEventArgs(treeView.SelectedNode));
						break;
				}
			}
		}

		#region Undo / redo

		void SplitContainer_Panel2_MouseUp(object sender, MouseEventArgs e)
		{
			UndoRedo(e);
		}

		void PictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			UndoRedo(e);
		}

		private void TreeView_MouseUp(object sender, MouseEventArgs e)
		{
			UndoRedo(e);
		}

		int ScrollBarPosition
		{
			get
			{
				return splitContainer.Panel2.VerticalScroll.Value;
			}

			set
			{
				splitContainer.Panel2.VerticalScroll.Value = Math.Min(value, splitContainer.Panel2.VerticalScroll.Maximum);
				splitContainer.Panel2.PerformLayout();
			}
		}

		void UndoRedo(MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.XButton1: //back
					if (undo.Count > 0)
					{
						redo.Push((treeView.SelectedNode, ScrollBarPosition));
						ignoreSelectedNodeEvent = true;
						(treeView.SelectedNode, ScrollBarPosition) = undo.Pop();
						ignoreSelectedNodeEvent = false;

					}
					break;

				case MouseButtons.XButton2: //forward
					if (redo.Count > 0)
					{
						undo.Push((treeView.SelectedNode, ScrollBarPosition));
						ignoreSelectedNodeEvent = true;
						(treeView.SelectedNode, ScrollBarPosition) = redo.Pop();
						ignoreSelectedNodeEvent = false;
					}
					break;
			}
		}

		private void TreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (!ignoreSelectedNodeEvent)
			{
				var node = treeView.SelectedNode;
				if (node != null)
				{
					undo.Push((node, ScrollBarPosition));
					redo.Clear();
				}
			}
		}

		#endregion

		void SetGrid(int size)
		{
			if (gridSize != size)
			{
				gridSize = size;
				x16ToolStripMenuItem.Checked = size == 0;
				x32ToolStripMenuItem.Checked = size == 1;
				x64ToolStripMenuItem.Checked = size == 2;

				SpriteViewer.TileSize = new int[] { 16, 32, 64 }[size];
				ResizeGrid();
			}
		}

		void X16ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetGrid(0);
		}

		void X32ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetGrid(1);
		}

		void X64ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetGrid(2);
		}
	}
}
