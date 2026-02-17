using System;
using System.ComponentModel.DataAnnotations;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public class CreateCoordinateSystemRequest
    {
        public Guid ProjectGuid { get; set; }
        public Guid SourceGuid { get; set; }            // required (payload identifier)
        public int? Srid { get; set; }                  // optional reference
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Tag { get; set; }
        public string SrWkt { get; set; } = "";
    }
}
