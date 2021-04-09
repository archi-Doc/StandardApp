using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using FastExpressionCompiler;

namespace Benchmark.ExpressionTree
{
    public class TestClass
    {
        public int X { get; init; }

        public int Test()
        {
            return 2;
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class ExpressionTreeCompile
    {
        private TestClass tc = default!;

        public ExpressionTreeCompile()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            this.tc = new TestClass();
        }

        [Benchmark]
        public Func<object, int>? Compile_Func()
        {
            var method = (Func<int>)this.tc.Test;
            var type = method.Target.GetType();
            var targetParam = Expression.Parameter(typeof(object));
            var compiledDelegate = Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    Expression.Convert(targetParam, type),
                    method.Method),
                targetParam)
                .Compile();

            return compiledDelegate;
        }

        [Benchmark]
        public Func<object, int>? CompileFast_Func()
        {
            var method = (Func<int>)this.tc.Test;
            var type = method.Target.GetType();
            var targetParam = Expression.Parameter(typeof(object));
            var compiledDelegate = Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    Expression.Convert(targetParam, type),
                    method.Method),
                targetParam)
                .CompileFast();

            return compiledDelegate;
        }

        [Benchmark]
        public Action<TestClass, int>? Compile_Setter()
        {
            var type = typeof(TestClass);
            var expType = Expression.Parameter(type);
            var mi = type.GetMethod("set_X")!;
            var exp = Expression.Parameter(typeof(int));
            var d = Expression.Lambda<Action<TestClass, int>>(Expression.Call(expType, mi!, exp), expType, exp).Compile();
            return d;
        }

        [Benchmark]
        public Action<TestClass, int>? CompileFast_Setter()
        {
            var type = typeof(TestClass);
            var expType = Expression.Parameter(type);
            var mi = type.GetMethod("set_X")!;
            var exp = Expression.Parameter(typeof(int));
            var d = Expression.Lambda<Action<TestClass, int>>(Expression.Call(expType, mi!, exp), expType, exp).CompileFast();
            return d;
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class ExpressionTreeInvoke
    {
        private TestClass tc = default!;
        private Func<object, int> compiled;
        private Func<object, int> fastCompiled;
        private Action<TestClass, int> compiled2;
        private Action<TestClass, int> fastCompiled2;

        public ExpressionTreeInvoke()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            this.tc = new TestClass();
            var method = (Func<int>)this.tc.Test;
            var type = method.Target.GetType();
            var targetParam = Expression.Parameter(typeof(object));

            this.compiled = Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    Expression.Convert(targetParam, type),
                    method.Method),
                targetParam)
                .Compile();

            this.fastCompiled = Expression.Lambda<Func<object, int>>(
                Expression.Call(
                    Expression.Convert(targetParam, type),
                    method.Method),
                targetParam)
                .CompileFast();

            var type2 = typeof(TestClass);
            var expType = Expression.Parameter(type2);
            var mi = type2.GetMethod("set_X")!;
            var exp = Expression.Parameter(typeof(int));
            this.compiled2 = Expression.Lambda<Action<TestClass, int>>(Expression.Call(expType, mi!, exp), expType, exp).Compile();
            this.fastCompiled2 = Expression.Lambda<Action<TestClass, int>>(Expression.Call(expType, mi!, exp), expType, exp).Compile();
        }

        [Benchmark]
        public int Compile_Func()
        {
            return this.compiled(this.tc);
        }

        [Benchmark]
        public int CompileFast_Func()
        {
            return this.fastCompiled(this.tc);
        }

        [Benchmark]
        public int Compile_Setter()
        {
            this.compiled2(this.tc, 33);
            return this.tc.X;
        }

        [Benchmark]
        public int CompileFast_Setter()
        {
            this.fastCompiled2(this.tc, 33);
            return this.tc.X;
        }
    }
}
