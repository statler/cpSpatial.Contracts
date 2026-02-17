using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class ImportLandXmlBodyRequest
    {
        public Guid? SurfaceId { get; set; }

        [Required]
        public Guid ClientProjectGuid { get; set; }

        public string? Name { get; set; }

        [Required]
        public Guid SourceCoordinateSystemId { get; set; }

        [Required]
        public Guid StoredCoordinateSystemId { get; set; }

        [Required]
        public double TileSizeMeters { get; set; }

        public double? SnapMetersXY { get; set; }
    }
}
