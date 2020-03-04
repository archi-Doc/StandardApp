// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1649 // File name should match first type name

namespace Arc.Visceral
{
    /// <summary>
    /// Reconstruct delegate.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="t">The object to be reconstructed.</param>
    public delegate void ReconstructAction<T>(ref T t);

    /// <summary>
    /// Reconstruct.Do() will call Reconstruct() method of each class which implements IReconstructable interface.
    /// </summary>
    public interface IReconstructable
    {
        public void Reconstruct();
    }

    /// <summary>
    /// Get the reconstruct delegate (ReconstructAction&lt;T&gt;).
    /// </summary>
    public interface IReconstructResolver
    {
        bool Get<T>(out ReconstructAction<T>? action);
    }

    public class DefaultReconstructResolver : IReconstructResolver
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly DefaultReconstructResolver Instance = new DefaultReconstructResolver();

        public bool Get<T>(out ReconstructAction<T>? action)
        {
            action = default;

            var type = typeof(T);
            if (type.IsPrimitive)
            {
                return true;
            }

            return false;
        }
    }

    public static class Reconstruct
    {
        private static IReconstructResolver[]? resolvers; // Internal use.

        static Reconstruct()
        {
        }

        /// <summary>
        /// Gets or sets InitialResolvers (custom resolvers).
        /// 
        /// </summary>
        public static IReconstructResolver[]? InitialResolvers { get; set; }

        private static IReconstructResolver[] Resolvers // Get Resolver.
        {
            get
            {
                if (resolvers == null)
                { // 1st. InitialResolvers.
                    resolvers = InitialResolvers;
                }

                if (resolvers == null)
                { // 2nd. Default Resolver.
                    resolvers = new IReconstructResolver[] { DefaultReconstructResolver.Instance };
                }

                return resolvers;
            }
        }

        public static void Do<T>(ref T t)
        {
            ResolverCache<T>.Cache?.Invoke(ref t);
        }

        private static ReconstructAction<T> BuildCode<T>()
        {
            var type = typeof(T);
            var typeRef = type.MakeByRefType();
            var info = ObjectInfo.CreateFromType(type);
            var expressions = new List<Expression>();

            // log Console.WriteLine("cache: " + type.Name);
            // log expressions.Add(Expression.Call(null, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }), Expression.Constant("reconstruct: " + type.Name)));
            var arg = Expression.Parameter(typeRef); // type
            foreach (var member in info.Members.Where(x => x.Type.IsClass))
            {// Class
                var prop = Expression.PropertyOrField(arg, member.Name);

                // new instance.
                if (member.Type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var e = Expression.IfThen(
                        Expression.Equal(prop, Expression.Constant(null)),
                        Expression.Assign(prop, Expression.New(member.Type)));
                    expressions.Add(e);
                }
                else if (member.Type == typeof(string))
                {
                    expressions.Add(Expression.Assign(prop, Expression.Constant(string.Empty)));
                }

                // reconstruct
                var memberReconstructorCache = typeof(ResolverCache<>).MakeGenericType(member.Type);
                var reconstructorAction = memberReconstructorCache.GetField(nameof(ResolverCache<int>.Cache));
                if (reconstructorAction?.GetValue(memberReconstructorCache) != null)
                {
                    expressions.Add(Expression.Invoke(Expression.MakeMemberAccess(null, reconstructorAction), prop));
                }
            }

            foreach (var member in info.Members.Where(x => x.Type.IsStruct()))
            {// Struct
                var prop = Expression.PropertyOrField(arg, member.Name);

                // reconstruct
                var memberReconstructorCache = typeof(ResolverCache<>).MakeGenericType(member.Type);
                var reconstructorAction = memberReconstructorCache.GetField(nameof(ResolverCache<int>.Cache));
                if (reconstructorAction?.GetValue(memberReconstructorCache) != null)
                {
                    expressions.Add(Expression.Invoke(Expression.MakeMemberAccess(null, reconstructorAction), prop));
                }
            }

            // IReconstruct.Reconstruct()
            try
            {
                var miReconstruct = type.GetInterfaceMap(typeof(IReconstructable)).InterfaceMethods.First(x => x.Name == "Reconstruct");
                expressions.Add(Expression.Call(arg, miReconstruct));
            }
            catch
            {
            }

            var body = Expression.Block(expressions.ToArray());
            var lamda = Expression.Lambda<ReconstructAction<T>>(body, arg);
            return lamda.Compile();
        }

        private static class ResolverCache<T>
        {
            public static readonly ReconstructAction<T>? Cache;

            static ResolverCache()
            {
                foreach (var x in Reconstruct.Resolvers)
                {
                    if (x.Get<T>(out var m))
                    { // found
                        Cache = m;
                        return;
                    }
                }

                Cache = Reconstruct.BuildCode<T>();
            }
        }
    }
}
