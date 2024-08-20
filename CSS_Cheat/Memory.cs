using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSS_Cheat
{
    public class ModuleInfo
    {
        public string ModuleName { get; set; }
        public IntPtr BaseAddress { get; set; }
        public uint ModuleSize { get; set; }
    }

    public enum MemoryValueType
    {
        Int32,
        Float,
        Double,
        Int64
    }


    public static class Memory
    {


        // 定义所需的Windows API函数
        private delegate bool ModuleCallback(string ModuleName, IntPtr ModuleBase, uint ModuleSize, IntPtr UserContext);

        [DllImport("dbghelp.dll", CharSet = CharSet.Ansi)]
        private static extern bool EnumerateLoadedModules(IntPtr hProcess, ModuleCallback EnumLoadedModulesCallback, IntPtr UserContext);

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] char[] lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

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

        private const uint LIST_MODULES_ALL = 0x03; // List all modules
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
        private const uint TH32CS_SNAPMODULE32 = 0x00000010;

        public static IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId)
        {
            return Process.GetProcessById(dwProcessId).Handle;
        }

        public static IntPtr GetModuleHandle(IntPtr processHandle, uint processId, string moduleName)
        {
            List<ModuleInfo> modules = GetAllModules(processHandle, processId);

            foreach (var module in modules)
            {
                if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return module.BaseAddress;
                }
            }

            return IntPtr.Zero;
        }

        public static List<ModuleInfo> GetAllModules(IntPtr processHandle, uint processId)
        {
            List<ModuleInfo> modules = new List<ModuleInfo>();
  
            // 使用 CreateToolhelp32Snapshot 获取常规模块信息
            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE| TH32CS_SNAPMODULE32, processId);
            if (hSnapshot != IntPtr.Zero)
            {
                MODULEENTRY32 moduleEntry = new MODULEENTRY32();
                moduleEntry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                if (Module32First(hSnapshot, ref moduleEntry))
                {
                    do
                    {
                        modules.Add(new ModuleInfo
                        {
                            ModuleName = moduleEntry.szModule,
                            BaseAddress = moduleEntry.modBaseAddr,
                            ModuleSize = moduleEntry.modBaseSize
                        });
                    } while (Module32Next(hSnapshot, ref moduleEntry));
                }

                CloseHandle(hSnapshot);
            }

            // 使用 DbgHelp.dll 的 EnumerateLoadedModules 获取动态加载的模块信息
            //EnumerateLoadedModules(processHandle, (moduleName, moduleBase, moduleSize, userContext) =>
            //{
            //    modules.Add(new ModuleInfo
            //    {
            //        ModuleName = moduleName,
            //        BaseAddress = moduleBase,
            //        ModuleSize = moduleSize
            //    });
            //    return true; // 继续枚举
            //}, IntPtr.Zero);

            return modules;
        }
        public static object ReadMemoryValue(IntPtr processHandle, IntPtr moduleBase, int offset, MemoryValueType valueType)
        {
            IntPtr address = moduleBase + offset;
            byte[] buffer;
            object result = null;

            switch (valueType)
            {
                case MemoryValueType.Int32:
                    buffer = new byte[4];
                    ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesReadInt32);
                    result = BitConverter.ToInt32(buffer, 0);
                    break;
                case MemoryValueType.Float:
                    buffer = new byte[4];
                    ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesReadFloat);
                    result = BitConverter.ToSingle(buffer, 0);
                    break;
                case MemoryValueType.Double:
                    buffer = new byte[8];
                    ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesReadDouble);
                    result = BitConverter.ToDouble(buffer, 0);
                    break;
                case MemoryValueType.Int64:
                    buffer = new byte[8];
                    ReadProcessMemory(processHandle, address, buffer, buffer.Length, out int bytesReadInt64);
                    result = BitConverter.ToInt64(buffer, 0);
                    break;
            }

            return result;
        }
    }
}
