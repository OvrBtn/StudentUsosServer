using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudentUsosServer.Database;
using StudentUsosServer.Features.CampusMap.Models;
using StudentUsosServer.Features.CampusMap.Repositories;
using StudentUsosServer.Filters;

namespace StudentUsosServer.Controllers.V1
{
    [ApiController, Route("v{version:apiVersion}/CampusMap"), ApiVersion(1)]
    public class CampusMapController : ControllerBase
    {
        MainDBContext dbContext;
        ICampusMapRepository campusMapRepository;
        Secrets secrets;

        public CampusMapController(MainDBContext dBContext, ICampusMapRepository campusMapRepository, Secrets secrets)
        {
            this.dbContext = dBContext;
            this.campusMapRepository = campusMapRepository;
            this.secrets = secrets;
        }

        [HttpGet("CampusSvg"), AuthorizeAccessFilter(AuthorizeAccessFilter.Mode.Full)]
        public async Task<ActionResult<string>> GetCampusSvgAsync()
        {
            StreamReader streamReader = new(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CampusMap", "CampusMap.svg"));
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
        public ActionResult RegisterUserSuggestion(UserRoomInfoSuggestion userRoomInfoSuggestion, [FromHeader] string installation, [FromHeader] string internalAccessToken)
        {
            var user = dbContext.Users.FirstOrDefault(x => x.Installation == installation && x.InternalAccessToken == internalAccessToken);
            if (user is null || user.StudentNumber != userRoomInfoSuggestion.UserStudentNumber)
            {
                return BadRequest();
            }

            if (campusMapRepository.CanRegisterUserSuggestion(userRoomInfoSuggestion) == false)
            {
                return Forbid();
            }

            userRoomInfoSuggestion.UserInstallation = installation;
            userRoomInfoSuggestion.SuggestionWeight = 1;
            if (userRoomInfoSuggestion.UserStudentNumber == secrets.RootUserStudentNumber)
            {
                userRoomInfoSuggestion.SuggestionWeight = RootUserSuggestionWeight;
            }

            campusMapRepository.RegisterUserSuggestion(userRoomInfoSuggestion);
            return Ok();
        }
    }
}
