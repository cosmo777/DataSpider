using System.ComponentModel;

namespace DataSpider.SearchTools
{
    public class SpiderResult : OffsetObject
    {
        private string _current;

        public SpiderResult()
        {

        }

        public SpiderResult(OffsetObject objectToCopy):base(objectToCopy)
        {

        }


        public string Current { get; set; }

        public string Value { get; set; }
    }
}
