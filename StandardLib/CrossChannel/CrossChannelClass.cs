// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public struct Identifier_KeyMessage
    {
        public Type Key;
        public Type Message;

        public Identifier_KeyMessage(Type key, Type message)
        {
            this.Key = key;
            this.Message = message;
        }

        public override int GetHashCode() => HashCode.Combine(this.Key, this.Message);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_KeyMessage))
            {
                return false;
            }

            var x = (Identifier_KeyMessage)obj;
            return this.Key == x.Key && this.Message == x.Message;
        }
    }

    public class CrossChannelClass
    {
        private int cleanupCount = 0;

        public XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            /*FastList<XChannel_Message<TMessage>>? list;
            var key = typeof(TMessage);
            lock (this.tableMessage)
            {
                list = this.tableMessage[typeof(TMessage)] as FastList<XChannel_Message<TMessage>>;
                if (list == null)
                {
                    list = new();
                    this.tableMessage[key] = list;
                }
            }*/

            var list = (FastList<XChannel_Message<TMessage>>)this.tableMessage.GetOrAdd(
                typeof(TMessage),
                x => new FastList<XChannel_Message<TMessage>>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;
        }

        public XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            FastList<XChannel_MessageResult<TMessage, TResult>>? list;
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
                    list = (FastList<XChannel_MessageResult<TMessage, TResult>>)obj;
                }
            }

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list,  weakReference, method);
            return channel;
        }

        public XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            var collection = (XCollection_KeyMessage<TKey, TMessage>)this.tableKeyMessage.GetOrAdd(
                new Identifier_KeyMessage(typeof(TKey), typeof(TMessage)),
                x => new XCollection_KeyMessage<TKey, TMessage>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                // Cleanup(list);
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(collection, key, weakReference, method);
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
            /*var list = this.tableMessage[typeof(TMessage)] as FastList<XChannel_Message<TMessage>>;
            if (list == null)
            {
                return 0;
            }*/

            if (!this.tableMessage.TryGetValue(typeof(TMessage), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel_Message<TMessage>>)obj;

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

            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)obj;
            return list.Send(message);
        }

        public int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.tableKeyMessage.TryGetValue(new Identifier_KeyMessage(typeof(TKey), typeof(TMessage)), out var obj))
            {
                return 0;
            }

            var collection = (XCollection_KeyMessage<TKey, TMessage>)obj;
            if (!collection.Dictionary.TryGetValue(key, out var list))
            {
                return 0;
            }

            return list.Send(message);
        }

        // private Hashtable tableMessage = new();
        private ConcurrentDictionary<Type, object> tableMessage = new(); // FastList<XChannel_Message<TMessage>>
        private ConcurrentDictionary<Identifier_KeyMessage, object> tableKeyMessage = new(); // XCollection_KeyMessage<TKey, TMessage>
        private Dictionary<(Type, Type), object> tableMessageResult = new();
    }
}
