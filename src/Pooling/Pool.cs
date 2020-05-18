using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LiteLoader.Pooling
{
    public sealed class Pool
    {
        static Pool()
        {
            ArrayPools = new Dictionary<Type, IArrayPool>();
        }

        public static void Free(object obj)
        {
            if (obj == null)
            {
                return;
            }

            Type type = obj.GetType();
            
            if (type.IsArray)
            {
                Type element = type.GetElementType();
                lock (ArrayPools)
                {
                    if (ArrayPools.TryGetValue(element, out IArrayPool arrayPool))
                    {
                        arrayPool.Free((Array)obj);
                    }
                }

                return;
            }
        }

        #region ArrayPool

        private static readonly Dictionary<Type, IArrayPool> ArrayPools;

        public static T[] Array<T>(int length)
        {
            Type type = typeof(T);
            Array get = Array(length, type);
            return (T[])get;
        }

        public static Array Array(int length, Type type)
        {
            if (length < 0)
            {
                throw new ArgumentException("Array Length can't be less then zero", nameof(length));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            lock (ArrayPools)
            {
                if (ArrayPools.TryGetValue(type, out IArrayPool pool))
                {
                    return pool.Get(length);
                }

                Type pt = typeof(ArrayPool<>).MakeGenericType(type);

                pool = (IArrayPool)Activator.CreateInstance(pt, 25, 150, 50);
                ArrayPools.Add(type, pool);
                return pool.Get(length);
            }
        }

        #endregion
    }
}
