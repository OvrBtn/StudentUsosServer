using System.Text.Json;

namespace StudentUsosServer
{
    public class Secrets
    {
        public static Secrets Default { get; private set; }

        const string FileName = "secrets.json";
        static bool isInitialized = false;
        public static async Task Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);
            var jsonData = await File.ReadAllTextAsync(filePath);

            Default = JsonSerializer.Deserialize<Secrets>(jsonData);
            isInitialized = true;
        }

        public string InternalConsumerKey { get; init; }
        public string InternalConsumerKeySecret { get; init; }
        public string UsosConsumerKey { get; init; }
        public string UsosConsumerKeySecret { get; init; }
        public string FirebaseServiceAccountJsonFileName { get; init; }
        public string EscUrl { get; init; }
        public string UsosPushNotificationsHubVerifyToken { get; init; }

    }
}
