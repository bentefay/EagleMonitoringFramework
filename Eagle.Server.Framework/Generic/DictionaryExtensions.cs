using System;
using System.Collections.Generic;

namespace Eagle.Server.Framework.Generic
{
    public static class DictionaryExtensions
    {
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, TValue @default)
        {
            return target.GetValue(key, () => @default);
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, Func<TValue> factory)
        {
            TValue value;
            return target.TryGetValue(key, out value) ? value : factory();
        }

        public static TValue GetValueOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, TValue @default)
        {
            return target.GetValueOrAdd(key, () => @default);
        }

        public static TValue GetValueOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, Func<TValue> factory)
        {
            TValue value;
            if (target.TryGetValue(key, out value))
                return value;

            target[key] = value = factory();
            return value;
        }

        public static TValue ChangeValue<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, Func<TValue, TValue> operation, Func<TValue> factory)
        {
            TValue value;
            if (target.TryGetValue(key, out value))
                target[key] = value = operation(value);
            else
                target[key] = value = factory();

            return value;
        }

        public static Maybe<TValue> ChangeValue<TKey, TValue>(this Dictionary<TKey, TValue> target, TKey key, Func<TValue, TValue> operation)
        {
            TValue value;
            if (!target.TryGetValue(key, out value)) 
                return Maybe.Empty;

            target[key] = value = operation(value);
            return value;
        }
    }
}
