using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataSpider.Annotations;
using DataSpider.MemoryTools;
using MoreLinq;

namespace DataSpider.SearchTools
{
    public class SpiderMatcher : INotifyPropertyChanged
    {
        public bool Is64Bit { get; set; }
        public DataType DataType { get; set; }
        public int StringLength { get; set; }

        public List<SpiderMatch> Matches
        {
            get { return _matches; }
            set
            {
                if (Equals(value, _matches)) return;
                _matches = value;
                OnPropertyChanged();
            }
        }

        public List<SpiderMatchSearchData> SearchData { get; private set; }

        private List<SpiderMatch> _matches;

        public SpiderMatcher()
        {
            SearchData = new List<SpiderMatchSearchData>();
            Matches = new List<SpiderMatch>();
        }

        public void Match(List<SpiderSearch> searches, Memory memory)
        {
            SearchData = searches.Select(o => new SpiderMatchSearchData { BaseAddress = o.Address, Name = o.Name }).ToList();

            if (searches.Count == 0)
            {
                Matches = new List<SpiderMatch>();
            }
            var firstSearch = searches.First();
            Is64Bit = firstSearch.Is64Bit;
            StringLength = firstSearch.StringLength;
            DataType = firstSearch.DataType;

            var hashes = new ConcurrentBag<SpiderHash>();
            Parallel.ForEach(searches, o =>
            {
                var currentHash = new SpiderHash(o);
                hashes.Add(currentHash);
            });

            var smallestHash = hashes.MinBy(o => o.Hashes.Count);
            var otherHashes = hashes.Where(o => o != smallestHash);
            Parallel.ForEach(otherHashes, hash =>
            {
                foreach (var matchHash in smallestHash.Hashes.Keys.ToList())
                {
                    if (matchHash != null)
                    {
                        if (!hash.Hashes.ContainsKey(matchHash))
                        {
                            lock (smallestHash)
                            {
                                smallestHash.Hashes.Remove(matchHash);
                            }
                        }

                    }
                }
            });
            Matches = new List<SpiderMatch>();
            foreach (var matchedResult in smallestHash.Hashes)
            {
                var match = new SpiderMatch(matchedResult.Value);
                Matches.Add(match);
                foreach (var spiderHash in hashes)
                {
                    var search = spiderHash.GetSpiderSearch();
                    var otherResult = spiderHash.Hashes[matchedResult.Key];
                    match.Values.Add(new SpiderMatchValue
                    {
                        Name = search.Name,
                        Address = search.Address,
                        Origional = otherResult.Value
                    });
                }
            }
            ReloadValues(memory);
        }

        public void ReloadValues(Memory memory)
        {
            foreach (var spiderMatch in Matches)
            {
                foreach (var searchData in SearchData)
                {
                    if (Is64Bit)
                    {
                        long[] addresses = new long[spiderMatch.Level];
                        for (int x = 0; x < spiderMatch.Level; x++)
                        {
                            if (x == 0)
                            {
                                addresses[x] = searchData.BaseAddress + spiderMatch.GetOffset(x);
                            }
                            else
                            {
                                addresses[x] = spiderMatch.GetOffset(x);
                            }
                        }
                        var mapping64 = new LongMemoryObject(memory, null, addresses);
                        var currentValue = mapping64.ReadValue(DataType, (int)spiderMatch.GetOffset(spiderMatch.Level), StringLength).ToString();
                        var searchValue = spiderMatch.Values.FirstOrDefault(o => o.Address == searchData.BaseAddress);
                        if (searchValue != null)
                        {
                            searchValue.Current = currentValue;
                        }
                    }
                    else
                    {
                    }
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

    public class SpiderHash
    {
        private readonly SpiderSearch _spiderSearch;

        public SpiderSearch GetSpiderSearch()
        {
            return _spiderSearch;
        }

        public Dictionary<string, SpiderResult> Hashes { get; }

        public SpiderHash(SpiderSearch spiderSearch)
        {
            _spiderSearch = spiderSearch;
            Hashes = new Dictionary<string, SpiderResult>();
            foreach (var spiderResult in spiderSearch.Results)
            {
                var builder = new StringBuilder();
                builder.Append(spiderResult.Level);
                builder.Append(spiderResult.Value == spiderSearch.StringValue);
                builder.Append(spiderResult.Offset0.ToString("00000"));
                builder.Append(spiderResult.Offset1.ToString("00000"));
                builder.Append(spiderResult.Offset2.ToString("00000"));
                builder.Append(spiderResult.Offset3.ToString("00000"));
                builder.Append(spiderResult.Offset4.ToString("00000"));
                builder.Append(spiderResult.Offset5.ToString("00000"));
                builder.Append(spiderResult.Offset6.ToString("00000"));
                builder.Append(spiderResult.Offset7.ToString("00000"));
                builder.Append(spiderResult.Offset8.ToString("00000"));
                builder.Append(spiderResult.Offset9.ToString("00000"));
                builder.Append(spiderResult.Offset10.ToString("00000"));
                var hash = builder.ToString();
                Hashes.Add(hash, spiderResult);
            }
        }
    }
}
