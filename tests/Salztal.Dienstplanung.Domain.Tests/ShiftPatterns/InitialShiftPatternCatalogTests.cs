using Salztal.Dienstplanung.Domain.ShiftPatterns;

namespace Salztal.Dienstplanung.Domain.Tests.ShiftPatterns;

public sealed class InitialShiftPatternCatalogTests
{
    [Fact]
    public void AllWhenReadContainsConfirmedPatternsWithStableIdentifiers()
    {
        Assert.Collection(
            InitialShiftPatternCatalog.All,
            splitShift =>
            {
                SplitShiftPattern pattern = Assert.IsType<SplitShiftPattern>(splitShift);
                Assert.Equal(
                    new Guid("06fc39e4-1b95-4416-a342-7641d3553fb9"),
                    pattern.Id.Value);
                Assert.Equal("D", pattern.DisplayCode);
            },
            reliefShift =>
            {
                ReliefShiftPattern pattern = Assert.IsType<ReliefShiftPattern>(reliefShift);
                Assert.Equal(
                    new Guid("aa8759ef-2043-4b96-985c-d91d2a20e8a4"),
                    pattern.Id.Value);
                Assert.Equal("Spr", pattern.DisplayCode);
            });
    }
}
