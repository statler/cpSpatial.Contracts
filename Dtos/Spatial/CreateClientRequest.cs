namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class CreateClientRequest
    {
        public string client_name { get; set; } = string.Empty;
        public string? client_url { get; set; }
        public DateTimeOffset? valid_from_utc { get; set; }
        public int? rate_limit_rpm { get; set; }
        public int? heavy_rate_limit_rpm { get; set; }
        public string? description { get; set; }
    }
}
