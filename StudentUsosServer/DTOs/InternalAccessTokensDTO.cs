using StudentUsosServer.Models;

namespace StudentUsosServer.DTOs
{
    public record InternalAccessTokensDTO(string internalAccessToken, string internalAccessTokenSecret);

    public static class InternalAccessTokensExtensions
    {
        public static InternalAccessTokensDTO ToDTO(this InternalAccessTokens internalAccessTokens)
        {
            return new(internalAccessTokens.InternalAccessToken, internalAccessTokens.InternalAccessTokenSecret);
        }
    }

}
