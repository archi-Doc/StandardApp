// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Arc.CrossChannel
{
    internal sealed class FreeList<T> : IDisposable
        where T : class
    {
        private const int InitialCapacity = 4;
        private const int MinShrinkStart = 8;

        private readonly object gate = new object();

        private T?[] values = default!;
        private int count;
        private FastQueue<int> freeIndex = default!;
        private bool isDisposed;

        public FreeList()
        {
            this.Initialize();
        }

        public T?[] GetValues() => this.values; // no lock, safe for iterate

        public int GetCount()
        {
            lock (this.gate)
            {
                return this.count;
            }
        }

        public int Add(T value)
        {
            lock (this.gate)
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(nameof(FreeList<T>));
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

        public void Remove(int index, bool shrinkWhenEmpty)
        {
            lock (this.gate)
            {
                if (this.isDisposed)
                {
                    return; // do nothing
                }

                ref var v = ref this.values[index];
                if (v == null)
                {
                    throw new KeyNotFoundException($"key index {index} is not found.");
                }

                v = null;
                this.freeIndex.Enqueue(index);
                this.count--;

                if (shrinkWhenEmpty && this.count == 0 && this.values.Length > MinShrinkStart)
                {
                    this.Initialize(); // re-init.
                }
            }
        }

        /// <summary>
        /// Dispose and get cleared count.
        /// </summary>
        /// <param name="clearedCount">The number of items cleared.</param>
        /// <returns>True if successfully disposed.</returns>
        public bool TryDispose(out int clearedCount)
        {
            lock (this.gate)
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
            lock (this.gate)
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
            this.freeIndex = new FastQueue<int>(InitialCapacity);
            for (int i = 0; i < InitialCapacity; i++)
            {
                this.freeIndex.Enqueue(i);
            }

            this.count = 0;

            var v = new T?[InitialCapacity];
            Volatile.Write(ref this.values, v);
        }
    }

    internal class FastQueue<T>
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
                this.ThrowForFullQueue();
            }

            this.array[this.tail] = item;
            this.MoveNext(ref this.tail);
            this.size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            if (this.size == 0)
            {
                this.ThrowForEmptyQueue();
            }

            var head = this.head;
            var array = this.array;
            var removed = array[head];
            array[head] = default!;
            this.MoveNext(ref this.head);
            this.size--;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == this.array.Length)
            {
                tmp = 0;
            }

            index = tmp;
        }

        private void ThrowForEmptyQueue() => throw new InvalidOperationException("Queue is empty.");

        private void ThrowForFullQueue() => throw new InvalidOperationException("Queue is full.");
    }
}
