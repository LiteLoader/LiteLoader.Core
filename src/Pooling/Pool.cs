using LiteLoader.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace LiteLoader.Pooling
{
    public sealed class Pool
    {
        private static readonly IArrayPool<object> _poolHelpers;
        private static readonly Dictionary<Type, object> _pools;
        private static readonly Type _poolType;
        private static readonly Type[] _getPoolParamTypes;
        private static readonly Type[] _getArrayParamTypes;

        static Pool()
        {
            _poolType = typeof(IPool<>);
            _pools = new Dictionary<Type, object>();
            _poolHelpers = new ArrayPool<object>(1, 1, 20);
            _getArrayParamTypes = new Type[] { typeof(int) };
            _getPoolParamTypes = new Type[0];
        }

        internal static object FindPool(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_poolType.IsAssignableFrom(type))
            {
                type = type.GetGenericArguments()[0];
            }

            lock (_pools)
            {
                if (!_pools.TryGetValue(type, out object pool))
                {
                    return null;
                }

                return pool;
            }
        }

        public static void RegisterPool<T>(IPool<T> pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException(nameof(pool));
            }

            Type type = typeof(T);
            
            lock (_pools)
            {
                if (_pools.TryGetValue(type, out object poolObj))
                {
                    throw new InvalidOperationException("Pool already exists");
                }

                _pools.Add(type, pool);
                Interface.CoreModule.ServiceProvider.AddSingleton(pool);
            }
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

            return pool.GetType().GetMethod("Get", _getPoolParamTypes).Invoke(pool, _poolHelpers.Empty);
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
            pool.GetType().GetMethod("Free").Invoke(pool, free);
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

            object pool;
            Type aT = type.MakeArrayType();
            lock (_pools)
            {
                if (!_pools.TryGetValue(aT, out pool))
                {
                    pool = CreateArrayPool(type, 1, 255, 100);
                    _pools.Add(aT, pool);
                    Interface.CoreModule.ServiceProvider.AddSingleton(typeof(IPool<>).MakeGenericType(aT), pool);
                }
            }

            object[] args = _poolHelpers.Get(1);
            args[0] = length;
            Array array = (Array)pool.GetType().GetMethod("Get", _getArrayParamTypes).Invoke(pool, args);
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
