namespace StudentUsosServer.Models.Usos
{
    public class UsosRequestTokens
    {
        public string RequestToken { get; set; }
        public string RequestTokenSecret { get; set; }
        public DateTime CreationDate { get; init; } = DateTime.Now;

        public UsosRequestTokens(string RequestToken, string RequestTokenSecret)
        {
            this.RequestToken = RequestToken;
            this.RequestTokenSecret = RequestTokenSecret;
        }
    }
}
