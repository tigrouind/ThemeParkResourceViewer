namespace ThemeParkResourceViewer
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			menuStrip = new System.Windows.Forms.MenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			setGameLocationFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			setFFmpegLocationPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exportToPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoom1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoom2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoom3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			zoom4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			gridSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			x16ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			x32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			x64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			showSpritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			loopSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			treeView = new System.Windows.Forms.TreeView();
			timer = new System.Windows.Forms.Timer(components);
			splitContainer = new System.Windows.Forms.SplitContainer();
			pictureBox = new PictureBoxWithInterpolationMode();
			menuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, settingsToolStripMenuItem, aboutToolStripMenuItem });
			menuStrip.Location = new System.Drawing.Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Padding = new System.Windows.Forms.Padding(8, 3, 0, 3);
			menuStrip.Size = new System.Drawing.Size(832, 30);
			menuStrip.TabIndex = 2;
			menuStrip.Text = "menuStrip";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { setGameLocationFolderToolStripMenuItem, setFFmpegLocationPathToolStripMenuItem, exportToPNGToolStripMenuItem, exitToolStripMenuItem });
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
			fileToolStripMenuItem.Text = "File";
			// 
			// setGameLocationFolderToolStripMenuItem
			// 
			setGameLocationFolderToolStripMenuItem.Name = "setGameLocationFolderToolStripMenuItem";
			setGameLocationFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L;
			setGameLocationFolderToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			setGameLocationFolderToolStripMenuItem.Text = "Set game location folder...";
			setGameLocationFolderToolStripMenuItem.Click += SetGameLocationFolderToolStripMenuItem_Click;
			// 
			// setFFmpegLocationPathToolStripMenuItem
			// 
			setFFmpegLocationPathToolStripMenuItem.Name = "setFFmpegLocationPathToolStripMenuItem";
			setFFmpegLocationPathToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			setFFmpegLocationPathToolStripMenuItem.Text = "Set FFmpeg location path...";
			setFFmpegLocationPathToolStripMenuItem.Click += SetFFmpegLocationPathToolStripMenuItem_Click;
			// 
			// exportToPNGToolStripMenuItem
			// 
			exportToPNGToolStripMenuItem.Enabled = false;
			exportToPNGToolStripMenuItem.Name = "exportToPNGToolStripMenuItem";
			exportToPNGToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
			exportToPNGToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			exportToPNGToolStripMenuItem.Text = "Export to...";
			exportToPNGToolStripMenuItem.Click += ExportToPNGToolStripMenuItem_Click;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
			// 
			// settingsToolStripMenuItem
			// 
			settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { zoomToolStripMenuItem, gridSizeToolStripMenuItem, showSpritesToolStripMenuItem, loopSoundToolStripMenuItem });
			settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			settingsToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
			settingsToolStripMenuItem.Text = "Settings";
			// 
			// zoomToolStripMenuItem
			// 
			zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { zoom1ToolStripMenuItem, zoom2ToolStripMenuItem, zoom3ToolStripMenuItem, zoom4ToolStripMenuItem });
			zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
			zoomToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			zoomToolStripMenuItem.Text = "Zoom";
			// 
			// zoom1ToolStripMenuItem
			// 
			zoom1ToolStripMenuItem.Name = "zoom1ToolStripMenuItem";
			zoom1ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			zoom1ToolStripMenuItem.Text = "1x";
			zoom1ToolStripMenuItem.Click += Zoom1ToolStripMenuItem_Click;
			// 
			// zoom2ToolStripMenuItem
			// 
			zoom2ToolStripMenuItem.Name = "zoom2ToolStripMenuItem";
			zoom2ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			zoom2ToolStripMenuItem.Text = "2x";
			zoom2ToolStripMenuItem.Click += Zoom2ToolStripMenuItem_Click;
			// 
			// zoom3ToolStripMenuItem
			// 
			zoom3ToolStripMenuItem.Name = "zoom3ToolStripMenuItem";
			zoom3ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			zoom3ToolStripMenuItem.Text = "3x";
			zoom3ToolStripMenuItem.Click += Zoom3ToolStripMenuItem_Click;
			// 
			// zoom4ToolStripMenuItem
			// 
			zoom4ToolStripMenuItem.Name = "zoom4ToolStripMenuItem";
			zoom4ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			zoom4ToolStripMenuItem.Text = "4x";
			zoom4ToolStripMenuItem.Click += Zoom4ToolStripMenuItem_Click;
			// 
			// gridSizeToolStripMenuItem
			// 
			gridSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { x16ToolStripMenuItem, x32ToolStripMenuItem, x64ToolStripMenuItem });
			gridSizeToolStripMenuItem.Name = "gridSizeToolStripMenuItem";
			gridSizeToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			gridSizeToolStripMenuItem.Text = "Grid size";
			// 
			// x16ToolStripMenuItem
			// 
			x16ToolStripMenuItem.CheckOnClick = true;
			x16ToolStripMenuItem.Name = "x16ToolStripMenuItem";
			x16ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			x16ToolStripMenuItem.Text = "16 x 16";
			x16ToolStripMenuItem.Click += X16ToolStripMenuItem_Click;
			// 
			// x32ToolStripMenuItem
			// 
			x32ToolStripMenuItem.Name = "x32ToolStripMenuItem";
			x32ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			x32ToolStripMenuItem.Text = "32 x 32";
			x32ToolStripMenuItem.Click += X32ToolStripMenuItem_Click;
			// 
			// x64ToolStripMenuItem
			// 
			x64ToolStripMenuItem.Name = "x64ToolStripMenuItem";
			x64ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			x64ToolStripMenuItem.Text = "64 x 64";
			x64ToolStripMenuItem.Click += X64ToolStripMenuItem_Click;
			// 
			// showSpritesToolStripMenuItem
			// 
			showSpritesToolStripMenuItem.CheckOnClick = true;
			showSpritesToolStripMenuItem.Name = "showSpritesToolStripMenuItem";
			showSpritesToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			showSpritesToolStripMenuItem.Text = "Show sprites boundaries";
			showSpritesToolStripMenuItem.Click += ShowAnimationElementsToolStripMenuItem_Click;
			// 
			// loopSoundToolStripMenuItem
			// 
			loopSoundToolStripMenuItem.CheckOnClick = true;
			loopSoundToolStripMenuItem.Name = "loopSoundToolStripMenuItem";
			loopSoundToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			loopSoundToolStripMenuItem.Text = "Loop sound";
			loopSoundToolStripMenuItem.Click += LoopSoundToolStripMenuItem_Click;
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new System.Drawing.Size(62, 24);
			aboutToolStripMenuItem.Text = "About";
			aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
			// 
			// treeView
			// 
			treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			treeView.Location = new System.Drawing.Point(0, 0);
			treeView.Margin = new System.Windows.Forms.Padding(4, 5, 0, 5);
			treeView.Name = "treeView";
			treeView.Size = new System.Drawing.Size(245, 644);
			treeView.TabIndex = 3;
			treeView.BeforeExpand += TreeView1_BeforeExpand;
			treeView.BeforeSelect += TreeView_BeforeSelect;
			treeView.AfterSelect += TreeView1_AfterSelect;
			treeView.MouseUp += TreeView_MouseUp;
			// 
			// timer
			// 
			timer.Tick += Timer1_Tick;
			// 
			// splitContainer
			// 
			splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			splitContainer.Location = new System.Drawing.Point(0, 30);
			splitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(treeView);
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.AutoScroll = true;
			splitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
			splitContainer.Panel2.Controls.Add(pictureBox);
			splitContainer.Panel2.MouseUp += SplitContainer_Panel2_MouseUp;
			splitContainer.Panel2.Resize += SplitContainer_Panel2_Resize;
			splitContainer.Size = new System.Drawing.Size(832, 648);
			splitContainer.SplitterDistance = 249;
			splitContainer.SplitterWidth = 7;
			splitContainer.TabIndex = 4;
			// 
			// pictureBox
			// 
			pictureBox.BackColor = System.Drawing.Color.Magenta;
			pictureBox.Location = new System.Drawing.Point(0, 0);
			pictureBox.Margin = new System.Windows.Forms.Padding(0);
			pictureBox.Name = "pictureBox";
			pictureBox.Size = new System.Drawing.Size(85, 98);
			pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			pictureBox.TabIndex = 1;
			pictureBox.TabStop = false;
			pictureBox.Visible = false;
			pictureBox.MouseWheel += MainForm_MouseWheel;
			pictureBox.MouseDoubleClick += PictureBox_MouseDoubleClick;
			pictureBox.MouseUp += PictureBox_MouseUp;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(832, 678);
			Controls.Add(splitContainer);
			Controls.Add(menuStrip);
			MainMenuStrip = menuStrip;
			Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			Name = "MainForm";
			Text = "ThemeParkResourceViewer";
			Load += MainForm_Load;
			MouseWheel += MainForm_MouseWheel;
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem loopSoundToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.ToolStripMenuItem showSpritesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setGameLocationFolderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToPNGToolStripMenuItem;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.ToolStripMenuItem zoom4ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom3ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom2ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoom1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip;
		private PictureBoxWithInterpolationMode pictureBox;
		private System.Windows.Forms.ToolStripMenuItem gridSizeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem x32ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem x64ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem x16ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setFFmpegLocationPathToolStripMenuItem;
	}
}
