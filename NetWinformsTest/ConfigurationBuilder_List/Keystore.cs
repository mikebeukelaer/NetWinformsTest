using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWinformsTest
{
    [DebuggerDisplay("{DebuggerDisplayCount,nq}")]
    [DebuggerTypeProxy(typeof(KeyStoreDebugView))]
    public class Keystore : IKeyStoreManager
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<ConfigurationItem> values = new List<ConfigurationItem>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplayCount { get { return $"Sections = {values.Count} "; } }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string this[string key]
        {
            // Getter
            get
            {
                var itm = values.FirstOrDefault(x => x.Key == key);
                if (itm != null)
                {
                    return $"{itm.Value}";
                }
                return null;
            }

        }

        void IKeyStoreManager.Add(ConfigurationItem item)
        {


            var idx = values.FindIndex(itm => itm.Key == item.Key);

            if (idx == -1)
            {
                values.Add(item);
            }
            else
            {
                values[idx] = item;
            }

        }

        public Keystore Build()
        {
            return this;
        }

        private class KeyStoreDebugView
        {
            private Keystore _keyStore;

            public KeyStoreDebugView(Keystore sample)
            {
                _keyStore = sample;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public List<ConfigurationItem> Items
            {
                get
                {
                    List<ConfigurationItem> list = new List<ConfigurationItem>();
                    foreach (var itm in _keyStore.values)
                    {
                        list.Add(itm);
                    }
                    return list;
                }
            }
        }

    }
}
