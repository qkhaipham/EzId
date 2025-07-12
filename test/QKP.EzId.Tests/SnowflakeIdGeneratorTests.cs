using FluentAssertions;
using System.Collections.Concurrent;

namespace QKP.EzId.Tests
{
    public class SnowflakeIdGeneratorTests
    {
        private readonly SnowflakeIdGenerator _snowflakeIdGenerator = new(12);

        [Fact]
        public void When_generating_it_must_return_expected()
        {
            // Act
            long id = _snowflakeIdGenerator.GetNextId();

            // Assert
            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void When_generating_multiple_ids_it_must_return_expected()
        {
            // Act
            var ids = Enumerable.Range(0, 1000).Select(_ => _snowflakeIdGenerator.GetNextId()).ToList();

            // Assert
            ids.Should().NotBeEmpty();
            ids.Should().OnlyHaveUniqueItems();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1024)]
        public void When_generator_id_is_out_of_range_it_must_throw_exception(long generatorId)
        {
            // Act
            Action act = () => new SnowflakeIdGenerator(generatorId);

            // Assert
            if (generatorId is < 1 or > 1024)
            {
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
            else
            {
                act.Should().NotThrow();
            }
        }

        [Fact]
        public void When_generating_max_sequence_in_same_millisecond_it_must_handle_correctly()
        {
            var idGenerator = new SnowflakeIdGenerator(1);
            var ids = new List<long>();

            // ACT
            // Generate 4097 IDs (max sequence number) + 1 more
            for (int i = 0; i < 4096 + 1; i++)
            {
                long id = idGenerator.GetNextId();
                ids.Contains(id).Should().BeFalse($"{i}");
                ids.Add(id);
            }

            ids.Count.Should().Be(4097);
            long firstId = ids.First();
            long lastId = ids.Last();
            long firstTimestamp = firstId >> (10 + 12); // 10 for GeneratorId, 12 for Sequence
            long lastTimestamp = lastId >> (10 + 12);
            lastTimestamp.Should().BeGreaterThan(firstTimestamp, "Last ID should have a newer timestamp");
        }

        [Fact]
        public async Task When_generating_ids_concurrently_they_must_be_unique()
        {
            const int numThreads = 4;
            const int idsPerThread = 100;
            var idGenerator = new SnowflakeIdGenerator(1);
            var ids = new ConcurrentBag<long>();
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < numThreads; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < idsPerThread; j++)
                    {
                        ids.Add(idGenerator.GetNextId());
                    }
                }));
            }
            await Task.WhenAll(tasks);

            // Assert
            ids.Should().HaveCount(numThreads * idsPerThread);
            ids.Distinct().Should().HaveCount(numThreads * idsPerThread);
        }

        [Fact]
        public void When_generating_ids_then_bit_structure_must_be_correct()
        {
            const long generatorId = 123;
            var idGenerator = new SnowflakeIdGenerator(generatorId);

            // Act
            long id = idGenerator.GetNextId();

            // Assert
            long timestamp = id >> 22; // 41 bits for timestamp (after first unused bit)
            long extractedGeneratorId = (id >> 12) & 0x3FF; // 10 bits for generator ID
            long sequence = id & 0xFFF; // 12 bits for sequence

            extractedGeneratorId.Should().Be(generatorId);
            sequence.Should().Be(1); // First sequence number
            timestamp.Should().BeGreaterThan(0);

            // Verify first bit is unused (should be 0)
            ((id >> 63) & 1).Should().Be(0);
        }

        [Fact]
        public async Task When_multiple_generators_run_concurrently_ids_must_be_unique()
        {
            const int numGenerators = 4;
            const int idsPerGenerator = 1000;
            var generators = Enumerable.Range(0, numGenerators)
                .Select(i => new SnowflakeIdGenerator(i))
                .ToList();
            var ids = new ConcurrentBag<long>();

            // Act
            await Task.WhenAll(generators.Select(generator => Task.Run(() =>
            {
                for (int i = 0; i < idsPerGenerator; i++)
                {
                    ids.Add(generator.GetNextId());
                }
            })).ToArray());

            // Assert
            ids.Should().HaveCount(numGenerators * idsPerGenerator);
            ids.Distinct().Should().HaveCount(numGenerators * idsPerGenerator);
        }

        [Fact]
        public void When_clock_moves_backwards_it_must_throw_exception()
        {
            // Arrange
            var tickProvider = new StubTickProvider(1000);
            var idGenerator = new SnowflakeIdGenerator(1, tickProvider);

            // First call to establish last tick
            idGenerator.GetNextId();

            // Simulate clock moving backwards
            tickProvider.SetTick(500);

            // Act
            Action act = () => idGenerator.GetNextId();

            // Assert
            act.Should().Throw<DayLightSavingChangedException>();
        }
    }

    internal class StubTickProvider(long initialTick) : ITickProvider
    {
        private long _currentTick = initialTick;

        public void SetTick(long tick)
        {
            _currentTick = tick;
        }

        public long GetTick()
        {
            return _currentTick;
        }
    }
}
