
using FirebaseAdmin.Messaging;
using StudentUsosServer.Services.Interfaces;

namespace StudentUsosServer.Services
{
    public class PushNotificationsService : IPushNotificationsService
    {
        public async Task SendNotifications(List<string> tokens, Dictionary<string, string> data)
        {
            List<Task<BatchResponse>> tasks = new();

            //firebase accepts max 500 tokens in one request
            //https://firebase.google.com/docs/cloud-messaging/send-message#send-messages-to-multiple-devices
            int maxTokensInOneRequest = 500;
            for (int i = 0; i < Math.Ceiling(1.0 * tokens.Count / maxTokensInOneRequest); i++)
            {
                int startIndex = Math.Min(i * maxTokensInOneRequest, tokens.Count);
                int count = maxTokensInOneRequest;
                if (startIndex + count >= tokens.Count)
                {
                    count = tokens.Count - startIndex;
                }
                var message = new MulticastMessage()
                {
                    Tokens = tokens.GetRange(startIndex, count),
                    Data = data,
                    Android = new()
                    {
                        Priority = Priority.High
                    }
                };

                tasks.Add(FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message));
            }

            await Task.WhenAll(tasks);
        }
    }
}
