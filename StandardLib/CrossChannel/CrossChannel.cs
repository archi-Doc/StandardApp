// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public static class CrossChannel
    {
        internal const int CleanupThreshold = 32;
        internal const int DictionaryThreshold = 16;

        /// <summary>
        /// Open a channel to receive the message.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="weakReference">A weak reference of the object.<br/>
        /// The channel will be automatically closed when the object is garbage collected.</param>
        /// <param name="method">The delegate that is called when the message is sent.</param>
        /// <returns>A new instance of XChannel.<br/>
        /// You need to call <see cref="XChannel.Dispose()"/> when the channel is no longer necessary, unless the weak reference is specified.</returns>
        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            var list = Cache_Message<TMessage>.List;
            if (list.CleanupCount++ >= CleanupThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Cleanup();
                }
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;
        }

        public static XChannel OpenAsync<TMessage>(object? weakReference, Func<TMessage, Task> method) => CrossChannel.OpenTwoWay<TMessage, Task>(weakReference, method);

        public static XChannel OpenAsyncKey<TKey, TMessage>(object? weakReference, TKey key, Func<TMessage, Task> method)
            where TKey : notnull => CrossChannel.OpenTwoWayKey<TKey, TMessage, Task>(weakReference, key, method);

        public static XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            var collection = Cache_KeyMessage<TKey, TMessage>.Collection;
            if (collection.CleanupCount++ >= CleanupThreshold)
            {
                lock (collection)
                {
                    collection.CleanupCount = 0;
                    collection.Cleanup();
                }
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(collection, key, weakReference, method);
            return channel;
        }

        public static XChannel OpenTwoWay<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            var list = Cache_MessageResult<TMessage, TResult>.List;
            if (++list.CleanupCount >= CleanupThreshold)
            {
                lock (list)
                {
                    list.CleanupCount = 0;
                    list.Cleanup();
                }
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
            return channel;
        }

        public static XChannel OpenTwoWayAsync<TMessage, TResult>(object? weakReference, Func<TMessage, Task<TResult>> method) => CrossChannel.OpenTwoWay<TMessage, Task<TResult>>(weakReference, method);

        public static XChannel OpenTwoWayAsyncKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, Task<TResult>> method)
             where TKey : notnull => CrossChannel.OpenTwoWayKey<TKey, TMessage, Task<TResult>>(weakReference, key, method);

        public static XChannel OpenTwoWayKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            var collection = Cache_KeyMessageResult<TKey, TMessage, TResult>.Collection;
            if (++collection.CleanupCount >= CleanupThreshold)
            {
                lock (collection)
                {
                    collection.CleanupCount = 0;
                    collection.Cleanup();
                }
            }

            var channel = new XChannel_KeyMessageResult<TKey, TMessage, TResult>(collection, key, weakReference, method);
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
            return Cache_Message<TMessage>.List.Send(message);
        }

        public static Task SendAsync<TMessage>(TMessage message)
        {
            return Cache_MessageResult<TMessage, Task>.List.SendAsync(message);
        }

        public static Task SendAsyncKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!Cache_KeyMessageResult<TKey, TMessage, Task>.Collection.Dictionary.TryGetValue(key, out var list))
            {
                return Task.CompletedTask;
            }

            return list.SendAsync(message);
        }

        public static int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            /*var list = Cache_KeyMessage<TKey, TMessage>.Map[key] as FastList<XChannel_KeyMessage<TKey, TMessage>>;
            if (list == null)
            {
                return 0;
            }*/

            /*if (!Cache_KeyMessage<TKey, TMessage>.Map.TryGetValue(key, out var list))
            {
                return 0;
            }*/

            if (!Cache_KeyMessage<TKey, TMessage>.Collection.Dictionary.TryGetValue(key, out var list))
            {
                return 0;
            }

            return list.Send(message);
        }

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>An array of the return values (TResult).</returns>
        public static TResult[] SendTwoWay<TMessage, TResult>(TMessage message)
        {
            return Cache_MessageResult<TMessage, TResult>.List.Send(message);
        }

        public static Task<TResult[]> SendTwoWayAsync<TMessage, TResult>(TMessage message)
        {
            return Cache_MessageResult<TMessage, Task<TResult>>.List.SendAsync(message);
        }

        public static TResult[] SendTwoWayKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!Cache_KeyMessageResult<TKey, TMessage, TResult>.Collection.Dictionary.TryGetValue(key, out var list))
            {
                return Array.Empty<TResult>();
            }

            return list.Send(message);
        }

        public static Task<TResult[]> SendTwoWayAsyncKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!Cache_KeyMessageResult<TKey, TMessage, Task<TResult>>.Collection.Dictionary.TryGetValue(key, out var list))
            {
                return Task.FromResult(Array.Empty<TResult>());
            }

            return list.SendAsync(message);
        }

#pragma warning disable SA1401 // Fields should be private
        internal static class Cache_Message<TMessage>
        {// lock (FastList<XChannel_Message<TMessage>>) : XChannel_Message<TMessage>
            public static FastList<XChannel_Message<TMessage>> List;

            static Cache_Message()
            {
                List = new(static x => ref x.Index);
            }
        }

        internal static class Cache_MessageResult<TMessage, TResult>
        {// lock (FastList<XChannel_MessageResult<TMessage, TResult>>) : XChannel_MessageResult<TMessage, TResult>
            public static FastList<XChannel_MessageResult<TMessage, TResult>> List;

            static Cache_MessageResult()
            {
                List = new(static x => ref x.Index);
            }
        }

        internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {// lock (XCollection_KeyMessage<TKey, TMessage>) : ConcurrentDictionary<TKey, FastList<XChannel_KeyMessage<TKey, TMessage>>>
            public static XCollection_KeyMessage<TKey, TMessage> Collection;

            static Cache_KeyMessage()
            {
                Collection = new();
            }
        }

        internal static class Cache_KeyMessageResult<TKey, TMessage, TResult>
            where TKey : notnull
        {
            public static XCollection_KeyMessageResult<TKey, TMessage, TResult> Collection;

            static Cache_KeyMessageResult()
            {
                Collection = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }
}
