using cpSpatial.Contract.Enums;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class UpdateSurfaceRequest
    {
        public string? name { get; set; }
        public SurfaceStatusEnum? status { get; set; }

        public string? source_type { get; set; }
        public string? source_uri { get; set; }
        public string? import_hash { get; set; }

        [Required]
        public Guid source_ClientCoordinateGuid { get; set; }

        [Required]
        public Guid stored_ClientCoordinateGuid { get; set; }
        public double? tile_size_m { get; set; }

        public double? origin_x { get; set; }
        public double? origin_y { get; set; }

        public double? min_x { get; set; }
        public double? min_y { get; set; }
        public double? max_x { get; set; }
        public double? max_y { get; set; }
    }
}
