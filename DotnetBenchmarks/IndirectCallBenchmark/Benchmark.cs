﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;
using IndirectCallBenchmark.Models;

namespace IndirectCallBenchmark
{
    public class Benchmark
    {
        private const int NumberOfIterations = 100_000_000;
        private const int X = 1;
        private const int Y = 1;
        private static readonly ICalculator Calculator = new Calculator();
        private static readonly object BoxedCalculator = new Calculator();
        private static readonly Func<ICalculator, int, int, int> EmittedDelegate;
        private static readonly MethodInfo MethodInfo;

        static Benchmark()
        {
            EmittedDelegate = CreateDelegate(typeof(Calculator));
            MethodInfo = typeof(Calculator).GetMethod(nameof(ICalculator.Add));
        }
        
        public static Func<ICalculator, int, int, int> CreateDelegate(Type objType)
        {
            var methodInfo = objType.GetMethod(nameof(ICalculator.Add));
            
            var method = new DynamicMethod("Add", typeof(int), new[] {typeof(ICalculator), typeof(int), typeof(int)});
            var ilGenerator = method.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
            ilGenerator.Emit(OpCodes.Ret);

            return method.CreateDelegate<Func<ICalculator, int, int, int>>();
        }

        [Benchmark(Description = "Direct interface call", Baseline = true)]
        public void RunDirectInterfaceCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += Calculator.Add(X, Y);
            }
        }
        
        [Benchmark(Description = "Emitted delegate call")]
        public void RunEmittedDelegateCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += EmittedDelegate(Calculator, X, Y);
            }
        }
        
        [Benchmark(Description = "Dynamic call")]
        public void RunDynamicCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += ((dynamic) BoxedCalculator).Add(X, Y);
            }
        }
        
        [Benchmark(Description = "Reflection call")]
        public void RunReflectionCall()
        {
            var sum = 0L;
            var parameters = new object[] {X, Y};
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += (int) MethodInfo.Invoke(BoxedCalculator, parameters);
            }
        }
    }
}