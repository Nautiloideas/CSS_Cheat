using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSS_Cheat
{
    public static class Memory
    {
        // 定义所需的Windows API函数
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_OPERATION = 0x0008;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        private const uint TH32CS_SNAPMODULE = 0x00000008;
        private const uint TH32CS_SNAPMODULE32 = 0x00000008;

        public static IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId)
        {
            return Process.GetProcessById(dwProcessId).Handle;
        }

        public static IntPtr GetModuleHandle(uint processId, string moduleName)
        {
            if (processId == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, processId);
            if (hSnapshot == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            MODULEENTRY32 moduleEntry = new MODULEENTRY32();
            moduleEntry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

            if (Module32First(hSnapshot, ref moduleEntry))
            {
                do
                {
                    if (moduleEntry.szModule.IndexOf(moduleName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        CloseHandle(hSnapshot);
                        return moduleEntry.modBaseAddr;  // 返回模块基地址
                    }
                }
                while (Module32Next(hSnapshot, ref moduleEntry));
            }

            CloseHandle(hSnapshot);
            return IntPtr.Zero;
        }

        public static string ReadMemoryValue(IntPtr processHandle, IntPtr moduleBase, int offset)
        {
            IntPtr address = moduleBase + offset;
            byte[] buffer = new byte[4];
            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesRead);
            int value = BitConverter.ToInt32(buffer, 0);
            return value.ToString("X");
        }
    }
}
