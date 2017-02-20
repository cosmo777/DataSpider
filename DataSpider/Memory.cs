using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using DataSpider.MemoryTools;

namespace DataSpider
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    public struct Pattern
    {
        public byte[] Bytes;
        public string Mask;

        public Pattern(byte[] pattern, string mask)
        {
            Bytes = pattern;
            Mask = mask;
        }

        public Pattern(string pattern, string mask)
        {
            var arr = pattern.Split(new[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries);
            Bytes = arr.Select(y => byte.Parse(y, NumberStyles.HexNumber)).ToArray();
            Mask = mask;
        }
    }

    public class Memory : IDisposable
    {
        public static IntPtr OpenProcess(Process process, ProcessAccessFlags flags)
        {
            return OpenProcess(flags, false, process.Id);
        }

        public static bool ReadProcessMemory(IntPtr handle, IntPtr baseAddress, byte[] buffer)
        {
            IntPtr bytesRead;
            return ReadProcessMemory(handle, baseAddress, buffer, buffer.Length, out bytesRead);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hWnd, IntPtr baseAddr, byte[] buffer, int size, out IntPtr bytesRead);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);


        public readonly long AddressOfProcess;
        private bool _closed;
        private readonly IntPtr _procHandle;

        public Memory(int pId)
        {
            try
            {
                Process = Process.GetProcessById(pId);
                AddressOfProcess = Process.MainModule.BaseAddress.ToInt64();
                _procHandle = OpenProcess(Process, ProcessAccessFlags.All);
            }
            catch (Win32Exception ex)
            {
                throw new Exception("You should run program as an administrator", ex);
            }
        }

        public Process Process { get; }

        public void Dispose()
        {
            Close();
        }

        ~Memory()
        {
            Close();
        }

        public byte[] ReadBytes(long addr, int length)
        {
            return ReadMem(addr, length);
        }

        public byte[] ReadBytes(int addr, int length)
        {
            return ReadMem(addr, length);
        }

        public void Close()
        {
            if (!_closed)
            {
                _closed = true;
                CloseHandle(_procHandle);
            }
        }

        private byte[] ReadMem(int addr, int size)
        {
            var array = new byte[size];
            ReadProcessMemory(_procHandle, (IntPtr)addr, array);
            return array;
        }

        private byte[] ReadMem(long addr, int size)
        {
            var array = new byte[size];
            ReadProcessMemory(_procHandle, (IntPtr)addr, array);
            return array;
        }

    }
}