using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace ThemeParkResourceViewer
{
	public class PictureBoxWithInterpolationMode : PictureBox
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.NearestNeighbor;

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.InterpolationMode = InterpolationMode;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			base.OnPaint(e);
		}

		const int WM_MOUSEWHEEL = 0x020A;
		const int MK_SHIFT = 0x0004;
		const int MK_CONTROL = 0x0008;
		public new event MouseEventHandler MouseWheel;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEWHEEL && (LOWORD((uint)m.WParam) == MK_CONTROL || LOWORD((uint)m.WParam) == MK_SHIFT))
			{
				int delta = SignedHIWORD((uint)m.WParam);
				MouseWheel(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0, delta));
				return;
			}

			base.WndProc(ref m);
		}

		static int SignedHIWORD(uint n)
		{
			return unchecked((short)HIWORD(n));
		}

		static uint HIWORD(uint n)
		{
			return (n >> 16) & 0xFFFF;
		}

		static uint LOWORD(uint n)
		{
			return n & 0xffff;
		}
	}
}
