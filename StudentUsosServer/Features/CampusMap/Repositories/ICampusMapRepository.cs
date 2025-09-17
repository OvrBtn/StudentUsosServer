using StudentUsosServer.Features.CampusMap.Models;
using StudentUsosServer.Models;

namespace StudentUsosServer.Features.CampusMap.Repositories;

public interface ICampusMapRepository
{
    public Task<string> GetBuildingsListRawJsonAsync();
    public Task<List<CampusBuilding>> GetBuildingsListAsync();

    public Task<string> GetFloorSvgAsync(string buildingId, string floor);
    public List<RoomInfo> GetFloorData(string buildingId, string floor);

    public bool CanRegisterUserSuggestion(UserRoomInfoSuggestionDTO userRoomInfoSuggestion);
    public void RegisterUserSuggestion(UserRoomInfoSuggestion userRoomInfoSuggestion);

    public List<UserSuggestionVote> GetBuildingAndFloorUserSuggestionVotes(string buildingId, string floor);
    public Task<bool> UserSuggestionCastVoteAsync(string buildingId, string floor, int roomId, int userSuggestionId, int vote, User user);
}
