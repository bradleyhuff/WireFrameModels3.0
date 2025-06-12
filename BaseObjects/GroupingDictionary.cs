using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseObjects
{
    public class GroupingDictionary<K, V>: Dictionary<K, V>
    {
        private Func<V> getNew;
        public GroupingDictionary(Func<V> getNew)
        {
            this.getNew = getNew;
        }

        public new V this[K key]
        {
            get
            {
                if (!base.ContainsKey(key)) { base[key] = getNew(); }
                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
