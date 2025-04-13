namespace StudentUsosServer.Services.Interfaces
{
    public interface IPushNotificationsService
    {
        public Task SendNotifications(List<string> tokens, Dictionary<string, string> data);
    }
}
