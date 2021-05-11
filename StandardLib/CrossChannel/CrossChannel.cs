// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Arc.WeakDelegate;

namespace Arc.CrossChannel
{
    public static class CrossChannel
    {
        public const int CleanupThreshold = 16;
        private static int cleanupCount = 0;

        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                Cache_Message<TMessage>.List.Cleanup();
            }

            var channel = new XChannel_Message<TMessage>(Cache_Message<TMessage>.List, weakReference, method);
            return channel;
        }

        public static XChannel Open<TMessage, TResult>(object? weakReference, Func<TMessage, TResult> method)
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                Cache_MessageResult<TMessage, TResult>.List.Cleanup();
            }

            var channel = new XChannel<TMessage, TResult>(Cache_MessageResult<TMessage, TResult>.List, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage>(TKey key, object? weakReference, Action<TMessage> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_KeyMessage<TKey, TMessage>(Cache_KeyMessage<TKey, TMessage>.Map, key, weakReference, method);
            return channel;
        }

        /*public static XChannel OpenKey<TKey, TMessage, TResult>(TKey key, object? weakReference, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_KeyMessageResult<TKey, TMessage, TResult>(Cache_KeyMessage<TKey, TMessage>.Map, key, weakReference, method);
            return channel;
        }*/

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
        /// <param name="message">The message to send.</param>
        /// <typeparam name="TResult">The type of the return value.</typeparam>
        /// <returns>An array of the return values (TResult).</returns>
        public static TResult[] Send<TMessage, TResult>(TMessage message)
        {
            return Cache_MessageResult<TMessage, TResult>.List.Send(message);
        }

        public static int SendKey<TKey, TMessage>(TKey key, TMessage message)
            where TKey : notnull
        {
            var list = Cache_KeyMessage<TKey, TMessage>.Map[key] as FastList<XChannel_KeyMessage<TKey, TMessage>>;
            if (list == null)
            {
                return 0;
            }

            /*FastList<XChannel_Key<TKey, TMessage>>? list;
            var map = Cache_KeyMessage<TKey, TMessage>.Map;
            lock (map)
            {
                if (!map.TryGetValue(key, out list))
                {
                    return 0;
                }
            }*/

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
            public static FastList<XChannel<TMessage, TResult>> List;

            static Cache_MessageResult()
            {
                List = new();
            }
        }

        internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {
            public static Hashtable Map;

            static Cache_KeyMessage()
            {
                Map = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }
}
