using AnimatedGif;
using System;
using System.Collections.Generic;
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
			imageList.Images.Add(new Bitmap(16, 16)); //empty
			imageList.Images.Add(Icons.Extract("shell32.dll", -4)); //closed folder
			imageList.Images.Add(Icons.Extract("shell32.dll", -0)); //file
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
				if (Directory.Exists(subfolder) && Directory.EnumerateFiles(subfolder, "*.TAB").Any() && Directory.EnumerateFiles(subfolder, "*.DAT").Any())
				{
					result = subfolder;
					return true;
				}
			}

			result = folder;
			return false;
		}

		#region Treeview

		public TreeNode GetNode(string key, string text, int imageIndex)
		{
			return GetNode(key, text, treeView.Nodes, imageIndex);
		}

		public static TreeNode GetNode(string key, string text, TreeNodeCollection nodes, int imageIndex)
		{
			var index = nodes.IndexOfKey(key);
			if (index == -1)
			{
				var node = nodes.Add(key, text);
				node.StateImageIndex = imageIndex;
				return node;
			}
			else
			{
				return nodes[index];
			}
		}

		void LoadTreeView()
		{
			var allfiles = Directory.EnumerateFiles(gameFolder)
				.Where(x => !SpriteViewer.IsRNCCompressed(x))
				.Select(Path.GetFileName)
				.ToHashSet();

			string[] anim = ["MFRA-0.ANI", "MELE-0.ANI", "MSTA-0.ANI", "MSPR-0.DAT"];
			string[] animUS = ["MUSFRA.ANI", "MUSELE.ANI", "MUSSTA.ANI", "MUS.DAT"];
			string[] animDEMO = ["MDFRA-0.ANI", "MDELE-0.ANI", "MDSTA-0.ANI", "MDSPR-0.DAT"];

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
								GetNode("BKG", "BACKGROUNDS", 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension + " (320x200)").SetStateImageIndex(2);
							}
							if (filesize == 65536)
							{
								GetNode("BKG", "BACKGROUNDS", 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension + " (256x256)").SetStateImageIndex(2);
							}
							break;

						case ".TAB":
							if (allfiles.Contains(fileNameWithoutExtension + ".DAT"))
							{
								TreeNode node = GetNode("SPR", "SPRITES", 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension).SetStateImageIndex(2);
								node.Nodes.Add("");
							}
							break;
					}
				}
				if ((fileNameWithoutExtension == "MSTA-0" && anim.All(allfiles.Contains))
					|| (fileNameWithoutExtension == "MUSSTA" && animUS.All(allfiles.Contains))
					|| (fileNameWithoutExtension == "MDSTA-0" && animDEMO.All(allfiles.Contains)))
				{
					var node = GetNode("ANI", "ANIMATIONS", 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension).SetStateImageIndex(2);
					node.Nodes.Add("");
				}

				if (fileNameWithoutExtension.StartsWith("LANG"))
				{
					var node = GetNode("LAN", "LANGUAGE", 1).Nodes.Add(fileName, $"{fileNameWithoutExtension}").SetStateImageIndex(2);
					node.Nodes.Add("");
				}

				if (fileNameWithoutExtension == "INTRO" || fileNameWithoutExtension == "WINGAME" || fileNameWithoutExtension == "RIDEANI")
				{
					GetNode("FLI", "MOVIES", 1).Nodes.Add(fileName, $"{(fileNameWithoutExtension == "RIDEANI" ? fileName : fileNameWithoutExtension)} ({MoviePlayer.GetDuration(Path.Combine(gameFolder, fileName)):m\\:ss})").SetStateImageIndex(2);
				}

				if ((fileName == "TAKOVER.ANM" || fileName == "BUSTED.ANM")
					&& allfiles.Contains(fileNameWithoutExtension + ".DAT"))
				{
					GetNode("FLI", "MOVIES", 1).Nodes.Add(fileName, $"{fileNameWithoutExtension} ({MoviePlayer.GetDuration(Path.Combine(gameFolder, fileName), 0):m\\:ss})").SetStateImageIndex(2);
				}

				if (fileName.StartsWith("SND") && Path.GetExtension(fileName) == ".TAB")
				{
					var lastChar = int.Parse(fileNameWithoutExtension[^1..]);
					var node = GetNode("SND", "SOUNDS", 1);
					node = GetNode("SND" + lastChar, new[] { "11KHZ 8-BIT", "22KHZ 8-BIT", "44KHZ 16-BIT" }[lastChar], node.Nodes, 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension).SetStateImageIndex(2);
					node.Nodes.Add("");
				}

				if (fileName.StartsWith("MUSIC") && Path.GetExtension(fileName) == ".TAB")
				{
					int lastChar = int.Parse(fileNameWithoutExtension[^1..]);
					var node = GetNode("MUS", "MUSIC", 1);
					node = GetNode("MUS" + lastChar, new[] { "ADLIB", "ROLAND", "GENERAL MIDI" }[lastChar], node.Nodes, 1).Nodes.Add(fileNameWithoutExtension, fileNameWithoutExtension).SetStateImageIndex(2);
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

				case "LAN|1":
					if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
					{
						node.Nodes.Clear();
						var sections = Package.LoadText(Path.Combine(gameFolder, node.Name));
						foreach (var section in sections)
						{
							var subNode = node.Nodes.Add("LAN", $"SECTION {node.Nodes.Count}");
							subNode.Tag = section;
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
						foreach (SoundTab snd in Package.SoundTabs.OrderBy(x => x.Name))
						{
							if (!string.IsNullOrEmpty(snd.Name) && snd.Name != "NULL.RAW")
							{
								soundBytes = new byte[snd.Length];
								Array.Copy(Package.SoundData, snd.Offset, soundBytes, 0, snd.Length);

								TimeSpan duration;
								if (root.Name == "SND")
								{
									var lastChar = int.Parse(node.Name[^1..]);
									duration = AudioPlayer.GetDuration(soundBytes, (new ushort[] { 8, 8, 16 })[lastChar], new uint[] { 11025, 22050, 44100 }[lastChar]);
								}
								else
								{
									duration = MusicPlayer.GetDuration(soundBytes);
								}

								int seconds = (int)Math.Round(duration.TotalSeconds);
								node.Nodes.Add(snd.Name, $"{snd.Name} ({TimeSpan.FromSeconds(seconds):m\\:ss})");
							}
							index++;
						}
					}
					break;
			}
		}

		void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ClearBitmap();
			timer.Stop();
			AudioPlayer.Stop();
			MusicPlayer.Stop();

			var node = e.Node;
			var root = GetRoot(node);
			var rootName = $"{root.Name}|{node.Level}";
			exportToPNGToolStripMenuItem.Enabled = (node.Nodes.Count == 0 && rootName != "LAN|2") || rootName == "LAN|1";

			switch (rootName)
			{
				case "BKG|1":
					{
						Package.LoadBackground(gameFolder, node.Name);
						DisplaySprite(Package.SpriteTabs[0], Package.SpriteData, Package.Palette);
						SetView(ViewType.SPRITE);
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

						trackBar.Value = 0;
						trackBar.Maximum = moviePlayer.FrameCount;
						SetView(ViewType.MOVIE);
					}
					break;

				case "LAN|1":
					TreeView1_BeforeExpand(sender, new TreeViewCancelEventArgs(e.Node, false, e.Action));
					textBox.Text = string.Join("\r\n\r\n",
						 node.Nodes.Cast<TreeNode>().Select((x, i) => string.Join("\r\n", new string[] { $"[SECTION {i}]" }
							.Concat(((string[])x.Tag).Select((x, i) => $"{i}: {x}")))));
					SetView(ViewType.TEXT);
					break;

				case "LAN|2":
					textBox.Text = string.Join("\r\n", ((string[])node.Tag)
						.Select((x, i) => $"{i}: {x}"));
					SetView(ViewType.TEXT);
					break;

				case "SPR|1":
					{
						Package.LoadSprite(gameFolder, node.Name);
						DrawGrid();
						SetView(ViewType.SPRITE);
					}
					break;

				case "SPR|2":
					{
						Package.LoadSprite(gameFolder, node.Parent.Name);
						var tab = Package.SpriteTabs[int.Parse(node.Name)];
						var pixelData = Package.GetPixels(tab, Package.SpriteData);
						DisplaySprite(tab, pixelData, Package.Palette);
						SetView(ViewType.SPRITE);
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
						SetView(ViewType.SPRITE);
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
						SetView(ViewType.SPRITE);
					}
					break;

				case "SND|3":
					{
						string filename = node.Parent.Name;
						Package.LoadSound(gameFolder, filename);
						SoundTab tab = Package.SoundTabs.First(x => x.Name == node.Name);
						var lastChar = int.Parse(filename[^1..]);
						soundBytes = AudioPlayer.CreateWave(Package.SoundData, tab.Offset, tab.Length - 16, (new ushort[] { 8, 8, 16 })[lastChar], new uint[] { 11025, 22050, 44100 }[lastChar], 1);
						AudioPlayer.Play(soundBytes, loopSoundToolStripMenuItem.Checked);
						SetView(ViewType.NONE);
					}
					break;

				case "MUS|3":
					{
						string filename = node.Parent.Name;
						Package.LoadSound(gameFolder, filename);
						SoundTab tab = Package.SoundTabs.First(x => x.Name == node.Name);
						soundBytes = new byte[tab.Length];
						Array.Copy(Package.SoundData, tab.Offset, soundBytes, 0, tab.Length);

						MusicPlayer.Play(soundBytes);
						MusicPlayer.Loop = loopSoundToolStripMenuItem.Checked;

						timer.Interval = 60; //for trackbar update
						timer.Start();

						trackBar.Value = 0;
						trackBar.Maximum = (int)MusicPlayer.TotalTime.TotalMilliseconds;
						SetView(ViewType.MUSIC);
					}
					break;

				default:
					SetView(ViewType.NONE);
					break;
			}
		}

		void ExportToPNGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			var root = GetRoot(node);
			var rootName = $"{root.Name}|{node.Level}";
			if ((node != null && node.Nodes.Count == 0) || rootName == "LAN|1")
			{
				switch (rootName)
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
								FileName = $"{node.Name.Replace(".", "_")}.mp4",
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

					case "LAN|1":
						{
							SaveFileDialog saveDialog = new()
							{
								FileName = $"{node.Text}.txt",
								Filter = "TXT (*.txt)|*.txt"
							};

							if (saveDialog.ShowDialog() == DialogResult.OK)
							{
								var sections = Package.LoadText(Path.Combine(gameFolder, node.Name));
								File.AppendAllText(saveDialog.FileName, textBox.Text);
							}
						}
						break;
				}
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
		}

		void SetView(ViewType view)
		{
			pictureBox.Visible = view == ViewType.SPRITE || view == ViewType.MOVIE;
			panelTrackBar.Visible = view == ViewType.MOVIE || view == ViewType.MUSIC;
			tableLayoutPanel1.Dock = view == ViewType.MUSIC || view == ViewType.TEXT ? DockStyle.Fill : DockStyle.None;
			textBox.Visible = view == ViewType.TEXT;
		}

		void SetBitmapSize(int width, int height)
		{
			if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
			{
				bitmap?.Dispose();

				bitmap = new DirectBitmap(width, height);
				pictureBox.Width = width * zoom;
				pictureBox.Height = height * zoom;
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
				var center = SpriteViewer.GetSpriteCenter(tab.Width, tab.Height, SpriteViewer.TileSize, SpriteViewer.TileSize);
				int posX = index % tilesPerRow * SpriteViewer.TileSize + center.X;
				int posY = index / tilesPerRow * SpriteViewer.TileSize + center.Y;

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
			const int maxHeight = 32768; //max picture box height that can handle mouse click events
			int maxRows = maxHeight / (SpriteViewer.TileSize * zoom);
			int minTilesPerRow = (tilesCount + maxRows - 1) / maxRows; //round up

			int tilePerRow = Math.Max(minTilesPerRow, splitContainer.Panel2.ClientRectangle.Width / (SpriteViewer.TileSize * zoom));
			return tilePerRow;
		}

		void PrepareGrid(int gridItems)
		{
			tilesCount = gridItems;
			tilesPerRow = GetTilePerRow();

			if (gridItems <= tilesPerRow)
			{
				SetBitmapSize(gridItems * SpriteViewer.TileSize, 1 * SpriteViewer.TileSize);
			}
			else
			{
				int tilesHeight = (gridItems + tilesPerRow - 1) / tilesPerRow; //round up
				SetBitmapSize(tilesPerRow * SpriteViewer.TileSize, tilesHeight * SpriteViewer.TileSize);
			}

			Array.Clear(bitmap.Bits, 0, bitmap.Bits.Length);
		}

		void ResizeGrid(bool force = false)
		{
			var node = treeView.SelectedNode;
			if (node != null && node.Level == 1
					&& (node.Parent.Name == "ANI" || node.Parent.Name == "SPR")
					&& (GetTilePerRow() != tilesPerRow || force))
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

				ResizeGrid(true);

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
						{
							if (!moviePlayer.NextFrame())
							{
								trackBar.Maximum = moviePlayer.CurrentFrame; //FrameCount might be more than reality
								if (!loopSoundToolStripMenuItem.Checked)
								{
									timer.Stop();
								}
							}
							moviePlayer.RenderBitmap(bitmap);

							UpdateTrackBarText();
							trackBar.Value = Math.Min(moviePlayer.CurrentFrame, moviePlayer.FrameCount);
						}
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

					case "MUS|3":
						{
							UpdateTrackBarText();
							trackBar.Value = Math.Min((int)MusicPlayer.CurrentTime.TotalMilliseconds, (int)MusicPlayer.TotalTime.TotalMilliseconds);
						}
						break;
				}

				pictureBox.Invalidate();
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
							var center = SpriteViewer.GetSpriteCenter(anim.Width, anim.Height, SpriteViewer.TileSize, SpriteViewer.TileSize);
							int posX = index % tilesPerRow * SpriteViewer.TileSize + center.X;
							int posY = index / tilesPerRow * SpriteViewer.TileSize + center.Y;
							SpriteViewer.ClearSprite(bitmap, posX, posY, anim.Width, anim.Height);

							if (currentFrame.Width > 0 && currentFrame.Height > 0)
							{
								DrawAnimatedSprite();

								void DrawAnimatedSprite()
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
					while (moviePlayer.NextFrame())
					{
						moviePlayer.RenderBitmap(bitmap);
						var byteArray = new byte[bitmap.Bits.Length * sizeof(uint)];
						Buffer.BlockCopy(bitmap.Bits, 0, byteArray, 0, byteArray.Length);
						input.Write(byteArray, 0, byteArray.Length);
						if (stopwatch.Elapsed > TimeSpan.FromSeconds(0.5))
						{
							stopwatch.Restart();
							trackBar.Value = Math.Min(moviePlayer.CurrentFrame, moviePlayer.FrameCount);
							UpdateTrackBarText();
							labCurrentTime.Update();
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
						Package.Clear();
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

		void LoopSoundToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "SND|3":
						if (loopSoundToolStripMenuItem.Checked)
						{
							AudioPlayer.Play(soundBytes, loopSoundToolStripMenuItem.Checked);
						}
						else
						{
							AudioPlayer.Stop();
						}
						break;

					case "MUS|3":
						MusicPlayer.Loop = loopSoundToolStripMenuItem.Checked;
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

				SpriteViewer.TileSize = new[] { 16, 32, 64 }[size];
				ResizeGrid(true);
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

		#region TrackBar

		void TrackBar1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				timer.Stop();
				MusicPlayer.Mute();
				SetTrackPositionFromMouse(e.X);
			}
		}

		void TrackBar1_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				SetTrackPositionFromMouse(e.X);
			}
		}

		void TrackBar1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var node = treeView.SelectedNode;
				if (node != null)
				{
					var root = GetRoot(node);
					switch ($"{root.Name}|{node.Level}")
					{
						case "FLI|1":
							if (loopSoundToolStripMenuItem.Checked || moviePlayer.CurrentFrame < moviePlayer.FrameCount)
							{
								timer.Start();
							}
							break;

						case "MUS|3":
							MusicPlayer.Play();
							timer.Start();
							break;
					}
				}
			}
		}

		private void UpdateTrackBarText()
		{
			var node = treeView.SelectedNode;
			var root = GetRoot(node);
			switch ($"{root.Name}|{node.Level}")
			{
				case "FLI|1":
					labCurrentTime.Text = $"{TimeSpan.FromMilliseconds(moviePlayer.CurrentFrame * 1000 / 15):mm\\:ss}";
					labTotalTime.Text = $"{TimeSpan.FromMilliseconds(moviePlayer.FrameCount * 1000 / 15):mm\\:ss}";
					break;

				case "MUS|3":
					labCurrentTime.Text = $"{MusicPlayer.CurrentTime:mm\\:ss}";
					labTotalTime.Text = $"{MusicPlayer.TotalTime:mm\\:ss}";
					break;
			}
		}

		void SetTrackPositionFromMouse(int mouse)
		{
			const int border = 14;
			double ratio = (double)(mouse - border) / (trackBar.Width - border * 2);
			int newValue = (int)Math.Round(ratio * trackBar.Maximum);

			newValue = Math.Max(trackBar.Minimum, Math.Min(trackBar.Maximum, newValue));
			trackBar.Value = newValue;

			var node = treeView.SelectedNode;
			if (node != null)
			{
				var root = GetRoot(node);
				switch ($"{root.Name}|{node.Level}")
				{
					case "FLI|1":
						moviePlayer.Seek(newValue);
						moviePlayer.RenderBitmap(bitmap);
						UpdateTrackBarText();
						pictureBox.Invalidate();
						break;

					case "MUS|3":
						MusicPlayer.Seek(TimeSpan.FromMilliseconds(trackBar.Value));
						UpdateTrackBarText();
						break;
				}
			}
		}

		#endregion
	}
}
