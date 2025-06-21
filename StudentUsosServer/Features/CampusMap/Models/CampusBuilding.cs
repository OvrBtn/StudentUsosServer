namespace StudentUsosServer.Features.CampusMap.Models;

public class CampusBuilding
{
    public string Id { get; set; }
    public Dictionary<string, string> Name { get; set; }
    public List<string> Floors { get; set; }
}
