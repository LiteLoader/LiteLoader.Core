using System;
using System.Collections.Generic;

namespace LiteLoader.Pooling
{
    public sealed class ArrayPool<T> : IArrayPool
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
        public Array Empty { get; }

        private readonly Type _type;
        private readonly List<Queue<Array>> _pool;

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
            Empty = Array.CreateInstance(_type, 0);
            _pool = new List<Queue<Array>>(maxArrayLength);

            for (int i = 1; i < MaxArrayLength; i++)
            {
                Queue<Array> queue = new Queue<Array>(InitialPooledItems);
                _pool.Add(queue);
                Setup(i, queue);
            }
        }

        /// <inheritdoc cref="IArrayPool.Get(int)"/>
        public Array Get(int length)
        {
            Console.WriteLine($"Get Array {typeof(T)} of length {length}");
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
                return Array.CreateInstance(_type, length);
            }

            Queue<Array> queue = _pool[length - 1];

            lock (queue)
            {
                if (queue.Count == 0)
                {
                    Setup(length, queue);
                }

                return queue.Dequeue();
            }
        }

        /// <inheritdoc cref="IArrayPool.Free(Array)"/>
        public void Free(Array array)
        {
            if (array == null || array.Length == 0)
            {
                return;
            }

            Console.WriteLine($"Free Array {array.GetType().GetElementType()} of length {array.Length}");

            for (int i = 0; i < array.Length; i++)
            {
                array.SetValue(null, i);
            }

            if (array.Length > MaxArrayLength)
            {
                return;
            }

            Queue<Array> queue = _pool[array.Length - 1];

            lock (queue)
            {
                if (queue.Count >= MaxPooledArrays)
                {
                    return;
                }

                queue.Enqueue(array);
            }
        }

        private void Setup(int length, Queue<Array> queue)
        {
            while (queue.Count < InitialPooledItems)
            {
                queue.Enqueue(Array.CreateInstance(_type, length));
            }
        }
    }
}
