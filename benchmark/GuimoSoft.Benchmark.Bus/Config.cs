using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using GuimoSoft.Benchmark.Bus.Helpers;

namespace GuimoSoft.Benchmark.Bus
{
    public class Config : ManualConfig
    {
        public const int Iterations = 2000;

        public Config()
        {
            AddLogger(ConsoleLogger.Default);

            AddExporter(CsvExporter.Default);
            AddExporter(MarkdownExporter.GitHub);
            AddExporter(HtmlExporter.Default);

            var md = MemoryDiagnoser.Default;
            AddDiagnoser(md);
            AddColumn(new ImplementationColumn());
            //AddColumn(TargetMethodColumn.Method);
            AddColumn(new RuntimeColumn());
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Error);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumnProvider(DefaultColumnProviders.Metrics);

            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core50)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(Iterations)
                   .WithIterationCount(10)
            );

            AddJob(Job.Default
                   .WithRuntime(CoreRuntime.Core60)
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(Iterations)
                   .WithIterationCount(10)
            );
            Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
            Options |= ConfigOptions.JoinSummary;
        }
    }
}
