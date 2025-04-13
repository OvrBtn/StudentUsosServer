using StudentUsosServer.Models.Usos;
using StudentUsosServer.Services.Interfaces;

namespace StudentUsosServer.Services
{
    public class UsosAuthorizationService : IUsosAuthorizationService
    {
        IUsosApiService _usosApiService;
        public UsosAuthorizationService(IUsosApiService usosApiService)
        {
            _usosApiService = usosApiService;
        }

        public async Task<UsosAccessTokens?> GetAccessToken(string installation, string requestToken, string requestTokenSecret, string verifier)
        {
            var args = new Dictionary<string, string>();
            args.Add("oauth_verifier", verifier);
            string tokenstring = await _usosApiService.SendRequestAsync("services/oauth/access_token", args, installation, requestToken, requestTokenSecret);
            if (tokenstring == null)
            {
                return null;
            }

            string? accessToken = null;
            string? accessTokenSecret = null;
            var parts = tokenstring.Split('&');
            foreach (string part in parts)
            {
                if (part.StartsWith("oauth_token="))
                    accessToken = part.Substring("oauth_token=".Length);
                if (part.StartsWith("oauth_token_secret="))
                    accessTokenSecret = part.Substring("oauth_token_secret=".Length);
            }
            if (accessToken == null || accessTokenSecret == null) throw new Exception("Couldn't parse access token. Try to do this sequence manually!");

            return new(accessToken, accessTokenSecret);
        }

        public async Task<UsosRequestTokens?> GetRequestToken(string installation, string scopes, string callback)
        {
            var requestTokenArgs = new Dictionary<string, string>
            {
                { "oauth_callback", callback },
                { "scopes", scopes }
            };
            string tokenString = await _usosApiService.SendRequestAsync("services/oauth/request_token", requestTokenArgs, installation, "", "");
            if (tokenString == null)
            {
                return null;
            }

            string? requestToken = null;
            string? requestTokenSecret = null;
            string[] parts = tokenString.Split('&');
            foreach (string part in parts)
            {
                if (part.StartsWith("oauth_token="))
                    requestToken = part.Substring("oauth_token=".Length);
                if (part.StartsWith("oauth_token_secret="))
                    requestTokenSecret = part.Substring("oauth_token_secret=".Length);
            }
            if (requestToken == null || requestTokenSecret == null) throw new Exception("Couldn't parse request token.");
            return new(requestToken, requestTokenSecret);
        }
    }
}
