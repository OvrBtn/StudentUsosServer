using System.ComponentModel.DataAnnotations;

namespace StudentUsosServer.Features.CampusMap.Models;

public class RoomInfo
{
    [Key]
    public int InternalId { get; set; }
    public required int RoomId { get; set; }
    public required string BuildingId { get; set; }
    public required string Floor { get; set; }
    public required string Name { get; set; }
    public required int NameWeight { get; set; }

}
