using System;
using System.Linq;

namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class ImportLandXmlAsSurfaceRequest
    {
        public Guid ProjectGuid { get; set; }

        public string? Name { get; set; }
        public Guid? SurfaceId { get; set; }

        public int SridSource { get; set; }
        public string? WktSource { get; set; }//not implemented yet, but we can support if users have WKT instead of SRID
        public int? SridTarget { get; set; }
        public string? WktTarget { get; set; }//not implemented yet, but we can support if users have WKT instead of SRID

        public double? TileSizeMeters { get; set; }
        public double? SnapMetersXY { get; set; }
    }
}
