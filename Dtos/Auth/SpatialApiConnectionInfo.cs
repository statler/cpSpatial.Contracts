using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace cpSpatial.Contract.Dtos.Auth
{
    public class SpatialApiConnectionInfo
    {
        private string? secretKey;

        /// <summary>Base URL for the Spatial API, e.g. https://spatialapi.yourdomain.com</summary>
        public required string BaseUrl { get; init; } = "https://spatial.civilpro.com";

        /// <summary>
        /// Relative path to the token endpoint, e.g. /auth/token or /api/auth/token
        /// Keep it configurable because it will differ between environments.
        /// </summary>
        public required string TokenPath { get; init; } = "/auth/GetToken";

        /// <summary>
        /// The Spatial API client id (the one already created in Spatial API).
        /// If your Spatial API uses GUIDs, this should be a GUID string.
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Reference/key to where the secret is stored in YOUR encrypted KV store.
        /// Don’t store the secret in this object if you can avoid it.
        /// </summary>
        public required string? SecretKey { get => secretKey; init => secretKey = value; }


        public void ClearKey()
        {
            secretKey = null;
        }

        public void SetSecretValueToMessage(string message)
        {
            secretKey = message;
        }
    }
}
