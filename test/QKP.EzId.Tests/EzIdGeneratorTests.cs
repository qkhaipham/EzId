using FluentAssertions;

namespace QKP.EzId.Tests
{
    public class EzIdGeneratorTests
    {
        private readonly EzIdGenerator<EzId> _sut = new(12);

        [Fact]
        public void Given_ez_id_type_when_generating_it_must_return_expected()
        {
            EzId id = _sut.GetNextId();
            id.Should().NotBeNull();
        }
    }
}
