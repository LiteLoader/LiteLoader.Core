using System;
using System.Collections.Generic;

namespace LiteLoader.Pooling
{
    public sealed class ArrayPool<T> : IArrayPool<T>
    {

        /// <summary>
        /// Initial startup count of each array length 
        /// </summary>
        public readonly int InitialPooledItems;

        /// <summary>
        /// Maximum array length
        /// </summary>
        public readonly int MaxArrayLength;

        /// <summary>
        /// Maximum amount of arrays allowed to be pooled per length
        /// </summary>
        public readonly int MaxPooledArrays;

        /// <inheritdoc cref="IArrayPool.Empty"/>
        public T[] Empty { get; }

        private readonly Type _type;
        private readonly List<Queue<T[]>> _pool;

        public ArrayPool(int initialArrayCount, int maxArrayLength, int maxPooledArrays)
        {
            if (maxArrayLength < 1)
            {
                throw new ArgumentException("Must pool at least one array with a length of 1", nameof(maxArrayLength));
            }

            if (initialArrayCount < 0)
            {
                throw new ArgumentException("Negative values are not allowed", nameof(initialArrayCount));
            }

            if (maxPooledArrays < 5)
            {
                throw new ArgumentException("Value less then 5 is not allowed", nameof(maxPooledArrays));
            }

            if (initialArrayCount > maxPooledArrays)
            {
                throw new ArgumentException("Initial count can't be larger then max value", $"{nameof(initialArrayCount)} & {nameof(maxPooledArrays)}");
            }

            InitialPooledItems = initialArrayCount;
            MaxArrayLength = maxArrayLength;
            MaxPooledArrays = maxPooledArrays;

            _type = typeof(T);
            Empty = new T[0];
            _pool = new List<Queue<T[]>>(maxArrayLength);

            for (int i = 0; i < MaxArrayLength; i++)
            {
                Queue<T[]> queue = new Queue<T[]>(InitialPooledItems);
                _pool.Add(queue);
                Setup(i + 1, queue);
            }
        }

        /// <inheritdoc cref="IArrayPool.Get" />
        public T[] Get()
        {
            return Empty;
        }

        /// <inheritdoc cref="IArrayPool.Get(int)"/>
        public T[] Get(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Negative values are not allowed", nameof(length));
            }

            if (length == 0)
            {
                return Empty;
            }

            if (length > MaxArrayLength)
            {
                return new T[length];
            }

            Queue<T[]> queue = _pool[length - 1];

            lock (queue)
            {
                if (queue.Count == 0)
                {
                    Setup(length, queue);
                }

                return queue.Dequeue();
            }
        }

        /// <inheritdoc cref="IPool.Free(T)"/>
        public void Free(T[] item)
        {
            if (item == null || item.Length == 0)
            {
                return;
            }

            for (int i = 0; i < item.Length; i++)
            {
                item[i] = default;
            }

            if (item.Length > MaxArrayLength)
            {
                return;
            }

            Queue<T[]> queue = _pool[item.Length - 1];

            lock (queue)
            {
                if (queue.Count >= MaxPooledArrays)
                {
                    return;
                }

                queue.Enqueue(item);
            }
        }

        private void Setup(int length, Queue<T[]> queue)
        {
            while (queue.Count < InitialPooledItems)
            {
                queue.Enqueue(new T[length]);
            }
        }
    }
}
