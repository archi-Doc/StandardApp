// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Arc.CrossChannel
{
    internal static partial class CrossChannelExtensions
    {
        internal static int Send<TMessage>(this FastList<XChannel_Message<TMessage>> list, TMessage message)
        {
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

        internal static int Send<TKey, TMessage>(this FastList<XChannel_KeyMessage<TKey, TMessage>> list, TMessage message)
            where TKey : notnull
        {
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

        internal static Task SendAsync<TMessage>(this FastList<XChannel_MessageResult<TMessage, Task>> list, TMessage message)
        {
            // (var array, var count) = list.GetValuesAndCount(); // Slow
            var array = list.GetValues();
            var results = new Task[array.Length];
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

            return Task.WhenAll(results);
        }

        internal static TResult[] Send<TMessage, TResult>(this FastList<XChannel_MessageResult<TMessage, TResult>> list, TMessage message)
        {
            // (var array, var count) = list.GetValuesAndCount(); // Slow
            var array = list.GetValues();
            var results = new TResult[array.Length];
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

        internal static Task<TResult[]> SendAsync<TMessage, TResult>(this FastList<XChannel_MessageResult<TMessage, Task<TResult>>> list, TMessage message)
        {
            // (var array, var count) = list.GetValuesAndCount(); // Slow
            var array = list.GetValues();
            var results = new Task<TResult>[array.Length];
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

            return Task.WhenAll(results);
        }

        internal static TResult[] Send<TKey, TMessage, TResult>(this FastList<XChannel_KeyMessageResult<TKey, TMessage, TResult>> list, TMessage message)
            where TKey : notnull
        {
            // (var array, var count) = list.GetValuesAndCount(); // Slow
            var array = list.GetValues();
            var results = new TResult[array.Length];
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

        internal static bool Cleanup<TMessage>(this FastList<XChannel_Message<TMessage>> list)
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

            return list.TryShrink();
        }

        internal static bool Cleanup<TMessage, TResult>(this FastList<XChannel_MessageResult<TMessage, TResult>> list)
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

            return list.TryShrink();
        }

        internal static bool Cleanup<TKey, TMessage>(this XCollection_KeyMessage<TKey, TMessage> collection)
            where TKey : notnull
        {
            lock (collection)
            {
                foreach (var x in collection.Dictionary)
                {
                    var array = x.Value.GetValues();
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
                }

                return collection.Count == 0;
            }
        }

        internal static bool Cleanup<TKey, TMessage, TResult>(this XCollection_KeyMessageResult<TKey, TMessage, TResult> collection)
            where TKey : notnull
        {
            lock (collection)
            {
                foreach (var x in collection.Dictionary)
                {
                    var array = x.Value.GetValues();
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
                }

                return collection.Count == 0;
            }
        }
    }
}
