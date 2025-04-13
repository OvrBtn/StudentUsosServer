using StudentUsosServer.Database;
using StudentUsosServer.Models.Usos;
using StudentUsosServer.Services.Interfaces;

namespace StudentUsosServer.Services
{
    public class UsosPushNotificationsService : IUsosPushNotificationsService
    {
        MainDBContext _dbContext;
        IPushNotificationsService _pushNotificationsService;
        public UsosPushNotificationsService(MainDBContext dbContext, IPushNotificationsService pushNotificationsService)
        {
            _dbContext = dbContext;
            _pushNotificationsService = pushNotificationsService;
        }

        public async Task SendNewGradeNotification(IList<UsosGradeCreatedEntry> entries, string installation)
        {
            var grouped = GroupNotifications(entries);

            List<Task> tasks = new();

            foreach (var entry in grouped)
            {
                var relatedFcmTokens = GetFcmTokensFromDatabase(entry, installation);
                Dictionary<string, string> data = new()
                {
                    { "type", "usos/grades/grade"},
                    { "examId", entry.ExamId.ToString() },
                    { "examSessionNumber", entry.ExamSessionNumber.ToString() }

                };
                tasks.Add(_pushNotificationsService.SendNotifications(relatedFcmTokens, data));
            }

            await Task.WhenAll(tasks);
        }

        List<string> GetFcmTokensFromDatabase(NotificationGroup entry, string installation)
        {
            var users = _dbContext.Users
                .Where(x => x.Installation == installation && entry.RelatedUserIds.Contains(x.USOSId))
                .ToList();
            var tokens = users.
                SelectMany(x => x.GetFcmTokens())
                .ToList();
            return tokens;
        }

        List<NotificationGroup> GroupNotifications(IList<UsosGradeCreatedEntry> entries)
        {
            List<NotificationGroup> groups = new();
            foreach (var entry in entries)
            {
                if (entry.Operation != "create")
                {
                    continue;
                }

                var found = groups.Where(x => x.ExamId == entry.ExamId && x.ExamSessionNumber == entry.ExamSessionNumber).FirstOrDefault();
                if (found == null)
                {
                    found = new() { ExamId = entry.ExamId, ExamSessionNumber = entry.ExamSessionNumber };
                    groups.Add(found);
                }

                found.RelatedUserIds.AddRange(entry.RelatedUserIds);
            }
            return groups;
        }

        class NotificationGroup
        {
            public int ExamId { get; set; }
            public int ExamSessionNumber { get; set; }
            public List<string> RelatedUserIds { get; set; } = new();
        }

    }
}
