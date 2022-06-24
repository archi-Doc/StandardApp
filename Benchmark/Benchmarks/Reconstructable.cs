// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;

#pragma warning disable SA1401 // Fields should be private

namespace Arc.Visceral;

/// <summary>
/// Reconstruct delegate.
/// </summary>
/// <typeparam name="T">The type of the object.</typeparam>
/// <param name="t">The object to be reconstructed.</param>
/// <returns>The object reconstructed.</returns>
public delegate T ReconstructFunc<T>(T t);

public delegate void ReconstructAction<T>(ref T t);

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ReconstructableAttribute : Attribute
{
}

/// <summary>
/// Reconstruct.Do() will call OnAfterReconstruct() method of each class which implements IReconstructable interface.
/// </summary>
public interface IReconstructable
{
    void OnAfterReconstruct();
}

public static partial class Reconstruct
{
    static Reconstruct()
    {
        Load();
    }

    static partial void Load();

    public static T Do<T>(T t)
        where T : class
    {
        var c = ResolverFuncCache<T>.Cache;
        if (c != null)
        {
            return c.Invoke(t);
        }

        return t;
    }

    public static void Do<T>(ref T t)
        where T : struct
    {
        var c = ResolverActionCache<T>.Cache;
        if (c != null)
        {
            c.Invoke(ref t);
        }
    }

    public static void Cache<T>(ReconstructFunc<T> cache)
    {
        ResolverFuncCache<T>.Cache = cache;
    }

    public static void Cache<T>(ReconstructAction<T> cache)
    {
        ResolverActionCache<T>.Cache = cache;
    }

#pragma warning disable 0649
    private static class ResolverFuncCache<T>
    {
        public static ReconstructFunc<T>? Cache;

        static ResolverFuncCache()
        {
        }
    }

    private static class ResolverActionCache<T>
    {
        public static ReconstructAction<T>? Cache;

        static ResolverActionCache()
        {
        }
    }
#pragma warning restore 0649
}
