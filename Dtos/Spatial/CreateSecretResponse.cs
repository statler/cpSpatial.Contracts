namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class CreateSecretResponse
    {
        public Guid client_id { get; set; }
        public Guid client_secret_id { get; set; }
        public string client_secret { get; set; } = string.Empty;
    }
}
