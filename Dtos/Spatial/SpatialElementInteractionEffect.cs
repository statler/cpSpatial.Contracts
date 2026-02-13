using cpSpatial.Contract.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class SpatialElementInteractionEffect
    {
        // Needed for EF to track items in a JSON array reliably (OwnsMany needs a key)
        public Guid EffectId { get; set; } = Guid.NewGuid();

        public decimal OrderId { get; set; } = 0; // Order in which effects are applied (lower first). 

        // Who this effect is applied to (higher/lower, host/tool, etc.)
        public SpatialElementEffectTargetEnum Target { get; set; }

        // What to do
        public SpatialElementInteractionActionEnum Action { get; set; }

        /// <summary>
        /// Persisted action-specific parameters (JSON object).
        /// Null => assume defaults for the specified Action.
        /// </summary>
        public SpatialElementInteractionEffectArgs? Args { get; set; }


        public string ArgsString => Args == null ? "" : Args.ToDisplayString(Action);

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

    public sealed record ArgFieldDescriptor(
        string Name,
        string Label,
        ArgFieldTypeEnum Type,
        bool IsRequired,
        string? HelpText = null,
        decimal? Min = null,
        decimal? Max = null,
        UnitKind Unit = UnitKind.None
    );

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
                SpatialElementInteractionActionEnum.None => Array.Empty<ArgFieldDescriptor>(),

                SpatialElementInteractionActionEnum.Ignore => Array.Empty<ArgFieldDescriptor>(),

                SpatialElementInteractionActionEnum.Terminate => new[]
                {
                    new ArgFieldDescriptor(nameof(Boundary), "Terminate at", ArgFieldTypeEnum.Enum, false,
                        "Which boundary reference to terminate against (inner/outer/etc.)."),

                    new ArgFieldDescriptor(nameof(OffsetM), "Terminate offset", ArgFieldTypeEnum.Decimal, false,
                        "Stop short/long by this amount.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(KeepInside), "Keep inside", ArgFieldTypeEnum.Boolean, false,
                        "Keep only the inside portion after trimming."),

                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Optional clearance applied at the termination.", Unit: UnitKind.Length),
                },

                SpatialElementInteractionActionEnum.TrimToBoundary => new[]
                {
                    new ArgFieldDescriptor(nameof(Boundary), "Trim to", ArgFieldTypeEnum.Enum, false,
                        "Which boundary reference to trim against."),

                    new ArgFieldDescriptor(nameof(OffsetM), "Offset", ArgFieldTypeEnum.Decimal, false,
                        "Offset the trim boundary by this amount.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(KeepInside), "Keep inside", ArgFieldTypeEnum.Boolean, false,
                        "Keep only the inside portion after trimming."),

                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false, Unit: UnitKind.Length),
                },

                SpatialElementInteractionActionEnum.SubtractVolume => new[]
                {
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Optional clearance applied to the subtraction tool/profile.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(ThroughAll), "Through all", ArgFieldTypeEnum.Boolean, false,
                        "If applicable, subtract through the full thickness/depth."),
                },

                SpatialElementInteractionActionEnum.AddOpening => new[]
                {
                    new ArgFieldDescriptor(nameof(OpeningOversizeM), "Opening oversize", ArgFieldTypeEnum.Decimal, false,
                        "Oversize applied to the opening profile.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Optional alternative/additional clearance.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(ThroughAll), "Through all", ArgFieldTypeEnum.Boolean, false,
                        "Penetrate full thickness."),
                },

                SpatialElementInteractionActionEnum.OffsetAround => new[]
                {
                    new ArgFieldDescriptor(nameof(OffsetM), "Offset", ArgFieldTypeEnum.Decimal, false,
                        "Offset distance applied around the target/tool.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(Boundary), "Reference", ArgFieldTypeEnum.Enum, false,
                        "Optional reference used to interpret the offset."),

                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Optional clearance if distinguished from offset.", Unit: UnitKind.Length),
                },

                SpatialElementInteractionActionEnum.StepOver => new[]
                {
                    new ArgFieldDescriptor(nameof(StepDeltaM), "Step", ArgFieldTypeEnum.Decimal, true,
                        "Raise/lower amount.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(MinCoverM), "Min cover", ArgFieldTypeEnum.Decimal, false,
                        "Optional minimum cover to preserve.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false , Unit: UnitKind.Length),
                },

                SpatialElementInteractionActionEnum.Split => Array.Empty<ArgFieldDescriptor>(),

                SpatialElementInteractionActionEnum.Merge => Array.Empty<ArgFieldDescriptor>(),

                SpatialElementInteractionActionEnum.ClearanceOnly => new[]
                {
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Clearance distance used for clash/clearance evaluation only.", Unit: UnitKind.Length),
                },

                SpatialElementInteractionActionEnum.ClearanceEnvelope => new[]
                {
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false,
                        "Clearance distance used to form an envelope.", Unit: UnitKind.Length),

                    new ArgFieldDescriptor(nameof(Boundary), "Reference", ArgFieldTypeEnum.Enum, false,
                        "Optional reference used to interpret the envelope."),
                },

                _ => new[]
                {
                    new ArgFieldDescriptor(nameof(ClearanceM), "Clearance", ArgFieldTypeEnum.Decimal, false, Unit: UnitKind.Length),
                    new ArgFieldDescriptor(nameof(OffsetM), "Offset", ArgFieldTypeEnum.Decimal, false, Unit: UnitKind.Length),
                }
            };

        }

        public string ToDisplayString(
            SpatialElementInteractionActionEnum action,
            UnitPreferences? units = null,
            CultureInfo? culture = null)
        {
            units ??= new UnitPreferences();
            culture ??= CultureInfo.CurrentCulture;

            var fields = DescribeFields(action);
            if (fields.Count == 0) return "(defaults)";

            var parts = new List<string>();

            foreach (var f in fields)
            {
                var prop = GetType().GetProperty(f.Name, BindingFlags.Instance | BindingFlags.Public);
                if (prop is null || !prop.CanRead) continue;

                var val = prop.GetValue(this);
                if (val is null) continue;

                var formatted = FormatValue(f, val, units, culture);
                var label = GetDisplayLabel(f, units);
                parts.Add($"{label}: {formatted}");
            }

            return parts.Count == 0 ? "(defaults)" : string.Join(" | ", parts);
        }

        private static string GetDisplayLabel(
            ArgFieldDescriptor f,
            UnitPreferences units)
        {
            if (f.Unit != UnitKind.Length)
                return f.Label;

            string suffix = units.LengthUnit switch
            {
                LengthUnit.Millimetre => "mm",
                LengthUnit.Foot => "ft",
                _ => "m"
            };

            return $"{f.Label} ({suffix})";
        }

        private static string FormatValue(
            ArgFieldDescriptor f,
            object val,
            UnitPreferences units,
            CultureInfo culture)
        {
            return f.Type switch
            {
                ArgFieldTypeEnum.Decimal => FormatDecimal(f.Unit, val, units, culture),
                ArgFieldTypeEnum.Boolean => val is bool b ? (b ? "Yes" : "No") : val.ToString() ?? "",
                ArgFieldTypeEnum.Enum => val.ToString() ?? "",
                ArgFieldTypeEnum.String => val.ToString() ?? "",
                _ => val.ToString() ?? ""
            };
        }

        private static string FormatDecimal(
            UnitKind unitKind,
            object val,
            UnitPreferences units,
            CultureInfo culture)
        {
            var meters = val switch
            {
                decimal d => d,
                double d => (decimal)d,
                float f => (decimal)f,
                _ => Convert.ToDecimal(val, culture)
            };

            if (unitKind != UnitKind.Length)
            {
                return meters.ToString($"0.{new string('#', Math.Max(0, units.LengthDecimals))}", culture);
            }

            decimal displayValue;
            string suffix;

            switch (units.LengthUnit)
            {
                case LengthUnit.Millimetre:
                    displayValue = meters * 1000m;
                    suffix = "mm";
                    break;

                case LengthUnit.Foot:
                    displayValue = meters * 3.280839895013123m;
                    suffix = "ft";
                    break;

                case LengthUnit.Metre:
                default:
                    displayValue = meters;
                    suffix = "m";
                    break;
            }

            var fmt = $"0.{new string('#', Math.Max(0, units.LengthDecimals))}";
            var text = displayValue.ToString(fmt, culture);

            return units.IncludeUnitSuffix ? $"{text}{suffix}" : text;
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
