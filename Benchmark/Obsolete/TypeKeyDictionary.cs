// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
#pragma warning disable SA1124 // Do not use regions
#pragma warning disable SA1202 // Elements should be ordered by access

namespace Arc.CrossChannel;

/// <summary>
/// Represents a collection of objects. <see cref="TypeKeyDictionary{T}"/> uses a hash table structure to store objects.
/// </summary>
/// <typeparam name="T">The type of values in the collection.</typeparam>
public class TypeKeyDictionary<T>
{
    private struct Node
    {
        public const int UnusedNode = -2;

        public int HashCode; // Hash code
        public int Previous;   // Index of previous node, UnusedNode(-2) if the node is not used.
        public int Next;        // Index of next node
        public Type Key;      // Key
        public T Value; // Value

        public bool IsValid() => this.Previous != UnusedNode;

        public bool IsInvalid() => this.Previous == UnusedNode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeKeyDictionary{T}"/> class.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the collection can contain.</param>
    public TypeKeyDictionary(int capacity = 0)
    {
        this.Initialize(capacity);
    }

    private const int MinLogCapacity = 2;
    private const int MaxLogCapacity = 31;
    private int hashMask;
    private int[] buckets = default!;
    private Node[] nodes = default!;
    private int nodeCount;
    private int freeList;
    private int freeCount;

    private void Initialize(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

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

        var size = 1 << log;
        this.hashMask = size - 1;
        this.buckets = new int[size];
        for (n = 0; n < size; n++)
        {
            this.buckets[n] = -1;
        }

        this.nodes = new Node[size];
        this.freeList = -1;
    }

    /// <summary>
    /// Gets the number of nodes actually contained in the <see cref="TypeKeyDictionary{T}"/>.
    /// </summary>
    public int Count => this.nodeCount - this.freeCount;

    #region Main

    public T this[Type key]
    {
        get
        {
            var index = this.FindNode(key);
            if (index == -1)
            {
                throw new KeyNotFoundException();
            }

            return this.nodes[index].Value;
        }

        set
        {
            var result = this.Add(key, value);
            if (!result.newlyAdded)
            {
                this.nodes[result.nodeIndex].Value = value;
            }
        }
    }

    public bool TryGetValue(Type key, [MaybeNullWhen(false)] out T value)
    {
        var hashCode = key.GetHashCode();
        var index = hashCode & this.hashMask;
        var i = this.buckets[index];
        while (i >= 0)
        {
            if (this.nodes[i].HashCode == hashCode && this.nodes[i].Key == key)
            {// Identical
                value = this.nodes[i].Value;
                return true;
            }

            i = this.nodes[i].Next;
        }

        value = default;
        return false; // Not found
    }

    /// <summary>
    /// Removes all elements from a collection.
    /// </summary>
    public void Clear()
    {
        if (this.nodeCount > 0)
        {
            for (var i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i] = -1;
            }

            Array.Clear(this.nodes, 0, this.nodeCount);
            this.nodeCount = 0;
            this.freeList = -1;
            this.freeCount = 0;
        }
    }

    /// <summary>
    /// Removes the first element with the specified key from a collection.
    /// <br/>O(1) operation.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is found and successfully removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(Type key)
    {
        var p = this.FindNode(key);
        if (p == -1)
        {
            return false;
        }

        this.RemoveNode(p);
        return true;
    }

    /// <summary>
    /// Searches for the first <see cref="TypeKeyDictionary{T}.Node"/> index with the specified key.
    /// </summary>
    /// <param name="key">The key to search in a collection.</param>
    /// <returns>The first node index with the specified key. -1: not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FindNode(Type key)
    {
        var hashCode = key.GetHashCode();
        var index = hashCode & this.hashMask;
        var i = this.buckets[index];
        while (i >= 0)
        {
            if (this.nodes[i].HashCode == hashCode && this.nodes[i].Key == key)
            {// Identical
                return i;
            }

            i = this.nodes[i].Next;
        }

        return -1; // Not found
    }

    /// <summary>
    /// Determines whether the collection contains a specific key and value.
    /// </summary>
    /// <param name="key">The key to search in a collection.</param>
    /// <returns>true if the key and value is found in the collection.</returns>
    public bool Contains(Type key) => this.FindNode(key) != -1;

    /// <summary>
    /// Adds an element to a collection. If the element is already in the map, this method returns the stored element without creating a new node, and sets newlyAdded to false.
    /// <br/>O(1) operation.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns>nodeIndex: the added <see cref="TypeKeyDictionary{T}.Node"/>.<br/>
    /// newlyAdded:true if the new key is inserted.</returns>
    public (int nodeIndex, bool newlyAdded) Add(Type key, T value) => this.Probe(key, value);

