using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace QKP.EzId.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class IdGeneratorBenchmarks
{
    private IdGenerator _idGenerator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _idGenerator = new IdGenerator(1);
    }

    [Benchmark]
    public long Generate_Single_Id()
    {
        return _idGenerator.GetNextId();
    }

    [Benchmark]
    public void Generate_Batch_Of_Ids()
    {
        for (int i = 0; i < 1000; i++)
        {
            _idGenerator.GetNextId();
        }
    }

    [Benchmark]
    public void Generate_Max_Sequence_In_Millisecond()
    {
        for (int i = 0; i < 4096; i++)
        {
            _idGenerator.GetNextId();
        }
    }

    [Benchmark]
    public void Generate_Concurrent_Ids()
    {
        const int numThreads = 4;
        const int idsPerThread = 1000;
        var tasks = new Task[numThreads];

        for (int i = 0; i < numThreads; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < idsPerThread; j++)
                {
                    _idGenerator.GetNextId();
                }
            });
        }

        Task.WaitAll(tasks);
    }
}
