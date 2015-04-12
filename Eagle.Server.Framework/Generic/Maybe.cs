using System;

namespace Eagle.Server.Framework.Generic
{
    public class Maybe<T>
    {
        private readonly T _value;

        public Maybe(T value)
        {
            HasValue = true;
            _value = value;
        }

        public Maybe()
        {
            HasValue = false;
        }

        public bool HasValue { get; private set; }

        public T Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException("Cannot access value if HasValue is false");
                return _value;
            }
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static implicit operator Maybe<T>(Maybe empty)
        {
            return new Maybe<T>();
        }
    }

    public class Maybe
    {
        private Maybe()
        {
        }

        public static readonly Maybe Empty = new Maybe();

        public static Maybe<T> Value<T>(T value)
        {
            return new Maybe<T>(value);
        }
    }
}
