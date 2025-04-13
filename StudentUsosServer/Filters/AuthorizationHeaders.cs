using System.Text.Json.Serialization;
using static StudentUsosServer.Filters.Constants;

namespace StudentUsosServer.Filters
{
    public class AuthorizationHeaders
    {
        [JsonPropertyName(InternalConsumerKeyKey)]
        public required string InternalConsumerKey { get; set; }
        [JsonPropertyName(InstallationKey)]
        public required string Installation { get; set; }
        [JsonPropertyName(TimestampKey)]
        /// <summary>
        /// Unix timestamp from UTC time
        /// </summary>
        public required string Timestamp { get; set; }
        [JsonPropertyName(HashKey)]
        public required string Hash { get; set; }
        [JsonPropertyName(ApiVersionKey)]
        public required string ApiVersion { get; set; }
        [JsonPropertyName(ApplicationVersionKey)]
        public required string ApplicationVersion { get; set; }

        [JsonPropertyName(UsosAccessTokenKey)]
        public string? UsosAccessToken { get; set; }
        [JsonPropertyName(InternalAccessTokenKey)]
        public string? InternalAccessToken { get; set; }
    }

    public static class AuthorizationHeadersExtenstions
    {
        public static Dictionary<string, string> ToDictionary(this AuthorizationHeaders headers)
        {
            var dictionary = new Dictionary<string, string>
            {
                { InternalConsumerKeyKey, headers.InternalConsumerKey },
                { InstallationKey, headers.Installation },
                { TimestampKey, headers.Timestamp },
                { HashKey, headers.Hash },
                { ApiVersionKey, headers.ApiVersion },
                { ApplicationVersionKey, headers.ApplicationVersion }
            };

            if (string.IsNullOrEmpty(headers.UsosAccessToken) == false)
            {
                dictionary[UsosAccessTokenKey] = headers.UsosAccessToken;
            }

            if (string.IsNullOrEmpty(headers.InternalAccessToken) == false)
            {
                dictionary[InternalAccessTokenKey] = headers.InternalAccessToken;
            }

            return dictionary;
        }
    }

}
