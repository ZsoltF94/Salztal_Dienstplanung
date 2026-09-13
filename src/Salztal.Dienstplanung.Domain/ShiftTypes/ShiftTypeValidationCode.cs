namespace Salztal.Dienstplanung.Domain.ShiftTypes;

public enum ShiftTypeValidationCode
{
    IdentifierRequired,
    NameRequired,
    WorkLocationRequired,
    UnsupportedDisplayKind,
    AbbreviationRequired,
    AbbreviationNotAllowedForActualTime,
    StandardTimeRequired,
}
