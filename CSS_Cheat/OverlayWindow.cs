using CSS_Cheat;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CSS_Cheat
{
    public class OverlayWindow : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;
        const uint LWA_COLORKEY = 0x1;
        const uint LWA_ALPHA = 0x2;

        private IntPtr targetWindowHandle;
        private System.Windows.Forms.Timer updateTimer;

        private IntPtr processHandle;
        private IntPtr clientModuleBase;
        private IntPtr serverModuleBase;
        public OverlayWindow()
        {
                
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;

            SetWindowLong(this.Handle, GWL_EXSTYLE, GetWindowLong(this.Handle, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 128, LWA_ALPHA);

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 10; // 每秒更新一次
            updateTimer.Tick += UpdateOverlayPosition;
        }

        public void StartOverlay(uint targetProcessId, string windowTitle, IntPtr processHandle, IntPtr clientModuleBase, IntPtr serverModuleBase)
        {
            this.targetWindowHandle = WindowFinder.FindWindowByProcess(targetProcessId, windowTitle);
            if (targetWindowHandle == IntPtr.Zero)
            {
                MessageBox.Show("找不到目标窗口！");
                return;
            }

            this.processHandle = processHandle;
            this.clientModuleBase = clientModuleBase;
            this.serverModuleBase = serverModuleBase;

            this.Show();
            updateTimer.Start();
        }




        private void UpdateOverlayPosition(object sender, EventArgs e)
        {
            if (targetWindowHandle == IntPtr.Zero) return;

            RECT rect = new RECT();
            GetWindowRect(targetWindowHandle, ref rect);

            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Right - rect.Left;
            this.Height = rect.Bottom - rect.Top;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (targetWindowHandle == IntPtr.Zero || processHandle == IntPtr.Zero) return;

            Graphics g = e.Graphics;
            Font font = new Font("Arial", 16);
            Brush brush = Brushes.Red;

            // 示例：绘制实时数据
            string[] displayTexts = new string[]
            {
        $"FOV视场角: {Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x506910)}",
        $"本人坐标Z: {Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047F0)}",
        $"本人坐标Y: {Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047EC)}",
        $"本人坐标X: {Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047E8)}",
        $"第一号敌人坐标Z: {Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD2C)}",
        $"第一号敌人坐标Y: {Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD28)}",
        $"第一号敌人坐标X: {Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD24)}"
            };

            for (int i = 0; i < displayTexts.Length; i++)
            {
                g.DrawString(displayTexts[i], font, brush, new Point(10, 40 + i * 30));
            }
        }

        public void StopOverlay()
        {
            this.Hide(); // 隐藏 Overlay 窗口
            updateTimer.Stop(); // 停止定时器，停止更新位置
        }

    }
}
