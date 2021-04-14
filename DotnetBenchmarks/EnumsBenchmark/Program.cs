using System;
using System.Reflection;
using BenchmarkDotNet.Running;
using EnumsBenchmark.Models;

namespace EnumsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
        }
    }
}