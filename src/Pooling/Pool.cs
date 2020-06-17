using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteLoader.Pooling
{
    public sealed class Pool
    {
        private static readonly IArrayPool<object> _poolHelpers;
        private static readonly Dictionary<Type, object> _pools;
        private static readonly MethodInfo _freePool;
        private static readonly MethodInfo _getPool;
        private static readonly MethodInfo _getArray;

        static Pool()
        {
            _pools = new Dictionary<Type, object>();
            _poolHelpers = new ArrayPool<object>(1, 1, 10);
            _getArray = typeof(IArrayPool<>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.Equals("Get", StringComparison.Ordinal))
                .FirstOrDefault(m => m.GetParameters().Length == 0 && m.GetParameters()[0].ParameterType == typeof(int));
            _freePool = typeof(IPool<>)
                .GetMethod("Free");
            _getPool = typeof(IPool<>)
                .GetMethod("Get");
        }

        public static object Get(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            object pool;
            lock (_pools)
            {
                if (!_pools.TryGetValue(type, out pool))
                {
                    throw new NullReferenceException($"IPool<{type.FullName}> not found");
                }
            }

            return _getPool.Invoke(pool, _poolHelpers.Empty);
        }

        public static void Free(object obj)
        {
            if (obj == null)
            {
                return;
            }

            Type objType = obj.GetType();

            object pool;
            lock (_pools)
            {
                if (!_pools.TryGetValue(objType, out pool))
                {
                    return;
                }
            }

            object[] free = _poolHelpers.Get(1);
            free[0] = obj;
            _freePool.Invoke(pool, free);
            _poolHelpers.Free(free);
        }

        #region ArrayPool

        public static T[] Array<T>(int length)
        {
            Type type = typeof(T);
            Array get = Array(type, length);
            return (T[])get;
        }

        public static Array Array(Type type, int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Array Length can't be less then zero", nameof(length));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsArray)
            {
                type = type.MakeArrayType();
            }

            object pool;
            lock (_pools)
            {
                if (!_pools.TryGetValue(type, out pool))
                {
                    pool = CreateArrayPool(type, 0, 255, 100);
                    _pools.Add(type, pool);
                }
            }

            object[] args = _poolHelpers.Get(1);
            args[0] = length;
            Array array = (Array)_getArray.Invoke(pool, args);
            _poolHelpers.Free(args);
            return array;
        }

        private static object CreateArrayPool(Type type, int initalItemsPerLength, int maxArrayLength, int maxPooledArrays)
        {
            Type poolType = typeof(ArrayPool<>).MakeGenericType(type);
            object pool = Activator.CreateInstance(poolType, initalItemsPerLength, maxArrayLength, maxPooledArrays);
            return pool;
        }

        #endregion
    }
}
