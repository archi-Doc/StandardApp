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
                if (!this.tableMessage.TryGetValue(key, out var obj))
                {
                    list = new();
                    this.tableMessage[key] = list;
                }
                else
                {
                    list = (FastList<XChannel<TMessage>>)obj;
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
            var key = (typeof(TMessage), typeof(TResult));
            lock (this.tableMessageResult)
            {
                if (!this.tableMessageResult.TryGetValue(key, out var obj))
                {
                    list = new();
                    this.tableMessageResult[key] = list;
                }
                else
                {
                    list = (FastList<XChannel<TMessage, TResult>>)obj;
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

        public XChannel OpenKey<TKey, TMessage>(TKey key, object? weakReference, Action<TMessage> method)
            where TKey : notnull
        {
            FastList<XChannel<TMessage>>? list;
            var k = (typeof(TKey), typeof(TMessage));
            lock (this.tableKeyMessage)
            {
                if (!this.tableKeyMessage.TryGetValue(k, out var obj))
                {
                    list = new();
                    this.tableKeyMessage[k] = list;
                }
                else
                {
                    list = (FastList<XChannel<TMessage>>)obj;
                }
            }

            if (++this.cleanupCount >= CleanupThreshold)
            {
                this.cleanupCount = 0;
                // Cleanup(list);
            }

            var channel = new XChannel<TMessage>(list, weakReference, method);
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
            if (!this.tableMessage.TryGetValue(typeof(TMessage), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel<TMessage>>)obj;
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

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <returns>An array of the return values (TResult).</returns>
        public TResult[] Send<TMessage, TResult>(TMessage message)
        {
            if (!this.tableMessageResult.TryGetValue((typeof(TMessage), typeof(TResult)), out var obj))
            {
                return Array.Empty<TResult>();
            }

            var list = (FastList<XChannel<TMessage, TResult>>)obj;
            var array = list.GetValues();
            var results = new TResult[list.GetCount()];
            var numberReceived = 0;
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
            if (!this.tableKeyMessage.TryGetValue((typeof(TKey), typeof(TMessage)), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel<TMessage>>)obj;
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

        private Dictionary<Type, object> tableMessage = new();
        private Dictionary<(Type, Type), object> tableKeyMessage = new();
        private Dictionary<(Type, Type), object> tableMessageResult = new();
    }
}
