using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworksXD.DialogueXD.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<K, V>(this SerializableDictionary<K, List<V>> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
                return;
            }
            dictionary.Add(key, new List<V>() { value });
        }
    }
}
