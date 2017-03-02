using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataSpider.Annotations;

namespace DataSpider.SearchTools
{
    public class SpiderSearch : INotifyPropertyChanged
    {

        public SpiderSearch()
        {
            Results = new List<SpiderResult>();
        }

        public string Name { get; set; }
        public string StringValue { get; set; }

        public long LongValue { get; set; }

        public float FloatValue { get; set; }

        public DataType DataType { get; set; }

        public bool Aligned { get; set; }

        public int Variance { get; set; }

        public int Levels { get; set; }

        public int Length { get; set; }

        public long Address { get; set; }

        public int Alignment { get; set; }
        public int EndIncrement { get; set; }

        public bool Is64Bit { get; set; }

        public bool Match { get; set; }

        public int StringLength { get; set; }

        public int ExtraRead { get; set; }

        public List<SpiderResult> Results { get; set; }

        public void Reload(Memory memory)
        {
            Parallel.ForEach(Results, o =>
            {
                o.Current = o.ReadValue(memory, DataType, Is64Bit, Address, StringLength).ToString();

            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}