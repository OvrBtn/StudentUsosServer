namespace StudentUsosServer.Features.CampusMap.Models;

public class RoomInfo
{
    public int RoomId { get; set; }
    public required string BuildingId { get; set; }
    public required string Floor { get; set; }
    public required string Name { get; set; }
    public required int NameWeight { get; set; }

}
