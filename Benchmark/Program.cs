// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

/*  BenchmarkDotNet, small template code
 *  PM> Install-Package BenchmarkDotNet
 */

using BenchmarkDotNet.Running;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var summary = BenchmarkRunner.Run<ReconstructTest>();
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(ReconstructTest),
                typeof(DelegateBenchmark),
                typeof(CrossChannelBenchmark),
            });
            switcher.Run(args);
        }
    }

    public class BenchmarkConfig : BenchmarkDotNet.Configs.ManualConfig
    {
        public BenchmarkConfig()
        {
            this.AddExporter(BenchmarkDotNet.Exporters.MarkdownExporter.GitHub);
            this.AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

            // this.AddJob(Job.ShortRun.With(BenchmarkDotNet.Environments.Platform.X64).WithWarmupCount(1).WithIterationCount(1));
            // this.AddJob(BenchmarkDotNet.Jobs.Job.MediumRun.WithGcForce(true).WithId("GcForce medium"));
            // this.AddJob(BenchmarkDotNet.Jobs.Job.ShortRun);
            this.AddJob(BenchmarkDotNet.Jobs.Job.MediumRun);
        }
    }
}
