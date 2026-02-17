namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class CreateSecretRequest
    {
        public DateTimeOffset? valid_from_utc { get; set; }
        public string? label { get; set; }
    }
}
