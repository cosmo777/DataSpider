using System.ComponentModel;

namespace DataSpider.SearchTools
{
    public class SpiderResult : OffsetObject, INotifyPropertyChanged
    {
        private string _current;

        public SpiderResult()
        {

        }

        public SpiderResult(OffsetObject objectToCopy):base(objectToCopy)
        {

        }
        

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
    }
}
