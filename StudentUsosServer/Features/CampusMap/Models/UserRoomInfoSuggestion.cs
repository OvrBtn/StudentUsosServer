using System.ComponentModel.DataAnnotations;

namespace StudentUsosServer.Features.CampusMap.Models;

public class UserRoomInfoSuggestion
{
    [Key]
    public int Id { get; set; }
    [MaxLength(10)]
    public required string UserStudentNumber { get; set; }
    public required string UserInstallation { get; set; }
    [MaxLength(5)]
    public required string BuildingId { get; set; }
    [MaxLength(10)]
    public required int RoomId { get; set; }
    [MaxLength(10)]
    public required string Floor { get; set; }
    [MaxLength(25)]
    public required string SuggestedRoomName { get; set; }
    [Range(1, 20)]
    public required int SuggestionWeight { get; set; }
}
