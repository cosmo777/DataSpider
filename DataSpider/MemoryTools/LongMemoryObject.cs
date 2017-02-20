using System;
using System.Collections.Generic;
using System.Text;

namespace DataSpider.MemoryTools
{
    public class LongMemoryObject
    {
        protected Memory Memory { get; }
        public long BaseAddress { get; set; }
        public long[] AddressMappings { get; set; }
        public List<long> Addresses { get; set; }
        public List<long> AddressValues { get; set; }

        public LongMemoryObject(Memory memory, long? parentAddesss, params long[] addressMappings)
        {
            Memory = memory;
            AddressMappings = addressMappings;
            Addresses = new List<long>();
            AddressValues = new List<long>();
            CalcBaseAddress(parentAddesss);
        }

        public void CalcBaseAddress(long? parentAddress)
        {
            AddressValues.Clear();
            Addresses.Clear();
            if (AddressMappings.Length > 0 || parentAddress.HasValue)
            {
                int firstIndex;
                long firstAddress;

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
                long currentAddress = ReadLong(firstAddress);
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

        private long ReadLong(long address)
        {
            var bytes = Memory.ReadBytes(address, 8);
            var longValue = BitConverter.ToInt64(bytes, 0);
            return longValue;
        }


        public virtual void Refresh(long? parentAddesss)
        {
            CalcBaseAddress(parentAddesss);
        }

        public object ReadValue(DataType dataType,int offset, int stringLength = 200)
        {
            switch (dataType)
            {
                case DataType.UTF16:
                    string stringValue = Encoding.Unicode.GetString(Memory.ReadBytes(BaseAddress+ offset, stringLength));
                    int num = stringValue.IndexOf('\0');
                    if (num == 0)
                    {
                        return String.Empty;
                    }
                    stringValue = num > 0 ? stringValue.Substring(0, num) : stringValue;
                    if (String.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }
                    return stringValue;
                case DataType.Byte:
                    return Memory.ReadBytes(BaseAddress, 1);
                case DataType.Int:
                    return BitConverter.ToInt32(Memory.ReadBytes(BaseAddress, 4), 0);
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
