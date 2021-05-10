// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public class CrossChannelClass
    {
        private int cleanupCount = 0;

        public XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            FastList<XChannel<TMessage>>? list;
            var key = typeof(TMessage);
            lock (this.tableMessage)
            {
                list = this.tableMessage[typeof(TMessage)] as FastList<XChannel<TMessage>>;
                if (list == null)
                {
                    list = new();
                    this.tableMessage[key] = list;
                }
            }

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
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

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
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
                list = this.tableKeyMessage[k] as FastList<XChannel<TMessage>>;
                if (list == null)
                {
                    list = new();
                    this.tableKeyMessage[k] = list;
                }
            }

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
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
            var list = this.tableMessage[typeof(TMessage)] as FastList<XChannel<TMessage>>;
            if (list == null)
            {
                return 0;
            }

            return list.Send(message);
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
            return list.Send(message);
        }

        public int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            var list = this.tableKeyMessage[(typeof(TKey), typeof(TMessage))] as FastList<XChannel<TMessage>>;
            if (list == null)
            {
                return 0;
            }

            return list.Send(message);
        }

        private Hashtable tableMessage = new();
        private Hashtable tableKeyMessage = new();
        private Dictionary<(Type, Type), object> tableMessageResult = new();
    }
}
