namespace StudentUsosServer.Services.Interfaces
{
    public interface IUsosApiService
    {
        /// <summary>
        /// Get signed URL to USOS API endpoint with all necessary tokens, keys and hashes
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="installation"></param>
        /// <param name="arguments"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <returns></returns>
        public string? GetSignedUsosUrl(string methodName, string installation, Dictionary<string, string> arguments, string accessToken = "", string accessTokenSecret = "");

        /// <summary>
        /// Sends webrequest to USOS API
        /// </summary>
        /// <param name="methodName">Part of URL pointing to required method e.g. "services/users/user"</param>
        /// <param name="arguments">Method arguments e.g. { { "user_id", "1" } }</param>
        /// <param name="installation"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <returns>USOS API response</returns>
        public Task<string?> SendRequestAsync(string methodName, Dictionary<string, string> arguments, string installation, string accessToken, string accessTokenSecret);

        /// <summary>
        /// Sends webrequest to USOS API and return full response of <see cref="HttpClient"/>
        /// </summary>
        /// <param name="methodName">Part of URL pointing to required method e.g. "services/users/user"</param>
        /// <param name="arguments">Method arguments e.g. { { "user_id", "1" } }</param>
        /// <param name="installation"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <returns>USOS API response</returns>
        public Task<(bool IsSuccess, HttpResponseMessage? Response, string? ResponseContent)> SendRequestFullResponseAsync(string methodName, Dictionary<string, string> arguments,
            string installation, string accessToken, string accessTokenSecret);
    }
}
