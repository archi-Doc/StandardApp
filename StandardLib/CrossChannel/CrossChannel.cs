﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Concurrent;
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

            var channel = new XChannel_MessageResult<TMessage, TResult>(Cache_MessageResult<TMessage, TResult>.List, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage>(object? weakReference, TKey key, Action<TMessage> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_Key2<TKey, TMessage>(Cache_KeyMessage<TKey, TMessage>.Map, key, weakReference, method);
            return channel;
        }

        public static XChannel OpenKey<TKey, TMessage, TResult>(object? weakReference, TKey key, Func<TMessage, TResult> method)
            where TKey : notnull
        {
            if (++cleanupCount >= CleanupThreshold)
            {
                cleanupCount = 0;
                // Cleanup(Cache_MessageResult<TMessage, TResult>.List);
            }

            var channel = new XChannel_KeyMessageResult<TKey, TMessage, TResult>(Cache_KeyMessageResult<TKey, TMessage, TResult>.Map, key, weakReference, method);
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
            /*var list = Cache_KeyMessage<TKey, TMessage>.Map[key] as FastList<XChannel_KeyMessage<TKey, TMessage>>;
            if (list == null)
            {
                return 0;
            }*/

            if (!Cache_KeyMessage<TKey, TMessage>.Map.TryGetValue(key, out var list))
            {
                return 0;
            }

            return list.Send(message);
        }

        public static TResult[] SendKey<TKey, TMessage, TResult>(TKey key, TMessage message)
            where TKey : notnull
        {
            FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>>? list;
            if (!Cache_KeyMessageResult<TKey, TMessage, TResult>.Map.TryGetValue(key, out list))
            {
                return Array.Empty<TResult>();
            }

            return list.SendKey(message);
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

        internal static class Cache_KeyMessage<TKey, TMessage>
            where TKey : notnull
        {
            public static ConcurrentDictionary<TKey, FastList<XChannel_Key2<TKey, TMessage>>> Map;

            static Cache_KeyMessage()
            {
                Map = new();
            }
        }

        internal static class Cache_KeyMessageResult<TKey, TMessage, TResult>
            where TKey : notnull
        {
            public static ConcurrentDictionary<TKey, FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>>> Map;

            static Cache_KeyMessageResult()
            {
                Map = new();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }
}
