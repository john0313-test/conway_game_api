using BenchmarkDotNet.Running;
using ConwayGameOfLife.Benchmarks;

namespace ConwayGameOfLife.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<GameOfLifeBenchmarks>();
    }
}