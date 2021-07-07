// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arc.CrossChannel;
using Benchmark.Internal;
using BenchmarkDotNet.Attributes;

#pragma warning disable SA1649 // File name should match first type name

namespace Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    public class HashtableBenchmark
    {
        public Type[] Types { get; private set; } = default!;

        public ConcurrentDictionary<Type, object> TypeConcurrentDictionary { get; private set; } = default!;

        public Hashtable TypeHashtable { get; private set; } = default!;

        public Dictionary<Type, object> TypeDictionary { get; private set; } = default!;

        public TypeKeyDictionary<object> TypeKeyDictionary { get; private set; } = default!;

        public ThreadsafeTypeKeyHashtablePrime ThreadsafeTypeKeyHashtablePrime { get; private set; } = default!;

        internal ThreadsafeTypeKeyHashtable<object> ThreadsafeTypeKeyHashtable { get; private set; } = default!;

        public HashtableBenchmark()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            this.Types = new Type[]
            {
                typeof(object),
                typeof(int),
                typeof(byte),
                typeof(uint),
                typeof(string),
                typeof(short),
                typeof(float),
                typeof(long),
                typeof(double),
                typeof(decimal),
            };

            this.TypeConcurrentDictionary = new();
            this.TypeHashtable = new();
            this.TypeDictionary = new();
            this.TypeKeyDictionary = new();
            this.ThreadsafeTypeKeyHashtablePrime = new();
            this.ThreadsafeTypeKeyHashtable = new();
            foreach (var x in this.Types) // typeof(int).Assembly.GetTypes()
            {
                this.TypeConcurrentDictionary.TryAdd(x, x);
                this.TypeHashtable.Add(x, x);
                this.TypeDictionary.Add(x, x);
                this.TypeKeyDictionary.Add(x, x);
                this.ThreadsafeTypeKeyHashtablePrime.Add(x, x);
                this.ThreadsafeTypeKeyHashtable.TryAdd(x, x);
            }
        }

        [Benchmark]
        public ConcurrentDictionary<Type, object> CreateAndAdd_ConcurrentDictionary()
        {
            var c = new ConcurrentDictionary<Type, object>();
            foreach (var x in this.Types)
            {
                lock (c)
                {
                    c.TryAdd(x, x);
                }
            }

            return c;
        }

        [Benchmark]
        public Hashtable CreateAndAdd_Hashtable()
        {
            var c = new Hashtable();
            foreach (var x in this.Types)
            {
                lock (c)
                {
                    c.Add(x, x);
                }
            }

            return c;
        }

        [Benchmark]
        public Dictionary<Type, object> CreateAndAdd_Dictionary()
        {
            var c = new Dictionary<Type, object>();
            foreach (var x in this.Types)
            {
                lock (c)
                {
                    c.Add(x, x);
                }
            }

            return c;
        }

        [Benchmark]
        public TypeKeyDictionary<object> CreateAndAdd_TypeKeyDictionary()
        {
            var c = new TypeKeyDictionary<object>();
            foreach (var x in this.Types)
            {
                lock (c)
                {
                    c.Add(x, x);
                }
            }

            return c;
        }

        [Benchmark]
        public ThreadsafeTypeKeyHashtablePrime CreateAndAdd_ThreadsafeHashtablePrime()
        {
            var c = new ThreadsafeTypeKeyHashtablePrime();
            foreach (var x in this.Types)
            {
                lock (c)
                {
                    c.Add(x, x);
                }
            }

            return c;
        }

        [Benchmark]
        public ThreadsafeTypeKeyHashtable<object> CreateAndAdd_ThreadsafeHashtable()
        {
            var c = new ThreadsafeTypeKeyHashtable<object>();
            foreach (var x in this.Types)
            {
                c.TryAdd(x, x);
            }

            return c;
        }

        [Benchmark]
        public object Get_ConcurrentDictionary()
        {
            return this.TypeConcurrentDictionary[typeof(int)];
        }

        [Benchmark]
        public object Get_Hashtable()
        {
            return this.TypeHashtable[typeof(int)];
        }

        [Benchmark]
        public object Get_Dictionary()
        {
            lock (this.TypeDictionary)
            {
                return this.TypeDictionary[typeof(int)];
            }
        }

        [Benchmark]
        public object Get_DictionaryWithoutLock()
        {
            return this.TypeDictionary[typeof(int)];
        }

        [Benchmark]
        public object Get_TypeKeyDictionary()
        {
            lock (this.TypeKeyDictionary)
            {
                return this.TypeKeyDictionary[typeof(int)];
            }
        }

        [Benchmark]
        public object Get_ThreadsafeHashtablePrime()
        {
            return this.ThreadsafeTypeKeyHashtablePrime[typeof(int)];
        }

        [Benchmark]
        public object? Get_ThreadsafeHashtable()
        {
            this.ThreadsafeTypeKeyHashtable.TryGetValue(typeof(int), out var obj);
            return obj;
        }
    }
}
