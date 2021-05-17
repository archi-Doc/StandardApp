// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Arc.CrossChannel
{
    public class CrossChannelClass
    {
        public XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            var list = (FastList<XChannel_Message<TMessage>>)this.dictionaryMessage.GetOrAdd(
                typeof(TMessage),
                x => new FastList<XChannel_Message<TMessage>>());

            if (list.CleanupCount++ >= CrossChannel.Const.CleanupListThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Shrink();
                }
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;
        }

        public XChannel OpenAsync<TMessage>(object? weakReference, Func<TMessage, Task> method) => this.OpenTwoWay<TMessage, Task>(weakReference, method);

        public XChannel OpenAsyncKey<TKey, TMessage>(object? weakReference, TKey key, Func<TMessage, Task> method)
            where TKey : notnull => this.OpenTwoWayKey<TKey, TMessage, Task>(weakReference, key, method);

        public XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            var list = (FastList<XChannel_Message<TMessage>>)this.dictionaryKeyMessage.GetOrAdd(
                new Identifier_KeyMessage<TKey>(key, typeof(TMessage)),
                x => new FastList<XChannel_Message<TMessage>>());

            if (list.CleanupCount++ >= CrossChannel.Const.CleanupListThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Shrink();
                }
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;
        }

        public XChannel OpenTwoWay<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)this.dictionaryMessageResult.GetOrAdd(
                new Identifier_MessageResult(typeof(TMessage), typeof(TResult)),
                x => new FastList<XChannel_MessageResult<TMessage, TResult>>());

            if (list.CleanupCount++ >= CrossChannel.Const.CleanupListThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Shrink();
                }
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
            return channel;
        }

        public XChannel OpenTwoWayAsync<TMessage, TResult>(object? weakReference, Func<TMessage, Task<TResult>> method) => this.OpenTwoWay<TMessage, Task<TResult>>(weakReference, method);

        public XChannel OpenTwoWayAsyncKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, Task<TResult>> method)
             where TKey : notnull => this.OpenTwoWayKey<TKey, TMessage, Task<TResult>>(weakReference, key, method);

        public XChannel OpenTwoWayKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)this.dictionaryKeyMessageResult.GetOrAdd(
                new Identifier_KeyMessageResult<TKey>(key, typeof(TMessage), typeof(TResult)),
                x => new FastList<XChannel_MessageResult<TMessage, TResult>>());

            if (list.CleanupCount++ >= CrossChannel.Const.CleanupListThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Shrink();
                }
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
            return channel;
        }

        public void Close(XChannel channel) => channel.Dispose();

        public int Send<TMessage>(TMessage message)
        {
            if (!this.dictionaryMessage.TryGetValue(typeof(TMessage), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel_Message<TMessage>>)obj;
            return list.Send(message);
        }

        public Task SendAsync<TMessage>(TMessage message)
        {
            if (!this.dictionaryMessageResult.TryGetValue(new Identifier_MessageResult(typeof(TMessage), typeof(Task)), out var obj))
            {
                return Task.CompletedTask;
            }

            var list = (FastList<XChannel_MessageResult<TMessage, Task>>)obj;
            return list.SendAsync(message);
        }

        public Task SendAsyncKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.dictionaryKeyMessageResult.TryGetValue(new Identifier_KeyMessageResult<TKey>(key, typeof(TMessage), typeof(Task)), out var obj))
            {
                return Task.CompletedTask;
            }

            var list = (FastList<XChannel_MessageResult<TMessage, Task>>)obj;
            return list.SendAsync(message);
        }

        public int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.dictionaryKeyMessage.TryGetValue(new Identifier_KeyMessage<TKey>(key, typeof(TMessage)), out var obj))
            {
                return 0;
            }

            var list = (FastList<XChannel_Message<TMessage>>)obj;
            return list.Send(message);
        }

        public TResult[] SendTwoWay<TMessage, TResult>(TMessage message)
        {
            if (!this.dictionaryMessageResult.TryGetValue(new Identifier_MessageResult(typeof(TMessage), typeof(TResult)), out var obj))
            {
                return Array.Empty<TResult>();
            }

            var list = (FastList<XChannel_MessageResult<TMessage, TResult>>)obj;
            return list.Send(message);
        }

        public Task<TResult[]> SendTwoWayAsync<TMessage, TResult>(TMessage message)
        {
            if (!this.dictionaryMessageResult.TryGetValue(new Identifier_MessageResult(typeof(TMessage), typeof(Task<TResult>)), out var obj))
            {
                return Task.FromResult(Array.Empty<TResult>());
            }

            var list = (FastList<XChannel_MessageResult<TMessage, Task<TResult>>>)obj;
            return list.SendAsync(message);
        }

        public Task<TResult[]> SendTwoWayAsyncKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!this.dictionaryKeyMessageResult.TryGetValue(new Identifier_KeyMessageResult<TKey>(key, typeof(TMessage), typeof(Task<TResult>)), out var obj))
            {
                return Task.FromResult(Array.Empty<TResult>());
            }

            var list = (FastList<XChannel_MessageResult<TMessage, Task<TResult>>>)obj;
            return list.SendAsync(message);
        }

        public TResult[] SendTwoWayKey<TKey, TMessage, TResult>(TKey key, TMessage message)
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
