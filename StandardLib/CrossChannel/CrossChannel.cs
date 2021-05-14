// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public static class CrossChannel
    {
        internal const int CleanupThreshold = 32;
        internal const int DictionaryThreshold = 16;
        private static int cleanupCount = 0;

        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            var list = Cache_Message<TMessage>.List;
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_Message<TMessage>(list, weakReference, method);
            return channel;
        }

        public static XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            var list = Cache_MessageResult<TMessage, TResult>.List;
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                list.Cleanup();
            }

            var channel = new XChannel_MessageResult<TMessage, TResult>(list, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            var collection = Cache_KeyMessage<TKey, TMessage>.Collection;
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                collection.Cleanup();
            }

            // var channel = new XChannel_Key2<TKey, TMessage>(Cache_KeyMessage<TKey, TMessage>.Map, key, weakReference, method);

            var channel = new XChannel_KeyMessage<TKey, TMessage>(collection, key, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            var collection = Cache_KeyMessageResult<TKey, TMessage, TResult>.Collection;
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                collection.Cleanup();
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

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>An array of the return values (TResult).</returns>
        public static TResult[] Send<TMessage, TResult>(TMessage message)
        {
            return Cache_MessageResult<TMessage, TResult>.List.Send(message);
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

        public static TResult[] SendKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            if (!Cache_KeyMessageResult<TKey, TMessage, TResult>.Collection.Dictionary.TryGetValue(key, out var list))
            {
                return Array.Empty<TResult>();
            }

            return list.Send(message);
        }

#pragma warning disable SA1401 // Fields should be private
        internal static class Cache_Message<TMessage>
        {
            public static FastList<XChannel_Message<TMessage>> List;

            static Cache_Message()
            {
                List = new();
            }
        }

        internal static class Cache_MessageResult<TMessage, TResult>
        {
            public static FastList<XChannel_MessageResult<TMessage, TResult>> List;

            static Cache_MessageResult()
            {
                List = new();
            }
        }

        /*internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {
            public static ConcurrentDictionary<TKey, FastList<XChannel_Key2<TKey, TMessage>>> Map;

            static Cache_KeyMessage()
            {
                Map = new();
            }
        }*/

        internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {
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
