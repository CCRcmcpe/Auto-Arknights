using System.Collections.Generic;

namespace REVUnit.AutoArknights.Core
{
    public class Cache<TId, TItem>
    {
        private readonly Dictionary<TId, TItem> _dict = new Dictionary<TId, TItem>();
        public IEnumerable<TItem> CachedItems => _dict.Values;

        public bool Get(TId id, out TItem result)
        {
            return _dict.TryGetValue(id, out result);
        }

        public void Register(TId id, TItem item)
        {
            _dict.Add(id, item);
        }
    }
}