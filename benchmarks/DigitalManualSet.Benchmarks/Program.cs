using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .WithArtifactsPath("output/benchmarks");

BenchmarkRunner.Run<SimpleBenchmark>(config);

[MemoryDiagnoser]
public class SimpleBenchmark
{
    [Benchmark]
    public string CreateString()
    {
        return "Digital" + "Manual" + "Set";
    }
}