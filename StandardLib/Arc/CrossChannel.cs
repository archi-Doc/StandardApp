// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public static class CrossChannel
    {
        private const int CleanupThreshold = 16;
        private static int cleanupCount = 0;

        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                Cleanup(CrossChannel.Cache_Message<TMessage>.List);
            }

            var channel = new XChannel<TMessage>(CrossChannel.Cache_Message<TMessage>.List, weakReference, method);
            return channel;
        }

        public static XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel<TMessage, TResult>(Cache_MessageResult<TMessage, TResult>.List, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage>(TKey key, object? weakReference, Action<TMessage> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(Cache_KeyMessage<TKey, TMessage>.Table, key, weakReference, method);
            return channel;
        }

        public static void Close(XChannel channel) => channel.Dispose();

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>A number of the receivers.</returns>
        public static int Send<TMessage>(TMessage message)
        {
            var numberReceived = 0;
            var array = Cache_Message<TMessage>.List.GetValues();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] is { } channel)
                {
                    if (channel.StrongDelegate != null)
                    {
                        channel.StrongDelegate(message);
                        numberReceived++;
                    }
                    else if (channel.WeakDelegate!.IsAlive)
                    {
                        channel.WeakDelegate!.Execute(message);
                        numberReceived++;
                    }
                    else
                    {
                        channel.Dispose();
                    }
                }
            }

            return numberReceived;
        }

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <returns>An array of the return values (TResult).</returns>
        public static TResult[] Send<TMessage, TResult>(TMessage message)
        {
            var numberReceived = 0;
            var list = Cache_MessageResult<TMessage, TResult>.List;
            var array = list.GetValues();
            var results = new TResult[list.GetCount()];
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] is { } channel)
                {
                    if (channel.StrongDelegate != null)
                    {
                        results[numberReceived++] = channel.StrongDelegate(message);
                    }
                    else if (channel.WeakDelegate!.IsAlive)
                    {
                        var result = channel.WeakDelegate!.Execute(message, out var executed);
                        if (executed)
                        {
                            results[numberReceived++] = result!;
                        }
                    }
                    else
                    {
                        channel.Dispose();
                    }
                }
            }

            if (results.Length != numberReceived)
            {
                Array.Resize(ref results, numberReceived);
            }

            return results;
        }

        public static int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            var numberReceived = 0;
            var table = Cache_KeyMessage<TKey, TMessage>.Table;
            var list = table[key] as FastList<XChannel_KeyMessage<TKey, TMessage>>;
            if (list == null)
            {
                return 0;
            }

            var array = list.GetValues();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] is { } channel)
                {
                    if (channel.StrongDelegate != null)
                    {
                        channel.StrongDelegate(message);
                        numberReceived++;
                    }
                    else if (channel.WeakDelegate!.IsAlive)
                    {
                        channel.WeakDelegate!.Execute(message);
                        numberReceived++;
                    }
                    else
                    {
                        channel.Dispose();
                    }
                }
            }

            return numberReceived;
        }

        private static void Cleanup<TMessage>(FastList<XChannel<TMessage>> list)
        {
            var array = list.GetValues();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] is { } c)
                {
                    if (c.WeakDelegate != null && !c.WeakDelegate.IsAlive)
                    {
                        c.Dispose();
                    }
                }
            }

            list.TryShrink();
        }

        private static void Cleanup<TMessage, TResult>(FastList<XChannel<TMessage, TResult>> list)
        {
            var array = list.GetValues();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] is { } c)
                {
                    if (c.WeakDelegate != null && !c.WeakDelegate.IsAlive)
                    {
                        c.Dispose();
                    }
                }
            }

            list.TryShrink();
        }

#pragma warning disable SA1401 // Fields should be private
        internal static class Cache_Message<TMessage>
        {
            public static FastList<XChannel<TMessage>> List;

            static Cache_Message()
            {
                List = new();
            }
        }

        internal static class Cache_MessageResult<TMessage, TResult>
        {
            public static FastList<XChannel<TMessage, TResult>> List;

            static Cache_MessageResult()
            {
                List = new();
            }
        }

        internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {
            public static Hashtable Table;

            static Cache_KeyMessage()
            {
                Table = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }

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

    internal class XChannel<TMessage> : XChannel
    {
        internal XChannel(FastList<XChannel<TMessage>> list, object? weakReference, Action<TMessage> method)
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

        internal FastList<XChannel<TMessage>> List { get; }

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
                this.List.Remove(this.Index);
                if (this.List.GetCount() == 0)
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
        public XChannel_Key(Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> dic, TKey key, object? weakReference, Action<TMessage> method)
        {
            this.Dic = dic;
            lock (this.Dic)
            {
                if (!this.Dic.TryGetValue(key, out this.List))
                {
                    this.List = new();
                    this.Dic[key] = this.List;
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

        internal Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Dic { get; }

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
                lock (this.Dic)
                {
                    this.List.Remove(this.Index);
                    if (this.List.GetCount() == 0)
                    {
                        this.Dic.Remove(this.Key);
                    }
                }

                this.Index = -1;
            }
        }
    }
}
