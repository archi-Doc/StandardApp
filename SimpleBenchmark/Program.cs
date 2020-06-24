// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Arc.Visceral;
using Arc.WeakDelegate;
using MessagePack;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1649 // File name should match first type name

namespace SimpleBenchmark
{
    public class Stopwatch
    {
        private readonly double frequencyR;
        private System.Diagnostics.Stopwatch stopwatch;
        private long restartTicks;

        public List<Record> Records { get; }

        public Stopwatch()
        {
            this.stopwatch = new System.Diagnostics.Stopwatch();
            this.frequencyR = 1.0d / (double)System.Diagnostics.Stopwatch.Frequency;
            this.stopwatch.Start();

            this.Records = new List<Record>();

            this.Restart();
        }

        public void Restart()
        {
            this.restartTicks = this.stopwatch.ElapsedTicks;
        }

        public void Lap(string? comment = null)
        {
            var record = new Record()
            {
                Elapsed = this.GetElapsed(),
                Comment = comment,
            };

            this.Restart();

            this.Records.Add(record);
        }

        public void Split(string? comment = null)
        {
            var record = new Record()
            {
                Elapsed = this.GetElapsed(),
                Comment = comment,
            };

            this.Records.Add(record);
        }

        public string ToSimpleString()
        {
            var sb = new StringBuilder();
            int n;

            void AppendText(Record record)
            {
                sb.Append(record.Comment);
                sb.Append(": ");
                var s = string.Format("{0:F1}", record.Elapsed * 1000_000);
                sb.Append(s);
            }

            for (n = 0; n < (this.Records.Count - 1); n++)
            {
                AppendText(this.Records[n]);
                sb.Append("\r\n");
            }

            if (n < this.Records.Count)
            {
                AppendText(this.Records[n]);
            }

            return sb.ToString();
        }

        public double GetElapsed() => (double)(this.stopwatch.ElapsedTicks - this.restartTicks) * this.frequencyR;

        public class Record
        {
            public double Elapsed { get; set; }

            public string? Comment { get; set; }
        }
    }

    public class Program
    {
        public static Stopwatch Stopwatch { get; } = new Stopwatch();

        public static void Main(string[] args)
        {
            Console.WriteLine("Simple Benchmark.");
            Console.WriteLine();

            Startup();
            Benchmark1();
            Benchmark1();
        }

        private static void Startup()
        {
            var c = new StartupClass();

            Stopwatch.Restart();
            Reconstruct.Do(c);
            Stopwatch.Lap("Reconstruct startup");

            c = new StartupClass();
            object? obj = c;
            Reflection.Reconstruct(ref obj);
            Stopwatch.Lap("Reflection startup");

            MessagePack.MessagePackSerializer.Serialize(c);
            Stopwatch.Lap("MessagePack startup");

            Console.WriteLine(Stopwatch.ToSimpleString());
            Console.WriteLine();
        }

        private static void Benchmark1()
        {
            var sw = new Stopwatch();

            sw.Restart();
            var tc = new TestClass();
            Reconstruct.Do(tc);
            sw.Lap("Reconstruct 1st");

            tc = new TestClass();
            Reconstruct.Do(tc);
            sw.Lap("Reconstruct 2nd");

            tc = new TestClass();
            Reconstruct.Do(tc);
            sw.Lap("Reconstruct 3rd");

            Reconstruct.BuildCode<TestClass>();
            sw.Lap("Build");

            Reconstruct.BuildCode<TestClass>();
            sw.Lap("Build");

            Reconstruct.BuildCode<TestClass>();
            sw.Lap("Build");

            tc = new TestClass();
            object? obj = tc;
            Reflection.Reconstruct(ref obj);
            sw.Lap("Reconstruct Reflection");

            Reflection.Reconstruct(ref obj);
            sw.Lap("Reconstruct Reflection");

            Reflection.Reconstruct(ref obj);
            sw.Lap("Reconstruct Reflection");

            /*b = MessagePack.MessagePackSerializer.Serialize(tc);
            sw.Lap("MessagePack serialize 1st");

            b = MessagePack.MessagePackSerializer.Serialize(tc);
            sw.Lap("MessagePack serialize 2nd");

            var tc2 = MessagePack.MessagePackSerializer.Deserialize<TestClass>(b);
            sw.Lap("MessagePack deserialize");*/

            Console.WriteLine(sw.ToSimpleString());
            Console.WriteLine();
        }

