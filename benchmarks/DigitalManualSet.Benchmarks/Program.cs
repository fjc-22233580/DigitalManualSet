using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .WithArtifactsPath(Path.Combine("output", "benchmarks", DateTime.Now.ToString("yyyyMMdd-HHmmss")));

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);