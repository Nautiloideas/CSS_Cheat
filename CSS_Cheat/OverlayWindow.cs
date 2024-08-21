using CSS_Cheat;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Security.Cryptography;

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
        private System.Windows.Forms.Timer refreshTimer;

        private IntPtr processHandle;
        private IntPtr clientModuleBase;
        private IntPtr serverModuleBase;
        private uint processId;


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
            // 启用双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
                
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
            //displayTexts = new string[]
            //{
            //    "FOV视场角: ",
            //    "本人坐标Z: ",
            //    "本人坐标Y: ",
            //    "本人坐标X: ",
            //    "本人鼠标角度Y: ",
            //    "本人鼠标角度X: ",
            //    "本人血量: ",
            //    "玩家数量: ",
            //    "第一号敌人坐标Z: ",
            //    "第一号敌人坐标Y: ",
            //    "第一号敌人坐标X: ",
            //    "第一号敌人血量: ",
            //};

            //// 创建缓存的图像
            //CreateTextBitmap();
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

        public void StartOverlay(uint targetProcessId)
        {
            Process targetProcess = Process.GetProcessById((int)targetProcessId);
            if (targetProcess == null)
            {
                MessageBox.Show("找不到目标进程！");
                return;
            }

            this.targetWindowHandle = WindowFinder.FindWindowByProcess(targetProcessId, targetProcess.MainWindowTitle);
            if (targetWindowHandle == IntPtr.Zero)
            {
                MessageBox.Show("找不到目标窗口！");
                return;
            }
            this.processId = targetProcessId;
            this.processHandle = Memory.OpenProcess(Memory.PROCESS_VM_READ | Memory.PROCESS_VM_OPERATION, false, targetProcess.Id);

            // 获取模块基地址
            (clientModuleBase, serverModuleBase) = Memory.GetModuleBases(processHandle, (uint)targetProcess.Id);

            this.Show();
            updateTimer.Start();
        }

        public void StopOverlay()
        {
            this.Hide(); // 隐藏 Overlay 窗口
            updateTimer.Stop(); // 停止定时器，停止更新位置
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

            // 打开进程句柄
            IntPtr processHandle = Memory.OpenProcess(Memory.PROCESS_VM_READ | Memory.PROCESS_VM_OPERATION, false, (int)this.processId);
            if (processHandle == IntPtr.Zero)
            {
                MessageBox.Show("无法打开进程！");
                return new PlayerData(); // 返回空的 PlayerData 对象
            }

            // 获取模块基地址
            (IntPtr clientModuleBase, IntPtr serverModuleBase) = Memory.GetModuleBases(processHandle, this.processId);
            if (clientModuleBase == IntPtr.Zero || serverModuleBase == IntPtr.Zero)
            {
                MessageBox.Show("无法获取模块基地址！");
                return new PlayerData(); // 返回空的 PlayerData 对象
            }

            PlayerData playerData = new PlayerData();
            try
            {
                playerData.FOV = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x506910, MemoryValueType.Float);
                playerData.PlayerPosX = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047E8, MemoryValueType.Float);
                playerData.PlayerPosY = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047EC, MemoryValueType.Float);
                playerData.PlayerPosZ = (float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x5047F0, MemoryValueType.Float);
                playerData.MouseAngleX = AdjustMouseAngleX((float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x4FFCF0, MemoryValueType.Float));
                playerData.MouseAngleY = AdjustMouseAngleY((float)Memory.ReadMemoryValue(processHandle, clientModuleBase, 0x4FFCEC, MemoryValueType.Float));
                playerData.PlayerHealth = GetPlayerHealth();
                playerData.Enemies = new List<EnemyData>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                if (processHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("processHandle is null.");
                }
                if (clientModuleBase == IntPtr.Zero)
                {
                    Debug.WriteLine("clientModuleBase is null.");
                }
                // Add more checks if necessary
            }

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

        private float AdjustMouseAngleX(float angleX)
        {
            if (angleX < 0)
            {
                angleX = 360 + angleX;
            }
            return angleX;
        }

        private float AdjustMouseAngleY(float angleY)
        {
            return angleY * -1;
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
            if (targetWindowHandle == IntPtr.Zero || processHandle == IntPtr.Zero)
            {
                MessageBox.Show("目标窗口句柄或进程句柄为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Graphics g = e.Graphics;
            Font font = new Font("Arial", 16);
            Brush brush = Brushes.Red;

            // 绘制缓存的文本描述
            //g.DrawImage(textBitmap, 0, 0);

            // 获取实时数据
            PlayerData playerData = GetPlayerData();

            // 绘制实时数据
            //string[] values = new string[]
            //{
            //    $"{playerData.FOV:F3}",
            //    $"{playerData.PlayerPosZ:F3}",
            //    $"{playerData.PlayerPosY:F3}",
            //    $"{playerData.PlayerPosX:F3}",
            //    $"{playerData.MouseAngleY:F3}",
            //    $"{playerData.MouseAngleX:F3}",
            //    $"{playerData.PlayerHealth}",
            //    $"{playerData.Enemies.Count}"
            //};

            //for (int i = 0; i < values.Length; i++)
            //{
            //    g.DrawString(values[i], font, brush, new Point(220, 40 + i * 30)); // 假设实时数据绘制在固定文本描述的右侧
            //}

            // 绘制敌人数据
            for (int i = 0; i < playerData.Enemies.Count; i++)
            {
                EnemyData enemy = playerData.Enemies[i];
                //g.DrawString($"{enemy.EnemyPosZ:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4) * 30));
                //g.DrawString($"{enemy.EnemyPosY:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 1) * 30));
                //g.DrawString($"{enemy.EnemyPosX:F3}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 2) * 30));
                //g.DrawString($"{enemy.EnemyHealth}", font, brush, new Point(220, 40 + (values.Length + i * 4 + 3) * 30));

                // 将敌人的三维坐标转换为屏幕上的二维坐标
                ScreenCoordinateHelper.Vector3 playerPos = new ScreenCoordinateHelper.Vector3(playerData.PlayerPosX, playerData.PlayerPosY, playerData.PlayerPosZ);
                ScreenCoordinateHelper.Vector3 enemyPos = new ScreenCoordinateHelper.Vector3(enemy.EnemyPosX, enemy.EnemyPosY, enemy.EnemyPosZ);
                ScreenCoordinateHelper.Vector2 screenPos = ScreenCoordinateHelper.WorldToScreen(playerPos, enemyPos, playerData.MouseAngleX, playerData.MouseAngleY, playerData.FOV, this.Width, this.Height);

                float enemyDistance = (float)Math.Sqrt(
                    Math.Pow(enemy.EnemyPosX - playerData.PlayerPosX, 2) +
                    Math.Pow(enemy.EnemyPosY - playerData.PlayerPosY, 2) +
                    Math.Pow(enemy.EnemyPosZ - playerData.PlayerPosZ, 2)
                );
                // 绘制从敌人位置
                DrawEnemy(g,screenPos,playerPos,enemyDistance,enemy.EnemyHealth, playerData.FOV);
            }
        }

        private void DrawEnemy(Graphics g, ScreenCoordinateHelper.Vector2 screenPos, ScreenCoordinateHelper.Vector3 playerPos, float enemyDistance, float enemyHealth, float viewAngle)
        {
            // 检查敌人是否在视野范围内
            if (!IsEnemyInView(screenPos, playerPos, viewAngle))
            {
                return;
            }

            // 绘制从玩家位置到敌人位置的线段
            DrawLineToEnemy(g, screenPos);

            // 如果距离小于等于 600，绘制敌人方框
            if (enemyDistance <= 600)
            {
                DrawEnemyBox(g, screenPos, enemyDistance, enemyHealth);
            }
            else
            {
                // 绘制敌人坐标点
                DrawEnemyPoint(g, screenPos);
            }
        }

        
        private void DrawEnemyBox(Graphics g, ScreenCoordinateHelper.Vector2 screenPos, float enemyDistance, float enemyHealth)
        {
            // 计算方框的宽度和高度
            double[] distances = new double[] { 200, 300, 400, 500, 600 };
            double[] widths = new double[] { 98, 68, 48, 38, 28 };
            double[] heights = new double[] { 178, 108, 78, 68, 58 };

            // 使用 Math.NET Numerics 进行多项式拟合
            var widthCoefficients = Fit.Polynomial(distances, widths, 2);
            var heightCoefficients = Fit.Polynomial(distances, heights, 2);

            // 创建多项式对象
            var widthPolynomial = new Polynomial(widthCoefficients);
            var heightPolynomial = new Polynomial(heightCoefficients);

            // 计算宽度和高度
            double width = widthPolynomial.Evaluate(enemyDistance);
            double height = heightPolynomial.Evaluate(enemyDistance);

            // 绘制敌人方框
            Pen pen = new Pen(Color.Red, 2);
            g.DrawRectangle(pen, screenPos.X - (float)width / 2, screenPos.Y - (float)height / 2, (float)width, (float)height);

            // 绘制血量槽
            float healthBarHeight = (float)height * (enemyHealth / 100); // 根据敌人血量计算血量槽高度
            Brush healthBrush;
            if (enemyHealth >= 70)
            {
                healthBrush = Brushes.Green;
            }
            else if (enemyHealth >= 40)
            {
                healthBrush = Brushes.Yellow;
            }
            else
            {
                healthBrush = Brushes.Red;
            }
            g.FillRectangle(healthBrush, screenPos.X - (float)width / 2 - 10, screenPos.Y - (float)height / 2 + ((float)height - healthBarHeight), 10, healthBarHeight);

            // 绘制距离文本
            g.DrawString($"距离: {Math.Round(enemyDistance)}", new Font("Arial", 12), Brushes.Cyan, screenPos.X, screenPos.Y - (float)height / 2 - 20);

            // 绘制生命文本
            g.DrawString($"生命: {Math.Round(enemyHealth)}", new Font("Arial", 12), healthBrush, screenPos.X, screenPos.Y - (float)height / 2 - 40);

            // 绘制方框大小文本
            g.DrawString($"高度: {Math.Round(height)}", new Font("Arial", 12), Brushes.Red, screenPos.X, screenPos.Y - (float)height / 2 - 60);
            g.DrawString($"宽度: {Math.Round(width)}", new Font("Arial", 12), Brushes.Red, screenPos.X, screenPos.Y - (float)height / 2 - 80);
        }
        private void DrawEnemyPoint(Graphics g, ScreenCoordinateHelper.Vector2 screenPos)
        {
            // 实现绘制敌人坐标点的逻辑
            // 使用Graphics类在指定位置绘制一个半径为10的圆
            Brush brush = Brushes.Red;
            float radius = 5;
            g.FillEllipse(brush, screenPos.X - radius, screenPos.Y - radius, radius * 2, radius * 2);
        }

        private void DrawLineToEnemy(Graphics g, ScreenCoordinateHelper.Vector2 enemyScreenPos)
        {
            // 获取屏幕的宽度和高度
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // 计算屏幕正中下的位置
            ScreenCoordinateHelper.Vector2 screenBottomCenterPos = new ScreenCoordinateHelper.Vector2(screenWidth / 2, (float)screenHeight);

            // 实现绘制从屏幕正中下位置到敌人位置的线段的逻辑
            using (Pen pen = new Pen(Color.Green, 2))
            {
                g.DrawLine(pen, screenBottomCenterPos.X, screenBottomCenterPos.Y, enemyScreenPos.X, enemyScreenPos.Y);
            }
        }

        private bool IsEnemyInView(ScreenCoordinateHelper.Vector2 screenPos, ScreenCoordinateHelper.Vector3 playerPos, float viewAngle)
        {
            // 计算敌人相对于玩家的方向向量
            var directionToEnemy = new ScreenCoordinateHelper.Vector2(screenPos.X - playerPos.X, screenPos.Y - playerPos.Y);
            directionToEnemy = Normalize(directionToEnemy);

            // 计算玩家的视野方向向量
            var playerViewDirection = new ScreenCoordinateHelper.Vector2((float)Math.Cos(viewAngle), (float)Math.Sin(viewAngle));
            playerViewDirection = Normalize(playerViewDirection);

            // 计算方向向量之间的夹角
            float dotProduct = directionToEnemy.X * playerViewDirection.X + directionToEnemy.Y * playerViewDirection.Y;
            float angleBetween = (float)Math.Acos(dotProduct);

            // 检查敌人是否在视野范围内
            return angleBetween <= viewAngle / 2;
        }
        private ScreenCoordinateHelper.Vector2 Normalize(ScreenCoordinateHelper.Vector2 vector)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            return new ScreenCoordinateHelper.Vector2(vector.X / length, vector.Y / length);
        }
    
    }
}
