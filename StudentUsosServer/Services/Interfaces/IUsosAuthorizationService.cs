using StudentUsosServer.Models.Usos;

namespace StudentUsosServer.Services.Interfaces
{
    public interface IUsosAuthorizationService
    {
        public Task<UsosRequestTokens?> GetRequestToken(string installation, string scopes, string callback);
        public Task<UsosAccessTokens?> GetAccessToken(string installation, string requestToken, string requestTokenSecret, string verifier);
    }
}
