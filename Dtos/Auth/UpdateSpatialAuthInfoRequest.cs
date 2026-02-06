using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace cpSpatial.Contract.Dtos.Auth
{
    public class UpdateAuthInfoRequest
    {
        public required SpatialApiConnectionInfo Details { get; init; }
    }
}
