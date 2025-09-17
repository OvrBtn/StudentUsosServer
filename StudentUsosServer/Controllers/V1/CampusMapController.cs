using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentUsosServer.Database;
using StudentUsosServer.Features.CampusMap.Models;
using StudentUsosServer.Features.CampusMap.Repositories;
using StudentUsosServer.Filters;
using System.Text.Json;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/CampusMap"), ApiVersion(1)]
    public class CampusMapController : ControllerBase
    {
        MainDBContext dbContext;
        ICampusMapRepository campusMapRepository;

        public CampusMapController(MainDBContext dBContext, ICampusMapRepository campusMapRepository)
        {
            this.dbContext = dBContext;
            this.campusMapRepository = campusMapRepository;
        }

        [HttpGet("CampusSvg"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<string>> GetCampusSvgAsync()
        {
            using StreamReader streamReader = new(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CampusMap", "CampusMap.svg"));
            var campusMap = await streamReader.ReadToEndAsync();
            return Ok(campusMap);
        }

        /// <summary>
        /// Retrieves list of all buildings on the primary campus
        /// </summary>
        /// <returns></returns>
        [HttpGet("BuildingsList"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<string>> GetBuildingsListAsync()
        {
            var buildingsJson = await campusMapRepository.GetBuildingsListRawJsonAsync();
            return Ok(buildingsJson);
        }

        [HttpGet("FloorSvg"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<string>> GetFloorSvgAsync(string buildingId, string floor)
        {
            var svg = await campusMapRepository.GetFloorSvgAsync(buildingId, floor);
            return Ok(svg);
        }

        [HttpGet("FloorData"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public ActionResult<List<RoomInfo>> GetFloorData(string buildingId, string floor)
        {
            var rooms = campusMapRepository.GetFloorData(buildingId, floor);
            return Ok(rooms);
        }

        const int RootUserSuggestionWeight = 10;
        [HttpPost("UserSuggestion"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public ActionResult RegisterUserSuggestion(UserRoomInfoSuggestionDTO userRoomInfoSuggestionDto, [FromHeader] string installation, [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation && x.InternalAccessToken == internalAccessToken);
            if (user is null || user.StudentNumber != userRoomInfoSuggestionDto.UserStudentNumber)
            {
                return BadRequest();
            }

            bool isRootUser = userRoomInfoSuggestionDto.UserStudentNumber == Secrets.Default.RootUserStudentNumber;

            if (isRootUser == false && campusMapRepository.CanRegisterUserSuggestion(userRoomInfoSuggestionDto) == false)
            {
                return Forbid();
            }

            int suggestionWeight = 1;
            if (isRootUser)
            {
                suggestionWeight = RootUserSuggestionWeight;
            }

            UserRoomInfoSuggestion userRoomInfoSuggestion = new()
            {
                BuildingId = userRoomInfoSuggestionDto.BuildingId,
                Floor = userRoomInfoSuggestionDto.Floor,
                RoomId = int.Parse(userRoomInfoSuggestionDto.RoomId),
                SuggestedRoomName = userRoomInfoSuggestionDto.SuggestedRoomName,
                SuggestionWeight = suggestionWeight,
                UserInstallation = installation,
                UserStudentNumber = user.StudentNumber,
                IsCreatedByRootUser = isRootUser,
            };

            campusMapRepository.RegisterUserSuggestion(userRoomInfoSuggestion);
            return Ok();
        }

#if DEBUG

        class ImportedFloorData
        {
            public required int RoomId { get; set; }
            public required string RoomName { get; set; }
        }

        [HttpPost("ImportFloorData"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public ActionResult ImportFloorData(string json, string studentNumber, string installation, string buildingId, string floor)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.StudentNumber == studentNumber);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var deserialized = JsonSerializer.Deserialize<List<ImportedFloorData>>(json);
            if (deserialized is null)
            {
                return BadRequest("Error deserializing json");
            }

            for (int i = 0; i < deserialized.Count; i++)
            {

                var userRoomInfoSuggestion = new UserRoomInfoSuggestion()
                {
                    UserInstallation = installation,
                    SuggestionWeight = RootUserSuggestionWeight,
                    BuildingId = buildingId,
                    Floor = floor,
                    RoomId = deserialized[i].RoomId,
                    SuggestedRoomName = deserialized[i].RoomName,
                    UserStudentNumber = studentNumber,
                    IsCreatedByRootUser = true
                };

                campusMapRepository.RegisterUserSuggestion(userRoomInfoSuggestion);
            }

            return Ok();
        }
    }
#endif
}
