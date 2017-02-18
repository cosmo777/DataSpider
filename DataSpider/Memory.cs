using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
        private readonly Dictionary<string, int> modules;
        private bool closed;
        private IntPtr procHandle;

        public Memory(int pId)
        {
            try
            {
                Process = Process.GetProcessById(pId);
                AddressOfProcess = Process.MainModule.BaseAddress.ToInt64();
                procHandle = OpenProcess(Process, ProcessAccessFlags.All);
                modules = new Dictionary<string, int>();
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

        public int GetModule(string name)
        {
            if (modules.ContainsKey(name))
            {
                return modules[name];
            }
            int num = Process.Modules.Cast<ProcessModule>().First(m => m.ModuleName == name).BaseAddress.ToInt32();
            modules.Add(name, num);
            return num;
        }

        public bool IsInvalid()
        {
            return Process.HasExited || closed;
        }

        public int ReadInt(long addr)
        {
            return BitConverter.ToInt32(ReadMem(addr, 4), 0);
        }

        public int ReadInt(int addr, params int[] offsets)
        {
            int num = ReadInt(addr);
            return offsets.Aggregate(num, (current, num2) => ReadInt(current + num2));
        }

        public int ReadInt(long addr, params long[] offsets)
        {
            long num = ReadLong(addr);
            return (int)offsets.Aggregate(num, (current, num2) => ReadLong(current + num2));
        }




        public float ReadFloat(long addr)
        {
            return BitConverter.ToSingle(ReadMem(addr, 4), 0);
        }

        public long ReadLong(long addr)
        {
            return BitConverter.ToInt64(ReadMem(addr, 8), 0);
        }

        public long ReadLong(long addr, params long[] offsets)
        {
            long num = ReadLong(addr);
            return offsets.Aggregate(num, (current, num2) => ReadLong(current + num2));
        }

        public uint ReadUInt(long addr)
        {
            return BitConverter.ToUInt32(ReadMem(addr, 4), 0);
        }


        /// <summary>
        /// Read string as ASCII
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="length"></param>
        /// <param name="replaceNull"></param>
        /// <returns></returns>
        public string ReadString(long addr, int length = 256, bool replaceNull = true)
        {
            if (addr <= 65536 && addr >= -1)
            {
                return string.Empty;
            }
            string @string = Encoding.ASCII.GetString(ReadMem(addr, length));
            return replaceNull ? RTrimNull(@string) : @string;
        }

        private static string RTrimNull(string text)
        {
            int num = text.IndexOf('\0');
            return num > 0 ? text.Substring(0, num) : text;
        }

        /// <summary>
        /// Read string as Unicode
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="length"></param>
        /// <param name="replaceNull"></param>
        /// <returns></returns>
        public string ReadStringU(long addr, int length = 256, bool replaceNull = true)
        {
            if (addr <= 65536 && addr >= -1)
            {
                return string.Empty;
            }
            byte[] mem = ReadMem(addr, length);
            if (mem.Length == 0)
            {
                return String.Empty;
            }
            if (mem[0] == 0 && mem[1] == 0)
                return string.Empty;
            string @string = Encoding.Unicode.GetString(mem);
            return replaceNull ? RTrimNull(@string) : @string;
        }

        public string ReadStringU(byte[] mem, bool replaceNull = true)
        {
            if (mem.Length == 0)
            {
                return String.Empty;
            }
            if (mem[0] == 0 && mem[1] == 0)
                return string.Empty;
            string @string = Encoding.Unicode.GetString(mem);
            return replaceNull ? RTrimNull(@string) : @string;
        }

        public byte ReadByte(long addr)
        {
            return ReadBytes(addr, 1).FirstOrDefault();
        }

        public byte[] ReadBytes(long addr, int length)
        {
            return ReadMem(addr, length);
        }

        public void Close()
        {
            if (!closed)
            {
                closed = true;
                CloseHandle(procHandle);
            }
        }

        private byte[] ReadMem(long addr, int size)
        {
            var array = new byte[size];
            ReadProcessMemory(procHandle, (IntPtr)addr, array);
            return array;
        }

        public string DebugStr = "";
        public long[] FindPatterns(params Pattern[] patterns)
        {
            byte[] exeImage = ReadBytes(AddressOfProcess, 0x2000000); //33mb
            var address = new long[patterns.Length];

            for (int iPattern = 0; iPattern < patterns.Length; iPattern++)
            {
                Pattern pattern = patterns[iPattern];
                byte[] patternData = pattern.Bytes;
                int patternLength = patternData.Length;

                bool found = false;

                for (int offset = 0; offset < exeImage.Length - patternLength; offset++)
                {
                    if (CompareData(pattern, exeImage, offset))
                    {
                        found = true;
                        address[iPattern] = offset;
                        DebugStr += "Pattern " + iPattern + " is found at " + (AddressOfProcess + offset).ToString("X") + " offset: " + offset.ToString("X") + Environment.NewLine;
                        break;
                    }
                }

                if (!found)
                {
                    //System.Windows.Forms.MessageBox.Show("Pattern " + iPattern + " is not found!");
                    DebugStr += "Pattern " + iPattern + " is not found!" + Environment.NewLine;
                }
            }
            return address;
        }

        private bool CompareData(Pattern pattern, byte[] data, int offset)
        {
            return !pattern.Bytes.Where((t, i) => pattern.Mask[i] == 'x' && t != data[offset + i]).Any();
        }
    }
}