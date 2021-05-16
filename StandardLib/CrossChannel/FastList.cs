// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Arc.CrossChannel
{
    internal sealed class FastList<T> : IDisposable
        where T : class
    {
        private const int InitialCapacity = 4;
        private const int MinShrinkStart = 8;

        private readonly object cs = new();

        private T?[] values = default!;
        private int count;
        private FastIntQueue freeIndex = default!;
        private bool isDisposed;

        public FastList()
        {
            this.Initialize();
        }

        public int CleanupCount { get; set; } // no lock, not thread safe

        public T?[] GetValues() => this.values; // no lock, safe for iterate

        public bool IsEmpty => this.count == 0;

        internal bool IsShrinkRequired => this.values.Length > MinShrinkStart && (this.values.Length > this.count * 2);

        public int Add(T value)
        {
            lock (this.cs)
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(nameof(FastList<T>));
                }

                if (this.freeIndex.Count != 0)
                {
                    var index = this.freeIndex.Dequeue();
                    this.values[index] = value;
                    this.count++;
                    return index;
                }
                else
                {
                    // resize
                    var newValues = new T[this.values.Length * 2];
                    Array.Copy(this.values, 0, newValues, 0, this.values.Length);
                    this.freeIndex.EnsureNewCapacity(newValues.Length);
                    for (var i = this.values.Length; i < newValues.Length; i++)
                    {
                        this.freeIndex.Enqueue(i);
                    }

                    var index = this.freeIndex.Dequeue();
                    newValues[this.values.Length] = value;
                    this.count++;
                    Volatile.Write(ref this.values, newValues);
                    return index;
                }
            }
        }

        public bool Remove(int index)
        {
            lock (this.cs)
            {
                if (this.isDisposed)
                {
                    return true;
                }

                ref var v = ref this.values[index];
                if (v == null)
                {
                    throw new KeyNotFoundException($"key index {index} is not found.");
                }

                v = default(T);
                this.freeIndex.Enqueue(index);
                this.count--;

                return this.count == 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryShrink()
        {
            if (this.IsShrinkRequired)
            {
                return this.Shrink();
            }

            return false;
        }

        public bool Shrink()
        {
            lock (this.cs)
            {
                if (!this.IsShrinkRequired)
                {
                    return false;
                }

                var nextSize = this.values.Length / 2;
                for (var i = nextSize; i < this.values.Length; i++)
                {
                }
            }

            return true;
        }

        /// <summary>
        /// Dispose and get the number of cleared items.
        /// </summary>
        /// <param name="clearedCount">The number of cleared items.</param>
        /// <returns>True if successfully disposed.</returns>
        public bool TryDispose(out int clearedCount)
        {
            lock (this.cs)
            {
                if (this.isDisposed)
                {
                    clearedCount = 0;
                    return false;
                }

                clearedCount = this.count;
                this.Dispose();
                return true;
            }
        }

        public void Dispose()
        {
            lock (this.cs)
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.isDisposed = true;

                this.freeIndex = null!;
                this.values = Array.Empty<T?>();
                this.count = 0;
            }
        }

        private void Initialize()
        {
            this.freeIndex = new FastIntQueue(InitialCapacity);
            for (int i = 0; i < InitialCapacity; i++)
            {
                this.freeIndex.Enqueue(i);
            }

            this.count = 0;

            var v = new T?[InitialCapacity];
            Volatile.Write(ref this.values, v);
        }
    }

    internal class FastIntQueue
    {
        private int[] array;
        private int head;
        private int tail;
        private int size;

        public FastIntQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            this.array = new int[capacity];
            this.head = 0;
            this.tail = 0;
            this.size = 0;
        }

        public int Count => this.size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(int item)
        {
            if (this.size == this.array.Length)
            {
                throw new InvalidOperationException("Queue is full.");
            }

            this.array[this.tail] = item;
            this.size++;
            this.tail++;
            if (this.tail == this.array.Length)
            {
                this.tail = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Dequeue()
        {
            if (this.size == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            var removed = this.array[this.head];
            this.array[this.head] = default!;
            this.size--;

            this.head++;
            if (this.head == this.array.Length)
            {
                this.head = 0;
            }

            return removed;
        }

        public void EnsureNewCapacity(int capacity)
        {
            var newarray = new int[capacity];
            if (this.size > 0)
            {
                if (this.head < this.tail)
                {
                    Array.Copy(this.array, this.head, newarray, 0, this.size);
                }
                else
                {
                    Array.Copy(this.array, this.head, newarray, 0, this.array.Length - this.head);
                    Array.Copy(this.array, 0, newarray, this.array.Length - this.head, this.tail);
                }
            }

            this.array = newarray;
            this.head = 0;
            this.tail = (this.size == capacity) ? 0 : this.size;
        }
    }

    /*internal class FastQueue<T>
    {
        private T[] array;
        private int head;
        private int tail;
        private int size;

        public FastQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            this.array = new T[capacity];
            this.head = 0;
            this.tail = 0;
            this.size = 0;
        }

        public int Count => this.size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (this.size == this.array.Length)
            {
                throw new InvalidOperationException("Queue is full.");
            }

            this.array[this.tail] = item;
            this.size++;
            this.tail++;
            if(this.tail == this.array.Length)
            {
                this.tail = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            if (this.size == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            var removed = this.array[this.head];
            this.array[this.head] = default!;
            this.size--;

            this.head++;
            if (this.head == this.array.Length)
            {
                this.head = 0;
            }

            return removed;
        }

        public void EnsureNewCapacity(int capacity)
        {
            var newarray = new T[capacity];
            if (this.size > 0)
            {
                if (this.head < this.tail)
                {
                    Array.Copy(this.array, this.head, newarray, 0, this.size);
                }
                else
                {
                    Array.Copy(this.array, this.head, newarray, 0, this.array.Length - this.head);
                    Array.Copy(this.array, 0, newarray, this.array.Length - this.head, this.tail);
                }
            }

            this.array = newarray;
            this.head = 0;
            this.tail = (this.size == capacity) ? 0 : this.size;
        }
    }*/
}
