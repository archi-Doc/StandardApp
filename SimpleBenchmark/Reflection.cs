using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBenchmark
{
    public class Reflection
    {
        public static void Reconstruct(ref object? target)
        {
            void ReconstructAction(ref object? obj)
            {
                object? instance;

                if (obj == null)
                {
                    return;
                }

                foreach (var x in obj.GetType().GetFields())
                {// field
                    if (x.FieldType.IsClass) // Attribute.GetCustomAttribute(x.FieldType, typeof(MessagePackObjectAttribute)) != null
                    {
                        try
                        {
                            instance = x.GetValue(obj);
                            if (instance == null)
                            {
                                instance = Activator.CreateInstance(x.FieldType);
                                x.SetValue(obj, instance);
                            }

                            ReconstructAction(ref instance);
                        }
                        catch
                        {
                        }
                    }
                }

                foreach (var x in obj.GetType().GetProperties())
                {// property
                    if (x.PropertyType.IsClass) // (Attribute.GetCustomAttribute(x.PropertyType, typeof(MessagePackObjectAttribute)) != null
                    {
                        try
                        {
                            instance = x.GetValue(obj);
                            if (instance == null)
                            {
                                instance = Activator.CreateInstance(x.PropertyType);
                                x.SetValue(obj, instance);
                            }

                            ReconstructAction(ref instance);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            ReconstructAction(ref target);
        }
    }
}
