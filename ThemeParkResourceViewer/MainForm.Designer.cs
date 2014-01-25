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
			this.components = new System.ComponentModel.Container();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.setGameLocationFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportToPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.zoom4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gridSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.x16ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.x32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.x64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.useBilinearFilteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showSpritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loopSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.treeView = new System.Windows.Forms.TreeView();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.pictureBox = new ThemeParkResourceViewer.PictureBoxWithInterpolationMode();
			this.menuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.aboutToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(624, 28);
			this.menuStrip.TabIndex = 2;
			this.menuStrip.Text = "menuStrip";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setGameLocationFolderToolStripMenuItem,
            this.exportToPNGToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// setGameLocationFolderToolStripMenuItem
			// 
			this.setGameLocationFolderToolStripMenuItem.Name = "setGameLocationFolderToolStripMenuItem";
			this.setGameLocationFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
			this.setGameLocationFolderToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			this.setGameLocationFolderToolStripMenuItem.Text = "Set game location folder...";
			this.setGameLocationFolderToolStripMenuItem.Click += new System.EventHandler(this.SetGameLocationFolderToolStripMenuItem_Click);
			// 
			// exportToPNGToolStripMenuItem
			// 
			this.exportToPNGToolStripMenuItem.Enabled = false;
			this.exportToPNGToolStripMenuItem.Name = "exportToPNGToolStripMenuItem";
			this.exportToPNGToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.exportToPNGToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			this.exportToPNGToolStripMenuItem.Text = "Export to...";
			this.exportToPNGToolStripMenuItem.Click += new System.EventHandler(this.ExportToPNGToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(301, 24);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToolStripMenuItem,
            this.gridSizeToolStripMenuItem,
            this.showSpritesToolStripMenuItem,
            this.useBilinearFilteringToolStripMenuItem,
            this.loopSoundToolStripMenuItem});
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
			this.settingsToolStripMenuItem.Text = "Settings";
			// 
			// zoomToolStripMenuItem
			// 
			this.zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoom1ToolStripMenuItem,
            this.zoom2ToolStripMenuItem,
            this.zoom3ToolStripMenuItem,
            this.zoom4ToolStripMenuItem});
			this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
			this.zoomToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			this.zoomToolStripMenuItem.Text = "Zoom";
			// 
			// zoom1ToolStripMenuItem
			// 
			this.zoom1ToolStripMenuItem.Name = "zoom1ToolStripMenuItem";
			this.zoom1ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			this.zoom1ToolStripMenuItem.Text = "1x";
			this.zoom1ToolStripMenuItem.Click += new System.EventHandler(this.Zoom1ToolStripMenuItem_Click);
			// 
			// zoom2ToolStripMenuItem
			// 
			this.zoom2ToolStripMenuItem.Name = "zoom2ToolStripMenuItem";
			this.zoom2ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			this.zoom2ToolStripMenuItem.Text = "2x";
			this.zoom2ToolStripMenuItem.Click += new System.EventHandler(this.Zoom2ToolStripMenuItem_Click);
			// 
			// zoom3ToolStripMenuItem
			// 
			this.zoom3ToolStripMenuItem.Name = "zoom3ToolStripMenuItem";
			this.zoom3ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			this.zoom3ToolStripMenuItem.Text = "3x";
			this.zoom3ToolStripMenuItem.Click += new System.EventHandler(this.Zoom3ToolStripMenuItem_Click);
			// 
			// zoom4ToolStripMenuItem
			// 
			this.zoom4ToolStripMenuItem.Name = "zoom4ToolStripMenuItem";
			this.zoom4ToolStripMenuItem.Size = new System.Drawing.Size(93, 24);
			this.zoom4ToolStripMenuItem.Text = "4x";
			this.zoom4ToolStripMenuItem.Click += new System.EventHandler(this.Zoom4ToolStripMenuItem_Click);
			// 
			// gridSizeToolStripMenuItem
			// 
			this.gridSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.x16ToolStripMenuItem,
            this.x32ToolStripMenuItem,
            this.x64ToolStripMenuItem});
			this.gridSizeToolStripMenuItem.Name = "gridSizeToolStripMenuItem";
			this.gridSizeToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			this.gridSizeToolStripMenuItem.Text = "Grid size";
			// 
			// x16ToolStripMenuItem
			// 
			this.x16ToolStripMenuItem.CheckOnClick = true;
			this.x16ToolStripMenuItem.Name = "x16ToolStripMenuItem";
			this.x16ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			this.x16ToolStripMenuItem.Text = "16 x 16";
			this.x16ToolStripMenuItem.Click += new System.EventHandler(this.x16ToolStripMenuItem_Click);
			// 
			// x32ToolStripMenuItem
			// 
			this.x32ToolStripMenuItem.Name = "x32ToolStripMenuItem";
			this.x32ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			this.x32ToolStripMenuItem.Text = "32 x 32";
			this.x32ToolStripMenuItem.Click += new System.EventHandler(this.x32ToolStripMenuItem_Click);
			// 
			// x64ToolStripMenuItem
			// 
			this.x64ToolStripMenuItem.Name = "x64ToolStripMenuItem";
			this.x64ToolStripMenuItem.Size = new System.Drawing.Size(125, 24);
			this.x64ToolStripMenuItem.Text = "64 x 64";
			this.x64ToolStripMenuItem.Click += new System.EventHandler(this.x64ToolStripMenuItem_Click);
			// 
			// useBilinearFilteringToolStripMenuItem
			// 
			this.useBilinearFilteringToolStripMenuItem.CheckOnClick = true;
			this.useBilinearFilteringToolStripMenuItem.Name = "useBilinearFilteringToolStripMenuItem";
			this.useBilinearFilteringToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			this.useBilinearFilteringToolStripMenuItem.Text = "Use bilinear filtering";
			this.useBilinearFilteringToolStripMenuItem.Click += new System.EventHandler(this.UseBilinearFilteringToolStripMenuItem_Click);
			// 
			// showSpritesToolStripMenuItem
			// 
			this.showSpritesToolStripMenuItem.CheckOnClick = true;
			this.showSpritesToolStripMenuItem.Name = "showSpritesToolStripMenuItem";
			this.showSpritesToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			this.showSpritesToolStripMenuItem.Text = "Show sprites boundaries";
			this.showSpritesToolStripMenuItem.Click += new System.EventHandler(this.ShowAnimationElementsToolStripMenuItem_Click);
			// 
			// loopSoundToolStripMenuItem
			// 
			this.loopSoundToolStripMenuItem.CheckOnClick = true;
			this.loopSoundToolStripMenuItem.Name = "loopSoundToolStripMenuItem";
			this.loopSoundToolStripMenuItem.Size = new System.Drawing.Size(239, 24);
			this.loopSoundToolStripMenuItem.Text = "Loop sound";
			this.loopSoundToolStripMenuItem.Click += new System.EventHandler(this.LoopSoundToolStripMenuItem_Click);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(62, 24);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
			// 
			// treeView
			// 
			this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(183, 409);
			this.treeView.TabIndex = 3;
			this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView1_BeforeExpand);
			this.treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_BeforeSelect);
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1_AfterSelect);
			this.treeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseUp);
			// 
			// timer
			// 
			this.timer.Tick += new System.EventHandler(this.Timer1_Tick);
			// 
			// splitContainer
			// 
			this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = new System.Drawing.Point(0, 28);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.treeView);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.AutoScroll = true;
			this.splitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer.Panel2.Controls.Add(this.pictureBox);
			this.splitContainer.Panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SplitContainer_Panel2_MouseUp);
			this.splitContainer.Panel2.Resize += new System.EventHandler(this.SplitContainer_Panel2_Resize);
			this.splitContainer.Size = new System.Drawing.Size(624, 413);
			this.splitContainer.SplitterDistance = 187;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 4;
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.Color.Magenta;
			this.pictureBox.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			this.pictureBox.Location = new System.Drawing.Point(0, 0);
			this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(64, 64);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			this.pictureBox.Visible = false;
			this.pictureBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseWheel);
			this.pictureBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDoubleClick);
			this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseUp);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "MainForm";
			this.Text = "ThemeParkResourceViewer";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseWheel);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem loopSoundToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem useBilinearFilteringToolStripMenuItem;
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
	}
}
