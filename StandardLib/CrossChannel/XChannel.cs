// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public abstract class XChannel : IDisposable
    {
        internal int Index { get; set; }

        public virtual void Dispose()
        {
            if (this.Index != -1)
            {
                this.Index = -1;
            }
        }
    }

    internal class XChannel_Message<TMessage> : XChannel
    {
        internal XChannel_Message(FastList<XChannel_Message<TMessage>> list, object? weakReference, Action<TMessage> method)
        {
            this.List = list;
            this.Index = this.List.Add(this);
            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        internal FastList<XChannel_Message<TMessage>> List { get; }

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                this.List.Remove(this.Index);
                this.Index = -1;
            }
        }
    }

    internal class XChannel<TMessage, TResult> : XChannel
    {
        internal XChannel(FastList<XChannel<TMessage, TResult>> list, object? weakReference, Func<TMessage, TResult> method)
        {
            this.List = list;
            this.Index = this.List.Add(this);
            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        internal FastList<XChannel<TMessage, TResult>> List { get; }

        internal Func<TMessage, TResult>? StrongDelegate { get; set; }

        internal WeakFunc<TMessage, TResult>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                this.List.Remove(this.Index);
                this.Index = -1;
            }
        }
    }

    internal class XChannel_KeyMessage<TKey, TMessage> : XChannel
        where TKey : notnull
    {
        public XChannel_KeyMessage(Hashtable table, TKey key, object? weakReference, Action<TMessage> method)
        {
            this.Table = table;
            lock (this.Table)
            {
                var list = this.Table[key] as FastList<XChannel_KeyMessage<TKey, TMessage>>;
                if (list == null)
                {
                    list = new();
                    this.Table[key] = list;
                }

                this.List = list;
                this.Key = key;
                this.Index = this.List.Add(this);
            }

            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        public TKey Key { get; }

        internal Hashtable Table { get; }

        internal FastList<XChannel_KeyMessage<TKey, TMessage>> List { get; }

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                var empty = this.List.Remove(this.Index);
                if (empty)
                {
                    lock (this.Table)
                    {
                        this.Table.Remove(this.Key);
                    }
                }
            }

            this.Index = -1;
        }
    }

    internal class XChannel_Key<TKey, TMessage> : XChannel
        where TKey : notnull
    {
        public XChannel_Key(Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> map, TKey key, object? weakReference, Action<TMessage> method)
        {
            FastList<XChannel_Key<TKey, TMessage>>? list;
            lock (map)
            {
                if (!map.TryGetValue(key, out list))
                {
                    list = new();
                    map[key] = list;
                }
            }

            this.Map = map;
            this.List = list;
            this.Key = key;
            this.Index = this.List.Add(this);

            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        public TKey Key { get; }

        internal Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Map { get; }

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
        internal FastList<XChannel_Key<TKey, TMessage>> List;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1201 // Elements should appear in the correct order

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                var empty = this.List.Remove(this.Index);
                if (empty)
                {
                    lock (this.Map)
                    {
                        this.Map.Remove(this.Key);
                    }
                }

                this.Index = -1;
            }
        }
    }

    internal class XChannel_Key2<TKey, TMessage> : XChannel
        where TKey : notnull
    {
        public XChannel_Key2(ConcurrentDictionary<TKey, FastList<XChannel_Key2<TKey, TMessage>>> map, TKey key, object? weakReference, Action<TMessage> method)
        {
            this.Map = map;
            this.List = map.GetOrAdd(key, x => new FastList<XChannel_Key2<TKey, TMessage>>());
            this.Key = key;
            this.Index = this.List.Add(this);

            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        public TKey Key { get; }

        internal ConcurrentDictionary<TKey, FastList<XChannel_Key2<TKey, TMessage>>> Map { get; }

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
        internal FastList<XChannel_Key2<TKey, TMessage>> List;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1201 // Elements should appear in the correct order

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                var empty = this.List.Remove(this.Index);
                if (empty)
                {
                    this.Map.TryRemove(this.Key, out _);
                }

                this.Index = -1;
            }
        }
    }
}
