using System;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public class UpdateCoordinateSystemRequest
    {
        public int? Srid { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Tag { get; set; }
        public string SrWkt { get; set; } = "";
    }
}
