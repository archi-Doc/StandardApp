// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
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

        static CrossChannel()
        {
        }

        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                Cleanup(CrossChannel.Cache_Message<TMessage>.List);
            }

            var channel = new XChannel<TMessage>(weakReference, method);
            return channel;
        }

        public static XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;

                var list = CrossChannel.Cache_MessageResult<TMessage, TResult>.List;
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

            var channel = new XChannel<TMessage, TResult>(weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage>(TKey key, object? weakReference, Action<TMessage> method)
            where TKey : notnull => new XChannel_Key<TKey, TMessage>(key, weakReference, method);

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
            var dic = Cache_KeyMessage<TKey, TMessage>.Dic;
            FastList<XChannel_Key<TKey, TMessage>> list;
            lock (dic)
            {
                if (dic.TryGetValue(key, out list))
                {
                    return 0;
                }
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
            public static Dictionary<TKey, FastList<XChannel_Key<TKey, TMessage>>> Dic;

            static Cache_KeyMessage()
            {
                Dic = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }

    public class XChannel<TMessage> : XChannel
    {
        public XChannel(object? weakReference, Action<TMessage> method)
        {
            var list = CrossChannel.Cache_Message<TMessage>.List;
            this.Index = list.Add(this);
            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                CrossChannel.Cache_Message<TMessage>.List.Remove(this.Index);
                this.Index = -1;
            }
        }
    }

    public class XChannel<TMessage, TResult> : XChannel
    {
        public XChannel(object? weakReference, Func<TMessage, TResult> method)
        {
            var list = CrossChannel.Cache_MessageResult<TMessage, TResult>.List;
            this.Index = list.Add(this);
            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new(weakReference, method);
            }
        }

        internal Func<TMessage, TResult>? StrongDelegate { get; set; }

        internal WeakFunc<TMessage, TResult>? WeakDelegate { get; set; }

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                CrossChannel.Cache_MessageResult<TMessage, TResult>.List.Remove(this.Index);
                this.Index = -1;
            }
        }
    }

    public class XChannel_Key<TKey, TMessage> : XChannel
        where TKey : notnull
    {
        public XChannel_Key(TKey key, object? weakReference, Action<TMessage> method)
        {
            var dic = CrossChannel.Cache_KeyMessage<TKey, TMessage>.Dic;
            lock (dic)
            {
                if (!dic.TryGetValue(key, out this.list))
                {
                    this.list = new();
                    dic[key] = this.list;
                }

                this.Key = key;
                this.Index = this.list.Add(this);
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

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        private FastList<XChannel_Key<TKey, TMessage>> list;

        public override void Dispose()
        {
            if (this.Index != -1)
            {
                var dic = CrossChannel.Cache_KeyMessage<TKey, TMessage>.Dic;
                lock (dic)
                {
                    this.list.Remove(this.Index);
                    if (this.list.GetCount() == 0)
                    {
                        dic.Remove(this.Key);
                    }
                }

                this.Index = -1;
            }
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
}
