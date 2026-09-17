using Salztal.Dienstplanung.Application.Scheduling;

namespace Salztal.Dienstplanung.Application.Tests.Scheduling;

public sealed class AutomaticScheduleTechnicalFailureDetailsTests
{
    [Fact]
    public void StableTechnicalRuleCodesAreRetained()
    {
        TechnicalContextException exception = new(
            "MAXIMUM_CONSECUTIVE_WORKDAYS,AUXILIARY_WEEKLY_MAXIMUM");

        AutomaticScheduleTechnicalFailureDetails details =
            AutomaticScheduleTechnicalFailureDetails.FromException(
                AutomaticScheduleTechnicalStage.Solving,
                exception);

        Assert.Equal(exception.TechnicalContext, details.TechnicalContext);
    }

    [Theory]
    [InlineData("Mitarbeiter Max Beispiel")]
    [InlineData("C:\\Users\\Name\\dienstplanung.db")]
    public void FreeTextAndLocalPathsAreDiscarded(string technicalContext)
    {
        AutomaticScheduleTechnicalFailureDetails details =
            AutomaticScheduleTechnicalFailureDetails.FromException(
                AutomaticScheduleTechnicalStage.Solving,
                new TechnicalContextException(technicalContext));

        Assert.Null(details.TechnicalContext);
    }

    private sealed class TechnicalContextException(string technicalContext)
        : Exception, IAutomaticScheduleTechnicalContextProvider
    {
        public string TechnicalContext { get; } = technicalContext;
    }
}
