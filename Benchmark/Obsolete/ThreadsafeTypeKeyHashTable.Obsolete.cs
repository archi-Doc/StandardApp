// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable SA1202 // Elements should be ordered by access
#pragma warning disable SA1214 // Readonly fields should appear before non-readonly fields

namespace Arc.CrossChannel.Obsolete
{
    public class ThreadsafeTypeKeyHashtable
    {
        private const int MinLogCapacity = 2;
        private const int MaxLogCapacity = 31;
        // private static readonly Node DeletedNode = new Node(typeof(DeletedClass), null);

        private struct Node
        {
            public Type? Key;
            public object? Value;

            public bool IsEmpty => this.Key == null;

            public bool IsDeleted => this.Key == typeof(DeletedClass);

            public bool IsEmptyOrDeleted => this.Key == null || this.Key == typeof(DeletedClass);

            public override string ToString() => $"{this.Key?.ToString()} - {this.Value?.ToString()}";

            internal Node(Type? key, object? value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        private class DeletedClass
        {
        }

        private Node[] nodes;
        private readonly object writerLock = new object();

        public ThreadsafeTypeKeyHashtable(int capacity = 4)
        {
            var n = 1 << GetLog(capacity);
            this.nodes = new Node[n];
            this.Count = 0;
        }

        public int Count { get; private set; }

        public object? this[Type key]
        {
            get
            {// Thread safe
                var result = this.FindNode(key);
                if (result.index == -1)
                {
                    throw new KeyNotFoundException();
                }

                return result.value;
            }

            set
            {
                lock (this.writerLock)
                {
                    var result = this.Probe(key, value);
                    if (!result.newlyAdded)
                    {
                        this.nodes[result.nodeIndex].Value = value;
                    }
                }
            }
        }

        private static int GetLog(int capacity)
        {
            var log = -1;
            var n = capacity;
            while (n > 0)
            {
                log++;
                n >>= 1;
            }

            if (capacity != (1 << log))
            {
                log++;
            }

            if (log < MinLogCapacity)
            {
                log = MinLogCapacity;
            }
            else if (log > MaxLogCapacity)
            {
                log = MaxLogCapacity;
            }

            return log;
        }

        /// <summary>
        /// Adds an element to a collection. If the element is already in the map, this method returns the stored element without creating a new node, and sets newlyAdded to false.
        /// <br/>O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>nodeIndex: the added <see cref="ThreadsafeTypeKeyHashtable.Node"/>.<br/>
        /// newlyAdded:true if the new key is inserted.</returns>
        public (int nodeIndex, bool newlyAdded) Add(Type key, object? value)
        {
            lock (this.writerLock)
            {
                return this.Probe(key, value);
            }
        }

        /// <summary>
        /// Removes the element with the specified key from a collection.
        /// <br/>O(1) operation.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is found and successfully removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Type key)
        {
            lock (this.writerLock)
            {
                var result = this.FindNode(key);
                if (result.index == -1)
                {
                    return false;
                }

                Volatile.Write(ref this.nodes[result.index].Key, typeof(DeletedClass));
                Volatile.Write(ref this.nodes[result.index].Value, null);
                return true;
            }
        }

        /// <summary>
        /// Searches for the <see cref="ThreadsafeTypeKeyHashtable.Node"/> index with the specified key.
        /// </summary>
        /// <param name="key">The key to search in a collection.</param>
        /// <returns>The node index with the specified key( -1: if not found), value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int index, object? value) FindNode(Type key)
        {// Thread safe
            var table = this.nodes;
            var hashMask = table.Length - 1;
            var hashCode = key.GetHashCode();
            var index = hashCode & hashMask;
            while (true)
            {
                if (table[index].IsEmpty)
                {
                    return (-1, table[index].Value);
                }
                else if (table[index].Key == key)
                {
                    return (index, table[index].Value);
                }

                index = (index + 1) & hashMask;
            }
        }

        /// <summary>
        /// Adds an element to the collection. If the element is already in the collection, this method returns the stored node without creating a new node.
        /// <br/>O(1) operation.
        /// </summary>
        /// <param name="key">The element to add to the set.</param>
        /// <returns>node: the added <see cref="ThreadsafeTypeKeyHashtable.Node"/>.<br/>
        /// newlyAdded: true if the new key is inserted.</returns>
        private (int nodeIndex, bool newlyAdded) Probe(Type key, object? value)
        {// writerLock required.
            if (this.Count >= (this.nodes.Length >> 1))
            {
                this.Resize();
            }

            var hashMask = this.nodes.Length - 1;
            var hashCode = key.GetHashCode();
            var index = hashCode & hashMask;
            var newIndex = -1;
            while (true)
            {
                if (this.nodes[index].IsEmpty)
                {
                    if (newIndex == -1)
                    {
                        newIndex = index;
                    }

                    break;
                }
                else if (this.nodes[index].IsDeleted)
                {
                    if (newIndex == -1)
                    {
                        newIndex = index;
                    }
                }
                else if (this.nodes[index].Key == key)
                {
                    return (index, false);
                }

                index = (index + 1) & hashMask;
            }

            this.Count++;
            Volatile.Write(ref this.nodes[newIndex].Key, key);
            Volatile.Write(ref this.nodes[newIndex].Value, value);
            return (newIndex, true);
        }

        private void Resize()
        {// writerLock required.
            var hashMask = this.nodes.Length - 1;
            var nextCapacity = this.nodes.Length << 1;
            var nextMask = nextCapacity - 1;
            var nextTable = new Node[nextCapacity];
            for (var i = 0; i < this.nodes.Length; i++)
            {
                if (this.nodes[i].IsEmptyOrDeleted)
                {
                    continue;
                }

                var hashCode = this.nodes[i].Key!.GetHashCode();
                var index = hashCode & nextMask;
                while (true)
                {
                    if (nextTable[index].IsEmpty)
                    {
                        nextTable[index].Key = this.nodes[i].Key;
                        nextTable[index].Value = this.nodes[i].Value;
                        break;
                    }

                    index = (index + 1) & hashMask;
                }
            }

            System.Threading.Volatile.Write(ref this.nodes, nextTable); // replace field (threadsafe for read)
        }
    }
}
