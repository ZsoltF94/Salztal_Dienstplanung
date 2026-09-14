using System.Collections.ObjectModel;

namespace Salztal.Dienstplanung.Domain.StaffingDemands;

public sealed class StandardStaffingDemandRevisionSet
{
    private StandardStaffingDemandRevisionSet(
        ReadOnlyCollection<StandardStaffingDemandRevision> revisions)
    {
        Revisions = revisions;
    }

    public IReadOnlyList<StandardStaffingDemandRevision> Revisions { get; }

    public static StandardStaffingDemandRevisionSetValidationResult Create(
        IEnumerable<StandardStaffingDemandRevision> revisions)
    {
        ArgumentNullException.ThrowIfNull(revisions);

        StandardStaffingDemandRevision[] snapshot = revisions.ToArray();
        List<StandardStaffingDemandRevisionSetValidationError> errors = [];

        foreach (IGrouping<(
                     StandardStaffingDemandKey Key,
                     DateOnly EffectiveFromMonday,
                     int CorrectionSequence),
                     StandardStaffingDemandRevision> duplicateGroup in snapshot
                     .GroupBy(revision => (
                         revision.Key,
                         revision.EffectiveFromMonday,
                         revision.CorrectionSequence))
                     .Where(group => group.Count() > 1))
        {
            errors.Add(new StandardStaffingDemandRevisionSetValidationError(
                StandardStaffingDemandRevisionSetValidationCode
                    .DuplicateKeyEffectiveMondayAndCorrectionSequence,
                duplicateGroup.Key.Key,
                duplicateGroup.Key.EffectiveFromMonday));
        }

        foreach (IGrouping<(
                     StandardStaffingDemandKey Key,
                     DateOnly EffectiveFromMonday),
                     StandardStaffingDemandRevision> mondayHistory in snapshot
                     .GroupBy(revision => (revision.Key, revision.EffectiveFromMonday)))
        {
            int[] actualSequences = mondayHistory
                .Select(revision => revision.CorrectionSequence)
                .Distinct()
                .Order()
                .ToArray();
            int[] expectedSequences = Enumerable.Range(1, actualSequences.Length).ToArray();
            if (!actualSequences.SequenceEqual(expectedSequences))
            {
                errors.Add(new StandardStaffingDemandRevisionSetValidationError(
                    StandardStaffingDemandRevisionSetValidationCode
                        .NonContiguousCorrectionSequence,
                    mondayHistory.Key.Key,
                    mondayHistory.Key.EffectiveFromMonday));
            }
        }

        HashSet<StandardStaffingDemandKey> duplicateKeys = errors
            .Select(error => error.Key)
            .ToHashSet();

        foreach (IGrouping<StandardStaffingDemandKey, StandardStaffingDemandRevision> history
                 in snapshot.GroupBy(revision => revision.Key))
        {
            if (duplicateKeys.Contains(history.Key))
            {
                continue;
            }

            ValidateTransitions(history, errors);
        }

        if (errors.Count > 0)
        {
            return StandardStaffingDemandRevisionSetValidationResult.Failure(errors);
        }

        StandardStaffingDemandRevision[] ordered = snapshot
            .OrderBy(revision => revision.EffectiveFromMonday)
            .ThenBy(revision => GetMondayFirstDayOrder(revision.Key.DayOfWeek))
            .ThenBy(revision => revision.Key.WorkLocationId.Value)
            .ThenBy(revision => revision.Key.ShiftTypeId.Value)
            .ThenBy(revision => revision.CorrectionSequence)
            .ToArray();

        return StandardStaffingDemandRevisionSetValidationResult.Success(
            new StandardStaffingDemandRevisionSet(Array.AsReadOnly(ordered)));
    }

    public StandardStaffingDemandRevision? FindEffectiveRevision(
        StandardStaffingDemandKey key,
        DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(key);

        StandardStaffingDemandRevision? revision = Revisions
            .Where(candidate => candidate.Key == key)
            .Where(candidate => candidate.EffectiveFromMonday <= date)
            .OrderBy(candidate => candidate.EffectiveFromMonday)
            .ThenBy(candidate => candidate.CorrectionSequence)
            .LastOrDefault();

        return revision?.Kind == StandardStaffingDemandRevisionKind.Remove
            ? null
            : revision;
    }

    private static void ValidateTransitions(
        IEnumerable<StandardStaffingDemandRevision> history,
        List<StandardStaffingDemandRevisionSetValidationError> errors)
    {
        bool hasEffectiveStandard = false;

        foreach (StandardStaffingDemandRevision revision in history
                     .OrderBy(candidate => candidate.EffectiveFromMonday)
                     .ThenBy(candidate => candidate.CorrectionSequence))
        {
            StandardStaffingDemandRevisionSetValidationCode? errorCode = revision.Kind switch
            {
                StandardStaffingDemandRevisionKind.Add when hasEffectiveStandard =>
                    StandardStaffingDemandRevisionSetValidationCode
                        .AdditionRequiresMissingStandard,
                StandardStaffingDemandRevisionKind.Replace when !hasEffectiveStandard =>
                    StandardStaffingDemandRevisionSetValidationCode
                        .ReplacementRequiresExistingStandard,
                StandardStaffingDemandRevisionKind.Remove when !hasEffectiveStandard =>
                    StandardStaffingDemandRevisionSetValidationCode
                        .RemovalRequiresExistingStandard,
                _ => null,
            };

            if (errorCode is not null)
            {
                errors.Add(new StandardStaffingDemandRevisionSetValidationError(
                    errorCode.Value,
                    revision.Key,
                    revision.EffectiveFromMonday));
                continue;
            }

            hasEffectiveStandard = revision.Kind != StandardStaffingDemandRevisionKind.Remove;
        }
    }

    private static int GetMondayFirstDayOrder(DayOfWeek dayOfWeek)
    {
        return ((int)dayOfWeek + 6) % 7;
    }
}
