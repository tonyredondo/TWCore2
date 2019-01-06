using System;
using BenchmarkDotNet.Running;

namespace TWCore.Tests.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SerializersBench>();
        }
    }
}