using System;
using System.Collections.Generic;
using System.Text;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class SpatialElementInteractionEffect
    {
        // Needed for EF to track items in a JSON array reliably (OwnsMany needs a key)
        public Guid EffectId { get; set; } = Guid.NewGuid();

        // Who this effect is applied to (higher/lower, host/tool, etc.)
        public SpatialElementEffectTargetEnum Target { get; set; }

        // What to do
        public SpatialElementInteractionActionEnum Action { get; set; }

        /// <summary>
        /// Persisted action-specific parameters (JSON object).
        /// Null => assume defaults for the specified Action.
        /// </summary>
        public SpatialElementInteractionEffectArgs? Args { get; set; }

        /// <summary>
        /// Convenience: returns args with defaults applied (as the interface).
        /// This is not intended to be persisted.
        /// </summary>
        public IInteractionEffectArgs GetEffectiveArgs(InteractionDefaultsProfile profile)
            => InteractionDefaults.ResolveArgs(Action, Args, profile);
    }

    /// <summary>
    /// Engine-facing interface (kept for cleaner code / UI metadata hooks).
    /// Persisted type implements this interface.
    /// </summary>
    public interface IInteractionEffectArgs
    {
        decimal? ClearanceM { get; set; }
        decimal? OffsetM { get; set; }
        BoundaryReferenceEnum? Boundary { get; set; }
        bool? ThroughAll { get; set; }
        string? Notes { get; set; }

        IEnumerable<string> Validate(SpatialElementInteractionActionEnum action);

        IReadOnlyList<ArgFieldDescriptor> DescribeFields(SpatialElementInteractionActionEnum action);
    }

    public enum BoundaryReferenceEnum
    {
        Default = 0,
        OuterFace = 1,
        InnerFace = 2,
        Centerline = 3,
        HostBoundary = 4
    }

    public sealed record ArgFieldDescriptor(
        string Name,
        string Label,
        ArgFieldType Type,
        bool IsRequired,
        string? HelpText = null,
        decimal? Min = null,
        decimal? Max = null
    );

    public enum ArgFieldType
    {
        Decimal,
        Boolean,
        String,
        Enum
    }

    /// <summary>
    /// Single stable persisted args shape (JSON).
    /// UI shows only relevant fields based on Action.
    /// </summary>
    public sealed class SpatialElementInteractionEffectArgs : IInteractionEffectArgs
    {
        // Common knobs
        public decimal? ClearanceM { get; set; }
        public decimal? OffsetM { get; set; }
        public BoundaryReferenceEnum? Boundary { get; set; }
        public bool? ThroughAll { get; set; }
        public string? Notes { get; set; }

        // Terminate / Trim
        public bool? KeepInside { get; set; }

        // AddOpening / SubtractVolume
        public decimal? OpeningOversizeM { get; set; }

        // StepOver
        public decimal? StepDeltaM { get; set; }
        public decimal? MinCoverM { get; set; }

        public SpatialElementInteractionEffectArgs() { }
        public SpatialElementInteractionEffectArgs(SpatialElementInteractionEffectArgs other)
        {
            if (other == null) return;

            ClearanceM = other.ClearanceM;
            OffsetM = other.OffsetM;
            Boundary = other.Boundary;
            ThroughAll = other.ThroughAll;
            Notes = other.Notes;

            KeepInside = other.KeepInside;
            OpeningOversizeM = other.OpeningOversizeM;

            StepDeltaM = other.StepDeltaM;
            MinCoverM = other.MinCoverM;
        }

        public IEnumerable<string> Validate(SpatialElementInteractionActionEnum action)
        {
            if (ClearanceM is < 0) yield return "ClearanceM must be >= 0.";
            if (OffsetM is < 0) yield return "OffsetM must be >= 0.";

            if (OpeningOversizeM is < 0) yield return "OpeningOversizeM must be >= 0.";
            if (MinCoverM is < 0) yield return "MinCoverM must be >= 0.";

            if (action == SpatialElementInteractionActionEnum.StepOver && StepDeltaM is 0)
                yield return "StepDeltaM cannot be 0 for StepOver.";

            // You can add action-specific required checks here if you want.
        }

        public IReadOnlyList<ArgFieldDescriptor> DescribeFields(SpatialElementInteractionActionEnum action)
        {
            // If you prefer, move this to a separate service so your model stays "data only".
            return action switch
            {
                SpatialElementInteractionActionEnum.Terminate => new[]
                {
                    new ArgFieldDescriptor(nameof(Boundary), "Terminate at", ArgFieldType.Enum, false, "Inner face / outer face / etc."),
                    new ArgFieldDescriptor(nameof(OffsetM), "Terminate offset (m)", ArgFieldType.Decimal, false, "Stop short/long by this amount."),
                    new ArgFieldDescriptor(nameof(KeepInside), "Keep inside", ArgFieldType.Boolean, false, "Keep only inside portion after trimming."),
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(Notes), "Notes", ArgFieldType.String, false),
                },

                SpatialElementInteractionActionEnum.TrimToBoundary => new[]
                {
                    new ArgFieldDescriptor(nameof(Boundary), "Trim to", ArgFieldType.Enum, false),
                    new ArgFieldDescriptor(nameof(OffsetM), "Offset (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(Notes), "Notes", ArgFieldType.String, false),
                },

                SpatialElementInteractionActionEnum.AddOpening => new[]
                {
                    new ArgFieldDescriptor(nameof(OpeningOversizeM), "Opening oversize (m)", ArgFieldType.Decimal, false, "Oversize applied to opening profile."),
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance (m)", ArgFieldType.Decimal, false, "Alternative oversize/clearance."),
                    new ArgFieldDescriptor(nameof(ThroughAll), "Through all", ArgFieldType.Boolean, false, "Penetrate full thickness."),
                    new ArgFieldDescriptor(nameof(Notes), "Notes", ArgFieldType.String, false),
                },

                SpatialElementInteractionActionEnum.StepOver => new[]
                {
                    new ArgFieldDescriptor(nameof(StepDeltaM), "Step (m)", ArgFieldType.Decimal, true, "Raise/lower amount."),
                    new ArgFieldDescriptor(nameof(MinCoverM), "Min cover (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(Notes), "Notes", ArgFieldType.String, false),
                },

                _ => new[]
                {
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(OffsetM), "Offset (m)", ArgFieldType.Decimal, false),
                    new ArgFieldDescriptor(nameof(Boundary), "Boundary", ArgFieldType.Enum, false),
                    new ArgFieldDescriptor(nameof(ThroughAll), "Through all", ArgFieldType.Boolean, false),
                    new ArgFieldDescriptor(nameof(Notes), "Notes", ArgFieldType.String, false),
                }
            };
        }
    }

    public static class InteractionDefaults
    {
        /// <summary>
        /// Returns a concrete args object with defaults applied.
        /// </summary>
        public static SpatialElementInteractionEffectArgs ResolveArgs(
            SpatialElementInteractionActionEnum action,
            SpatialElementInteractionEffectArgs? argsFromJson,
            InteractionDefaultsProfile profile)
        {
            // CLONE so we never mutate persisted JSON state
            var a = argsFromJson is null
                ? new SpatialElementInteractionEffectArgs()
                : new SpatialElementInteractionEffectArgs(argsFromJson);

            switch (action)
            {
                case SpatialElementInteractionActionEnum.Terminate:
                    a.Boundary ??= profile.DefaultTerminateBoundary;
                    a.OffsetM ??= 0m;
                    a.ClearanceM ??= 0m;
                    break;

                case SpatialElementInteractionActionEnum.TrimToBoundary:
                    a.Boundary ??= profile.DefaultTrimBoundary;
                    a.OffsetM ??= 0m;
                    a.ClearanceM ??= 0m;
                    break;

                case SpatialElementInteractionActionEnum.AddOpening:
                    a.ClearanceM ??= profile.DefaultOpeningClearanceM;
                    a.ThroughAll ??= true;
                    break;
            }

            return a;
        }
    }

    public sealed record InteractionDefaultsProfile(
        BoundaryReferenceEnum DefaultTerminateBoundary,
        BoundaryReferenceEnum DefaultTrimBoundary,
        decimal DefaultOpeningClearanceM
    );
}
