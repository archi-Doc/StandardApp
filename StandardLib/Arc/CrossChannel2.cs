// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public class CrossChannel2
    {
        private const int CleanupThreshold = 16;

        private static CrossChannel2? @default;

        public static CrossChannel2 Default => @default ?? (@default = new());

        private int cleanupCount = 0;

        public XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            FastList<XChannel<TMessage>>? list;
            var key = typeof(TMessage);
            lock (this.tableMessage)
            {
                list = this.tableMessage[key] as FastList<XChannel<TMessage>>;
                if (list == null)
                {
                    list = new();
                    this.tableMessage[key] = list;
                }
            }

            if (++this.cleanupCount >= CleanupThreshold)
            {
                this.cleanupCount = 0;
                this.Cleanup(list);
            }

            var channel = new XChannel<TMessage>(list, weakReference, method);
            return channel;
        }

        public XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            FastList<XChannel<TMessage, TResult>>? list;
            var key = KeyValuePair.Create(typeof(TMessage), typeof(TResult));
            lock (this.tableMessageResult)
            {
                list = this.tableMessageResult[key] as FastList<XChannel<TMessage, TResult>>;
                if (list == null)
                {
                    list = new();
                    this.tableMessageResult[key] = list;
                }
            }

            if (++this.cleanupCount >= CleanupThreshold)
            {
                this.cleanupCount = 0;
                this.Cleanup(list);
            }

            var channel = new XChannel<TMessage, TResult>(list,  weakReference, method);
            return channel;
        }

        public XChannel OpenKey<TKey, TMessage>(TKey tkey, object? weakReference, Action<TMessage> method)
            where TKey : notnull
        {
            var key = KeyValuePair.Create(typeof(TKey), typeof(TMessage));
            var table = this.tableKeyMessage[key] as Hashtable;
            if (table == null)
            {
                table = new();
                this.tableKeyMessage[key] = table;
            }

            if (++this.cleanupCount >= CleanupThreshold)
            {
                this.cleanupCount = 0;
                // Cleanup(list);
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(table, tkey, weakReference, method);
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
            var list = this.tableMessage[typeof(TMessage)] as FastList<XChannel<TMessage>>;
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
            var list = this.tableMessageResult[KeyValuePair.Create(typeof(TMessage), typeof(TResult))] as FastList<XChannel<TMessage, TResult>>;
            if (list == null)
            {
                return Array.Empty<TResult>();
            }

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
            var numberReceived = 0;
            var k = KeyValuePair.Create(typeof(TKey), typeof(TMessage));
            var table = this.tableKeyMessage[k] as Hashtable;
            if (table == null)
            {
                return 0;
            }

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

        private Hashtable tableMessage = new();
        private Hashtable tableKeyMessage = new();
        private Hashtable tableMessageResult = new();
    }
}
