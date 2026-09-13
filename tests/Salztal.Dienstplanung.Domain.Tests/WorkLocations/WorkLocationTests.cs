using Salztal.Dienstplanung.Domain.WorkLocations;

namespace Salztal.Dienstplanung.Domain.Tests.WorkLocations;

public sealed class WorkLocationTests
{
    [Fact]
    public void CreateWhenValuesAreValidReturnsNormalizedWorkLocation()
    {
        Guid id = new("69f267cd-5d1a-4b0a-9d32-710799064026");

        WorkLocationValidationResult result = WorkLocation.Create(
            id,
            "  Terrasse  ",
            "  green  ");

        WorkLocation workLocation = Assert.IsType<WorkLocation>(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(id, workLocation.Id.Value);
        Assert.Equal("Terrasse", workLocation.Name.Value);
        Assert.Equal("green", workLocation.Color.Code);
    }

    [Fact]
    public void CreateWhenAllValuesAreInvalidReturnsAllValidationCodes()
    {
        WorkLocationValidationResult result = WorkLocation.Create(
            Guid.Empty,
            " ",
            null);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(
                WorkLocationValidationCode.IdentifierRequired,
                error.Code),
            error => Assert.Equal(
                WorkLocationValidationCode.NameRequired,
                error.Code),
            error => Assert.Equal(
                WorkLocationValidationCode.ColorRequired,
                error.Code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenNameIsMissingReturnsNameRequired(string? name)
    {
        WorkLocationValidationResult result = WorkLocation.Create(
            Guid.NewGuid(),
            name,
            "green");

        WorkLocationValidationError error = Assert.Single(result.Errors);
        Assert.Equal(WorkLocationValidationCode.NameRequired, error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWhenColorIsMissingReturnsColorRequired(string? colorCode)
    {
        WorkLocationValidationResult result = WorkLocation.Create(
            Guid.NewGuid(),
            "Terrasse",
            colorCode);

        WorkLocationValidationError error = Assert.Single(result.Errors);
        Assert.Equal(WorkLocationValidationCode.ColorRequired, error.Code);
    }

    [Fact]
    public void WithDetailsWhenValuesAreValidPreservesIdentifier()
    {
        WorkLocation original = Assert.IsType<WorkLocation>(
            WorkLocation.Create(
                new Guid("a27d3999-cf0f-41f2-a14f-69b9dcf478f2"),
                "Terrasse",
                "green").Value);

        WorkLocationValidationResult result = original.WithDetails(
            "Wintergarten",
            "orange");

        WorkLocation changed = Assert.IsType<WorkLocation>(result.Value);
        Assert.Equal(original.Id, changed.Id);
        Assert.Equal("Wintergarten", changed.Name.Value);
        Assert.Equal("orange", changed.Color.Code);
        Assert.Equal("Terrasse", original.Name.Value);
        Assert.Equal("green", original.Color.Code);
    }

    [Fact]
    public void WithDetailsWhenNameIsMissingReturnsErrorAndLeavesOriginalUnchanged()
    {
        WorkLocation original = Assert.IsType<WorkLocation>(
            WorkLocation.Create(
                new Guid("7e2d9fc8-841a-48b0-8eae-15eebf23d08b"),
                "Terrasse",
                "green").Value);

        WorkLocationValidationResult result = original.WithDetails(" ", "orange");

        Assert.False(result.IsSuccess);
        WorkLocationValidationError error = Assert.Single(result.Errors);
        Assert.Equal(WorkLocationValidationCode.NameRequired, error.Code);
        Assert.Equal("Terrasse", original.Name.Value);
        Assert.Equal("green", original.Color.Code);
    }

    [Fact]
    public void WorkLocationIdWhenValueIsEmptyCannotBeCreated()
    {
        bool wasCreated = WorkLocationId.TryCreate(Guid.Empty, out WorkLocationId? id);

        Assert.False(wasCreated);
        Assert.Null(id);
    }
}
