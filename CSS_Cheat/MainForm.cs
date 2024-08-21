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
        private ModuleListForm moduleListForm;

        public MainForm()
        {
            InitializeComponent();
            LoadProcesses();
            overlayWindow = new OverlayWindow();
            moduleListForm = new ModuleListForm();
            moduleListForm.Location = new Point(this.Right, this.Top); // 紧靠在 MainForm 右侧
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

            // 清空之前的模块列表
            moduleListForm.ClearModules();

            Process targetProcess = processes[0];
            this.processHandle = Memory.OpenProcess(Memory.PROCESS_VM_READ | Memory.PROCESS_VM_OPERATION, false, targetProcess.Id);

            // 获取模块基地址
            (clientModuleBase, serverModuleBase) = Memory.GetModuleBases(processHandle, (uint)targetProcess.Id);

            // 获取所有模块信息
            List<ModuleInfo> modules = Memory.GetAllModules(this.processHandle, (uint)targetProcess.Id);

            // 显示模块列表窗口，并加载模块列表
            moduleListForm.LoadModules(modules);
            moduleListForm.Show();
        }



        private void refreshProcessListButton_Click(object sender, EventArgs e)
        {
            LoadProcesses();
            moduleListForm.Hide(); // 隐藏模块列表窗口
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

                    // 调用 StartOverlay，并仅传递进程ID
                    overlayWindow.StartOverlay(targetProcessId);
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
