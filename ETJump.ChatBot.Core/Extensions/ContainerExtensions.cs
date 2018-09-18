using System;
using System.Collections.Generic;
using System.Text;

namespace ETJump.ChatBot.Core.Extensions
{
    public static class ContainerExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }
    }
}
