using System;
using System.Collections.Generic;

namespace REVUnit.AutoArknights.GUI.Core
{
    public class Cache<TIdentfier, TItem>
    {
        private readonly Dictionary<TIdentfier, TItem> _dict = new Dictionary<TIdentfier, TItem>();
        private readonly Dictionary<TIdentfier, int> _queriedCounts = new Dictionary<TIdentfier, int>();

        public Cache(int cacheThreshold = 3)
        {
            CacheThreshold = cacheThreshold;
        }

        public int CacheThreshold { get; set; }

        public TItem Get(TIdentfier id, Func<TItem> defaultGenerator)
        {
            if (Get(id, out TItem result)) return result;

            result = defaultGenerator();
            Register(id, result);
            return result;
        }

        public bool Get(TIdentfier id, out TItem result)
        {
            return _dict.TryGetValue(id, out result);
        }

        public void Register(TIdentfier id, TItem item)
        {
            if (!_queriedCounts.TryGetValue(id, out int queriedCount))
                _queriedCounts.Add(id, queriedCount /* it always = 0 here */);
            if (queriedCount == int.MaxValue) return;
            if (queriedCount == CacheThreshold)
            {
                _dict.Add(id, item);
                _queriedCounts[id] = int.MaxValue;
                return;
            }

            _queriedCounts[id]++;
        }
    }
}