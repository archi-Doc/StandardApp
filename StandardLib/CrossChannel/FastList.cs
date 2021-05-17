// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Arc.CrossChannel
{
    // NOT thread safe, highly customized for XChannel.
    internal sealed class FastList<T> : IDisposable
        where T : class
    {
        private const int InitialCapacity = 4;
        private const int MinCleanupStart = 8;

        public delegate ref int ObjectToIndexDelegete(T obj);

        private T?[] values = default!;
        private int count;
        private FastIntQueue freeIndex = default!;

        public FastList(ObjectToIndexDelegete objectToIndex)
        {
            this.objectToIndex = objectToIndex;
            this.Initialize();
        }

        internal int CleanupCount { get; set; } // no lock, not thread safe

        public T?[] GetValues() => this.values; // no lock, safe for iterate

        public bool IsDisposed => this.freeIndex == null;

        public bool IsEmpty => this.count == 0;

        internal bool IsCleanupRequired => this.values.Length > MinCleanupStart && (this.values.Length > this.count * 2);

        public int Add(T value)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastList<T>));
            }

            if (this.freeIndex.Count != 0)
            {
                var index = this.freeIndex.Dequeue();
                this.objectToIndex(value) = index;
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
                this.objectToIndex(value) = index;
                newValues[this.values.Length] = value;
                this.count++;
                Volatile.Write(ref this.values, newValues);
                return index;
            }
        }

        public bool Remove(T value)
        {
            if (this.IsDisposed)
            {
                return true;
            }

            ref var index = ref this.objectToIndex(value);
            ref var v = ref this.values[index];
            if (v == null)
            {
                throw new KeyNotFoundException($"key index {index} is not found.");
            }

            v = default(T);
            this.freeIndex.Enqueue(index);
            index = -1;
            this.count--;

            return this.count == 0;
        }

        public bool Cleanup()
        {
            if (!this.IsCleanupRequired)
            {
                return false;
            }

            var nextSize = this.values.Length / 2;
            for (var i = nextSize; i < this.values.Length; i++)
            {
            }

            return true;
        }

        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.freeIndex = null!;
            this.values = Array.Empty<T?>();
            this.count = 0;
        }

        private ObjectToIndexDelegete objectToIndex;

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
