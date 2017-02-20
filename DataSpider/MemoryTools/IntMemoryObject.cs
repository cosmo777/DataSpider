using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataSpider.MemoryTools
{
    public class IntMemoryObject
    {
        protected Memory Memory { get; }
        public int BaseAddress { get; set; }
        public int[] AddressMappings { get; set; }
        public List<int> Addresses { get; set; }
        public List<int> AddressValues { get; set; }

        public IntMemoryObject(Memory memory, int? parentAddesss, params int[] addressMappings)
        {
            Memory = memory;
            AddressMappings = addressMappings;
            Addresses = new List<int>();
            AddressValues = new List<int>();
            CalcBaseAddress(parentAddesss);
        }

        public void CalcBaseAddress(int? parentAddress)
        {
            AddressValues.Clear();
            Addresses.Clear();
            if (AddressMappings.Length > 0 || parentAddress.HasValue)
            {
                int firstIndex;
                int firstAddress;

                if (parentAddress.HasValue)
                {
                    firstAddress = parentAddress.Value;
                    firstIndex = 0;
                }
                else
                {
                    firstAddress = AddressMappings[0];
                    firstIndex = 1;
                }
                int currentAddress = ReadLong(firstAddress);
                Addresses.Add(firstAddress);
                AddressValues.Add(currentAddress);
                for (int x = firstIndex; x < AddressMappings.Length; x++)
                {
                    var offset = AddressMappings[x];
                    var nextAddress = currentAddress + offset;
                    Addresses.Add(nextAddress);
                    currentAddress = ReadLong(nextAddress);
                    AddressValues.Add(currentAddress);
                }
                BaseAddress = currentAddress;
            }
        }

        private int ReadLong(int address)
        {
            var bytes = Memory.ReadBytes(address, 8);
            var longValue = BitConverter.ToInt32(bytes, 0);
            return longValue;
        }

        public virtual void Refresh(int? parentAddesss)
        {
            CalcBaseAddress(parentAddesss);
        }

        public object ReadValue(DataType dataType, int stringLength = 200)
        {
            switch (dataType)
            {
                case DataType.UTF16:
                    string stringValue = Encoding.Unicode.GetString(Memory.ReadBytes(BaseAddress, stringLength));
                    int num = stringValue.IndexOf('\0');
                    stringValue = num > 0 ? stringValue.Substring(0, num) : stringValue;
                    if (String.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }
                    return stringValue;
                case DataType.Byte:
                    return Memory.ReadBytes(BaseAddress, 1);
                case DataType.Int:
                    return BitConverter.ToInt32(Memory.ReadBytes(BaseAddress,4),0);
                case DataType.Long:
                    return BitConverter.ToInt64(Memory.ReadBytes(BaseAddress, 8), 0);
                case DataType.Float:
                    return BitConverter.ToSingle(Memory.ReadBytes(BaseAddress, 8), 0);
                case DataType.UInt:
                    return BitConverter.ToUInt32(Memory.ReadBytes(BaseAddress, 4), 0);
            }
            return null;
        }
    }
}
