using StudentUsosServer.Models.Usos;

namespace StudentUsosServer.DTOs
{
    public record RequestTokenDTO(string RequestToken, string RequestTokenSecret);

    public static class RequestTokenDTOExtensions
    { 
        public static RequestTokenDTO ToDTO(this UsosRequestTokens requestToken)
        {
            return new(requestToken.RequestToken, requestToken.RequestTokenSecret);
        }
    }

}
