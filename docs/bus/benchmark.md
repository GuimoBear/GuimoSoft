``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1288 (20H2/October2020Update)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100-rc.2.21505.57
  [Host]     : .NET 6.0.0 (6.0.21.48005), X64 RyuJIT
  Job-MYIMRF : .NET 6.0.0 (6.0.21.48005), X64 RyuJIT
  Job-FGWRAT : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT


```
|                                               Implementation |  Runtime |     Mean |    StdDev |     Error |  Gen 0 |  Gen 1 | Allocated |
|------------------------------------------------------------- |--------- |---------:|----------:|----------:|-------:|-------:|----------:|
|            Consuming directly from IConsumer&lt;string, byte[]&gt; | .NET 6.0 | 1.031 μs | 0.0116 μs | 0.0175 μs | 0.1211 |      - |     768 B |
|            Consuming directly from IConsumer&lt;string, byte[]&gt; | .NET 5.0 | 1.075 μs | 0.0043 μs | 0.0083 μs | 0.1211 |      - |     768 B |
|             Consuming and send via EventDispatcherMiddleware | .NET 6.0 | 1.421 μs | 0.0040 μs | 0.0067 μs | 0.1875 |      - |   1,184 B |
|             Consuming and send via EventDispatcherMiddleware | .NET 5.0 | 1.501 μs | 0.0072 μs | 0.0138 μs | 0.1875 |      - |   1,184 B |
|                             Consuming using all Bus features | .NET 6.0 | 2.947 μs | 0.0537 μs | 0.0811 μs | 0.3047 | 0.0039 |   1,909 B |
|                             Consuming using all Bus features | .NET 5.0 | 3.015 μs | 0.0742 μs | 0.1247 μs | 0.3047 |      - |   1,922 B |
| Consuming using all Bus features and IConsumeContextAccessor | .NET 6.0 | 3.163 μs | 0.0977 μs | 0.1477 μs | 0.3281 | 0.0039 |   2,068 B |
| Consuming using all Bus features and IConsumeContextAccessor | .NET 5.0 | 3.365 μs | 0.0915 μs | 0.1383 μs | 0.3281 |      - |   2,092 B |
