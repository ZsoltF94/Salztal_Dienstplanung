using System.Collections.ObjectModel;
using Salztal.Dienstplanung.Domain.Rules;

namespace Salztal.Dienstplanung.Planning.Rules;

internal sealed class InitialRuleTranslationRegistry
{
    private static readonly ReadOnlyCollection<RuleTranslationDescriptor>
        DefaultDescriptors = Array.AsReadOnly(
            new[]
            {
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.KnownReferences),
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.SingleDailyAssignment),
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.NoTimeOverlap),
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.BlockedDayMarker),
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.NormalSlotFullCoverage),
                RuleTranslationDescriptor.For(
                    InitialStructureRuleDefinitions.ApprovedPlanImmutable),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ActiveEmployeesOnly),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ShiftEligibilityRequired),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ExplicitRunOptionRequired),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ServiceManagementManualOnly),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ServiceManagementWeeklyPrerequisite),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.NormalWeeklyMaximum),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.AuxiliaryWeeklyMaximum),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.MaximumConsecutiveWorkdays),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.PostVacationWeekendFree),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.AutomaticNoOverstaffing),
                RuleTranslationDescriptor.For(
                    InitialAutomaticHardRuleDefinitions.ReliefShiftEmergencyOnly),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.NormalWeeklyMinimum),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.WeeklyConsecutiveDaysOff),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.GuaranteedDayOffAdjacentDayOff),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.PreVacationWeekendFree),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.SplitShiftWeeklyMaximum),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.ThreeWeekFreeWeekend),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.MinimizeReliefShifts),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.MinimizeSplitShifts),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.AuxiliaryWeeklyMinimum),
                RuleTranslationDescriptor.For(
                    CurrentSoftRuleDefinitions.RelativeWeeklyTarget),
                RuleTranslationDescriptor.For(
                    InitialNoticeRuleDefinitions.AuxiliaryWeeklyLow),
                RuleTranslationDescriptor.For(
                    InitialNoticeRuleDefinitions.AuxiliaryWeeklyHigh),
                RuleTranslationDescriptor.For(
                    InitialStabilityRuleDefinitions.CurrentPeriodFairDistribution),
            });

    private readonly ReadOnlyCollection<RuleTranslationDescriptor> descriptors;

    public InitialRuleTranslationRegistry()
        : this(DefaultDescriptors)
    {
    }

    internal InitialRuleTranslationRegistry(
        IEnumerable<RuleTranslationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        this.descriptors = Array.AsReadOnly(descriptors.ToArray());
    }

    public IReadOnlyList<RuleTranslationDescriptor> Descriptors => descriptors;
}
