﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWinformsTest
{
    public class ConfigurationRoot<TKey, TValue> : Dictionary<TKey, TValue> where TValue : class
    {
        public new void Add(TKey key, TValue value)
        {
            if (value == null)
            {
                // adding null value is pointless...
                return;
            }
            base.Add(key, value);
        }

        public new void Remove(TKey key)
        {
            if (!ContainsKey(key))
            {
                // nothing to do
                return;
            }
            base.Remove(key);
        }

        public new TValue this[TKey key]
        {
            get
            {
                TValue value;
                return TryGetValue(key, out value) ? value : null;
            }
            set
            {
                if (value == null)
                {
                    // setting value null is same as removing it
                    Remove(key);
                }
                else
                {
                    base[key] = value;
                }
            }
        }
    }

}
