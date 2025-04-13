using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsosServer.Models
{
    public class User
    {
        public User()
        {

        }

        public User(string installation, string internalAccessToken, string internalAccessTokenSecret, string accessTokenSecret)
        {
            Installation = installation;
            InternalAccessToken = internalAccessToken;
            InternalAccessTokenSecret = internalAccessTokenSecret;
            AccessTokenSecret = accessTokenSecret;
        }

        public User(string installation, InternalAccessTokens internalAccessTokens, string accessTokenSecret)
        {
            Installation = installation;
            InternalAccessToken = internalAccessTokens.InternalAccessToken;
            InternalAccessTokenSecret = internalAccessTokens.InternalAccessTokenSecret;
            AccessTokenSecret = accessTokenSecret;
        }


        [Key]
        public int InternalId { get; set; }
        public string Installation { get; set; }
        public string InternalAccessToken { get; set; }
        public string InternalAccessTokenSecret { get; set; }
        public string AccessTokenSecret { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public long CreationDateUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;
        public long LastActiveDateUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string FcmTokensJson { get; private set; } = "";

        //USOS API
        [JsonPropertyName("id")]
        public string USOSId { get; set; } = string.Empty;
        [JsonPropertyName("student_number")]
        public string StudentNumber { get; set; } = string.Empty;

        public List<string> GetFcmTokens()
        {
            if (string.IsNullOrEmpty(FcmTokensJson))
            {
                return new();
            }
            return JsonSerializer.Deserialize<List<string>>(FcmTokensJson);
        }

        const int MaxTokens = 10;
        public void AddFcmToken(string token)
        {
            var tokens = GetFcmTokens();
            if (tokens.Contains(token))
            {
                return;
            }
            tokens.Add(token);
            if (tokens.Count > 10)
            {
                tokens.RemoveRange(0, tokens.Count - MaxTokens);
            }
            FcmTokensJson = JsonSerializer.Serialize(tokens);
        }
    }
}
