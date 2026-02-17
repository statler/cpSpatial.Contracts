using cpSpatial.Contract.Enums;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    // ============================================================
    // Requests/Responses
    // ============================================================

    public sealed class CreateSurfaceRequest
    {
        public Guid? surface_id { get; set; }
        public string? name { get; set; }

        [Required]
        public Guid client_project_guid { get; set; }

        public string? source_type { get; set; }
        public string? source_uri { get; set; }
        public string? import_hash { get; set; }

        public SurfaceStatusEnum? status { get; set; }

        [Required]
        public Guid source_ClientCoordinateGuid { get; set; }

        [Required]
        public Guid stored_ClientCoordinateGuid { get; set; }

        [Required]
        public double tile_size_m { get; set; }

        public double? origin_x { get; set; }
        public double? origin_y { get; set; }

        // Optional override
        public DateTimeOffset? imported_on { get; set; }
    }
}
