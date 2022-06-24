using System.Runtime.CompilerServices;

namespace Arc.Visceral;

public class ReconstructLoader
{
    // [ModuleInitializer]
    public static void Load()
    {
        Reconstruct.Cache<Benchmark.ChildClass>(Benchmark.ChildClass.Reconstruct);
        Reconstruct.Cache<Benchmark.ChildClass2>(Benchmark.ChildClass2.Reconstruct);
        Reconstruct.Cache<Benchmark.TestClass>(Benchmark.TestClass.Reconstruct);
    }
}
