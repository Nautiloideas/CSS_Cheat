using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CSS_Cheat
{
    public static class WindowFinder
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        public static IntPtr FindWindowByProcess(uint targetProcessId, string windowTitle)
        {
            IntPtr hWnd = GetDesktopWindow();
            IntPtr nextHWnd = GetWindow(hWnd, 5); // GW_CHILD

            while (nextHWnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(nextHWnd, out int processId);
                if (processId == targetProcessId && (string.IsNullOrEmpty(windowTitle) || GetWindowTitle(nextHWnd).Contains(windowTitle)))
                {
                    return nextHWnd;
                }
                nextHWnd = GetWindow(nextHWnd, 2); // GW_HWNDNEXT
            }

            return IntPtr.Zero;
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}
