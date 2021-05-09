// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public class CrossChannel : CrossChannelBase
    {
        public static CrossChannel Instance => instance ?? (instance = new());

        private static CrossChannel? instance;

        private CrossChannel()
        {
        }

        internal override FastList<XChannel<TMessage>> Get_Message<TMessage>() => Cache_Message<TMessage>.List;

        internal override FastList<XChannel<TMessage, TResult>> Get_MessageResult<TMessage, TResult>() => Cache_MessageResult<TMessage, TResult>.List;

        internal override Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Get_KeyMessage<TKey, TMessage>() => Cache_KeyMessage<TKey, TMessage>.Map;

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
            public static Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Map;

            static Cache_KeyMessage()
            {
                Map = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }

    public class GunjoChannel : CrossChannelBase
    {
        public static GunjoChannel Instance => instance ?? (instance = new());

        private static GunjoChannel? instance;

        private GunjoChannel()
        {
        }

        internal override FastList<XChannel<TMessage>> Get_Message<TMessage>() => Cache_Message<TMessage>.List;

        internal override FastList<XChannel<TMessage, TResult>> Get_MessageResult<TMessage, TResult>() => Cache_MessageResult<TMessage, TResult>.List;

        internal override Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Get_KeyMessage<TKey, TMessage>() => Cache_KeyMessage<TKey, TMessage>.Map;

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
            public static Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Map;

            static Cache_KeyMessage()
            {
                Map = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }

    public abstract class CrossChannelBase
    {
        private const int CleanupThreshold = 16;
        private static int cleanupCount = 0;

        public XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                this.Cleanup(this.Get_Message<TMessage>());
            }

            var channel = new XChannel<TMessage>(this.Get_Message<TMessage>(), weakReference, method);
            return channel;
        }

        public XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                this.Cleanup(this.Get_MessageResult<TMessage, TResult>());
            }

            var channel = new XChannel<TMessage, TResult>(this.Get_MessageResult<TMessage, TResult>(), weakReference, method);
            return channel;
        }

        public XChannel OpenKey<TKey, TMessage>(TKey key, object? weakReference, Action<TMessage> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_Key<TKey, TMessage>(this.Get_KeyMessage<TKey, TMessage>(), key, weakReference, method);
            return channel;
        }

        public void Close(XChannel channel) => channel.Dispose();

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>A number of the receivers.</returns>
        public int Send<TMessage>(TMessage message)
        {
            var numberReceived = 0;
            var array = this.Get_Message<TMessage>().GetValues();
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
        public TResult[] Send<TMessage, TResult>(TMessage message)
        {
            var numberReceived = 0;
            var list = this.Get_MessageResult<TMessage, TResult>();
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

        public int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.Get_KeyMessage<TKey, TMessage>().TryGetValue(key, out var list))
            {
                return 0;
            }

            var array = list.GetValues();
            var numberReceived = 0;
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

        internal abstract FastList<XChannel<TMessage>> Get_Message<TMessage>();

        internal abstract FastList<XChannel<TMessage, TResult>> Get_MessageResult<TMessage, TResult>();

        internal abstract Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Get_KeyMessage<TKey, TMessage>()
            where TKey : notnull;

        private void Cleanup<TMessage>(FastList<XChannel<TMessage>> list)
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

        private void Cleanup<TMessage, TResult>(FastList<XChannel<TMessage, TResult>> list)
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
                this.List.Remove(this.Index);
                if (this.List.GetCount() == 0)
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
}
