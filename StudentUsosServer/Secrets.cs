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

        public required string InternalConsumerKey { get; init; }
        public required string InternalConsumerKeySecret { get; init; }
        public required string UsosConsumerKey { get; init; }
        public required string UsosConsumerKeySecret { get; init; }
        public required string FirebaseServiceAccountJsonFileName { get; init; }
        public required string EscUrl { get; init; }
        public required string UsosPushNotificationsHubVerifyToken { get; init; }
        public required string RootUserStudentNumber { get; init; }

    }
}
