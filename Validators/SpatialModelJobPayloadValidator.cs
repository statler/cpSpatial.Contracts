using cpSpatial.Contract.Dtos.Spatial;
using cpSpatial.Contract.Enums;

namespace cpSpatial.Contract.Validators
{

    public static class SpatialModelJobPayloadValidator
    {
        // Your enums:
        // ZSourceSettingEnum: 1=From_Geometry, 2=From_Surface, 3=Use_Fixed_Z
        private const int Z_FROM_GEOMETRY = (int)ZSourceSettingEnum.From_Geometry;
        private const int Z_FROM_SURFACE = (int)ZSourceSettingEnum.From_Surface;
        private const int Z_FIXED = (int)ZSourceSettingEnum.Use_Fixed_Z;

        // SurfaceModelTypeEnum:
        private const int SMT_MESH_ONE_SURFACE = (int)SurfaceModelTypeEnum.One_Surface_To_3D_Mesh;
        private const int SMT_SOLID_ONE_SURFACE_HEIGHT = (int)SurfaceModelTypeEnum.One_Surface_With_Height_To_3DSolid;
        private const int SMT_SOLID_TWO_SURFACES = (int)SurfaceModelTypeEnum.Two_Surfaces_To_3DSolid;
        private const int SMT_EXTRUDE_ALONG_PATH = (int)SurfaceModelTypeEnum.Extrude_Shape_Along_Path;
        private const int SMT_EXTRUDE_VERTICAL_FROM_POINT = (int)SurfaceModelTypeEnum.Extrude_Vertical_Shape_From_Point;
        // 6 ignored

