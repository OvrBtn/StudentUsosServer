using StudentUsosServer.Models.Usos;

namespace StudentUsosServer.Services.Interfaces
{
    public interface IUsosPushNotificationsService
    {
        public Task SendNewGradeNotification(IList<UsosGradeCreatedEntry> entries, string installationUrl);
    }
}
