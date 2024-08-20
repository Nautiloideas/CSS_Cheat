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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

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



        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private IntPtr targetWindowHandle;
        private System.Windows.Forms.Timer updateTimer;

        private IntPtr processHandle;
        private IntPtr clientModuleBase;
        private IntPtr serverModuleBase;


        private string[] displayTexts;
        private Bitmap textBitmap;

        private struct PlayerData
        {
            public float FOV;
            public float PlayerPosX;
            public float PlayerPosY;
            public float PlayerPosZ;
            public float MouseAngleX;
            public float MouseAngleY;
            public int PlayerHealth;
            public List<EnemyData> Enemies;
        }

        private struct EnemyData
        {
            public float EnemyPosX;
            public float EnemyPosY;
            public float EnemyPosZ;
            public int EnemyHealth;
        }

        public OverlayWindow()
        {
                
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;

            SetWindowLong(this.Handle, GWL_EXSTYLE, GetWindowLong(this.Handle, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 0, LWA_COLORKEY);

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 80; // 每秒更新一次
            updateTimer.Tick += UpdateOverlay;



            // 初始化固定文本描述
            displayTexts = new string[]
            {
                "FOV视场角: ",
                "本人坐标Z: ",
                "本人坐标Y: ",
                "本人坐标X: ",
                "本人鼠标角度Y: ",
                "本人鼠标角度X: ",
                "本人血量: ",
                "玩家数量: ",
                "第一号敌人坐标Z: ",
                "第一号敌人坐标Y: ",
                "第一号敌人坐标X: ",
                "第一号敌人血量: ",
            };

            // 创建缓存的图像
            CreateTextBitmap();
        }


        private void CreateTextBitmap()
        {
            textBitmap = new Bitmap(200, 400); // 假设文本区域大小为200x300
            using (Graphics g = Graphics.FromImage(textBitmap))
            {
                Font font = new Font("Arial", 16);
                Brush brush = Brushes.Red;

                for (int i = 0; i < displayTexts.Length; i++)
                {
                    g.DrawString(displayTexts[i], font, brush, new Point(10, 40 + i * 30));
                }
            }
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


        private void UpdateOverlay(object sender, EventArgs e)
        {
            UpdateOverlayPosition();
            this.Invalidate(); // 强制重绘窗口
        }


        private void UpdateOverlayPosition()
        {
            if (targetWindowHandle == IntPtr.Zero) return;

            RECT rect = new RECT();
            GetWindowRect(targetWindowHandle, ref rect);

            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Right - rect.Left;
            this.Height = rect.Bottom - rect.Top;
            // 确保窗口始终位于目标窗口的上层
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private PlayerData GetPlayerData()
        {
            PlayerData playerData = new PlayerData
            {
                FOV = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x506910, MemoryValueType.Float),
                PlayerPosX = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047E8, MemoryValueType.Float),
                PlayerPosY = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047EC, MemoryValueType.Float),
                PlayerPosZ = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047F0, MemoryValueType.Float),
                MouseAngleX = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x4FFCF0, MemoryValueType.Float),
                MouseAngleY = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x4FFCEC, MemoryValueType.Float),
                PlayerHealth = GetPlayerHealth(),
                Enemies = new List<EnemyData>()
            };

            int playerCount = (int)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x5119C4, MemoryValueType.Int32);
            int enemyOffset = 0x24;
            int healthOffset = 0x10;

            for (int i = 0; i < playerCount - 1; i++)
            {
                EnemyData enemyData = new EnemyData
                {
                    EnemyPosX = (float)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD24 + i * enemyOffset, MemoryValueType.Float),
                    EnemyPosY = (float)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD28 + i * enemyOffset, MemoryValueType.Float),
                    EnemyPosZ = (float)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x54DD2C + i * enemyOffset, MemoryValueType.Float),
                    EnemyHealth = GetEnemyHealth(i), // 调用 GetEnemyHealth 方法
                };
                playerData.Enemies.Add(enemyData);
            }

            return playerData;
        }
        private int GetPlayerHealth()
        {
            int baseAddressValue = (int)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x4F615C, MemoryValueType.Int32);
            IntPtr baseAddress = new IntPtr(baseAddressValue);
            return (int)Memory.ReadMemoryValue(processHandle, baseAddress, 0xE4, MemoryValueType.Int32);
        }
        private int GetEnemyHealth(int enemyIndex)
        {
            // 读取基地址
            int baseAddressValue = (int)Memory.ReadMemoryValue(processHandle, serverModuleBase, 0x4F616C + enemyIndex * 0x10, MemoryValueType.Int32);
            IntPtr baseAddress = new IntPtr(baseAddressValue);

            // 从新的地址读取 EnemyHealth
            return (int)Memory.ReadMemoryValue(processHandle, baseAddress, 0xE4, MemoryValueType.Int32);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (targetWindowHandle == IntPtr.Zero || processHandle == IntPtr.Zero) return;

            Graphics g = e.Graphics;
            Font font = new Font("Arial", 16);
            Brush brush = Brushes.Red;

            // 绘制缓存的文本描述
            g.DrawImage(textBitmap, 0, 0);

            // 获取实时数据
            PlayerData playerData = GetPlayerData();

            // 绘制实时数据
            string[] values = new string[]
            {
                $"{playerData.FOV:F3}",
                $"{playerData.PlayerPosZ:F3}",
                $"{playerData.PlayerPosY:F3}",
                $"{playerData.PlayerPosX:F3}",
                $"{playerData.MouseAngleY:F3}",
                $"{playerData.MouseAngleX:F3}",
                $"{playerData.PlayerHealth}",
                $"{playerData.Enemies.Count}"
            };

            for (int i = 0; i < values.Length; i++)
            {
                g.DrawString(values[i], font, brush, new Point(220, 40 + i * 30)); // 假设实时数据绘制在固定文本描述的右侧
            }

            // 绘制敌人数据
            for (int i = 0; i < playerData.Enemies.Count; i++)
            {
                EnemyData enemy = playerData.Enemies[i];
                g.DrawString($"{enemy.EnemyPosZ:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4) * 30));
                g.DrawString($"{enemy.EnemyPosY:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 1) * 30));
                g.DrawString($"{enemy.EnemyPosX:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 2) * 30));
                g.DrawString($"{enemy.EnemyHealth}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 3) * 30));
            }
        }

        public void StopOverlay()
        {
            this.Hide(); // 隐藏 Overlay 窗口
            updateTimer.Stop(); // 停止定时器，停止更新位置
        }

    }
}
