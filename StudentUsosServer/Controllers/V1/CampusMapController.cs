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
        const string whitelistedInstallationUrl = "https://usosapps.put.poznan.pl/";

        MainDBContext dbContext;
        ICampusMapRepository campusMapRepository;

        public CampusMapController(MainDBContext dBContext, ICampusMapRepository campusMapRepository)
        {
            this.dbContext = dBContext;
            this.campusMapRepository = campusMapRepository;
        }

        [HttpGet("CampusSvg"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
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
        [HttpGet("BuildingsList"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public async Task<ActionResult<string>> GetBuildingsListAsync()
        {
            var buildingsJson = await campusMapRepository.GetBuildingsListRawJsonAsync();
            return Ok(buildingsJson);
        }

        [HttpGet("FloorSvg"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public async Task<ActionResult<string>> GetFloorSvgAsync(string buildingId, string floor)
        {
            var svg = await campusMapRepository.GetFloorSvgAsync(buildingId, floor);
            return Ok(svg);
        }

        [HttpGet("FloorData"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public ActionResult<List<RoomInfo>> GetFloorData(string buildingId, string floor)
        {
            var rooms = campusMapRepository.GetFloorData(buildingId, floor);
            return Ok(rooms);
        }

        const int RootUserSuggestionWeight = 10;
        [HttpPost("UserSuggestion"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
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
                return StatusCode(StatusCodes.Status403Forbidden);
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

        [HttpGet("BuildingAndFloorUserSuggestionVotes"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public ActionResult<List<UserSuggestionVote>> GetBuildingAndFloorUserSuggestionVotes(string buildingId, string floor)
        {
            var data = campusMapRepository.GetFloorData(buildingId, floor);
            return Ok(data);
        }

        public class UserSuggestionVoteDto
        {
            required public string BuildingId { get; set; }
            required public string Floor { get; set; }
            required public string RoomId { get; set; }
            required public string InternalUserSuggestionId { get; set; }
            required public string StudentNumber { get; set; }
        }

        [HttpPost("UpvoteUserSuggestion"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public async Task<ActionResult<List<UserSuggestionVote>>> UpvoteUserSuggestion(UserSuggestionVoteDto userSuggestionVoteDto,
            [FromHeader] string installation,
            [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation &&
            x.InternalAccessToken == internalAccessToken &&
            x.StudentNumber == userSuggestionVoteDto.StudentNumber);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var result = await campusMapRepository.UserSuggestionCastVoteAsync(userSuggestionVoteDto.BuildingId,
                userSuggestionVoteDto.Floor,
                int.Parse(userSuggestionVoteDto.RoomId),
                int.Parse(userSuggestionVoteDto.InternalUserSuggestionId),
                1,
                user);

            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("DownvoteUserSuggestion"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public async Task<ActionResult<List<UserSuggestionVote>>> DownvoteUserSuggestion(UserSuggestionVoteDto userSuggestionVoteDto,
            [FromHeader] string installation,
            [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation &&
            x.InternalAccessToken == internalAccessToken &&
            x.StudentNumber == userSuggestionVoteDto.StudentNumber);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var result = await campusMapRepository.UserSuggestionCastVoteAsync(userSuggestionVoteDto.BuildingId,
                userSuggestionVoteDto.Floor,
                int.Parse(userSuggestionVoteDto.RoomId),
                int.Parse(userSuggestionVoteDto.InternalUserSuggestionId),
                -1,
                user);

            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet("UsersUpvotes"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public ActionResult<List<UserSuggestionVote>> GetUsersUpvotes([FromHeader] string installation, [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation &&
            x.InternalAccessToken == internalAccessToken);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var result = campusMapRepository.GetUsersUpvotes(user).Select(x => x.InternalUserSuggestionId.ToString());
            return Ok(result);
        }

        [HttpGet("UsersDownvotes"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
        public ActionResult<List<UserSuggestionVote>> GetUsersDownvotes([FromHeader] string installation, [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation &&
            x.InternalAccessToken == internalAccessToken);
            if (user is null)
            {
                return BadRequest("User not found");
            }

            var result = campusMapRepository.GetUsersDownvotes(user).Select(x => x.InternalUserSuggestionId.ToString());
            return Ok(result);
        }


#if DEBUG

        class ImportedFloorData
        {
            public required int RoomId { get; set; }
            public required string RoomName { get; set; }
        }

        [HttpPost("ImportFloorData"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full, whitelistedInstallationUrl)]
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
                    IsCreatedByRootUser = true,
                    IsImported = true
                };

                campusMapRepository.RegisterUserSuggestion(userRoomInfoSuggestion);
            }

            return Ok();
        }
#endif
    }
}
