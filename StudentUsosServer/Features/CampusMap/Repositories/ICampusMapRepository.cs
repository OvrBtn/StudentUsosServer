using StudentUsosServer.Features.CampusMap.Models;

namespace StudentUsosServer.Features.CampusMap.Repositories;

public interface ICampusMapRepository
{
    public Task<string> GetBuildingsListRawJsonAsync();
    public Task<List<CampusBuilding>> GetBuildingsListAsync();

    public Task<string> GetFloorSvgAsync(string buildingId, string floor);
    public List<RoomInfo> GetFloorData(string buildingId, string floor);

    public bool CanRegisterUserSuggestion(UserRoomInfoSuggestion userRoomInfoSuggestion);
    public void RegisterUserSuggestion(UserRoomInfoSuggestion userRoomInfoSuggestion);
}
