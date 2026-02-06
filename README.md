# CivilPro.Spatial.Contracts

Shared **schema contracts** for Spatial job payloads used by CivilPro services.

This package defines the **canonical DTOs, enums, and schema versions** used to exchange spatial model jobs between the *producer* (CivilPro application / APIs) and the *consumer* (Spatial Service).

It exists to guarantee that both sides agree on **structure, semantics, and evolution rules** of spatial job payloads.

---

## Why This Package Exists

Distributed systems fail most often at **contract boundaries**.

This library provides:
- A single source of truth for spatial job schemas
- Strong typing across services
- Explicit schema versioning
- Clear compatibility rules

It allows CivilPro services to evolve **independently but safely**.

---

## What This Package Is (and Is Not)

### ✔ Included
- Job payload DTOs (e.g. `SpatialModelJobPayload`)
- Element, preset, shape, style, geometry payloads
- Shared enums (e.g. `SurfaceModelTypeEnum`, `ZSourceSettingEnum`)
- JSON-serialisable, transport-safe models
- Schema version constants and documentation

### ❌ Explicitly Not Included
- Entity Framework models or DbContexts
- Validation or resolution logic
- Geometry processing algorithms
- Azure, storage, or job orchestration concerns
- Any runtime spatial computation

Those responsibilities belong in **service-specific libraries**, not in the contract.

---

## Architectural Role

```
+----------------------+         +----------------------+
|  CivilPro API / UI  |  JSON   |   Spatial Service     |
|----------------------|-------->|----------------------|
| EF Models            |         | Geometry processing  |
| Payload assembly     |         | Validation + meshing |
| Client-side checks   |         | IFC generation       |
+----------------------+         +----------------------+
            ^
            |
            |   Shared Contract
            |
+----------------------------------------------+
|        CivilPro.Spatial.Contracts             |
|----------------------------------------------|
| Payload DTOs                                 |
| Enums                                        |
| Schema version                               |
+----------------------------------------------+
```

---

## Schema Versioning & Compatibility

Each payload includes an explicit schema version:

```csharp
public sealed class SpatialModelJobPayload
{
    public int SchemaVersion { get; set; } = 3;
}
```

### Versioning Rules

This package follows **semantic versioning aligned to schema compatibility**:

| Change type | Package version | Schema impact |
|------------|----------------|---------------|
| Bugfix / docs only | Patch (`1.0.x`) | None |
| Backward-compatible schema extension | Minor (`1.1.0`) | Optional fields only |
| Breaking schema change | Major (`2.0.0`) | Consumer update required |

### Service Expectations

- **Producers** must emit the correct `SchemaVersion`
- **Consumers** must validate the version before processing
- Older schema versions *may* be supported for migration windows
- Unknown schema versions must be rejected explicitly

---

## Payload Resolution Model

Payloads represent **raw configuration**, not resolved values.

Resolution is intentionally deferred to the spatial service.

Resolution order:

1. Element override
2. Preset defaults
3. Model defaults
4. Validation failure if unresolved

Benefits:
- Smaller payloads
- Clear responsibility boundaries
- Easier long-term evolution

---

## Geometry Handling Contract

- Payload geometries reference **native coordinate systems**
- Geometry is serialized as arrays of `[x, y, z]`
- Coordinate system metadata (SRID + WKT) is embedded
- WGS geometry is intentionally excluded

The spatial service is responsible for:
- Interpreting geometry
- Applying Z sourcing rules
- Querying surfaces when required

---

## Stability Guarantees

This contract makes the following guarantees:

- DTO property names are **stable once released**
- Enums are **append-only**
- Existing fields are never repurposed
- Breaking changes require a **major version bump**
- Schema validation failures are explicit and deterministic

---

## Usage

Add the package to producer and consumer projects:

```bash
dotnet add package CivilPro.Spatial.Contracts
```

For strict compatibility:

```xml
<PackageReference Include="CivilPro.Spatial.Contracts" Version="[1.0.0]" />
```

---

## Changelog

All notable schema changes are documented here.

### [1.0.0]
- Initial public release
- Spatial model job payload v3
- Element, preset, shape, style, geometry contracts

### [1.1.0]
- (example) Added optional mesh override fields
- No breaking changes

### [2.0.0]
- (example) Breaking schema restructure
- Requires consumer update

---

## Design Philosophy

This library is intentionally:
- Boring
- Explicit
- Stable
- Minimal

If a change feels convenient but couples services together,
**it probably doesn’t belong here**.

---

## License

MIT
