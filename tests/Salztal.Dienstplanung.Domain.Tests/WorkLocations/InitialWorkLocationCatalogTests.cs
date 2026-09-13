using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.WorkLocations;

public sealed class InitialWorkLocationCatalogTests
{
    [Fact]
    public void AllWhenReadContainsConfirmedInitialLocations()
    {
        Assert.Collection(
            InitialWorkLocationCatalog.All,
            cafeteria =>
            {
                Assert.Equal("Cafeteria", cafeteria.Name.Value);
                Assert.Equal("yellow", cafeteria.Color.Code);
                Assert.Equal(
                    new Guid("6d38f48b-f938-4aaa-82bd-c59372f599f4"),
                    cafeteria.Id.Value);
            },
            restaurant =>
            {
                Assert.Equal("Restaurant", restaurant.Name.Value);
                Assert.Equal("red", restaurant.Color.Code);
                Assert.Equal(
                    new Guid("2d2bf3b2-4744-4b56-8515-f33adf9f2161"),
                    restaurant.Id.Value);
            });
    }

    [Fact]
    public void InitialLocationWhenChangedLeavesCatalogValueUnchanged()
    {
        WorkLocationValidationResult result = InitialWorkLocationCatalog.Cafeteria.WithDetails(
            "Cafeteria Neu",
            "amber");

        WorkLocation changed = Assert.IsType<WorkLocation>(result.Value);
        Assert.Equal("Cafeteria Neu", changed.Name.Value);
        Assert.Equal("amber", changed.Color.Code);
        Assert.Equal("Cafeteria", InitialWorkLocationCatalog.Cafeteria.Name.Value);
        Assert.Equal("yellow", InitialWorkLocationCatalog.Cafeteria.Color.Code);
    }

    [Fact]
    public void CreateWhenLocationIsAdditionalDoesNotRequireCatalogChange()
    {
        WorkLocationValidationResult result = WorkLocation.Create(
            new Guid("df27f23c-6b4b-409a-8b30-98f2dbd9b7ef"),
            "Wintergarten",
            "green");

        Assert.True(result.IsSuccess);
        Assert.Equal("Wintergarten", result.Value?.Name.Value);
        Assert.DoesNotContain(
            InitialWorkLocationCatalog.All,
            location => location.Id == result.Value?.Id);
    }
}
