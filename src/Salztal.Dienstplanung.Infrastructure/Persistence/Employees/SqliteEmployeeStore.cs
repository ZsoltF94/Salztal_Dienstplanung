using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Salztal.Dienstplanung.Application.Employees;
using Salztal.Dienstplanung.Application.ServiceCatalog;
using Salztal.Dienstplanung.Domain.Employees;
using Salztal.Dienstplanung.Domain.ShiftPatterns;
using Salztal.Dienstplanung.Domain.ShiftTypes;
using Salztal.Dienstplanung.Infrastructure.Persistence.ServiceCatalog;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

public sealed class SqliteEmployeeStore :
    IEmployeeReader,
    ICreateEmployeeStore,
    IUpdateEmployeeNameStore,
    IChangeEmployeeTypeStore,
    IDeactivateEmployeeStore,
    IReactivateEmployeeStore,
    IDeleteEmployeeStore,
    ICreateEmployeeTypeStore,
    IUpdateEmployeeTypeStore,
    IDeleteEmployeeTypeStore
{
    private readonly ServiceCatalogDbContextFactory _contextFactory;

    public SqliteEmployeeStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _contextFactory = new ServiceCatalogDbContextFactory(Path.GetFullPath(databasePath));
    }

    public async Task<EmployeeReadData> LoadAsync(CancellationToken cancellationToken)
    {
        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<WorkLocationEntity> workLocations = await context.WorkLocations
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftTypeEntity> shiftTypes = await context.ShiftTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<ShiftPatternEntity> shiftPatterns = await context.ShiftPatterns
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        List<EmployeeTypeEntity> employeeTypes = await context.EmployeeTypes
            .AsNoTracking()
            .OrderBy(employeeType => employeeType.Code)
            .ToListAsync(cancellationToken);
        List<EmployeeTypeShiftEligibilityEntity> shiftEligibilities =
            await context.EmployeeTypeShiftEligibilities
                .AsNoTracking()
                .OrderBy(eligibility => eligibility.Id)
                .ToListAsync(cancellationToken);
        List<EmployeeEntity> employees = await context.Employees
            .AsNoTracking()
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ThenBy(employee => employee.Id)
            .ToListAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        ServiceCatalogData serviceCatalog = SqliteServiceCatalogStore.CreateData(
            workLocations,
            shiftTypes,
            shiftPatterns);
        EmployeeType[] mappedEmployeeTypes = MapEmployeeTypes(
            employeeTypes,
            shiftEligibilities,
            serviceCatalog);
        Employee[] mappedEmployees = employees.Select(MapEmployee).ToArray();

        return new EmployeeReadData(mappedEmployees, mappedEmployeeTypes, serviceCatalog);
    }

    async Task<EmployeeWriteStoreResult> ICreateEmployeeStore.CreateAsync(
        Employee employee,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentNullException.ThrowIfNull(type1Id);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);
        context.Employees.Add(CreateEntity(employee));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return EmployeeWriteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            return IsActiveType1(employee, type1Id)
                ? EmployeeWriteStoreResult.ActiveType1Conflict
                : EmployeeWriteStoreResult.Conflict;
        }
    }

    async Task<EmployeeWriteStoreResult> IUpdateEmployeeNameStore.UpdateNameAsync(
        Employee expected,
        Employee replacement,
        CancellationToken cancellationToken)
    {
        ValidateNameReplacement(expected, replacement);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        int affectedRows = await MatchingEmployee(context, expected)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(employee => employee.FirstName, replacement.FirstName.Value)
                    .SetProperty(employee => employee.LastName, replacement.LastName.Value),
                cancellationToken);

        return ToWriteResult(affectedRows);
    }

    async Task<EmployeeWriteStoreResult> IChangeEmployeeTypeStore.ChangeTypeAsync(
        Employee expected,
        Employee replacement,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken)
    {
        ValidateTypeReplacement(expected, replacement);
        ArgumentNullException.ThrowIfNull(type1Id);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            int affectedRows = await MatchingEmployee(context, expected)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        employee => employee.EmployeeTypeId,
                        replacement.EmployeeTypeId.Value),
                    cancellationToken);

            if (affectedRows != 1)
            {
                return EmployeeWriteStoreResult.Conflict;
            }

            await transaction.CommitAsync(cancellationToken);
            return EmployeeWriteStoreResult.Succeeded;
        }
        catch (SqliteException exception) when (IsConstraintViolation(exception))
        {
            return IsActiveType1(replacement, type1Id)
                ? EmployeeWriteStoreResult.ActiveType1Conflict
                : EmployeeWriteStoreResult.Conflict;
        }
    }

    async Task<EmployeeWriteStoreResult> IDeactivateEmployeeStore.DeactivateAsync(
        Employee expected,
        Employee replacement,
        CancellationToken cancellationToken)
    {
        ValidateDeactivation(expected, replacement);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        int affectedRows = await MatchingEmployee(context, expected)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    employee => employee.IsActive,
                    replacement.IsActive),
                cancellationToken);

        return ToWriteResult(affectedRows);
    }

    async Task<EmployeeWriteStoreResult> IReactivateEmployeeStore.ReactivateAsync(
        Employee expected,
        Employee replacement,
        EmployeeTypeId type1Id,
        CancellationToken cancellationToken)
    {
        ValidateReactivation(expected, replacement);
        ArgumentNullException.ThrowIfNull(type1Id);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            int affectedRows = await MatchingEmployee(context, expected)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        employee => employee.IsActive,
                        replacement.IsActive),
                    cancellationToken);

            if (affectedRows != 1)
            {
                return EmployeeWriteStoreResult.Conflict;
            }

            await transaction.CommitAsync(cancellationToken);
            return EmployeeWriteStoreResult.Succeeded;
        }
        catch (SqliteException exception) when (IsConstraintViolation(exception))
        {
            return IsActiveType1(replacement, type1Id)
                ? EmployeeWriteStoreResult.ActiveType1Conflict
                : EmployeeWriteStoreResult.Conflict;
        }
    }

    async Task<EmployeeDeleteStoreResult> IDeleteEmployeeStore.DeleteAsync(
        Employee expected,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (expected.IsActive)
        {
            throw new ArgumentException(
                "Only an inactive employee can be deleted.",
                nameof(expected));
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            int affectedRows = await MatchingEmployee(context, expected)
                .ExecuteDeleteAsync(cancellationToken);
            if (affectedRows != 1)
            {
                return EmployeeDeleteStoreResult.Conflict;
            }

            await transaction.CommitAsync(cancellationToken);
            return EmployeeDeleteStoreResult.Succeeded;
        }
        catch (SqliteException exception) when (IsForeignKeyConstraintViolation(exception))
        {
            return EmployeeDeleteStoreResult.Referenced;
        }
    }

    async Task<EmployeeTypeWriteStoreResult> ICreateEmployeeTypeStore.CreateAsync(
        EmployeeType employeeType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(employeeType);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);
        context.EmployeeTypes.Add(CreateEntity(employeeType));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return EmployeeTypeWriteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            return IsEmployeeTypeCodeConstraintViolation(exception)
                ? EmployeeTypeWriteStoreResult.DuplicateCode
                : EmployeeTypeWriteStoreResult.Conflict;
        }
    }

    async Task<EmployeeTypeWriteStoreResult> IUpdateEmployeeTypeStore.UpdateAsync(
        EmployeeType expected,
        EmployeeType replacement,
        CancellationToken cancellationToken)
    {
        ValidateEmployeeTypeReplacement(expected, replacement);

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);
        EmployeeTypeEntity? stored = await context.EmployeeTypes
            .Include(employeeType => employeeType.ShiftEligibilities)
            .SingleOrDefaultAsync(
                employeeType => employeeType.Id == expected.Id.Value,
                cancellationToken);
        if (stored is null || !MatchesEmployeeType(stored, expected))
        {
            return EmployeeTypeWriteStoreResult.Conflict;
        }

        ApplyDetails(stored, replacement);
        context.EmployeeTypeShiftEligibilities.RemoveRange(stored.ShiftEligibilities);
        stored.ShiftEligibilities.Clear();
        foreach (EmployeeTypeShiftEligibility eligibility in replacement.ShiftEligibilities)
        {
            stored.ShiftEligibilities.Add(CreateEntity(replacement.Id, eligibility));
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return EmployeeTypeWriteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            return EmployeeTypeWriteStoreResult.Conflict;
        }
    }

    async Task<EmployeeTypeDeleteStoreResult> IDeleteEmployeeTypeStore.DeleteAsync(
        EmployeeType expected,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (expected.PlanningPolicy.Role != EmployeeTypePlanningRole.Normal)
        {
            throw new ArgumentException(
                "Only an employee type with a normal planning role can be deleted.",
                nameof(expected));
        }

        await using ServiceCatalogDbContext context = _contextFactory.Create();
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);
        EmployeeTypeEntity? stored = await context.EmployeeTypes
            .Include(employeeType => employeeType.ShiftEligibilities)
            .SingleOrDefaultAsync(
                employeeType => employeeType.Id == expected.Id.Value,
                cancellationToken);
        if (stored is null || !MatchesEmployeeType(stored, expected))
        {
            return EmployeeTypeDeleteStoreResult.Conflict;
        }

        context.EmployeeTypes.Remove(stored);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return EmployeeTypeDeleteStoreResult.Succeeded;
        }
        catch (DbUpdateException exception) when (IsForeignKeyConstraintViolation(exception))
        {
            return EmployeeTypeDeleteStoreResult.Referenced;
        }
    }

    internal static EmployeeType[] MapEmployeeTypes(
        IEnumerable<EmployeeTypeEntity> employeeTypeEntities,
        IEnumerable<EmployeeTypeShiftEligibilityEntity> eligibilityEntities,
        ServiceCatalogData serviceCatalog)
    {
        IReadOnlyCollection<ShiftTypeId> knownShiftTypeIds = serviceCatalog.ShiftTypes
            .Select(shiftType => shiftType.Id)
            .ToArray();
        IReadOnlyCollection<ShiftPatternId> knownShiftPatternIds =
        [
            serviceCatalog.SplitShiftPattern.Id,
            serviceCatalog.ReliefShiftPattern.Id,
        ];
        ILookup<Guid, EmployeeTypeShiftEligibilityEntity> eligibilitiesByType =
            eligibilityEntities.ToLookup(eligibility => eligibility.EmployeeTypeId);

        return employeeTypeEntities
            .Select(entity => MapEmployeeType(
                entity,
                eligibilitiesByType[entity.Id],
                knownShiftTypeIds,
                knownShiftPatternIds))
            .ToArray();
    }

    private static EmployeeType MapEmployeeType(
        EmployeeTypeEntity entity,
        IEnumerable<EmployeeTypeShiftEligibilityEntity> eligibilityEntities,
        IReadOnlyCollection<ShiftTypeId> knownShiftTypeIds,
        IReadOnlyCollection<ShiftPatternId> knownShiftPatternIds)
    {
        EmployeeTypeShiftEligibility[] eligibilities = eligibilityEntities
            .Select(entity => MapEligibility(entity, knownShiftTypeIds, knownShiftPatternIds))
            .ToArray();
        EmployeeTypeValidationResult result = EmployeeType.Create(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.WeeklyWorkTargetMinutes,
            entity.AllowsVacationAndSickness,
            entity.AbsenceDayValueMinutes,
            eligibilities,
            MapPlanningPolicy(entity));

        return result.Value ?? throw InvalidStoredData("employee type", entity.Id);
    }

    private static EmployeeTypeShiftEligibility MapEligibility(
        EmployeeTypeShiftEligibilityEntity entity,
        IReadOnlyCollection<ShiftTypeId> knownShiftTypeIds,
        IReadOnlyCollection<ShiftPatternId> knownShiftPatternIds)
    {
        EmployeeTypeShiftEligibilityValidationResult result = entity.TargetKind switch
        {
            ShiftEligibilityTargetKind.ShiftType when entity.ShiftTypeId.HasValue =>
                EmployeeTypeShiftEligibility.CreateForShiftType(
                    entity.ShiftTypeId.Value,
                    entity.Mode,
                    knownShiftTypeIds),
            ShiftEligibilityTargetKind.ShiftPattern when entity.ShiftPatternId.HasValue =>
                EmployeeTypeShiftEligibility.CreateForShiftPattern(
                    entity.ShiftPatternId.Value,
                    entity.Mode,
                    entity.Activation,
                    knownShiftPatternIds),
            _ => throw InvalidStoredData("employee-type eligibility", entity.Id),
        };

        return result.Value ?? throw InvalidStoredData("employee-type eligibility", entity.Id);
    }

    private static EmployeeTypePlanningPolicy MapPlanningPolicy(EmployeeTypeEntity entity)
    {
        EmployeeTypePlanningPolicy policy = entity.PlanningRole switch
        {
            EmployeeTypePlanningRole.Normal => EmployeeTypePlanningPolicy.Standard,
            EmployeeTypePlanningRole.ServiceManagement =>
                EmployeeTypePlanningPolicy.ServiceManagement,
            EmployeeTypePlanningRole.Auxiliary => EmployeeTypePlanningPolicy.Auxiliary,
            _ => throw InvalidStoredData("employee-type planning role", entity.Id),
        };

        if (policy.AllowsAutomaticAssignment != entity.AllowsAutomaticAssignment
            || policy.RequiresWeeklyManualAssignment !=
                entity.RequiresWeeklyManualAssignment
            || policy.PreservesManualAssignmentsOnGeneration !=
                entity.PreservesManualAssignmentsOnGeneration
            || policy.ManualSuggestionPriority != entity.ManualSuggestionPriority)
        {
            throw InvalidStoredData("employee-type planning policy", entity.Id);
        }

        return policy;
    }

    internal static Employee MapEmployee(EmployeeEntity entity)
    {
        Employee employee = Employee.Create(
                entity.Id,
                entity.FirstName,
                entity.LastName,
                entity.EmployeeTypeId).Value
            ?? throw InvalidStoredData("employee", entity.Id);

        return entity.IsActive ? employee : employee.Deactivate();
    }

    private static EmployeeEntity CreateEntity(Employee employee)
    {
        return new EmployeeEntity
        {
            Id = employee.Id.Value,
            FirstName = employee.FirstName.Value,
            LastName = employee.LastName.Value,
            EmployeeTypeId = employee.EmployeeTypeId.Value,
            IsActive = employee.IsActive,
        };
    }

    private static EmployeeTypeEntity CreateEntity(EmployeeType employeeType)
    {
        EmployeeTypeEntity entity = new();
        ApplyDetails(entity, employeeType);
        entity.Id = employeeType.Id.Value;
        entity.Code = employeeType.Code.Value;

        foreach (EmployeeTypeShiftEligibility eligibility in employeeType.ShiftEligibilities)
        {
            entity.ShiftEligibilities.Add(CreateEntity(employeeType.Id, eligibility));
        }

        return entity;
    }

    private static EmployeeTypeShiftEligibilityEntity CreateEntity(
        EmployeeTypeId employeeTypeId,
        EmployeeTypeShiftEligibility eligibility)
    {
        return new EmployeeTypeShiftEligibilityEntity
        {
            EmployeeTypeId = employeeTypeId.Value,
            TargetKind = eligibility.TargetKind,
            ShiftTypeId = eligibility.ShiftTypeId?.Value,
            ShiftPatternId = eligibility.ShiftPatternId?.Value,
            Mode = eligibility.Mode,
            Activation = eligibility.Activation,
        };
    }

    private static void ApplyDetails(EmployeeTypeEntity entity, EmployeeType employeeType)
    {
        entity.Name = employeeType.Name.Value;
        entity.WeeklyWorkTargetMinutes = employeeType.WeeklyWorkTarget.Minutes;
        entity.AllowsVacationAndSickness =
            employeeType.AbsencePolicy.AllowsVacationAndSickness;
        entity.AbsenceDayValueMinutes = employeeType.AbsencePolicy.DayValue?.Minutes;
        entity.PlanningRole = employeeType.PlanningPolicy.Role;
        entity.AllowsAutomaticAssignment =
            employeeType.PlanningPolicy.AllowsAutomaticAssignment;
        entity.RequiresWeeklyManualAssignment =
            employeeType.PlanningPolicy.RequiresWeeklyManualAssignment;
        entity.PreservesManualAssignmentsOnGeneration =
            employeeType.PlanningPolicy.PreservesManualAssignmentsOnGeneration;
        entity.ManualSuggestionPriority =
            employeeType.PlanningPolicy.ManualSuggestionPriority;
    }

    private static bool MatchesEmployeeType(
        EmployeeTypeEntity entity,
        EmployeeType expected)
    {
        return entity.Id == expected.Id.Value
            && entity.Code == expected.Code.Value
            && entity.Name == expected.Name.Value
            && entity.WeeklyWorkTargetMinutes == expected.WeeklyWorkTarget.Minutes
            && entity.AllowsVacationAndSickness ==
                expected.AbsencePolicy.AllowsVacationAndSickness
            && entity.AbsenceDayValueMinutes == expected.AbsencePolicy.DayValue?.Minutes
            && entity.PlanningRole == expected.PlanningPolicy.Role
            && entity.AllowsAutomaticAssignment ==
                expected.PlanningPolicy.AllowsAutomaticAssignment
            && entity.RequiresWeeklyManualAssignment ==
                expected.PlanningPolicy.RequiresWeeklyManualAssignment
            && entity.PreservesManualAssignmentsOnGeneration ==
                expected.PlanningPolicy.PreservesManualAssignmentsOnGeneration
            && entity.ManualSuggestionPriority ==
                expected.PlanningPolicy.ManualSuggestionPriority
            && MatchesEligibilities(entity.ShiftEligibilities, expected.ShiftEligibilities);
    }

    private static bool MatchesEligibilities(
        List<EmployeeTypeShiftEligibilityEntity> stored,
        IReadOnlyCollection<EmployeeTypeShiftEligibility> expected)
    {
        return stored.Count == expected.Count
            && stored.All(entity => expected.Any(eligibility =>
                entity.TargetKind == eligibility.TargetKind
                && entity.ShiftTypeId == eligibility.ShiftTypeId?.Value
                && entity.ShiftPatternId == eligibility.ShiftPatternId?.Value
                && entity.Mode == eligibility.Mode
                && entity.Activation == eligibility.Activation));
    }

    private static IQueryable<EmployeeEntity> MatchingEmployee(
        ServiceCatalogDbContext context,
        Employee expected)
    {
        return context.Employees.Where(employee =>
            employee.Id == expected.Id.Value
            && employee.FirstName == expected.FirstName.Value
            && employee.LastName == expected.LastName.Value
            && employee.EmployeeTypeId == expected.EmployeeTypeId.Value
            && employee.IsActive == expected.IsActive);
    }

    private static void ValidateNameReplacement(Employee expected, Employee replacement)
    {
        ValidateEmployees(expected, replacement);

        if (expected.Id != replacement.Id
            || expected.EmployeeTypeId != replacement.EmployeeTypeId
            || expected.IsActive != replacement.IsActive)
        {
            throw new ArgumentException("Replacement may only change the employee name.");
        }
    }

    private static void ValidateTypeReplacement(Employee expected, Employee replacement)
    {
        ValidateEmployees(expected, replacement);

        if (expected.Id != replacement.Id
            || expected.FirstName != replacement.FirstName
            || expected.LastName != replacement.LastName
            || expected.IsActive != replacement.IsActive)
        {
            throw new ArgumentException("Replacement may only change the employee type.");
        }
    }

    private static void ValidateDeactivation(Employee expected, Employee replacement)
    {
        ValidateEmployees(expected, replacement);

        if (expected.Id != replacement.Id
            || expected.FirstName != replacement.FirstName
            || expected.LastName != replacement.LastName
            || expected.EmployeeTypeId != replacement.EmployeeTypeId
            || replacement.IsActive)
        {
            throw new ArgumentException("Replacement may only deactivate the employee.");
        }
    }

    private static void ValidateReactivation(Employee expected, Employee replacement)
    {
        ValidateEmployees(expected, replacement);

        if (expected.Id != replacement.Id
            || expected.FirstName != replacement.FirstName
            || expected.LastName != replacement.LastName
            || expected.EmployeeTypeId != replacement.EmployeeTypeId
            || expected.IsActive
            || !replacement.IsActive)
        {
            throw new ArgumentException("Replacement may only reactivate the employee.");
        }
    }

    private static void ValidateEmployees(Employee expected, Employee replacement)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(replacement);
    }

    private static void ValidateEmployeeTypeReplacement(
        EmployeeType expected,
        EmployeeType replacement)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(replacement);

        if (expected.Id != replacement.Id
            || expected.Code != replacement.Code
            || expected.PlanningPolicy != replacement.PlanningPolicy)
        {
            throw new ArgumentException(
                "Replacement may only change editable employee-type details.",
                nameof(replacement));
        }
    }

    private static bool IsActiveType1(Employee employee, EmployeeTypeId type1Id)
    {
        return employee.IsActive && employee.EmployeeTypeId == type1Id;
    }

    private static EmployeeWriteStoreResult ToWriteResult(int affectedRows)
    {
        return affectedRows == 1
            ? EmployeeWriteStoreResult.Succeeded
            : EmployeeWriteStoreResult.Conflict;
    }

    private static bool IsConstraintViolation(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqliteException
                && sqliteException.SqliteErrorCode == 19)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsForeignKeyConstraintViolation(SqliteException exception)
    {
        const int sqliteConstraintForeignKey = 787;
        return exception.SqliteExtendedErrorCode == sqliteConstraintForeignKey
            || (exception.SqliteErrorCode == 19
                && exception.Message.Contains(
                    "FOREIGN KEY constraint failed",
                    StringComparison.Ordinal));
    }

    private static bool IsForeignKeyConstraintViolation(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqliteException
                && IsForeignKeyConstraintViolation(sqliteException))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsEmployeeTypeCodeConstraintViolation(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqliteException
                && sqliteException.SqliteErrorCode == 19
                && sqliteException.Message.Contains(
                    "EmployeeTypes.Code",
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static InvalidDataException InvalidStoredData(string kind, object id)
    {
        return new InvalidDataException($"Stored {kind} '{id}' is invalid.");
    }
}
