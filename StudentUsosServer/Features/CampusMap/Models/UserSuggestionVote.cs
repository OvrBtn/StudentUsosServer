using System.ComponentModel.DataAnnotations;

namespace StudentUsosServer.Features.CampusMap.Models;

public class UserSuggestionVote
{
    [Key]
    public int Id { get; set; }
    public string CampusBuildingId { get; set; }
    public string Floor { get; set; }
    public int RoomId { get; set; }
    public int InternalUserSuggestionId { get; set; }
    public int Vote { get; set; }

    public string UserInstallation { get; set; }
    public string UserStudentNumber { get; set; }

    public UserSuggestionVote()
    {
        
    }
}
