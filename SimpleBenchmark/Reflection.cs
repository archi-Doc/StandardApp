// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleBenchmark
{
    public class Reflection
    {
        public static void Reconstruct(ref object? target)
        {
            void ReconstructAction(ref object? obj, Stack<Type> reconstructedType)
            {
                object? instance;

                if (obj == null)
                {
                    return;
                }

                var type = obj.GetType();
                if (type.IsPrimitive)
                {
                    return;
                }

                reconstructedType.Push(type);

                foreach (var x in type.GetFields())
                {// field
                    if (x.FieldType.IsClass)
                    {
                        try
                        { // new instance.
                            instance = x.GetValue(obj);
                            if (instance == null)
                            {
                                if (x.FieldType == typeof(string))
                                {
                                    instance = string.Empty;
                                }
                                else
                                {
                                    instance = Activator.CreateInstance(x.FieldType);
                                }

                                x.SetValue(obj, instance);
                            }

                            if (!reconstructedType.Contains(x.FieldType))
                            {// prevent circular dependency.
                                ReconstructAction(ref instance, reconstructedType);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                foreach (var x in type.GetProperties())
                {// property
                    if (x.PropertyType.IsClass)
                    {
                        try
                        {
                            instance = x.GetValue(obj);
                            if (instance == null)
                            {
                                if (x.PropertyType == typeof(string))
                                {
                                    instance = string.Empty;
                                }
                                else
                                {
                                    instance = Activator.CreateInstance(x.PropertyType);
                                }

                                x.SetValue(obj, instance);
                            }

                            if (!reconstructedType.Contains(x.PropertyType))
                            {
                                ReconstructAction(ref instance, reconstructedType);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                // IReconstruct.Reconstruct()
                try
                {
                    var miReconstruct = type.GetInterfaceMap(typeof(Arc.Visceral.IReconstructable)).InterfaceMethods.First(x => x.Name == "Reconstruct");
                    miReconstruct.Invoke(obj, null);
                }
                catch
                {
                }

                reconstructedType.Pop();
            }

            var r = new Stack<Type>();
            ReconstructAction(ref target, r);
        }
    }
}
