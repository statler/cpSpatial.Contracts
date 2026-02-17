namespace cpSpatial.Contract.Dtos.Spatial
{
    public sealed class UpdateClientRequest
    {
        public string? client_name { get; set; }
        public string? client_url { get; set; }
        public bool? is_enabled { get; set; }
        public DateTimeOffset? valid_from_utc { get; set; }
        public int? rate_limit_rpm { get; set; }
        public int? heavy_rate_limit_rpm { get; set; }
        public string? description { get; set; }
    }
}
