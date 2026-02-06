using cpSpatial.Contract.Dtos.Spatial;

namespace cpSpatial.Contract.Validators
{
    public static class ImportLandXmlAsSurfaceRequestValidator
    {
        public static void Validate(ImportLandXmlAsSurfaceRequest r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (r.ProjectGuid == Guid.Empty) throw new ArgumentException("ProjectGuid is required.", nameof(r.ProjectGuid));
            //if (string.IsNullOrWhiteSpace(r.FileName)) throw new ArgumentException("FileName is required.", nameof(r.FileName));
            if (r.SridSource <= 0) throw new ArgumentOutOfRangeException(nameof(r.SridSource), "SridSource must be > 0.");
            if (r.SridTarget.HasValue && r.SridTarget.Value <= 0) throw new ArgumentOutOfRangeException(nameof(r.SridTarget), "SridTarget must be > 0.");
        }
    }
}