    /// <summary>
    /// Removes a specified node from the collection.
    /// <br/>O(1) operation.
    /// </summary>
    /// <param name="nodeIndex">The <see cref="TypeKeyDictionary{T}.Node"/> to remove.</param>
    public void RemoveNode(int nodeIndex)
    {
        if (this.nodes[nodeIndex].IsInvalid())
        {
            return;
        }

        var nodePrevious = this.nodes[nodeIndex].Previous;
        var nodeNext = this.nodes[nodeIndex].Next;

        // node index <= this.nodeCount
        var index = this.nodes[nodeIndex].HashCode & this.hashMask;
        if (nodePrevious == -1)
        {
            this.buckets[index] = nodeNext;
        }
        else
        {
            this.nodes[nodePrevious].Next = nodeNext;
        }

        if (nodeNext != -1)
        {
            this.nodes[nodeNext].Previous = nodePrevious;
        }

        this.nodes[nodeIndex].HashCode = 0;
        this.nodes[nodeIndex].Previous = Node.UnusedNode;
        this.nodes[nodeIndex].Next = this.freeList;
        this.nodes[nodeIndex].Key = default!;
        this.nodes[nodeIndex].Value = default!;
        this.freeList = nodeIndex;
        this.freeCount++;
    }

    /// <summary>
    /// Adds an element to the map. If the element is already in the map, this method returns the stored node without creating a new node.
    /// <br/>O(1) operation.
    /// </summary>
    /// <param name="key">The element to add to the set.</param>
    /// <returns>node: the added <see cref="TypeKeyDictionary{T}.Node"/>.<br/>
    /// newlyAdded: true if the new key is inserted.</returns>
    private (int nodeIndex, bool newlyAdded) Probe(Type key, T value)
    {
        if (this.nodeCount == this.nodes.Length)
        {
            this.Resize();
        }

        int newIndex;
        var hashCode = key.GetHashCode();
        var index = hashCode & this.hashMask;
        var i = this.buckets[index];
        while (i >= 0)
        {
            if (this.nodes[i].HashCode == hashCode && this.nodes[i].Key == key)
            {// Identical
                return (i, false);
            }

            i = this.nodes[i].Next;
        }

        newIndex = this.NewNode();
        this.nodes[newIndex].HashCode = hashCode;
        this.nodes[newIndex].Key = key;
        this.nodes[newIndex].Value = value;

        if (this.buckets[index] == -1)
        {
            this.nodes[newIndex].Previous = -1;
            this.nodes[newIndex].Next = -1;
            this.buckets[index] = newIndex;
        }
        else
        {
            this.nodes[newIndex].Previous = -1;
            this.nodes[newIndex].Next = this.buckets[index];
            this.nodes[this.buckets[index]].Previous = newIndex;
            this.buckets[index] = newIndex;
        }

        return (newIndex, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NewNode()
    {
        int index;
        if (this.freeCount > 0)
        {// Free list
            index = this.freeList;
            this.freeList = this.nodes[index].Next;
            this.freeCount--;
        }
        else
        {
            index = this.nodeCount;
            this.nodeCount++;
        }

        return index;
    }

    internal void Resize()
    {
        const int minimumCapacity = 1 << MinLogCapacity;
        var newSize = this.nodes.Length << 1;
        if (newSize < minimumCapacity)
        {
            newSize = minimumCapacity;
        }

        var newMask = newSize - 1;
        var newBuckets = new int[newSize];
        for (var i = 0; i < newBuckets.Length; i++)
        {
            newBuckets[i] = -1;
        }

        var newNodes = new Node[newSize];
        Array.Copy(this.nodes, 0, newNodes, 0, this.nodeCount);

        for (var i = 0; i < this.nodeCount; i++)
        {
            ref Node newNode = ref newNodes[i];
            if (newNode.IsValid())
            {
                if (newNode.Key == null)
                {// Null list. No need to modify.
                }
                else
                {
                    var bucket = newNode.HashCode & newMask;
                    if (newBuckets[bucket] == -1)
                    {
                        newNode.Previous = -1;
                        newNode.Next = -1;
                        newBuckets[bucket] = i;
                    }
                    else
                    {
                        var newBucket = newBuckets[bucket];
                        newNode.Previous = -1;
                        newNode.Next = newBucket;
                        newBuckets[bucket] = i;
                        newNodes[newBucket].Previous = i;
                    }
                }
            }
        }

        // Update
        this.hashMask = newMask;
        this.buckets = newBuckets;
        this.nodes = newNodes;
    }

    #endregion
}
