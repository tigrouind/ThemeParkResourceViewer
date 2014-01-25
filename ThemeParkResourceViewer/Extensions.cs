using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThemeParkResourceViewer
{
	public static class Extensions
	{
		public static TreeNode SetStateImageIndex(this TreeNode node, int imageIndex)
		{
			node.StateImageIndex = imageIndex;
			return node;
		}
	}
}
