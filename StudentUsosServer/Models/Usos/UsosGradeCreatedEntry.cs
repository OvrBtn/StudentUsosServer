using System.Text.Json.Serialization;

namespace StudentUsosServer.Models.Usos
{
    public class UsosGradeCreatedEntry
    {
        [JsonPropertyName("exam_id")]
        public int ExamId { get; set; }

        [JsonPropertyName("exam_session_number")]
        public int ExamSessionNumber { get; set; }

        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("related_user_ids")]
        public List<string> RelatedUserIds { get; set; }
    }
}
