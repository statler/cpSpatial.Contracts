namespace cpSpatial.Contract.Dtos.Spatial
{

    public sealed class UpsertModelFromIfcResult
    {
        public IfcModelDto Model { get; set; } = new();
        public int InsertedObjects { get; set; }
        public int DeletedObjects { get; set; }
    }
}
