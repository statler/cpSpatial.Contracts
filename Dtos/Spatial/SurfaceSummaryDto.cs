using cpSpatial.Contract.Enums;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class SurfaceSummaryDto
    {
        public Guid surface_id { get; set; }
        public string? name { get; set; }
        public string? sourceUri { get; set; }

        public SurfaceStatusEnum status { get; set; }

        // 🔁 Replaces srid
        public Guid source_coordinate_system_id { get; set; }
        public Guid stored_coordinate_system_id { get; set; }

        public double tile_size_m { get; set; }
        public long? triangle_count { get; set; }

        public DateTimeOffset? imported_on { get; set; }
        public DateTimeOffset? replaced_on { get; set; }

        public Guid ProjectGuid { get; set; }
    }
}