        private static void Benchmark2()
        {
            var sw = new Stopwatch();
            var tc = new TestClass();

            new WeakFunc<int, int>(a => a);
            sw.Lap("WeakDelegate a => a");
            new WeakFunc<int, int>(b => b);
            sw.Lap("WeakDelegate b => b");
            new WeakFunc<string, string>(n => n + "a");
            sw.Lap("WeakDelegate n => n + \"a\"");
            new WeakAction<string>(n => { });
            sw.Lap("WeakDelegate n => { }");
            new WeakFunc<uint, uint>(TestDelegate);
            sw.Lap("WeakDelegate TestDelegate");
            new WeakFunc<uint, uint>(TestDelegate);
            sw.Lap("WeakDelegate TestDelegate 2nd");

            /* new Arc.WeakDelegate.Original.WeakFunc<int, int>(a => a);
            sw.Lap("WeakDelegate Original a => a");
            new Arc.WeakDelegate.Original.WeakFunc<int, int>(b => b);
            sw.Lap("WeakDelegate Original b => b");
            new Arc.WeakDelegate.Original.WeakFunc<long, long>(c => c * 2);
            new Arc.WeakDelegate.Original.WeakFunc<long, long>(c => c * 3);
            sw.Lap("WeakDelegate Original c => c * 2");
            new Arc.WeakDelegate.Original.WeakFunc<string, string>(d => d + "d");
            sw.Lap("WeakDelegate Original d => d * \"d\"");
            new Arc.WeakDelegate.Original.WeakFunc<uint, uint>(TestDelegate);
            sw.Lap("WeakDelegate Original TestDelegate");
            new Arc.WeakDelegate.Original.WeakFunc<uint, uint>(TestDelegate);
            sw.Lap("WeakDelegate Original TestDelegate 2nd");*/

            Console.WriteLine(sw.ToSimpleString());
            Console.WriteLine();
        }

        private static uint TestDelegate(uint x)
        {
            return x * 2;
        }
    }

    [MessagePackObject]
    public class TestClass
    {
        [Key(0)]
        public int IntX;
        [Key(1)]
        public int IntY;

        [Key(2)]
        public ChildClass ChildA { get; set; }

        [Key(3)]
        public ChildClass? ChildB { get; set; }

        [Key(4)]
        public ChildStruct ChildC { get; set; }

        [Key(5)]
        public ChildStruct ChildD;

        [Key(6)]
        public string? Information;

        [Key(7)]
        public TestClass? Circular { get; set; }

        [Key(8)]
        public ChildClass? Child8;
        [Key(9)]
        public ChildClass? Child9;
        [Key(10)]
        public ChildClass? Child10;
        [Key(11)]
        public ChildClass? Child11;
        [Key(12)]
        public ChildClass? Child12;
        [Key(13)]
        public ChildClass? Child13;
        [Key(14)]
        public ChildClass? Child14;
        [Key(15)]
        public ChildClass? Child15;

        public TestClass()
        {
            this.ChildA = new ChildClass();
            this.IntX = 100;
        }
    }

    [MessagePackObject]
    public class ChildClass : IReconstructable
    {
        [Key(0)]
        public string? Name { get; set; }

        [Key(1)]
        public int Id { get; set; }

        [Key(2)]
        public string? Address { get; set; }

        [Key(3)]
        public ChildStruct ChildStructA { get; set; }

        [Key(4)]
        public ChildStruct ChildStructB { get; set; }

        [Key(5)]
        public ChildOption ChildOption { get; set; } = default!;

        [Key(6)]
        public ChildOption ChildOption2 { get; set; } = default!;

        [Key(7)]
        public ChildOption ChildOption3 { get; set; } = default!;

        [Key(8)]
        public ChildOption ChildOption4 { get; set; } = default!;

        public void Reconstruct()
        {
            this.Address = "address...";
            this.ChildOption.Weight = 50.0;
            this.ChildOption.Description = "dest.";
        }
    }

    [MessagePackObject]
    [Reconstructable]
    public struct ChildStruct
    {
        [Key(0)]
        public int Age;
        [Key(1)]
        public string Memo;
        [Key(2)]
        public double Height;
    }

    [MessagePackObject]
    [Reconstructable]
    public class ChildOption
    {
        [Key(0)]
        [Reconstructable]
        public string? Date { get; set; }

        [Key(1)]
        public double Weight;

        [Key(2)]
        [Reconstructable]
        public string? Description { get; set; }

        [Key(3)]
        public TestClass? Circular { get; set; }
    }

    [MessagePackObject]
    [Reconstructable]
    public class StartupClass
    {
        [Key(0)]
        public int N { get; set; }

        [Key(1)]
        public string? M { get; set; }
    }
}