        public static List<string> Validate(SpatialModelJobPayload payload)
        {
            var errors = new List<string>();

            if (payload.Model == null)
            {
                errors.Add("Payload.Model is null.");
                return errors;
            }

            // Fast lookups
            var presetById = payload.Presets?.ToDictionary(p => p.SpatialElementPresetId)
                             ?? new Dictionary<int, SpatialElementPresetPayload>();

            var shapeIds = new HashSet<int>(payload.Shapes?.Select(s => s.SpatialShapeId) ?? Enumerable.Empty<int>());
            var styleIds = new HashSet<int>(payload.Styles?.Select(s => s.SpatialModelStyleId) ?? Enumerable.Empty<int>());

            var geometryById = payload.Geometries?.ToDictionary(g => g.GeometryId)
                               ?? new Dictionary<int, GeometryPayload>();

            // Model mesh defaults sanity
            ValidateMeshValues("Model", payload.Model.MeshResolutionInM, payload.Model.MaxExtrapolationDistanceInM, errors);

            foreach (var e in payload.Elements ?? Enumerable.Empty<SpatialModelElementPayload>())
            {
                // Resolve effective config: element override -> preset -> required
                var preset = (e.SpatialElementPresetId != null && presetById.TryGetValue(e.SpatialElementPresetId.Value, out var p))
                    ? p
                    : null;

                var surfaceModelType = e.ElementOverride?.SurfaceModelType ?? preset?.SurfaceModelType;
                var zSource = e.ElementOverride?.ZSurfaceSetting ?? preset?.ZSurfaceSetting;

                int? shapeId = e.ElementOverride?.SpatialShapeId ?? preset?.SpatialShapeId;
                int? styleId = e.ElementOverride?.SpatialModelStyleId ?? preset?.SpatialModelStyleId;

                Guid? baseSurfaceId = e.ElementOverride?.BaseSurfaceId ?? preset?.BaseSurfaceId;
                Guid? topSurfaceId = e.ElementOverride?.TopSurfaceId ?? preset?.TopSurfaceId;

                decimal? baseOffset = e.ElementOverride?.BaseOffsetInM ?? preset?.BaseOffsetInM ?? 0m;
                decimal? topOffset = e.ElementOverride?.TopOffsetInM ?? preset?.TopOffsetInM ?? 0m;

                decimal? height = e.HeightInM ?? preset?.HeightInM;

                // Geometry presence
                GeometryPayload? geom = null;
                if (e.GeometryId != null)
                {
                    if (!geometryById.TryGetValue(e.GeometryId.Value, out geom))
                        errors.Add($"Element {e.SpatialModelElementId}: GeometryId {e.GeometryId.Value} missing from payload.Geometries.");
                }
                else
                {
                    errors.Add($"Element {e.SpatialModelElementId}: GeometryId is null (geometry is required for spatial processing).");
                }

                // Must have SurfaceModelType + ZSourceSetting resolved
                if (surfaceModelType == null)
                    errors.Add($"Element {e.SpatialModelElementId}: SurfaceModelType not resolved (need preset or element override).");

                if (zSource == null)
                    errors.Add($"Element {e.SpatialModelElementId}: ZSurfaceSetting not resolved (need preset or element override).");

                // Validate style/shape references if present
                if (shapeId != null && !shapeIds.Contains(shapeId.Value))
                    errors.Add($"Element {e.SpatialModelElementId}: SpatialShapeId {shapeId.Value} not present in payload.References.Shapes.");

                if (styleId != null && !styleIds.Contains(styleId.Value))
                    errors.Add($"Element {e.SpatialModelElementId}: SpatialModelStyleId {styleId.Value} not present in payload.References.Styles.");

                // Mesh override chain sanity: element mesh -> preset mesh -> model
                if (e.MeshOverride != null)
                    ValidateMeshValues($"Element {e.SpatialModelElementId} mesh override", e.MeshOverride.MeshResolutionInM, e.MeshOverride.MaxExtrapolationDistanceInM, errors);

                if (preset?.PresetMeshOverride != null)
                    ValidateMeshValues($"Preset {preset.SpatialElementPresetId} mesh override", preset?.PresetMeshOverride.MeshResolutionInM, preset?.PresetMeshOverride.MaxExtrapolationDistanceInM, errors);

                // If we’re missing required basics, skip deeper checks to avoid cascades
                if (surfaceModelType == null || zSource == null || geom == null)
                    continue;

                // Geometry type ↔ model type expectations (your assumption: polyline implies extrusion)
                if (geom.GeometryType.Equals("LineString", StringComparison.OrdinalIgnoreCase)
                    && surfaceModelType != SurfaceModelTypeEnum.Extrude_Shape_Along_Path)
                {
                    errors.Add($"Element {e.SpatialModelElementId}: GeometryType LineString implies SurfaceModelType=Extrude_Shape_Along_Path (4). Actual={surfaceModelType}.");
                }

                if (geom.GeometryType.Equals("Point", StringComparison.OrdinalIgnoreCase)
                    && surfaceModelType == SurfaceModelTypeEnum.Extrude_Vertical_Shape_From_Point)
                {
                    errors.Add($"Element {e.SpatialModelElementId}: GeometryType Point implies SurfaceModelType=Extrude_Vertical_Shape_From_Point (5). Actual={surfaceModelType}.");
                }

                if (geom.GeometryType.Equals("Polygon", StringComparison.OrdinalIgnoreCase)
                    && (surfaceModelType == SurfaceModelTypeEnum.Extrude_Shape_Along_Path || surfaceModelType == SurfaceModelTypeEnum.Extrude_Vertical_Shape_From_Point))
                {
                    errors.Add($"Element {e.SpatialModelElementId}: GeometryType Polygon is not valid for extrusion SurfaceModelType {surfaceModelType}.");
                }

                // ZSourceSetting rules
                if (zSource == ZSourceSettingEnum.From_Geometry)
                {
                    if (surfaceModelType is SurfaceModelTypeEnum.One_Surface_To_3D_Mesh or SurfaceModelTypeEnum.One_Surface_With_Height_To_3DSolid or SurfaceModelTypeEnum.Two_Surfaces_To_3DSolid)
                        errors.Add($"Element {e.SpatialModelElementId}: ZSourceSetting=From_Geometry only valid for extrusions (4/5).");

                    // Must have Z present on geometry coordinates
                    if (!HasZ(geom))
                        errors.Add($"Element {e.SpatialModelElementId}: ZSourceSetting=From_Geometry requires Z values on geometry.");
                }
                else if (zSource == ZSourceSettingEnum.From_Surface)
                {
                    // require base surface for types needing base
                    if (surfaceModelType is SurfaceModelTypeEnum.One_Surface_To_3D_Mesh or SurfaceModelTypeEnum.One_Surface_With_Height_To_3DSolid or SurfaceModelTypeEnum.Extrude_Shape_Along_Path or SurfaceModelTypeEnum.Extrude_Vertical_Shape_From_Point)
                    {
                        if (baseSurfaceId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: ZSourceSetting=From_Surface requires BaseSurfaceId for SurfaceModelType {surfaceModelType}.");
                    }

                    if (surfaceModelType == SurfaceModelTypeEnum.Two_Surfaces_To_3DSolid)
                    {
                        if (baseSurfaceId == null || topSurfaceId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: Two_Surfaces_To_3DSolid requires both BaseSurfaceId and TopSurfaceId.");
                    }
                }
                else if (zSource == ZSourceSettingEnum.Use_Fixed_Z)
                {
                    // Surfaces redundant; we don’t hard-fail, but flag as warning-like errors if you want strictness.
                    // If you want non-strict, remove these.
                    if (baseSurfaceId != null || topSurfaceId != null)
                        errors.Add($"Element {e.SpatialModelElementId}: ZSourceSetting=Use_Fixed_Z makes BaseSurfaceId/TopSurfaceId redundant (should be null).");

                    // Offsets default to 0; OK.
                    _ = baseOffset;
                    _ = topOffset;
                }
                else
                {
                    errors.Add($"Element {e.SpatialModelElementId}: Unknown ZSourceSetting value: {zSource}.");
                }

                // SurfaceModelType rules
                switch (surfaceModelType)
                {
                    case SurfaceModelTypeEnum.One_Surface_To_3D_Mesh:
                        if (zSource != ZSourceSettingEnum.From_Surface)
                            errors.Add($"Element {e.SpatialModelElementId}: One_Surface_To_3D_Mesh requires ZSourceSetting=From_Surface.");
                        if (baseSurfaceId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: One_Surface_To_3D_Mesh requires BaseSurfaceId.");
                        break;

                    case SurfaceModelTypeEnum.One_Surface_With_Height_To_3DSolid:
                        if (zSource != ZSourceSettingEnum.From_Surface)
                            errors.Add($"Element {e.SpatialModelElementId}: One_Surface_With_Height_To_3DSolid requires ZSourceSetting=From_Surface.");
                        if (baseSurfaceId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: One_Surface_With_Height_To_3DSolid requires BaseSurfaceId.");
                        if (height == null || height <= 0)
                            errors.Add($"Element {e.SpatialModelElementId}: One_Surface_With_Height_To_3DSolid requires HeightInM > 0 (element core or preset).");
                        break;

                    case SurfaceModelTypeEnum.Two_Surfaces_To_3DSolid:
                        if (baseSurfaceId == null || topSurfaceId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: Two_Surfaces_To_3DSolid requires both BaseSurfaceId and TopSurfaceId.");
                        // Height redundant; we don’t fail.
                        break;

                    case SurfaceModelTypeEnum.Extrude_Shape_Along_Path:
                        if (shapeId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: Extrude_Shape_Along_Path requires SpatialShapeId (preset or override).");
                        if (!geom.GeometryType.Equals("LineString", StringComparison.OrdinalIgnoreCase))
                            errors.Add($"Element {e.SpatialModelElementId}: Extrude_Shape_Along_Path requires LineString geometry.");
                        break;

                    case SurfaceModelTypeEnum.Extrude_Vertical_Shape_From_Point:
                        if (shapeId == null)
                            errors.Add($"Element {e.SpatialModelElementId}: Extrude_Vertical_Shape_From_Point requires SpatialShapeId (preset or override).");
                        if (!geom.GeometryType.Equals("Point", StringComparison.OrdinalIgnoreCase))
                            errors.Add($"Element {e.SpatialModelElementId}: Extrude_Vertical_Shape_From_Point requires Point geometry.");
                        if (height == null || height <= 0)
                            errors.Add($"Element {e.SpatialModelElementId}: Extrude_Vertical_Shape_From_Point requires HeightInM > 0 (element core or preset).");
                        break;

                    default:
                        errors.Add($"Element {e.SpatialModelElementId}: Unknown or unsupported SurfaceModelType: {surfaceModelType}.");
                        break;
                }
            }

            return errors;
        }

        private static bool HasZ(GeometryPayload g)
        {
            var pts = g?.Geometry;
            if (pts == null || pts.Count == 0) return false;

            static bool IsValidZ(double? z) => z.HasValue && !double.IsNaN(z.Value);

            // We require first and last to have Z (covers the “single point” case too).
            if (!IsValidZ(pts[0].Z)) return false;
            if (!IsValidZ(pts[pts.Count - 1].Z)) return false;

            return true;
        }

        private static void ValidateMeshValues(string scope, decimal? meshRes, decimal? maxExtrap, List<string> errors)
        {
            if (meshRes != null && meshRes < 0)
                errors.Add($"{scope}: MeshResolutionInM must be >= 0.");
            if (maxExtrap != null && maxExtrap is < 0)
                errors.Add($"{scope}: MaxExtrapolationDistanceInM must be >= 0.");
        }
    }

}