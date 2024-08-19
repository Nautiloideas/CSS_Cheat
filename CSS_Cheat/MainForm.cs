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
            processListBox.Items.Clear(); // ˢ��ʱ���
            foreach (Process process in Process.GetProcesses())
            {
                if (process.MainWindowHandle != IntPtr.Zero) // ֻ��ʾ�д��ڵ�Ӧ�ó���
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
                MessageBox.Show("����ѡ��һ�����̣�");
                return;
            }

            Process[] processes = Process.GetProcessesByName(selectedProcessName);
            if (processes.Length == 0)
            {
                MessageBox.Show("δ�ҵ��ý��̣�");
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

                    // ���� StartOverlay�������ݽ���ID�����ڱ������������
                    overlayWindow.StartOverlay(targetProcessId, targetProcess.MainWindowTitle, this.processHandle, this.clientModuleBase, this.serverModuleBase);
                }
                else
                {
                    MessageBox.Show("δ�ҵ��ý��̣�");
                }
            }
            else
            {
                MessageBox.Show("����ѡ��һ�����̣�");
            }
        }


        private void btnStopDrawing_Click(object sender, EventArgs e)
        {
            overlayWindow.StopOverlay();
        }
    }
}
