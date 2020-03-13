﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using Arc.Visceral;
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
                sb.Append(record.Elapsed.ToString("F6"));
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
            Benchmark();
            Benchmark();
        }

        private static void Startup()
        {
            var c = new StartupClass();

            Stopwatch.Restart();
            Reconstruct.Do(ref c);
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

        private static void Benchmark()
        {
            var sw = new Stopwatch();
            var tc = new TestClass();
            byte[] b;

            sw.Restart();
            Reconstruct.Do(ref tc);
            sw.Lap("Reconstruct 1st");

            tc = new TestClass();
            Reconstruct.Do(ref tc);
            sw.Lap("Reconstruct 2nd");

            tc = new TestClass();
            object? obj = tc;
            Reflection.Reconstruct(ref obj);
            sw.Lap("Reconstruct Reflection");

            b = MessagePack.MessagePackSerializer.Serialize(tc);
            sw.Lap("MessagePack serialize 1st");

            b = MessagePack.MessagePackSerializer.Serialize(tc);
            sw.Lap("MessagePack serialize 2nd");

            var tc2 = MessagePack.MessagePackSerializer.Deserialize<TestClass>(b);
            sw.Lap("MessagePack deserialize");

            Console.WriteLine(sw.ToSimpleString());
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

        public void Reconstruct()
        {
            this.Address = "address...";
            this.ChildOption.Weight = 50.0;
            this.ChildOption.Description = "dest.";
        }
    }

    [MessagePackObject]
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
    public class ChildOption
    {
        [Key(0)]
        public string? Date { get; set; }

        [Key(1)]
        public double Weight;

        [Key(2)]
        public string? Description { get; set; }

        [Key(3)]
        public TestClass? Circular { get; set; }
    }

    [MessagePackObject]
    public class StartupClass
    {
        [Key(0)]
        public int N { get; set; }

        [Key(1)]
        public string? M { get; set; }
    }
}
