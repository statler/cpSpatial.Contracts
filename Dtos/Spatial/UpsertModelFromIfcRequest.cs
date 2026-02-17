namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class UpsertModelFromIfcRequest
    {
        public Guid? ModelGuid { get; set; } // null => create; set => update/overwrite

        public Guid ClientId { get; set; }
        public Guid ProjectGuid { get; set; }

        public string ModelName { get; set; } = "";
        public string? ModelDescription { get; set; }

        /// <summary>Blob filename. Blob path will be "{ClientId}/{FileName}".</summary>
        public string FileName { get; set; } = "";
        public Guid SourceClientCoordinateGuid { get; set; }
        public Guid StoredClientCoordinateGuid { get; set; }
    }
}
