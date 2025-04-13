using System.Text.Json;

namespace StudentUsosServer.Filters
{
    public static class AuthorizeFilterHelper
    {
        public static async Task<string> ReadPostRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            var stream = request.Body;
            using StreamReader reader = new(stream, leaveOpen: true);
            string requestBodyJson = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return requestBodyJson;
        }

        public static Dictionary<string, string> ReadGetRequestBody(HttpRequest request)
        {
            return request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static AuthorizationHeaders GetAuthorizationDataFromHeaders(Dictionary<string, string> headers, AuthorizeAccessFilter.Mode mode)
        {
            var headersSerialized = JsonSerializer.Serialize(headers);

            try
            {
                var deserializedResult = JsonSerializer.Deserialize<AuthorizationHeaders>(headersSerialized, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                });
                if (deserializedResult is null)
                {
                    throw new ArgumentException("Deserialized headers are null");
                }
                if (mode == AuthorizeAccessFilter.Mode.Full)
                {
                    if (string.IsNullOrEmpty(deserializedResult.UsosAccessToken))
                    {
                        throw new ArgumentException("Missing usos access token");
                    }
                    if (string.IsNullOrEmpty(deserializedResult.InternalAccessToken))
                    {
                        throw new ArgumentException("Missing internal access token");
                    }
                }
                return deserializedResult;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Incorrect headers, can't parse to target object.", ex);
            }
        }
    }
}
