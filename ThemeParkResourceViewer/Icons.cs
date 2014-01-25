using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ThemeParkResourceViewer;

public static class Icons
{
    [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);
    
    public static Icon Extract(string file, int index)
    {
        ExtractIconEx(file, index, out var _, out var small, 1);
        if (small == IntPtr.Zero)
        {
            return Icon.FromHandle(new Bitmap(16, 16).GetHicon());
        }
        return Icon.FromHandle(small);
    }
}