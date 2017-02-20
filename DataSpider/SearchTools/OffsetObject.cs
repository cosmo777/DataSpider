using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataSpider.Annotations;
using DataSpider.MemoryTools;

namespace DataSpider.SearchTools
{
    public class OffsetObject
    {
        public OffsetObject()
        {

        }

        public OffsetObject(OffsetObject spiderResult)
        {
            if (spiderResult != null)
            {
                Level = spiderResult.Level;
                Offset0 = spiderResult.Offset0;
                Offset1 = spiderResult.Offset1;
                Offset2 = spiderResult.Offset2;
                Offset3 = spiderResult.Offset3;
                Offset4 = spiderResult.Offset4;
                Offset5 = spiderResult.Offset5;
                Offset6 = spiderResult.Offset6;
                Offset7 = spiderResult.Offset7;
                Offset8 = spiderResult.Offset8;
                Offset9 = spiderResult.Offset9;
                Offset10 = spiderResult.Offset10;
            }
        }


        public void SetOffset(int level, int offset, long address)
        {
            switch (level)
            {
                case 0:
                    Offset0 = offset;
                    break;
                case 1:
                    Offset1 = offset;
                    break;
                case 2:
                    Offset2 = offset;
                    break;
                case 3:
                    Offset3 = offset;
                    break;
                case 4:
                    Offset4 = offset;
                    break;
                case 5:
                    Offset5 = offset;
                    break;
                case 6:
                    Offset6 = offset;
                    break;
                case 7:
                    Offset7 = offset;
                    break;
                case 8:
                    Offset8 = offset;
                    break;
                case 9:
                    Offset9 = offset;
                    break;
                case 10:
                    Offset10 = offset;
                    break;
            }
        }

        public int Level { get; set; }
        public int Offset0 { get; set; }
        public int Offset1 { get; set; }
        public int Offset2 { get; set; }
        public int Offset3 { get; set; }
        public int Offset4 { get; set; }
        public int Offset5 { get; set; }
        public int Offset6 { get; set; }
        public int Offset7 { get; set; }
        public int Offset8 { get; set; }
        public int Offset9 { get; set; }
        public int Offset10 { get; set; }

        public long GetOffset(int level)
        {
            switch (level)
            {
                case 0:
                    return Offset0;
                case 1:
                    return Offset1;
                case 2:
                    return Offset2;
                case 3:
                    return Offset3;
                case 4:
                    return Offset4;
                case 5:
                    return Offset5;
                case 6:
                    return Offset6;
                case 7:
                    return Offset7;
                case 8:
                    return Offset8;
                case 9:
                    return Offset9;
                case 10:
                    return Offset10;
            }
            return 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object ReadValue(Memory memory, DataType dataType, bool is64Bit, long address, int stringLength=200)
        {
            if (is64Bit)
            {
                long[] addresses = new long[Level];
                for (int x = 0; x < Level; x++)
                {
                    if (x == 0)
                    {
                        addresses[x] = address + GetOffset(x);
                    }
                    else
                    {
                        addresses[x] = GetOffset(x);
                    }
                }
                var mapping64 = new LongMemoryObject(memory, null, addresses);
                return mapping64.ReadValue(dataType, (int)GetOffset(Level), stringLength).ToString();
            }
            else
            {
                int[] addresses = new int[Level];
                for (int x = 0; x < Level; x++)
                {
                    if (x == 0)
                    {
                        addresses[x] = (int)address + (int)GetOffset(x);
                    }
                    else
                    {
                        addresses[x] = (int)GetOffset(x);
                    }
                }
                var mapping64 = new IntMemoryObject(memory, null, addresses);
                return mapping64.ReadValue(dataType, (int)GetOffset(Level), stringLength).ToString();
            }
        }
    }
}