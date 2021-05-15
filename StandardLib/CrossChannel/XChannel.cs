// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                this.WeakDelegate?.MarkForDeletion();
            }
        }
    }

    internal class XChannel_MessageResult<TMessage, TResult> : XChannel
    {
        internal XChannel_MessageResult(FastList<XChannel_MessageResult<TMessage, TResult>> list, object? weakReference, Func<TMessage, TResult> method)
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

        internal FastList<XChannel_MessageResult<TMessage, TResult>> List { get; }

        internal Func<TMessage, TResult>? StrongDelegate { get; set; }

        internal WeakFunc<TMessage, TResult>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                this.List.Remove(this.Index);
                this.Index = -1;
                this.WeakDelegate?.MarkForDeletion();
            }
        }
    }

    internal class XCollection_KeyMessage<TKey, TMessage>
        where TKey : notnull
    {
        internal ConcurrentDictionary<TKey, FastList<XChannel_KeyMessage<TKey, TMessage>>> Dictionary { get; } = new();

        internal int Count { get; set; } // ConcurrentDictionary.Count is just slow.

        internal int CleanupCount { get; set; }
    }

    internal class XChannel_KeyMessage<TKey, TMessage> : XChannel
        where TKey : notnull
    {
        public XChannel_KeyMessage(XCollection_KeyMessage<TKey, TMessage> collection, TKey key, object? weakReference, Action<TMessage> method)
        {
            this.Collection = collection;
            lock (this.Collection)
            {
                if (!this.Collection.Dictionary.TryGetValue(key, out this.List))
                {
                    this.List = new FastList<XChannel_KeyMessage<TKey, TMessage>>();
                    this.Collection.Dictionary.TryAdd(key, this.List);
                    this.Collection.Count++;
                }

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

        internal XCollection_KeyMessage<TKey, TMessage> Collection { get; }

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
        internal FastList<XChannel_KeyMessage<TKey, TMessage>> List;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1201 // Elements should appear in the correct order

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                if (this.Collection.Count <= CrossChannel.DictionaryThreshold)
                {
                    this.List.Remove(this.Index);
                }
                else
                {
                    lock (this.Collection)
                    {
                        var empty = this.List.Remove(this.Index);
                        if (empty)
                        {
                            this.Collection.Dictionary.TryRemove(this.Key, out _);
                            this.Collection.Count--;
                            this.List.Dispose();
                        }
                    }
                }

                this.Index = -1;
                this.WeakDelegate?.MarkForDeletion();
            }
        }
    }

    internal class XCollection_KeyMessageResult<TKey, TMessage, TResult>
        where TKey : notnull
    {
        internal ConcurrentDictionary<TKey, FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>>> Dictionary { get; } = new();

        internal int Count { get; set; } // ConcurrentDictionary.Count is just slow.

        internal int CleanupCount { get; set; }
    }

    internal class XChannel_KeyMessageResult<TKey, TMessage, TResult> : XChannel
        where TKey : notnull
    {
        public XChannel_KeyMessageResult(XCollection_KeyMessageResult<TKey, TMessage, TResult> collection, TKey key, object? weakReference, Func<TMessage, TResult> method)
        {
            this.Collection = collection;
            lock (this.Collection)
            {
                if (!this.Collection.Dictionary.TryGetValue(key, out this.List))
                {
                    this.List = new FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>>();
                    this.Collection.Dictionary.TryAdd(key, this.List);
                    this.Collection.Count++;
                }

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

        internal XCollection_KeyMessageResult<TKey, TMessage, TResult> Collection { get; }

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
        internal FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>> List;
#pragma warning restore SA1401 // Fields should be private
#pragma warning restore SA1201 // Elements should appear in the correct order

        internal Func<TMessage, TResult>? StrongDelegate { get; set; }

        internal WeakFunc<TMessage, TResult>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                if (this.Collection.Count <= CrossChannel.DictionaryThreshold)
                {
                    this.List.Remove(this.Index);
                }
                else
                {
                    lock (this.Collection)
                    {
                        var empty = this.List.Remove(this.Index);
                        if (empty)
                        {
                            this.Collection.Dictionary.TryRemove(this.Key, out _);
                            this.Collection.Count--;
                            this.List.Dispose();
                        }
                    }
                }

                this.Index = -1;
                this.WeakDelegate?.MarkForDeletion();
            }
        }
    }
}
