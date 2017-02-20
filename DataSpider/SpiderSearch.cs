using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using DataSpider.Annotations;
using DataSpider.MemoryTools;

namespace DataSpider
{
    public class SpiderSearch : INotifyPropertyChanged
    {
        private int _extraRead;
        private int _stringLength;
        private bool _match;
        private bool _is64Bit;
        private int _endIncrement;
        private int _alignment;
        private long _address;
        private int _length;
        private int _levels;
        private int _variance;
        private bool _aligned;
        private DataType _dataType;
        private float _floatValue;
        private long _longValue;
        private string _stringValue;
        private string _name;

        public SpiderSearch()
        {
            Results = new ObservableCollection<SpiderResult>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string StringValue
        {
            get { return _stringValue; }
            set
            {
                if (value == _stringValue) return;
                _stringValue = value;
                OnPropertyChanged();
            }
        }

        public long LongValue
        {
            get { return _longValue; }
            set
            {
                if (value == _longValue) return;
                _longValue = value;
                OnPropertyChanged();
            }
        }

        public float FloatValue
        {
            get { return _floatValue; }
            set
            {
                if (value.Equals(_floatValue)) return;
                _floatValue = value;
                OnPropertyChanged();
            }
        }

        public DataType DataType
        {
            get { return _dataType; }
            set
            {
                if (value == _dataType) return;
                _dataType = value;
                OnPropertyChanged();
            }
        }

        public bool Aligned
        {
            get { return _aligned; }
            set
            {
                if (value == _aligned) return;
                _aligned = value;
                OnPropertyChanged();
            }
        }

        public int Variance
        {
            get { return _variance; }
            set
            {
                if (value == _variance) return;
                _variance = value;
                OnPropertyChanged();
            }
        }

        public int Levels
        {
            get { return _levels; }
            set
            {
                if (value == _levels) return;
                _levels = value;
                OnPropertyChanged();
            }
        }

        public int Length
        {
            get { return _length; }
            set
            {
                if (value == _length) return;
                _length = value;
                OnPropertyChanged();
            }
        }

        public long Address
        {
            get { return _address; }
            set
            {
                if (value == _address) return;
                _address = value;
                OnPropertyChanged();
            }
        }

        public int Alignment
        {
            get { return _alignment; }
            set
            {
                if (value == _alignment) return;
                _alignment = value;
                OnPropertyChanged();
            }
        }

        public int EndIncrement
        {
            get { return _endIncrement; }
            set
            {
                if (value == _endIncrement) return;
                _endIncrement = value;
                OnPropertyChanged();
            }
        }

        public bool Is64Bit
        {
            get { return _is64Bit; }
            set
            {
                if (value == _is64Bit) return;
                _is64Bit = value;
                OnPropertyChanged();
            }
        }

        public bool Match
        {
            get { return _match; }
            set
            {
                if (value == _match) return;
                _match = value;
                OnPropertyChanged();
            }
        }

        public int StringLength
        {
            get { return _stringLength; }
            set
            {
                if (value == _stringLength) return;
                _stringLength = value;
                OnPropertyChanged();
            }
        }

        public int ExtraRead
        {
            get { return _extraRead; }
            set
            {
                if (value == _extraRead) return;
                _extraRead = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SpiderResult> Results { get; set; }

        public void Reload(Memory memory)
        {
            foreach (var spiderResult in Results)
            {
                if (Is64Bit)
                {
                    long[] addresses = new long[spiderResult.Level];
                    for (int x = 0; x < spiderResult.Level; x++)
                    {
                        if (x == 0)
                        {
                            addresses[x] = Address + spiderResult.GetOffset(x);
                        }
                        else
                        {
                            addresses[x] = spiderResult.GetOffset(x);
                        }
                    }
                    var mapping64 = new LongMemoryObject(memory,null,addresses);
                    spiderResult.Current = mapping64.ReadValue(DataType,(int)spiderResult.GetOffset(spiderResult.Level), StringLength).ToString();
                }
                else
                {
                    int[] addresses = new int[spiderResult.Level + 1];
                    for (int x = 0; x < spiderResult.Level; x++)
                    {
                        addresses[x] = (int)spiderResult.GetOffset(x);
                    }
                    var mapping64 = new IntMemoryObject(memory, (int)Address, addresses);
                    spiderResult.Current = mapping64.ReadValue(DataType, StringLength).ToString();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}