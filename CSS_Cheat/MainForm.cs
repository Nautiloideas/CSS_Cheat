using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CSS_Cheat
{
    public partial class MainForm : Form
    {
        private IntPtr processHandle;
        private IntPtr clientModuleBase;
        private IntPtr serverModuleBase;
        private OverlayWindow overlayWindow;

        public MainForm()
        {
            InitializeComponent();
            LoadProcesses();
            overlayWindow = new OverlayWindow();
        }

        private void LoadProcesses()
        {
            processListBox.Items.Clear(); // 刷新时清空
            foreach (Process process in Process.GetProcesses())
            {
                if (process.MainWindowHandle != IntPtr.Zero) // 只显示有窗口的应用程序
                {
                    processListBox.Items.Add(process.ProcessName);
                }
            }
        }

        private void selectProcessButton_Click(object sender, EventArgs e)
        {
            string selectedProcessName = processListBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedProcessName))
            {
                MessageBox.Show("请先选择一个进程！");
                return;
            }

            Process[] processes = Process.GetProcessesByName(selectedProcessName);
            if (processes.Length == 0)
            {
                MessageBox.Show("未找到该进程！");
                return;
            }

            Process targetProcess = processes[0];
            this.processHandle = Memory.OpenProcess(Memory.PROCESS_VM_READ | Memory.PROCESS_VM_OPERATION, false, targetProcess.Id);

            clientModuleBase = Memory.GetModuleHandle((uint)targetProcess.Id, "client.dll");
            serverModuleBase = Memory.GetModuleHandle((uint)targetProcess.Id, "server.dll");
        }

      
        private void refreshProcessListButton_Click(object sender, EventArgs e)
        {
            LoadProcesses();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (processHandle != IntPtr.Zero)
            {
                Memory.CloseHandle(processHandle);
            }
        }

        private void btnStartDrawing_Click(object sender, EventArgs e)
        {
            string selectedProcessName = processListBox.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedProcessName))
            {
                Process[] processes = Process.GetProcessesByName(selectedProcessName);
                if (processes.Length > 0)
                {
                    Process targetProcess = processes[0]; 
                    uint targetProcessId = (uint)targetProcess.Id;

                    // 调用 StartOverlay，并传递进程ID、窗口标题和其他参数
                    overlayWindow.StartOverlay(targetProcessId, targetProcess.MainWindowTitle, this.processHandle, this.clientModuleBase, this.serverModuleBase);
                }
                else
                {
                    MessageBox.Show("未找到该进程！");
                }
            }
            else
            {
                MessageBox.Show("请先选择一个进程！");
            }
        }


        private void btnStopDrawing_Click(object sender, EventArgs e)
        {
            overlayWindow.StopOverlay();
        }
    }
}
