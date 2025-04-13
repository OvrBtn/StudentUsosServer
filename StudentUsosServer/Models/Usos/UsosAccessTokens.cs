namespace StudentUsosServer.Models.Usos
{
    public class UsosAccessTokens
    {
        public UsosAccessTokens(string AccessToken, string AccessTokenSecret)
        {
            this.AccessToken = AccessToken;
            this.AccessTokenSecret = AccessTokenSecret;
        }

        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
