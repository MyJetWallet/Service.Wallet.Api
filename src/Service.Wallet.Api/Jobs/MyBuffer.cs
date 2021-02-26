using System.Collections.Generic;

namespace Service.Wallet.Api.Jobs
{
    public class MyBuffer<T>
    {
        private List<T> _data = new List<T>();
        private object _gate = new object();

        public void Add(T item)
        {
            lock (_gate)
            {
                _data.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_gate)
            {
                _data.AddRange(items);
            }
        }

        public List<T> ExtractAll()
        {
            lock (_gate)
            {
                var res = _data;
                _data = new List<T>();
                return res;
            }
        }
    }
}