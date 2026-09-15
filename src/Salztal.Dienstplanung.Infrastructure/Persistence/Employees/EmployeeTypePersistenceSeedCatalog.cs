using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Infrastructure.Persistence.Employees;

internal static class EmployeeTypePersistenceSeedCatalog
{
    public static IReadOnlyList<EmployeeType> All { get; } = Array.AsReadOnly(
        new[]
        {
            // Preserve the published eligibility identifiers for the original eight types.
            InitialEmployeeTypeCatalog.Type1,
            InitialEmployeeTypeCatalog.Type25,
            InitialEmployeeTypeCatalog.Type30,
            InitialEmployeeTypeCatalog.Type30a,
            InitialEmployeeTypeCatalog.Type35,
            InitialEmployeeTypeCatalog.Type35a,
            InitialEmployeeTypeCatalog.TypeAh1,
            InitialEmployeeTypeCatalog.TypeAh2,
            InitialEmployeeTypeCatalog.Type20,
            InitialEmployeeTypeCatalog.Type20a,
            InitialEmployeeTypeCatalog.Type25a,
        });
}
