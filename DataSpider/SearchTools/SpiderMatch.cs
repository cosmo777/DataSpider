using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSpider.SearchTools
{
    public class SpiderMatch: OffsetObject
    {

        public SpiderMatch()
        {
            Values = new List<SpiderMatchValue>();
        }

        public SpiderMatch(OffsetObject objectToCopy):base(objectToCopy)
        {
            Values = new List<SpiderMatchValue>();
        }

        public List<SpiderMatchValue> Values { get; set; }
    }
}
