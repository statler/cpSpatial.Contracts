using System;
using System.Linq;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public class SurfaceSummary
    {
        public Guid surface_id { get; set; }
        public required string name { get; set; }
        public int? status { get; set; } // or your enum
        public int? srid { get; set; }
        public double tile_size_m { get; set; }
        public long? triangle_count { get; set; }
        public DateTimeOffset? imported_on { get; set; }
        public DateTimeOffset? replaced_on { get; set; }
        public int clientProjectId { get; set; }
        public Guid projectGuid { get; set; }
        public string? sourceUri { get; set; }
    }
}
