namespace DataSpider
{

    public enum DataType
    {
        Byte,
        Int,
        Long,
        Float,
        UInt,
        UTF16
    }

    public class SpiderResult
    {
        public SpiderResult()
        {

        }

        public SpiderResult(SpiderResult parent)
        {
            if (parent == null)
            {
                return;
            }
            Level = parent.Level;
            HexValue = parent.HexValue;
            Offset0 = parent.Offset0;
            Offset1 = parent.Offset1;
            Offset2 = parent.Offset2;
            Offset3 = parent.Offset3;
            Offset4 = parent.Offset4;
            Offset5 = parent.Offset5;
            Offset6 = parent.Offset6;
            Offset7 = parent.Offset7;
            Offset8 = parent.Offset8;
            Offset9 = parent.Offset9;
            Offset10 = parent.Offset10;
        }

        public void SetOffset(int level, int offset)
        {
            switch (level)
            {
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
        public long Address { get; set; }
        public int Level { get; set; }
        public string HexValue { get; set; }
        public string Value { get; set; }
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
    }

    public class SpiderSearchRequest
    {
        public string StringValue { get; set; }
        public long LongValue { get; set; }
        public float FloatValue { get; set; }
        public DataType DataType { get; set; }
        public bool Aligned { get; set; }
        public int Variance { get; set; }
        public int Levels { get; set; }
        public int Length { get; set; }
        public long Address { get; set; }
        public int AddressIncrement { get; set; }
        public int EndIncrement { get; set; }
        public bool Is64Bit { get; set; }
        public bool Match { get; set; }
        public int StringLength { get; set; }
        public int ExtraRead { get; set; }
    }
}
