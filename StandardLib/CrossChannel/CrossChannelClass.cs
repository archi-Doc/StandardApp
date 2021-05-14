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
    public class Identifier_KeyMessage
    {
    }

    public class Identifier_MessageResult
    {
        public Type MessageType { get; }

        public Type ResultType { get; }

        public Identifier_MessageResult(Type messageType, Type resultType)
        {
            this.MessageType = messageType;
            this.ResultType = resultType;
        }

        public override int GetHashCode() => HashCode.Combine(this.MessageType, this.ResultType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_MessageResult))
            {
                return false;
            }

            var x = (Identifier_MessageResult)obj;
            return this.MessageType == x.MessageType && this.ResultType == x.ResultType;
        }
    }

    public class Identifier_KeyMessage<TKey> : Identifier_KeyMessage
        where TKey : notnull
    {
        public TKey Key { get; }

        public Type MessageType { get; }

        public Identifier_KeyMessage(TKey key, Type messageType)
        {
            this.Key = key;
            this.MessageType = messageType;
        }

        public override int GetHashCode() => HashCode.Combine(this.Key, this.MessageType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_KeyMessage<TKey>))
            {
                return false;
            }

            var x = (Identifier_KeyMessage<TKey>)obj;
            return EqualityComparer<TKey>.Default.Equals(this.Key, x.Key) && this.MessageType == x.MessageType;
        }
    }

    public class Identifier_KeyMessageResult
    {
    }

    public class Identifier_KeyMessageResult<TKey> : Identifier_KeyMessageResult
        where TKey : notnull
    {
        public TKey Key { get; }

        public Type MessageType { get; }

        public Type ResultType { get; }

        public Identifier_KeyMessageResult(TKey key, Type messageType, Type resultType)
        {
            this.Key = key;
            this.MessageType = messageType;
            this.ResultType = resultType;
        }

        public override int GetHashCode() => HashCode.Combine(this.Key, this.MessageType, this.ResultType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_KeyMessageResult<TKey>))
            {
                return false;
            }

            var x = (Identifier_KeyMessageResult<TKey>)obj;
            return EqualityComparer<TKey>.Default.Equals(this.Key, x.Key) && this.MessageType == x.MessageType && this.ResultType == x.ResultType;
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

            var list = (FastList<XChannel_Message<TMessage>>)this.dictionaryMessage.GetOrAdd(
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
            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)this.dictionaryMessageResult.GetOrAdd(
                new Identifier_MessageResult(typeof(TMessage), typeof(TResult)),
                x => new FastList<XChannel_MessageResult<TMessage, TResult>>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
            return channel;
        }

        public XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            var list = (FastList<XChannel_Message<TMessage>>)this.dictionaryKeyMessage.GetOrAdd(
                new Identifier_KeyMessage<TKey>(key, typeof(TMessage)),
                x => new FastList<XChannel_Message<TMessage>>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;

            /*var collection = (XCollection_KeyMessage<TKey, TMessage>)this.tableKeyMessage.GetOrAdd(
                new Identifier_KeyMessage(typeof(TKey), typeof(TMessage)),
                x => new XCollection_KeyMessage<TKey, TMessage>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                // Cleanup(list);
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(collection, key, weakReference, method);
            return channel;*/
        }

        public XChannel OpenKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)this.dictionaryKeyMessageResult.GetOrAdd(
                new Identifier_KeyMessageResult<TKey>(key, typeof(TMessage), typeof(TResult)),
                x => new FastList<XChannel_MessageResult<TMessage, TResult>>());

            if (++this.cleanupCount >= CrossChannel.CleanupThreshold)
            {
                this.cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
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

            if (!this.dictionaryMessage.TryGetValue(typeof(TMessage), out var obj))
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
            if (!this.dictionaryMessageResult.TryGetValue(new Identifier_MessageResult(typeof(TMessage), typeof(TResult)), out var obj))
            {
                return Array.Empty<TResult>();
            }

            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)obj;

            return list.Send(message);
        }

        public int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.dictionaryKeyMessage.TryGetValue(new Identifier_KeyMessage<TKey>(key, typeof(TMessage)), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel_Message<TMessage>>)obj;
            /*var collection = (XCollection_KeyMessage<TKey, TMessage>)obj;
            if (!collection.Dictionary.TryGetValue(key, out var list))
            {
                return 0;
            }*/

            return list.Send(message);
        }

        public TResult[] SendKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.dictionaryKeyMessageResult.TryGetValue(new Identifier_KeyMessageResult<TKey>(key, typeof(TMessage), typeof(TResult)), out var obj))
            {
                return Array.Empty<TResult>();
            }

            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)obj;
            return list.Send(message);
        }

        // private Hashtable tableMessage = new();
        private ConcurrentDictionary<Type, object> dictionaryMessage = new(); // FastList<XChannel_Message<TMessage>>
        private ConcurrentDictionary<Identifier_MessageResult, object> dictionaryMessageResult = new(); // FastList<XChannel_MessageResult<TMessage, TResult>>
        private ConcurrentDictionary<Identifier_KeyMessage, object> dictionaryKeyMessage = new(); // FastList<XChannel_Message<TMessage>>
        private ConcurrentDictionary<Identifier_KeyMessageResult, object> dictionaryKeyMessageResult = new(); // FastList<XChannel_MessageResult<TMessage, TResult>>
    }
}
