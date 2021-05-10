// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Threading;

#pragma warning disable SA1202 // Elements should be ordered by access

namespace Arc.CrossChannel
{
    public class ThreadsafeTypeKeyHashtable
    {
        internal const int HashPrime = 101;
        private const int InitialSize = 3;

        private struct Bucket
        {
            public object? Key;
            public object? Value;
            public int HashColl; // Store hash code; sign bit means there was a collision.
        }

        private Bucket[] buckets;
        private int count;
        private int occupancy;
        private int loadsize;
        private float loadFactor;
        private volatile int version;
        private volatile bool isWriterInProgress;

        public ThreadsafeTypeKeyHashtable()
            : this(0, 1.0f)
        {
        }

        public ThreadsafeTypeKeyHashtable(int capacity)
            : this(capacity, 1.0f)
        {
        }

        public ThreadsafeTypeKeyHashtable(int capacity, float loadFactor)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (!(loadFactor >= 0.1f && loadFactor <= 1.0f))
            {
                throw new ArgumentOutOfRangeException(nameof(loadFactor));
            }

            // Based on perf work, .72 is the optimal load factor for this table.
            this.loadFactor = 0.72f * loadFactor;

            double rawsize = capacity / this.loadFactor;
            if (rawsize > int.MaxValue)
            {
                throw new ArgumentException();
            }

            // Avoid awfully small sizes
            int hashsize = (rawsize > InitialSize) ? HashHelpers.GetPrime((int)rawsize) : InitialSize;
            this.buckets = new Bucket[hashsize];

            this.loadsize = (int)(this.loadFactor * hashsize);
            this.isWriterInProgress = false;
        }

        private uint InitHash(Type key, int hashsize, out uint seed, out uint incr)
        {
            uint hashcode = (uint)key.GetHashCode() & 0x7FFFFFFF;
            seed = hashcode;
            incr = 1 + ((seed * HashPrime) % ((uint)hashsize - 1));
            return hashcode;
        }

        public virtual void Add(Type key, object value)
        {
            this.Insert(key, value, true);
        }

        public virtual void Clear()
        {
            if (this.count == 0 && this.occupancy == 0)
            {
                return;
            }

            Thread.BeginCriticalRegion();

            this.isWriterInProgress = true;
            for (var i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i].HashColl = 0;
                this.buckets[i].Key = null;
                this.buckets[i].Value = null;
            }

            this.count = 0;
            this.occupancy = 0;
            this.version++;
            this.isWriterInProgress = false;

