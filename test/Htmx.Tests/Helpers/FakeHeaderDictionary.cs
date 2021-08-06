using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Htmx.Tests
{
    public class FakeHeaderDictionary : IHeaderDictionary
    {
        private Dictionary<string, StringValues> values
            = new Dictionary<string, StringValues>();

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, StringValues> item)
        {
            values.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, StringValues> item)
        {
            return values.ContainsKey(item.Key) && values.ContainsValue(item.Value);
        }

        public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, StringValues> item)
        {
            return values.Remove(item.Key);
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public void Add(string key, StringValues value)
        {
            values.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return values.Remove(key);
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            return values.TryGetValue(key, out value);
        }

        public StringValues this[string key]
        {
            get => values[key];
            set => values[key] = value;
        }

        public long? ContentLength { get; set; }
        public ICollection<string> Keys => values.Keys;
        public ICollection<StringValues> Values => values.Values;
    }
}