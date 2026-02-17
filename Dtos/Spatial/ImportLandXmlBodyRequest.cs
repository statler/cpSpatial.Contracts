using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public class ImportLandXmlBodyRequest
    {
        public Guid? SurfaceId { get; set; }

        [Required]
        public Guid ClientProjectGuid { get; set; }

        public string? Name { get; set; }

        [Required]
        public Guid SourceClientCoordinateGuid { get; set; }

        [Required]
        public Guid StoredClientCoordinateGuid { get; set; }

        [Required]
        public double TileSizeMeters { get; set; }

        public double? SnapMetersXY { get; set; }
    }
}
