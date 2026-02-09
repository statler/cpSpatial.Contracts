using cpSpatial.Contract.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed partial class SpatialModelJobPayload
    {
        public int SchemaVersion { get; set; } = 1;
        public string ContractName { get; set; } = "SpatialModelJobPayload";
        public string SchemaHash { get; set; } = "";
        public Guid JobId { get; set; }

        public int ProjectId { get; set; }
        public Guid ProjectGuid { get; set; }
        public int SpatialModelId { get; set; }

        public required SpatialModelPayload Model { get; set; } = new();
        public required List<SpatialModelElementPayload> Elements { get; set; } = new();
        public required List<GeometryPayload> Geometries { get; set; } = new();
        public required List<CoordinateSystemPayload> CoordinateSystems { get; set; } = new();

        public required List<LotInfo> LstLotInfo { get; set; } = new();
        public required List<WorkTypeInfo> LstWorkTypes { get; set; } = new();

        public required List<SpatialElementPresetPayload> Presets { get; set; } = new();

        public required List<SpatialShapePayload> Shapes { get; set; } = new();
        public required List<SpatialModelStylePayload> Styles { get; set; } = new();
    }

    public sealed partial class SpatialModelPayload
    {
        public int CoordinateSystemId { get; set; }
        public int Srid { get; set; }

        public decimal? MeshResolutionInM { get; set; }
        public MeshResolutionStrategyEnum? MeshResolutionStrategy { get; set; }
        public decimal? MaxExtrapolationDistanceInM { get; set; }
        public ExtrapolationModeEnum? ExtrapolationMode { get; set; }

        public decimal? PointBufferRadiusM { get; set; }
        public decimal? PolyLineBufferRadiusM { get; set; }
    }

    public sealed partial class SpatialModelElementPayload
    {
        public int SpatialModelElementId { get; set; }

        public int LotId { get; set; }
        public int? GeometryId { get; set; }              // LotGeometryId
        public int? SpatialElementPresetId { get; set; }

        public decimal? HeightInM { get; set; }           // core
        public int? Rotation { get; set; }                // core

        public SpatialModelElementOverridePayload? ElementOverride { get; set; }
        public SpatialModelMeshOverridePayload? MeshOverride { get; set; }
    }

    public sealed partial class SpatialModelMeshOverridePayload
    {
        public int SpatialModelElementId { get; set; }

        public decimal? MeshResolutionInM { get; set; }
        public ExtrapolationModeEnum? ExtrapolationMode { get; set; }
        public decimal? MaxExtrapolationDistanceInM { get; set; }
        public MeshResolutionStrategyEnum? MeshResolutionStrategy { get; set; }
    }

    public sealed partial class SpatialElementPresetPayload
    {
        public int SpatialElementPresetId { get; set; }

        // Optional: helps diagnostics and stability if you ever remap IDs
        public Guid? UniqueId { get; set; }
        public string? PresetName { get; set; }

        // References (the spatial service must dereference these from the embedded collections)
        public int? SpatialModelStyleId { get; set; }
        public int? SpatialShapeId { get; set; }

        // Defaults for resolution
        public SurfaceModelTypeEnum SurfaceModelType { get; set; }      // SurfaceModelTypeEnum
        public ZSourceSettingEnum ZSurfaceSetting { get; set; }       // ZSourceSettingEnum

        public int TrimPriority { get; set; }          // default 0
        public SpatialElementInteractionEffect? InteractionEffects { get; set; } // JSON as JsonElement/JsonDocument/string (pick one)

        public AnchorModeEnum? AnchorMode { get; set; }
        public decimal AnchorXOffsetM { get; set; }
        public decimal AnchorYOffsetM { get; set; }
        public int Rotation { get; set; }

        // Surfaces (GUIDs only; spatial service resolves surface content)
        public Guid? BaseSurfaceId { get; set; }
        public Guid? TopSurfaceId { get; set; }
        public decimal? BaseOffsetInM { get; set; }
        public decimal? TopOffsetInM { get; set; }

        // Core default (note: element has HeightInM too; resolution chooses element core first if present)
        public decimal? HeightInM { get; set; }

        public SpatialElementPresetMeshOverridePayload? PresetMeshOverride { get; set; } = new();
    }

    public sealed partial class SpatialElementPresetMeshOverridePayload
    {
        public int SpatialElementPresetId { get; set; }

        public decimal? MeshResolutionInM { get; set; }
        public ExtrapolationModeEnum? ExtrapolationMode { get; set; }
        public decimal? MaxExtrapolationDistanceInM { get; set; }
        public MeshResolutionStrategyEnum? MeshResolutionStrategy { get; set; }
    }

    public sealed partial class SpatialShapePayload
    {
        public int SpatialShapeId { get; set; }
        public Guid? UniqueId { get; set; }
        public required string ShapeName { get; set; }

        public SpatialShapeTypeEnum ShapeType { get; set; }      // 1=Circle, 2=Rectangle, 3=Polygon
        public SpatialProfileKindEnum ProfileKind { get; set; }    // 1=Solid, 2=Hollow

        // Circle
        public decimal? OuterDiameterM { get; set; }
        public decimal? WallThicknessM { get; set; }

        // Rectangle
        public decimal? RectWidthM { get; set; }
        public decimal? RectHeightM { get; set; }

        // Polygon profile (outer ring only for now; per your earlier assumptions)
        public object? ProfileJson { get; set; } // JSON (same JSON type choice)

        // Anchor defaults relevant to extrusion placement rules
        public AnchorModeEnum AnchorMode { get; set; }      // your AnchorMode enum stored as int
    }

    public sealed partial class SpatialModelStylePayload
    {
        public int SpatialModelStyleId { get; set; }
        public Guid? UniqueId { get; set; }
        public required string StyleName { get; set; }

        // Surface styling
        public int? SurfaceColorArgb { get; set; }
        public decimal? SurfaceTransparency { get; set; } // 0..1 (where 1 = fully transparent)

        // Curve / linework styling
        public int? CurveColorArgb { get; set; }
        public decimal? CurveWidthM { get; set; }         // metres
        public byte? CurveLinePattern { get; set; }       // your enum for solid/dash/dot etc.
        public decimal? CurveDashLengthM { get; set; }
        public decimal? CurveGapLengthM { get; set; }
    }

    public sealed partial class CoordinateSystemPayload
    {
        public int CoordinateSystemId { get; set; }
        public int Srid { get; set; }
        public string Wkt { get; set; } = "";
    }

    public sealed partial class GeometryPayload
    {
        public int GeometryId { get; set; }          // LotGeometryId
        public int LotId { get; set; }
        public int CoordinateSystemId { get; set; }
        public Guid UniqueId { get; set; }

        // Optional, but strongly recommended for fast validation/routing:
        // "Point" | "LineString" | "Polygon"
        public string GeometryType { get; set; } = "";

        public required List<(double X, double Y, double? Z)> Geometry { get; set; }
    }

    public sealed partial class SpatialModelElementOverridePayload
    {
        public int? SpatialModelStyleId { get; set; }
        public int? SpatialShapeId { get; set; }

        public int? TrimPriority { get; set; }
        public List<SpatialElementInteractionEffect>? InteractionEffects { get; set; } // your JSON shape (or JsonDocument)

        public SurfaceModelTypeEnum? SurfaceModelType { get; set; }
        public ZSourceSettingEnum? ZSurfaceSetting { get; set; }

        public AnchorModeEnum? AnchorMode { get; set; }
        public decimal? AnchorXOffsetM { get; set; }
        public decimal? AnchorYOffsetM { get; set; }

        public Guid? BaseSurfaceId { get; set; }
        public Guid? TopSurfaceId { get; set; }

        public decimal? BaseOffsetInM { get; set; }
        public decimal? TopOffsetInM { get; set; }
    }

    public sealed class LotInfo 
    {
        public Guid LotUniqueId { get; set; }
        public long LotId { get; set; }
        public string LotNumber { get; set; } = "";
        public string WorkType { get; set; } = "";
        public string? Description { get; set; }
        public string Status { get; set; } = "";
        public DateTimeOffset? DateOpen { get; set; }
        public DateTimeOffset? DateConformed { get; set; }
        public DateTimeOffset? DateWorkStarted { get; set; }
        public DateTimeOffset? DateWorkCompleted { get; set; }
        public string? NCRs { get; set; }
        public List<AdditionalProperty> LstAdditionalProperties { get; set; } = new();
        public string? URL { get; set; }

        public long EntityId => LotId;

        public Guid EntityGuid => LotUniqueId;
    }
    public class WorkTypeInfo
    {
        public required string WorkTypeName { get; set; }
        public string? WorkTypeDescription { get; set; }

        public WorkTypeInfo()
        {

        }
    }

    public sealed class AdditionalProperty
    {
        public string Key { get; set; } = "";
        public string? Value { get; set; }
    }
}

public enum SpatialElementEffectTargetEnum
{
    Higher = 0,
    Lower = 1,
    Host = 2,
    Tool = 3,
    Auxiliary = 4
}

public enum SpatialElementInteractionActionEnum
{
    None = 0,

    Terminate = 1,
    TrimToBoundary = 2,

    SubtractVolume = 3,
    AddOpening = 4,

    OffsetAround = 5,
    StepOver = 6,

    Split = 7,
    Merge = 8,

    ClearanceOnly = 9,
    ClearanceEnvelope = 10,

    Ignore = 11
}
