// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arc.WeakDelegate;

namespace Arc.CrossChannel2
{
    public static class CrossChannel
    {
        private const int CleanupThreshold = 16;

        private static object cs = new object();

        private static int cleanupCount = 0;

        static CrossChannel()
        {
        }

        public static XChannel Open<TMessage>(object? weakReference, Action<TMessage> method)
        {
            var channel = new XChannel<TMessage>(weakReference, method);

            Array.Resize(ref list, list.Length + 1);
            list[index] = channel;
            CleanupList(list);

            return channel;
        }

        private static void CleanupList(LinkedList<XChannel> list)
        {
            if (++cleanupCount < CleanupThreshold)
            {
                return;
            }

            cleanupCount = 0; // Initialize.

            LinkedListNode<XChannel>? node = list.First;
            LinkedListNode<XChannel>? nextNode;

            while (node != null)
            {
                nextNode = node.Next;

                if (!node.Value.IsAlive && node.Value.ReferenceCount == 0)
                {
                    CloseChannel(node.Value);
                }

                node = nextNode;
            }
        }

        public static void Close(XChannel channel)
        {
            if (channel.IsClosed())
            {// Already closed.
                return;
            }

            while (true)
            {
                lock (cs)
                {
                    if (channel.ReferenceCount == 0)
                    {// reference countが0（Send / Receive処理をしていない状態）になったら、Close
                        CloseChannel(channel);
                        break;
                    }
                }
#if NETFX_CORE
                Task.Delay(50).Wait();
#else
                System.Threading.Thread.Sleep(50);
#endif
            }
        }

        private static void CloseChannel(XChannel channel)
        {// lock (cs) required. Reference count must be 0.
            // list: Identification to XChannels.
            if (!channel.IsClosed())
            {
                var list = channel.GetArray();
                if (list != null)
                {
                    list.Remove(channel.Node);
                }
            }

            channel.MarkForDeletion();
        }

        private static XChannel[] PrepareXChannelArray(LinkedList<XChannel> list)
        { // lock (cs) required. Convert LinkedList to Array, release garbage collected object, and increment reference count.
            var array = new XChannel[list.Count];
            var arrayCount = 0;
            var node = list.First;
            LinkedListNode<XChannel>? nextNode;

            if (targetId == null)
            {
                while (node != null)
                {
                    nextNode = node.Next;

                    if (node.Value.IsAlive)
                    {// The instance is still alive.
                        array[arrayCount++] = node.Value;
                    }
                    else if (node.Value.ReferenceCount == 0)
                    {// The instance is garbage collected and reference count is 0.
                        CloseChannel(node.Value);
                    }

                    node = nextNode;
                }
            }
            else
            {
                while (node != null)
                {
                    nextNode = node.Next;

                    if (node.Value.IsAlive)
                    {// The instance is still alive.
                        if (node.Value.TargetId == targetId)
                        {
                            array[arrayCount++] = node.Value;
                        }
                    }
                    else if (node.Value.ReferenceCount == 0)
                    {// Garbage collected and reference count is 0.
                        CloseChannel(node.Value);
                    }

                    node = nextNode;
                }
            }

            if (array.Length != arrayCount)
            {
                Array.Resize(ref array, arrayCount);
            }

            foreach (var x in array)
            {
                x.ReferenceCount++;
            }

            return array;
        }

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <returns>A number of the receivers.</returns>
        public static int Send<TMessage>(TMessage message) => SendTarget<TMessage>(message, null);

        /// <summary>
        /// Send a message to receivers.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="targetId">The receiver with the same target id will receive this message. Set null to broadcast.</param>
        /// <returns>A number of the receivers.</returns>
        public static int SendTarget<TMessage>(TMessage message)
        {
            XChannel[] array;
            var numberReceived = 0;

            lock (cs)
            {
                array = PrepareXChannelArray(Cache_Message<TMessage>.List, targetId);
            }

            try
            {
                foreach (var x in array)
                {
                    var d = x.WeakDelegate as WeakAction<TMessage>;
                    if (d == null)
                    {
                        continue;
                    }

                    d.Execute(message, out var executed);
                    if (executed)
                    {
                        numberReceived++;
                    }
                }
            }
            finally
            {
                DecrementReferenceCount(array);
            }

            return numberReceived;
        }

#pragma warning disable SA1401 // Fields should be private
        internal static class Cache_Message<TMessage>
        {
            public static Arc.CrossChannel.FreeList<XChannel<TMessage>> List;

            static Cache_Message()
            {
                List = new();
            }
        }

        private static class Cache_WeakFunction<TMessage, TResult>
        {
            public static LinkedList<XChannel> List;

            static Cache_WeakFunction()
            {
                List = new LinkedList<XChannel>();
            }
        }
#pragma warning restore SA1401 // Fields should be private
    }

    public class XChannel<TMessage> : XChannel
    {
        public XChannel(object? weakReference, Action<TMessage> method)
        {
            var list = CrossChannel.Cache_Message<TMessage>.List;
            this.Index = list.Add(this);
            if (weakReference == null)
            {
                this.StrongDelegate = method;
            }
            else
            {
                this.WeakDelegate = new WeakAction<TMessage>(weakReference, method);
            }
        }

        internal Action<TMessage>? StrongDelegate { get; set; }

        internal WeakAction<TMessage>? WeakDelegate { get; set; }

        public override bool IsAlive() => this.StrongDelegate != null || this.WeakDelegate?.IsAlive == true;

        public override object GetArray() => CrossChannel.Cache_Message<TMessage>.List;

        public override void MarkForDeletion()
        {
            this.ReferenceCount = 0;
            if (this.Index != -1)
            {
                CrossChannel.Cache_Message<TMessage>.List.Remove(this.Index, false);
                this.Index = -1;
            }
        }
    }

    public abstract class XChannel : IDisposable
    {
        internal int Index { get; set; }

        internal int ReferenceCount { get; set; }

        public bool IsClosed() => this.Index == -1;

        public abstract bool IsAlive();

        public abstract object GetArray();

        public virtual void MarkForDeletion()
        {
            this.Index = -1;
            this.ReferenceCount = 0;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.Index != -1)
            {
                if (disposing)
                {
                    CrossChannel.Close(this);
                }

                this.Index = -1;
            }
        }
    }
}
