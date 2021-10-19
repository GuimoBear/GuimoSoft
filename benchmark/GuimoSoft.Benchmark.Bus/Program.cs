using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Benchmark.Bus
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Iterations: " + Config.Iterations);
            new BenchmarkSwitcher(typeof(BenchmarkBase).Assembly).Run(args, new Config());
        }
    }
}
