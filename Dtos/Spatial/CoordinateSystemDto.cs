using System;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public record CoordinateSystemDto(
        Guid CoordinateSystemId,
        Guid ProjectGuid,
        Guid SourceGuid,
        int? Srid,
        string Name,
        string? Description,
        string? Tag,
        string SrWkt);
}
