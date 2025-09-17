namespace StudentUsosServer.Features.CampusMap.Models;

public record UserRoomInfoSuggestionDTO
{
    public required string UserStudentNumber { get; set; }
    public required string BuildingId { get; set; }
    public required string RoomId { get; set; }
    public required string Floor { get; set; }
    public required string SuggestedRoomName { get; set; }
}
