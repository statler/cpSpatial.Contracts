namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class IfcModelDto
    {
        public long ModelId { get; set; }
        public Guid ModelGuid { get; set; }

        public Guid ClientId { get; set; }
        public Guid? ProjectGuid { get; set; }

        public string ModelName { get; set; } = "";
        public string? ModelDescription { get; set; }
        public string IfcGlobalId { get; set; } = "";

        public string? OriginalFilename { get; set; }
        public string? FilenameOnServer { get; set; }
        public DateTimeOffset? DateLastAccessed { get; set; }

        public DateTimeOffset DateUploaded { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset ModifiedUtc { get; set; }
        public Guid CoordinateSystemId { get; set; }

    }
}
