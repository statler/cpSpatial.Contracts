namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class IfcModelObjectDto
    {
        public long ModelObjectId { get; set; }
        public Guid ModelGuid { get; set; }       // object guid
        public string ModelUuid { get; set; } = ""; // IFC object GlobalId
        public string? IfcType { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Tag { get; set; }

        public bool IsAssignable { get; set; }

        public List<CoordinateDto>? EnvelopeGeography { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset ModifiedUtc { get; set; }
    }
}
