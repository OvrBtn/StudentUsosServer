using StudentUsosServer.Models.Usos;

namespace StudentUsosServer.DTOs
{
    public record AccessTokenDTO(string AccessToken, string InternalAccessToken, string InternalAccessTokenSecret);

    public static class AccessTokenExtensions
    {
        public static AccessTokenDTO ToDTO(this UsosAccessTokens accessToken, string internalAccessToken, string internalAccessTokenSecret)
        {
            return new(accessToken.AccessToken, internalAccessToken, internalAccessTokenSecret);
        }
    }
}