            Thread.EndCriticalRegion();
        }

        public virtual object? this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                uint seed;
                uint incr;

                // Take a snapshot of buckets, in case another thread does a resize
                Bucket[] lbuckets = this.buckets;
                uint hashcode = this.InitHash(key, lbuckets.Length, out seed, out incr);
                int ntry = 0;

                Bucket b;
                int bucketNumber = (int)(seed % (uint)lbuckets.Length);
                do
                {
                    int currentversion;
                    int spinCount = 0;
                    do
                    {
                        // this is violate read, following memory accesses can not be moved ahead of it.
                        currentversion = this.version;
                        b = lbuckets[bucketNumber];

                        // The contention between reader and writer shouldn't happen frequently.
                        // But just in case this will burn CPU, yield the control of CPU if we spinned a few times.
                        // 8 is just a random number I pick.
                        if ((++spinCount) % 8 == 0)
                        {
                            Thread.Sleep(1);   // 1 means we are yeilding control to all threads, including low-priority ones.
                        }
                    }
                    while (this.isWriterInProgress || (currentversion != this.version));

                    if (b.Key == null)
                    {
                        return null;
                    }

                    if (((b.HashColl & 0x7FFFFFFF) == hashcode) && b.Key == key)
                    {
                        return b.Value;
                    }

                    bucketNumber = (int)(((long)bucketNumber + incr) % (uint)lbuckets.Length);
                }
                while (b.HashColl < 0 && ++ntry < lbuckets.Length);

                return null;
            }

            set
            {
                this.Insert(key, value, false);
            }
        }

        // Increases the bucket count of this hashtable. This method is called from
        // the Insert method when the actual load factor of the hashtable reaches
        // the upper limit specified when the hashtable was constructed. The number
        // of buckets in the hashtable is increased to the smallest prime number
        // that is larger than twice the current number of buckets, and the entries
        // in the hashtable are redistributed into the new buckets using the cached
        // hashcodes.
        private void Expand()
        {
            int rawsize = HashHelpers.ExpandPrime(this.buckets.Length);
            this.Rehash(rawsize, false);
        }

        // We occationally need to rehash the table to clean up the collision bits.
        private void Rehash()
        {
            this.Rehash(this.buckets.Length, false);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private void Rehash(int newsize, bool forceNewHashCode)
        {
            // reset occupancy
            this.occupancy = 0;
            Bucket[] newBuckets = new Bucket[newsize];

            // rehash table into new buckets
            int nb;
            for (nb = 0; nb < this.buckets.Length; nb++)
            {
                Bucket oldb = this.buckets[nb];
                if ((oldb.Key != null) && (oldb.Key != this.buckets))
                {
                    int hashcode = (forceNewHashCode ? oldb.Key.GetHashCode() : oldb.HashColl) & 0x7FFFFFFF;
                    this.PutEntry(newBuckets, oldb.Key, oldb.Value, hashcode);
                }
            }

            // New bucket[] is good to go - replace buckets and other internal state.
            Thread.BeginCriticalRegion();
            this.isWriterInProgress = true;
            this.buckets = newBuckets;
            this.loadsize = (int)(this.loadFactor * newsize);
            this.version++;
            this.isWriterInProgress = false;
            Thread.EndCriticalRegion();
            return;
        }

        private void Insert(Type key, object? nvalue, bool add)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Contract.EndContractBlock();
            if (this.count >= this.loadsize)
            {
                this.Expand();
            }
            else if (this.occupancy > this.loadsize && this.count > 100)
            {
                this.Rehash();
            }

            uint seed;
            uint incr;
            // Assume we only have one thread writing concurrently.  Modify
            // buckets to contain new data, as long as we insert in the right order.
            uint hashcode = this.InitHash(key, this.buckets.Length, out seed, out incr);
            int ntry = 0;
            int emptySlotNumber = -1; // We use the empty slot number to cache the first empty slot. We chose to reuse slots
            // create by remove that have the collision bit set over using up new slots.
            int bucketNumber = (int)(seed % (uint)this.buckets.Length);
            do
            {
                if (emptySlotNumber == -1 && (this.buckets[bucketNumber].Key == this.buckets) && (this.buckets[bucketNumber].HashColl < 0))
                {
                    emptySlotNumber = bucketNumber;
                }

                // Insert the key/value pair into this bucket if this bucket is empty and has never contained an entry
                // OR
                // This bucket once contained an entry but there has never been a collision
                if ((this.buckets[bucketNumber].Key == null) ||
                    (this.buckets[bucketNumber].Key == this.buckets && ((this.buckets[bucketNumber].HashColl & unchecked(0x80000000)) == 0)))
                {
                    // If we have found an available bucket that has never had a collision, but we've seen an available
                    // bucket in the past that has the collision bit set, use the previous bucket instead
                    if (emptySlotNumber != -1)
                    {// Reuse slot
                        bucketNumber = emptySlotNumber;
                    }

                    // We pretty much have to insert in this order.  Don't set hash
                    // code until the value & key are set appropriately.
                    Thread.BeginCriticalRegion();
                    this.isWriterInProgress = true;
                    this.buckets[bucketNumber].Value = nvalue;
                    this.buckets[bucketNumber].Key = key;
                    this.buckets[bucketNumber].HashColl |= (int)hashcode;
                    this.count++;
                    this.version++;
                    this.isWriterInProgress = false;
                    Thread.EndCriticalRegion();

                    return;
                }

                if (((this.buckets[bucketNumber].HashColl & 0x7FFFFFFF) == hashcode) && this.buckets[bucketNumber].Key == key)
                {
                    if (add)
                    {
                        throw new ArgumentException();
                    }

                    Thread.BeginCriticalRegion();
                    this.isWriterInProgress = true;
                    this.buckets[bucketNumber].Value = nvalue;
                    this.version++;
                    this.isWriterInProgress = false;
                    Thread.EndCriticalRegion();
                    return;
                }

                if (emptySlotNumber == -1)
                {// We don't need to set the collision bit here since we already have an empty slot
                    if (this.buckets[bucketNumber].HashColl >= 0)
                    {
                        this.buckets[bucketNumber].HashColl |= unchecked((int)0x80000000);
                        this.occupancy++;
                    }
                }

                bucketNumber = (int)(((long)bucketNumber + incr) % (uint)this.buckets.Length);
            }
            while (++ntry < this.buckets.Length);

            // This code is here if and only if there were no buckets without a collision bit set in the entire table
            if (emptySlotNumber != -1)
            {
                // We pretty much have to insert in this order.  Don't set hash
                // code until the value & key are set appropriately.
                Thread.BeginCriticalRegion();
                this.isWriterInProgress = true;
                this.buckets[emptySlotNumber].Value = nvalue;
                this.buckets[emptySlotNumber].Key = key;
                this.buckets[emptySlotNumber].HashColl |= (int)hashcode;
                this.count++;
                this.version++;
                this.isWriterInProgress = false;
                Thread.EndCriticalRegion();

                return;
            }

            throw new InvalidOperationException();
        }

        private void PutEntry(Bucket[] newBuckets, object? key, object? nvalue, int hashcode)
        {
            Contract.Assert(hashcode >= 0, "hashcode >= 0");  // make sure collision bit (sign bit) wasn't set.

            uint seed = (uint)hashcode;
            uint incr = (uint)(1 + ((seed * HashPrime) % ((uint)newBuckets.Length - 1)));
            int bucketNumber = (int)(seed % (uint)newBuckets.Length);
            do
            {
                if ((newBuckets[bucketNumber].Key == null) || (newBuckets[bucketNumber].Key == this.buckets))
                {
                    newBuckets[bucketNumber].Value = nvalue;
                    newBuckets[bucketNumber].Key = key;
                    newBuckets[bucketNumber].HashColl |= hashcode;
                    return;
                }

                if (newBuckets[bucketNumber].HashColl >= 0)
                {
                    newBuckets[bucketNumber].HashColl |= unchecked((int)0x80000000);
                    this.occupancy++;
                }

                bucketNumber = (int)(((long)bucketNumber + incr) % (uint)newBuckets.Length);
            }
            while (true);
        }

        public virtual void Remove(Type key)
        {
            uint seed;
            uint incr;
            // Assuming only one concurrent writer, write directly into buckets.
            uint hashcode = this.InitHash(key, this.buckets.Length, out seed, out incr);
            int ntry = 0;

            Bucket b;
            int bn = (int)(seed % (uint)this.buckets.Length);  // bucketNumber
            do
            {
                b = this.buckets[bn];
                if (((b.HashColl & 0x7FFFFFFF) == hashcode) && b.Key == key)
                {
                    Thread.BeginCriticalRegion();
                    this.isWriterInProgress = true;
                    // Clear hash_coll field, then key, then value
                    this.buckets[bn].HashColl &= unchecked((int)0x80000000);
                    if (this.buckets[bn].HashColl != 0)
                    {
                        this.buckets[bn].Key = this.buckets;
                    }
                    else
                    {
                        this.buckets[bn].Key = null;
                    }

                    this.buckets[bn].Value = null;  // Free object references sooner & simplify ContainsValue.
                    this.count--;
                    this.version++;
                    this.isWriterInProgress = false;
                    Thread.EndCriticalRegion();
                    return;
                }

                bn = (int)(((long)bn + incr) % (uint)this.buckets.Length);
            }
            while (b.HashColl < 0 && ++ntry < this.buckets.Length);
        }

        public virtual int Count => this.count;
    }

    internal static class HashHelpers
    {
        // This is the maximum prime smaller than Array.MaxArrayLength
        public const int MaxPrimeArrayLength = 0x7FEFFFFD;

        public static readonly int[] Primes =
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369,
        };

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return candidate == 2;
        }

        public static int GetPrime(int min)
        {
            if (min < 0)
            {
                throw new ArgumentException(nameof(min));
            }

            for (int i = 0; i < Primes.Length; i++)
            {
                int prime = Primes[i];
                if (prime >= min)
                {
                    return prime;
                }
            }

            for (int i = min | 1; i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % ThreadsafeTypeKeyHashtable.HashPrime != 0))
                {
                    return i;
                }
            }

            return min;
        }

        public static int GetMinPrime()
        {
            return Primes[0];
        }

        // Returns size of hashtable to grow to.
        public static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && oldSize < MaxPrimeArrayLength)
            {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }
    }
}
