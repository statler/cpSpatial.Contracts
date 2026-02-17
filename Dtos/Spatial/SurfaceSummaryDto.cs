using cpSpatial.Contract.Enums;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class SurfaceSummaryDto
    {
        public Guid SurfaceId { get; set; }
        public string? SurfaceName { get; set; }
        public string? SourceUri { get; set; }

        public SurfaceStatusEnum Status { get; set; }
        public Guid SourceClientCoordinateGuid { get; set; }
        public Guid StoredClientCoordinateGuid { get; set; }

        public double TileSizeM { get; set; }
        public long? TriangleCount { get; set; }

        public DateTimeOffset? ImportedOn { get; set; }
        public DateTimeOffset? ReplacedOn { get; set; }

        public Guid ProjectGuid { get; set; }
    }
}
