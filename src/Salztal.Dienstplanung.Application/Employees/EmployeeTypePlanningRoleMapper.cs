using Salztal.Dienstplanung.Domain.Employees;

namespace Salztal.Dienstplanung.Application.Employees;

internal static class EmployeeTypePlanningRoleMapper
{
    public static EmployeeTypePlanningRoleKind ToSnapshotKind(
        EmployeeTypePlanningRole role)
    {
        return role switch
        {
            EmployeeTypePlanningRole.Normal => EmployeeTypePlanningRoleKind.Normal,
            EmployeeTypePlanningRole.ServiceManagement =>
                EmployeeTypePlanningRoleKind.ServiceManagement,
            EmployeeTypePlanningRole.Auxiliary => EmployeeTypePlanningRoleKind.Auxiliary,
            _ => throw new InvalidOperationException(
                $"Unsupported employee-type planning role: {role}"),
        };
    }
}
