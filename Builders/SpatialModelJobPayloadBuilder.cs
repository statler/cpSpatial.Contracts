using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Collections.Generic;
using cpSpatial.Contract.Validators;
using cpSpatial.Contract.Dtos.Spatial;

namespace cpSpatial.Contract.Builders
{
    /// <summary>
    /// Builder for SpatialModelJobPayload that supports incremental construction
    /// even when payload classes use C# 'required' properties.
    ///
    /// We intentionally use a private "seed" payload where required members are initialized
    /// with empty defaults so the compiler is satisfied, then we validate before returning.
    /// </summary>
    public sealed class SpatialModelJobPayloadBuilder
    {
        // We seed required props to empty collections/objects, then validate in Build().
        private readonly SpatialModelJobPayload _payload = new SpatialModelJobPayload
        {
            Model = new SpatialModelPayload(),
            Elements = new List<SpatialModelElementPayload>(),
            Geometries = new List<GeometryPayload>(),
            CoordinateSystems = new List<CoordinateSystemPayload>(),
            LstLotInfo = new List<LotInfo>(),
            LstWorkTypes = new List<WorkTypeInfo>(),
            Presets = new List<SpatialElementPresetPayload>(),
            Shapes = new List<SpatialShapePayload>(),
            Styles = new List<SpatialModelStylePayload>()
        };

        // Optional: collect build-time validation errors (in addition to throwing).
        private readonly List<string> _errors = new();

        public SpatialModelJobPayloadBuilder WithSchema(int schemaVersion, string? contractName = null, string? schemaHash = null)
        {
            _payload.SchemaVersion = schemaVersion;
            if (!string.IsNullOrWhiteSpace(contractName)) _payload.ContractName = contractName!;
            if (schemaHash != null) _payload.SchemaHash = schemaHash;
            return this;
        }

        public SpatialModelJobPayloadBuilder WithJob(Guid jobId)
        {
            _payload.JobId = jobId;
            return this;
        }

        public SpatialModelJobPayloadBuilder WithProject(int projectId, Guid projectGuid)
        {
            _payload.ProjectId = projectId;
            _payload.ProjectGuid = projectGuid;
            return this;
        }

        public SpatialModelJobPayloadBuilder WithSpatialModelId(int spatialModelId)
        {
            _payload.SpatialModelId = spatialModelId;
            return this;
        }

        public SpatialModelJobPayloadBuilder WithModel(SpatialModelPayload model)
        {
            _payload.Model = model ?? throw new ArgumentNullException(nameof(model));
            return this;
        }

        // ------------------ Collections (replace) ------------------

        public SpatialModelJobPayloadBuilder WithElements(List<SpatialModelElementPayload> elements)
        {
            _payload.Elements = elements ?? throw new ArgumentNullException(nameof(elements));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithGeometries(List<GeometryPayload> geometries)
        {
            _payload.Geometries = geometries ?? throw new ArgumentNullException(nameof(geometries));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithCoordinateSystems(List<CoordinateSystemPayload> coordinateSystems)
        {
            _payload.CoordinateSystems = coordinateSystems ?? throw new ArgumentNullException(nameof(coordinateSystems));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithLotInfo(List<LotInfo> lots)
        {
            _payload.LstLotInfo = lots ?? throw new ArgumentNullException(nameof(lots));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithWorkTypes(List<WorkTypeInfo> workTypes)
        {
            _payload.LstWorkTypes = workTypes ?? throw new ArgumentNullException(nameof(workTypes));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithPresets(List<SpatialElementPresetPayload> presets)
        {
            _payload.Presets = presets ?? throw new ArgumentNullException(nameof(presets));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithShapes(List<SpatialShapePayload> shapes)
        {
            _payload.Shapes = shapes ?? throw new ArgumentNullException(nameof(shapes));
            return this;
        }

        public SpatialModelJobPayloadBuilder WithStyles(List<SpatialModelStylePayload> styles)
        {
            _payload.Styles = styles ?? throw new ArgumentNullException(nameof(styles));
            return this;
        }

        // ------------------ Collections (add) ------------------

        public SpatialModelJobPayloadBuilder AddElement(SpatialModelElementPayload element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _payload.Elements.Add(element);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddGeometry(GeometryPayload geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            _payload.Geometries.Add(geometry);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddCoordinateSystem(CoordinateSystemPayload cs)
        {
            if (cs == null) throw new ArgumentNullException(nameof(cs));
            _payload.CoordinateSystems.Add(cs);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddLotInfo(LotInfo lot)
        {
            if (lot == null) throw new ArgumentNullException(nameof(lot));
            _payload.LstLotInfo.Add(lot);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddWorkType(WorkTypeInfo wt)
        {
            if (wt == null) throw new ArgumentNullException(nameof(wt));
            _payload.LstWorkTypes.Add(wt);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddPreset(SpatialElementPresetPayload preset)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            _payload.Presets.Add(preset);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddShape(SpatialShapePayload shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            _payload.Shapes.Add(shape);
            return this;
        }

        public SpatialModelJobPayloadBuilder AddStyle(SpatialModelStylePayload style)
        {
            if (style == null) throw new ArgumentNullException(nameof(style));
            _payload.Styles.Add(style);
            return this;
        }

        // ------------------ Validation hooks ------------------

        /// <summary>
        /// Adds a build-time validation error. We can use this from the payload factory/service
        /// without immediately throwing, then throw once at Build().
        /// </summary>
        public SpatialModelJobPayloadBuilder AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _errors.Add(message);
            return this;
        }

        /// <summary>
        /// Finalize and validate required members. Throws InvalidOperationException if invalid.
        /// Optionally runs a caller-provided validator (e.g. your SpatialModelJobPayloadValidator).
        /// </summary>
        public SpatialModelJobPayload Build()
        {
            // Required scalar checks (we can extend these over time).
            if (_payload.JobId == Guid.Empty) _errors.Add("JobId is required.");
            if (_payload.ProjectId <= 0) _errors.Add("ProjectId is required and must be > 0.");
            if (_payload.ProjectGuid == Guid.Empty) _errors.Add("ProjectGuid is required.");
            if (_payload.SpatialModelId <= 0) _errors.Add("SpatialModelId is required and must be > 0.");

            if (_payload.Model == null) _errors.Add("Model is required.");
            if (_payload.Elements == null) _errors.Add("Elements is required.");
            if (_payload.Geometries == null) _errors.Add("Geometries is required.");
            if (_payload.CoordinateSystems == null) _errors.Add("CoordinateSystems is required.");
            if (_payload.LstLotInfo == null) _errors.Add("LstLotInfo is required.");
            if (_payload.LstWorkTypes == null) _errors.Add("LstWorkTypes is required.");
            if (_payload.Presets == null) _errors.Add("Presets is required.");
            if (_payload.Shapes == null) _errors.Add("Shapes is required.");
            if (_payload.Styles == null) _errors.Add("Styles is required.");

            var _shapeValidationErrors = SpatialModelJobPayloadValidator.Validate(_payload);
            if (_shapeValidationErrors.Count > 0)
                _errors.AddRange(_shapeValidationErrors);

            if (_errors.Count > 0)
                throw new InvalidOperationException("SpatialModelJobPayloadBuilder.Build failed: " + string.Join(" | ", _errors));

            return _payload;
        }
    }
}

