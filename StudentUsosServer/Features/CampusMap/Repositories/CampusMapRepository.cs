using StudentUsosServer.Database;
using StudentUsosServer.Features.CampusMap.Models;
using StudentUsosServer.Models;
using System.Text.Json;

namespace StudentUsosServer.Features.CampusMap.Repositories;

public class CampusMapRepository : ICampusMapRepository
{
    MainDBContext dbContext;

    public CampusMapRepository(MainDBContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<string> GetBuildingsListRawJsonAsync()
    {
        using StreamReader streamReader = new(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CampusMapBuildings.json"));
        var json = await streamReader.ReadToEndAsync();
        return json;
    }

    public async Task<List<CampusBuilding>> GetBuildingsListAsync()
    {
        var json = await GetBuildingsListRawJsonAsync();
        return JsonSerializer.Deserialize<List<CampusBuilding>>(json) ?? new();
    }

    public async Task<string> GetFloorSvgAsync(string buildingId, string floor)
    {
        using StreamReader streamReader = new(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CampusMap", $"{buildingId}_{floor}.svg"));
        var svg = await streamReader.ReadToEndAsync();
        return svg;
    }

    const int MinWeightOfRoomInfo = 4;
    public List<RoomInfo> GetFloorData(string buildingId, string floor)
    {
        return dbContext.RoomInfos.Where(x => x.NameWeight >= MinWeightOfRoomInfo
        && x.BuildingId == buildingId
        && x.Floor == floor)
            .ToList();
    }

    public bool CanRegisterUserSuggestion(UserRoomInfoSuggestionDTO userRoomInfoSuggestion)
    {
        var result = dbContext.UserRoomInfoSuggestions.Any(x => x.UserStudentNumber == userRoomInfoSuggestion.UserStudentNumber
                                                                && x.RoomId.ToString() == userRoomInfoSuggestion.RoomId
                                                                && x.BuildingId == userRoomInfoSuggestion.BuildingId) == false;
        return result;
    }

    public void RegisterUserSuggestion(UserRoomInfoSuggestion userRoomInfoSuggestion)
    {
        dbContext.UserRoomInfoSuggestions.Add(userRoomInfoSuggestion);

        var roomInfo = dbContext.RoomInfos.FirstOrDefault(x => x.RoomId == userRoomInfoSuggestion.RoomId
        && x.BuildingId == userRoomInfoSuggestion.BuildingId
        && x.Floor == userRoomInfoSuggestion.Floor
        && x.Name == userRoomInfoSuggestion.SuggestedRoomName);

        if (roomInfo is null)
        {
            dbContext.RoomInfos.Add(new()
            {
                BuildingId = userRoomInfoSuggestion.BuildingId,
                RoomId = userRoomInfoSuggestion.RoomId,
                Floor = userRoomInfoSuggestion.Floor,
                Name = userRoomInfoSuggestion.SuggestedRoomName,
                NameWeight = userRoomInfoSuggestion.SuggestionWeight
            });
        }
        else
        {
            roomInfo.NameWeight += userRoomInfoSuggestion.SuggestionWeight;
        }

        dbContext.SaveChanges();
    }

    public List<UserSuggestionVote> GetBuildingAndFloorUserSuggestionVotes(string buildingId, string floor)
    {
        var result = dbContext.UserSuggestionVotes.Where(x => x.CampusBuildingId == buildingId && x.Floor == floor).ToList();
        return result;
    }

    public async Task<bool> UserSuggestionCastVoteAsync(string buildingId, string floor, int roomId, int userSuggestionId, int vote, User user)
    {
        var foundVote = dbContext.UserSuggestionVotes.FirstOrDefault(x => x.UserStudentNumber == user.StudentNumber && x.UserInstallation == user.Installation);
        RoomInfo? roomInfo = dbContext.RoomInfos.FirstOrDefault(x => x.BuildingId == buildingId && x.Floor == floor && x.RoomId == roomId && x.InternalId == userSuggestionId);
        if (roomInfo is null)
        {
            return false;
        }

        if (foundVote is not null)
        {
            //since only one vote is allowed, revert the previous vote
            roomInfo.NameWeight += foundVote.Vote * -1;
            dbContext.UserSuggestionVotes.Remove(foundVote);

            if (foundVote.Vote == vote)
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        UserSuggestionVote userVote = new()
        {
            CampusBuildingId = buildingId,
            Floor = floor,
            RoomId = roomId,
            InternalUserSuggestionId = userSuggestionId,
            Vote = vote,
            UserInstallation = user.Installation,
            UserStudentNumber = user.StudentNumber,
        };
        dbContext.UserSuggestionVotes.Add(userVote);

        roomInfo.NameWeight += vote;
        await dbContext.SaveChangesAsync();

        return true;
    }
}
