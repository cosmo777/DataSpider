using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataSpider.Annotations;

namespace DataSpider
{
    public class SpiderResult : INotifyPropertyChanged
    {
        private string _current;

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
            Address0 = parent.Address0;
            Address1 = parent.Address1;
            Address2 = parent.Address2;
            Address3 = parent.Address3;
            Address4 = parent.Address4;
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


        public void SetOffset(int level, int offset, long address)
        {
            switch (level)
            {
                case 0:
                    Offset0 = offset;
                    Address0 = address;
                    break;
                case 1:
                    Offset1 = offset;
                    Address1 = address;
                    break;
                case 2:
                    Offset2 = offset;
                    Address2 = address;
                    break;
                case 3:
                    Offset3 = offset;
                    Address3 = address;
                    break;
                case 4:
                    Offset4 = offset;
                    Address4 = address;
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

        public string Current
        {
            get { return _current; }
            set
            {
                if (value == _current) return;
                _current = value;
                OnPropertyChanged();
            }
        }

        public string Value { get; set; }
        public long Address0 { get; set; }
        public long Address1 { get; set; }
        public long Address2 { get; set; }
        public long Address3 { get; set; }
        public long Address4 { get; set; }
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
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
